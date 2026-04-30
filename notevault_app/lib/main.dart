// ignore_for_file: use_build_context_synchronously

import 'dart:convert';
import 'dart:math';
import 'dart:typed_data';

import 'package:file_picker/file_picker.dart';
import 'package:flutter/material.dart';
import 'package:flutter_markdown/flutter_markdown.dart';
import 'package:http/http.dart' as http;
import 'package:pointycastle/api.dart' show AEADParameters, KeyParameter;
import 'package:pointycastle/block/aes.dart';
import 'package:pointycastle/block/modes/gcm.dart';
import 'package:pointycastle/digests/sha256.dart';
import 'package:pointycastle/key_derivators/api.dart';
import 'package:pointycastle/key_derivators/pbkdf2.dart';
import 'package:pointycastle/macs/hmac.dart';
import 'package:provider/provider.dart';
import 'package:shared_preferences/shared_preferences.dart';

const apiBase = String.fromEnvironment('API_BASE', defaultValue: 'http://localhost:5111');

void main() => runApp(ChangeNotifierProvider(create: (_) => Session(), child: const NoteVaultApp()));

class NoteVaultApp extends StatelessWidget {
  const NoteVaultApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'NoteVault',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(colorScheme: ColorScheme.fromSeed(seedColor: const Color(0xff5b8def), brightness: Brightness.light), scaffoldBackgroundColor: const Color(0xfff7f8fa), useMaterial3: true),
      darkTheme: ThemeData(colorScheme: ColorScheme.fromSeed(seedColor: const Color(0xff7aa2ff), brightness: Brightness.dark), useMaterial3: true),
      home: Consumer<Session>(builder: (_, session, __) => session.accessToken == null ? const LoginPage() : const NotesPage()),
    );
  }
}

class Session extends ChangeNotifier {
  String? accessToken;
  String? refreshToken;
  bool ready = false;

  Session() {
    _load();
  }

  Future<void> _load() async {
    final prefs = await SharedPreferences.getInstance();
    accessToken = prefs.getString('access');
    refreshToken = prefs.getString('refresh');
    ready = true;
    notifyListeners();
  }

  Future<void> login(String username, String password) async {
    final res = await http.post(Uri.parse('$apiBase/api/auth/login'), headers: jsonHeaders(), body: jsonEncode({'username': username, 'password': password}));
    if (res.statusCode != 200) throw Exception('login failed');
    final data = jsonDecode(res.body);
    accessToken = data['accessToken'];
    refreshToken = data['refreshToken'];
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('access', accessToken!);
    await prefs.setString('refresh', refreshToken!);
    notifyListeners();
  }

  Future<void> logout() async {
    accessToken = null;
    refreshToken = null;
    final prefs = await SharedPreferences.getInstance();
    await prefs.clear();
    notifyListeners();
  }
}

Map<String, String> jsonHeaders([Session? session]) => {
      'content-type': 'application/json',
      if (session?.accessToken != null) 'authorization': 'Bearer ${session!.accessToken}',
    };

class Api {
  Api(this.session);
  final Session session;

  Future<List<dynamic>> notes({String q = '', bool recent = false}) async {
    final uri = Uri.parse('$apiBase/api/notes').replace(queryParameters: {if (q.isNotEmpty) 'q': q, if (recent) 'recent': 'true'});
    final res = await http.get(uri, headers: jsonHeaders(session));
    return jsonDecode(res.body);
  }

  Future<Map<String, dynamic>> detail(String id) async {
    final res = await http.get(Uri.parse('$apiBase/api/notes/$id'), headers: jsonHeaders(session));
    return jsonDecode(res.body);
  }

  Future<Map<String, dynamic>> save(Map<String, dynamic> note) async {
    final payload = {
      'folderId': note['folderId'],
      'title': note['title'] ?? '无标题',
      'content': note['content'] ?? '',
      'contentType': note['contentType'] ?? 'markdown',
      'isEncrypted': note['isEncrypted'] ?? false,
      'salt': note['salt'],
      'iv': note['iv'],
      'cipherText': note['cipherText'],
      'isPinned': note['isPinned'] ?? false,
      'tagIds': (note['tags'] as List? ?? []).map((x) => x['id']).toList(),
    };
    final id = note['id'];
    final res = id == null
        ? await http.post(Uri.parse('$apiBase/api/notes'), headers: jsonHeaders(session), body: jsonEncode(payload))
        : await http.put(Uri.parse('$apiBase/api/notes/$id'), headers: jsonHeaders(session), body: jsonEncode(payload));
    return jsonDecode(res.body);
  }

  Future<void> open(String id) => http.post(Uri.parse('$apiBase/api/notes/$id/open'), headers: jsonHeaders(session));

