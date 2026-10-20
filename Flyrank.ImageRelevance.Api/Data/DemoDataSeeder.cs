using Flyrank.ImageRelevance.Api.Domain.Models;
using Flyrank.ImageRelevance.Api.Services;

namespace Flyrank.ImageRelevance.Api.Data;

public static class DemoDataSeeder
{
    public static void Seed(IImageRelevanceRepository repository, IMismatchGuard mismatchGuard)
    {
        var foxPost = new Post(
            Guid.Parse("10000000-0000-0000-0000-000000000001"),
            "Behavior and Habitat of Red Foxes",
            "Red foxes are adaptable mammals that live in forests, grasslands, and urban areas.",
            [1, 0, 0, 0],
            DateTimeOffset.Parse("2026-01-10T09:00:00Z"));
        var wolfPost = new Post(
            Guid.Parse("10000000-0000-0000-0000-000000000002"),
            "How Wolves Live in Packs",
            "Wolves are social wild canines that hunt and travel in packs.",
            [0, 1, 0, 0],
            DateTimeOffset.Parse("2026-01-11T09:00:00Z"));
        var catPost = new Post(
            Guid.Parse("10000000-0000-0000-0000-000000000003"),
            "Indoor Cat Care",
            "Tips for keeping domestic cats active and healthy.",
            [0, 0, 1, 0],
            DateTimeOffset.Parse("2026-01-12T09:00:00Z"));

        foreach (var post in new[] { foxPost, wolfPost, catPost })
        {
            if (repository.GetPost(post.Id) is null)
                repository.AddPost(post);
        }

        SeedImage(
            repository,
            mismatchGuard,
            new ImageMetadata(
                Guid.Parse("20000000-0000-0000-0000-000000000001"),
                "https://images.example.test/red-fox.jpg",
                "fox",
                "wildlife",
                ["red fox", "red fur", "forest"],
                "A red fox in a forest.",
                0.97,
                DateTimeOffset.Parse("2026-01-13T09:00:00Z")),
            [1, 0, 0, 0]);
        SeedImage(
            repository,
            mismatchGuard,
            new ImageMetadata(
                Guid.Parse("20000000-0000-0000-0000-000000000002"),
                "https://images.example.test/domestic-cat.jpg",
                "cat",
                "pets",
                ["domestic cat", "tabby", "indoors"],
                "A tabby cat sitting indoors.",
                0.95,
                DateTimeOffset.Parse("2026-01-14T09:00:00Z")),
            [0, 0, 1, 0]);
    }

    private static void SeedImage(
        IImageRelevanceRepository repository,
        IMismatchGuard mismatchGuard,
        ImageMetadata image,
        IReadOnlyList<float> embedding)
    {
        if (repository.GetImage(image.Id) is not null)
            return;

        repository.AddImage(image);
        foreach (var post in repository.GetPosts())
        {
            var score = CosineSimilarity.Calculate(embedding, post.Embedding);
            repository.SaveSuggestion(new ImageSuggestion(
                post.Id,
                image.Id,
                score,
                false,
                mismatchGuard.GetRejectionReason(image, post)));
        }
    }
}
