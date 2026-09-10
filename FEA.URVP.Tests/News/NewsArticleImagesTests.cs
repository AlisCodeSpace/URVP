using FEA.URVP.Application.News;
using FEA.URVP.Domain.Catalog;
using FEA.URVP.Domain.Entities.Files;
using FEA.URVP.Domain.Entities.News;

namespace FEA.URVP.Tests.News;

public sealed class NewsArticleImagesTests
{
    [Fact]
    public void Append_adds_file_id_and_keeps_existing_order()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var article = new NewsArticle { ImageFileIds = [first] };

        NewsArticleImages.Append(article, second);

        Assert.Equal([first, second], article.ImageFileIds);
    }

    [Fact]
    public void Replace_soft_deletes_removed_files()
    {
        var keep = Guid.NewGuid();
        var drop = Guid.NewGuid();
        var article = new NewsArticle { Id = Guid.NewGuid(), ImageFileIds = [keep, drop] };
        var owned = new List<FileStorage>
        {
            NewsFile(keep, article.Id),
            NewsFile(drop, article.Id),
        };

        var removed = NewsArticleImages.Replace(article, [keep], owned);

        Assert.Equal([keep], article.ImageFileIds);
        Assert.Single(removed);
        Assert.Equal(drop, removed[0].Id);
        Assert.True(removed[0].IsDeleted);
        Assert.False(owned[0].IsDeleted);
    }

    private static FileStorage NewsFile(Guid id, Guid articleId) => new()
    {
        Id = id,
        EntityType = FileStorageCatalog.EntityNewsArticle,
        EntityId = articleId,
        FileCategory = FileStorageCatalog.CategoryNewsImage,
        FileName = "photo.jpg",
        MimeType = "image/jpeg",
        Content = [1],
    };
}