  Future<String> upload(PlatformFile file, String? noteId) async {
    final req = http.MultipartRequest('POST', Uri.parse('$apiBase/api/upload${noteId == null ? '' : '?noteId=$noteId'}'));
    req.headers['authorization'] = 'Bearer ${session.accessToken}';
    req.files.add(http.MultipartFile.fromBytes('file', file.bytes!, filename: file.name));
    final res = await req.send();
    final body = jsonDecode(await res.stream.bytesToString());
    return body['fileUrl'];
  }
}

class LoginPage extends StatefulWidget {
  const LoginPage({super.key});
  @override
  State<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
  final user = TextEditingController(text: 'admin');
  final pass = TextEditingController(text: 'admin123');
  bool loading = false;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(
        child: ConstrainedBox(
          constraints: const BoxConstraints(maxWidth: 380),
          child: Card(
            margin: const EdgeInsets.all(20),
            child: Padding(
              padding: const EdgeInsets.all(24),
              child: Column(mainAxisSize: MainAxisSize.min, crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                const Text('NoteVault', style: TextStyle(fontSize: 28, fontWeight: FontWeight.w700)),
                const SizedBox(height: 6),
                Text('私有云笔记系统', style: TextStyle(color: Theme.of(context).colorScheme.outline)),
                const SizedBox(height: 28),
                TextField(controller: user, decoration: const InputDecoration(labelText: '账号')),
                const SizedBox(height: 12),
                TextField(controller: pass, decoration: const InputDecoration(labelText: '密码'), obscureText: true),
                const SizedBox(height: 20),
                FilledButton(
                  onPressed: loading ? null : () async {
                    setState(() => loading = true);
                    try {
                      await context.read<Session>().login(user.text, pass.text);
                    } catch (_) {
                      if (mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('账号或密码不正确')));
                    } finally {
                      if (mounted) setState(() => loading = false);
                    }
                  },
                  child: Text(loading ? '登录中' : '登录'),
                )
              ]),
            ),
          ),
        ),
      ),
    );
  }
}

class NotesPage extends StatefulWidget {
  const NotesPage({super.key});
  @override
  State<NotesPage> createState() => _NotesPageState();
}

class _NotesPageState extends State<NotesPage> {
  final search = TextEditingController();
  List<dynamic> notes = [];
  bool recent = false;

  @override
  void initState() {
    super.initState();
    refresh();
  }

  Future<void> refresh() async {
    final api = Api(context.read<Session>());
    notes = await api.notes(q: search.text, recent: recent);
    if (mounted) setState(() {});
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('NoteVault'),
        actions: [
          IconButton(onPressed: () { recent = !recent; refresh(); }, icon: const Icon(Icons.history)),
          IconButton(onPressed: () => context.read<Session>().logout(), icon: const Icon(Icons.logout)),
        ],
      ),
      body: Column(children: [
        Padding(
          padding: const EdgeInsets.all(12),
          child: TextField(controller: search, onSubmitted: (_) => refresh(), decoration: const InputDecoration(prefixIcon: Icon(Icons.search), hintText: '搜索笔记标题', border: OutlineInputBorder())),
        ),
        Expanded(
          child: notes.isEmpty
              ? const Center(child: Text('还没有笔记'))
              : ListView.separated(
                  padding: const EdgeInsets.fromLTRB(12, 0, 12, 90),
                  itemCount: notes.length,
                  separatorBuilder: (_, __) => const SizedBox(height: 8),
                  itemBuilder: (context, i) {
                    final note = notes[i];
                    return Card(
                      child: ListTile(
                        leading: Icon(note['isEncrypted'] == true ? Icons.lock : Icons.description_outlined),
                        title: Text(note['title'] ?? '无标题'),
                        subtitle: Text(note['contentType'] ?? 'markdown'),
                        trailing: note['isPinned'] == true ? const Icon(Icons.push_pin, size: 18) : null,
                        onTap: () async {
                          await Navigator.of(context).push(MaterialPageRoute(builder: (_) => EditorPage(noteId: note['id'])));
                          refresh();
                        },
                      ),
                    );
                  },
                ),
        )
      ]),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () async {
          final api = Api(context.read<Session>());
          final note = await api.save({'title': '无标题', 'content': '', 'contentType': 'markdown', 'isEncrypted': false, 'isPinned': false, 'tags': []});
          if (mounted) await Navigator.of(context).push(MaterialPageRoute(builder: (_) => EditorPage(noteId: note['id'])));
          refresh();
        },
        icon: const Icon(Icons.add),
        label: const Text('新建'),
      ),
    );
  }
}

class EditorPage extends StatefulWidget {
  const EditorPage({super.key, required this.noteId});
  final String noteId;
  @override
  State<EditorPage> createState() => _EditorPageState();
}

class _EditorPageState extends State<EditorPage> {
  Map<String, dynamic>? note;
  final title = TextEditingController();
  final content = TextEditingController();
  bool preview = false;

