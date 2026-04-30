const enc = new TextEncoder()
const dec = new TextDecoder()

const b64 = (buf: ArrayBuffer | Uint8Array) => btoa(String.fromCharCode(...new Uint8Array(buf)))
const unb64 = (value: string) => Uint8Array.from(atob(value), c => c.charCodeAt(0))
const ab = (bytes: Uint8Array) => bytes.buffer.slice(bytes.byteOffset, bytes.byteOffset + bytes.byteLength) as ArrayBuffer

async function key(password: string, salt: Uint8Array) {
  const material = await crypto.subtle.importKey('raw', enc.encode(password), 'PBKDF2', false, ['deriveKey'])
  return crypto.subtle.deriveKey({ name: 'PBKDF2', salt: ab(salt), iterations: 210000, hash: 'SHA-256' }, material, { name: 'AES-GCM', length: 256 }, false, ['encrypt', 'decrypt'])
}

export async function encryptContent(plain: string, password: string) {
  const salt = crypto.getRandomValues(new Uint8Array(16))
  const iv = crypto.getRandomValues(new Uint8Array(12))
  const cipher = await crypto.subtle.encrypt({ name: 'AES-GCM', iv: ab(iv) }, await key(password, salt), enc.encode(plain))
  return { salt: b64(salt), iv: b64(iv), cipherText: b64(cipher) }
}

export async function decryptContent(cipherText: string, password: string, saltText: string, ivText: string) {
  const salt = unb64(saltText)
  const iv = unb64(ivText)
  const plain = await crypto.subtle.decrypt({ name: 'AES-GCM', iv: ab(iv) }, await key(password, salt), ab(unb64(cipherText)))
  return dec.decode(plain)
}
