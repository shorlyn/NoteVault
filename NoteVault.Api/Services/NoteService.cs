using NoteVault.Api.DTOs;
using NoteVault.Api.Models;
using SqlSugar;

namespace NoteVault.Api.Services;

public class NoteService(SqlSugarClient db)
{
    public async Task<List<NoteListDto>> ListAsync(string? folderId, string? q, string? tagId, bool deleted, bool recent)
    {
        var query = db.Queryable<Note>().Where(x => x.IsDeleted == deleted);
        if (!string.IsNullOrWhiteSpace(folderId)) query = query.Where(x => x.FolderId == folderId);
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(x => x.Title.Contains(q) || (!x.IsEncrypted && x.Content.Contains(q)));
        if (!string.IsNullOrWhiteSpace(tagId)) query = query.Where(x => SqlFunc.Subqueryable<NoteTag>().Where(nt => nt.NoteId == x.Id && nt.TagId == tagId).Any());
        if (recent) query = query.Where(x => x.LastOpenedAt != null);
        var notes = await query.OrderBy(x => x.IsPinned, OrderByType.Desc).OrderBy(x => recent ? x.LastOpenedAt : x.UpdatedAt, OrderByType.Desc).ToListAsync();
        var result = new List<NoteListDto>();
        foreach (var note in notes) result.Add(new NoteListDto(note.Id, note.FolderId, note.Title, note.ContentType, note.IsEncrypted, note.IsPinned, note.IsDeleted, note.LastOpenedAt, note.CreatedAt, note.UpdatedAt, await TagsForNoteAsync(note.Id)));
        return result;
    }

    public async Task<NoteDetailDto?> DetailAsync(string id)
    {
        var note = await db.Queryable<Note>().FirstAsync(x => x.Id == id);
        if (note is null) return null;
        var attachments = await db.Queryable<Attachment>().Where(x => x.NoteId == note.Id).ToListAsync();
        return ToDetail(note, await TagsForNoteAsync(note.Id), attachments.Select(x => x.ToDto()));
    }

    public async Task<NoteDetailDto> CreateAsync(NoteRequest request)
    {
        var note = Apply(new Note(), request);
        await db.Insertable(note).ExecuteCommandAsync();
        await SaveTagsAsync(note.Id, request.TagIds);
        return (await DetailAsync(note.Id))!;
    }

    public async Task<NoteDetailDto?> UpdateAsync(string id, NoteRequest request)
    {
        var note = await db.Queryable<Note>().FirstAsync(x => x.Id == id);
        if (note is null) return null;
        Apply(note, request);
        note.UpdatedAt = DateTime.UtcNow;
        await db.Updateable(note).ExecuteCommandAsync();
        await SaveTagsAsync(note.Id, request.TagIds);
        return await DetailAsync(note.Id);
    }

    public async Task<bool> SoftDeleteAsync(string id) => await SetDeletedAsync(id, true);
    public async Task<bool> RestoreAsync(string id) => await SetDeletedAsync(id, false);
    public async Task<bool> HardDeleteAsync(string id)
    {
        await db.Deleteable<NoteTag>().Where(x => x.NoteId == id).ExecuteCommandAsync();
        await db.Deleteable<Attachment>().Where(x => x.NoteId == id).ExecuteCommandAsync();
        return await db.Deleteable<Note>().Where(x => x.Id == id && x.IsDeleted).ExecuteCommandAsync() > 0;
    }

    public async Task<bool> OpenAsync(string id)
    {
        var note = await db.Queryable<Note>().FirstAsync(x => x.Id == id);
        if (note is null) return false;
        note.LastOpenedAt = DateTime.UtcNow;
        await db.Updateable(note).ExecuteCommandAsync();
        return true;
    }

    private async Task<bool> SetDeletedAsync(string id, bool deleted)
    {
        var note = await db.Queryable<Note>().FirstAsync(x => x.Id == id);
        if (note is null) return false;
        note.IsDeleted = deleted;
        note.UpdatedAt = DateTime.UtcNow;
        await db.Updateable(note).ExecuteCommandAsync();
        return true;
    }

    private async Task<List<TagDto>> TagsForNoteAsync(string noteId)
    {
        var tags = await db.Queryable<Tag>().InnerJoin<NoteTag>((t, nt) => t.Id == nt.TagId).Where((t, nt) => nt.NoteId == noteId).Select(t => t).ToListAsync();
        return tags.Select(x => x.ToDto()).ToList();
    }

    private async Task SaveTagsAsync(string noteId, string[] tagIds)
    {
        await db.Deleteable<NoteTag>().Where(x => x.NoteId == noteId).ExecuteCommandAsync();
        var rows = tagIds.Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => new NoteTag { NoteId = noteId, TagId = x }).ToList();
        if (rows.Count > 0) await db.Insertable(rows).ExecuteCommandAsync();
    }

    private static Note Apply(Note note, NoteRequest request)
    {
        note.FolderId = request.FolderId;
        note.Title = string.IsNullOrWhiteSpace(request.Title) ? "Untitled" : request.Title.Trim();
        note.ContentType = request.ContentType is "html" ? "html" : "markdown";
        note.IsEncrypted = request.IsEncrypted;
        note.IsPinned = request.IsPinned;
        note.Salt = request.Salt;
        note.Iv = request.Iv;
        note.CipherText = request.CipherText;
        note.Content = request.IsEncrypted ? "" : request.Content;
        return note;
    }

    private static NoteDetailDto ToDetail(Note n, IEnumerable<TagDto> tags, IEnumerable<AttachmentDto> attachments) =>
        new(n.Id, n.FolderId, n.Title, n.Content, n.ContentType, n.IsEncrypted, n.Salt, n.Iv, n.CipherText, n.IsPinned, n.IsDeleted, n.LastOpenedAt, n.CreatedAt, n.UpdatedAt, tags, attachments);
}
