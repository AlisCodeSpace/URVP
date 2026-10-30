using FEA.URVP.Domain.Catalog;
using FEA.URVP.Domain.Entities.Files;
using FEA.URVP.Domain.Entities.News;

namespace FEA.URVP.Application.News;

public static class NewsArticleImages
{
    public static List<Guid> Normalize(IEnumerable<Guid>? ids)
    {
        var result = new List<Guid>();
        if (ids is null)
        {
            return result;
        }

        var seen = new HashSet<Guid>();
        foreach (var id in ids)
        {
            if (id == Guid.Empty || !seen.Add(id))
            {
                continue;
            }

            result.Add(id);
        }

        return result;
    }

    public static void EnsureCapacity(int count)
    {
        if (count > FileStorageCatalog.MaxNewsImages)
        {
            throw new ArgumentException(
                $"A news article can include at most {FileStorageCatalog.MaxNewsImages} images.");
        }
    }

    public static void Append(NewsArticle article, Guid fileId)
    {
        var ids = Normalize(article.ImageFileIds);
        if (ids.Contains(fileId))
        {
            article.ImageFileIds = ids;
            return;
        }

        EnsureCapacity(ids.Count + 1);
        ids.Add(fileId);
        article.ImageFileIds = ids;
    }

    public static IReadOnlyList<FileStorage> Replace(
        NewsArticle article,
        IReadOnlyList<Guid> requestedIds,
        IReadOnlyList<FileStorage> ownedFiles)
    {
        var next = Normalize(requestedIds);
        EnsureCapacity(next.Count);

        var ownedById = ownedFiles.ToDictionary(file => file.Id);
        foreach (var id in next)
        {
            if (!ownedById.TryGetValue(id, out var file)
                || file.EntityType != FileStorageCatalog.EntityNewsArticle
                || file.EntityId != article.Id
                || file.FileCategory != FileStorageCatalog.CategoryNewsImage)
            {
                throw new ArgumentException("One or more news images could not be found.");
            }
        }

        var removed = new List<FileStorage>();
        var keep = next.ToHashSet();
        foreach (var file in ownedFiles)
        {
            if (keep.Contains(file.Id))
            {
                continue;
            }

            file.IsDeleted = true;
            removed.Add(file);
        }

        article.ImageFileIds = next;
        return removed;
    }
}