  @override
  void initState() {
    super.initState();
    load();
  }

  Future<void> load() async {
    final api = Api(context.read<Session>());
    final data = await api.detail(widget.noteId);
    await api.open(widget.noteId);
    note = data;
    title.text = data['title'] ?? '';
    if (data['isEncrypted'] == true) {
      final password = await askPassword('解锁笔记');
      if (password != null) {
        try {
          content.text = CryptoBox.decrypt(data['cipherText'], password, data['salt'], data['iv']);
        } catch (_) {
          if (mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('密码不正确')));
        }
      }
    } else {
      content.text = data['content'] ?? '';
    }
    if (mounted) setState(() {});
  }

  Future<String?> askPassword(String title) => showDialog<String>(
        context: context,
        builder: (context) {
          final c = TextEditingController();
          return AlertDialog(
            title: Text(title),
            content: TextField(controller: c, obscureText: true, decoration: const InputDecoration(labelText: '密码')),
            actions: [TextButton(onPressed: () => Navigator.pop(context), child: const Text('取消')), FilledButton(onPressed: () => Navigator.pop(context, c.text), child: const Text('确定'))],
          );
        },
      );

  Future<void> save({bool encrypted = false}) async {
    if (note == null) return;
    note!['title'] = title.text;
    if (encrypted) {
      final password = await askPassword('新建加密笔记');
      if (password == null || password.isEmpty) return;
      note!.addAll(CryptoBox.encrypt(content.text, password));
      note!['content'] = '';
      note!['isEncrypted'] = true;
    } else if (note!['isEncrypted'] != true) {
      note!['content'] = content.text;
    }
    note = await Api(context.read<Session>()).save(note!);
    if (mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('已保存')));
  }

  Future<void> uploadImage() async {
    final picked = await FilePicker.platform.pickFiles(type: FileType.image, withData: true);
    if (picked == null) return;
    final url = await Api(context.read<Session>()).upload(picked.files.first, note?['id']);
    content.text = '${content.text}\n![]($url)';
    setState(() {});
  }

  @override
  Widget build(BuildContext context) {
    if (note == null) return const Scaffold(body: Center(child: CircularProgressIndicator()));
    final isMarkdown = note!['contentType'] == 'markdown';
    return Scaffold(
      appBar: AppBar(
        title: TextField(controller: title, decoration: const InputDecoration(border: InputBorder.none, hintText: '标题')),
        actions: [
          IconButton(onPressed: uploadImage, icon: const Icon(Icons.image_outlined)),
          IconButton(onPressed: () => setState(() => preview = !preview), icon: const Icon(Icons.visibility_outlined)),
          IconButton(onPressed: () => save(encrypted: true), icon: const Icon(Icons.lock_outline)),
          IconButton(onPressed: () => save(), icon: const Icon(Icons.save_outlined)),
        ],
      ),
      body: Padding(
        padding: const EdgeInsets.all(12),
        child: preview && isMarkdown
            ? Markdown(data: content.text)
            : TextField(
                controller: content,
                keyboardType: TextInputType.multiline,
                maxLines: null,
                expands: true,
                textAlignVertical: TextAlignVertical.top,
                decoration: InputDecoration(
                  hintText: isMarkdown ? '写点 Markdown...' : '写点富文本 HTML...',
                  filled: true,
                  border: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: BorderSide.none),
                ),
              ),
      ),
    );
  }
}

class CryptoBox {
  static Map<String, String> encrypt(String plain, String password) {
    final salt = _random(16);
    final iv = _random(12);
    final cipher = GCMBlockCipher(AESEngine())..init(true, AEADParameters(KeyParameter(_key(password, salt)), 128, iv, Uint8List(0)));
    final out = cipher.process(Uint8List.fromList(utf8.encode(plain)));
    return {'salt': base64Encode(salt), 'iv': base64Encode(iv), 'cipherText': base64Encode(out)};
  }

  static String decrypt(String cipherText, String password, String saltText, String ivText) {
    final salt = base64Decode(saltText);
    final iv = base64Decode(ivText);
    final cipher = GCMBlockCipher(AESEngine())..init(false, AEADParameters(KeyParameter(_key(password, salt)), 128, iv, Uint8List(0)));
    final out = cipher.process(base64Decode(cipherText));
    return utf8.decode(out);
  }

  static Uint8List _key(String password, Uint8List salt) {
    final derivator = PBKDF2KeyDerivator(HMac(SHA256Digest(), 64))..init(Pbkdf2Parameters(salt, 210000, 32));
    return derivator.process(Uint8List.fromList(utf8.encode(password)));
  }

  static Uint8List _random(int length) {
    final rnd = Random.secure();
    return Uint8List.fromList(List.generate(length, (_) => rnd.nextInt(256)));
  }
}
