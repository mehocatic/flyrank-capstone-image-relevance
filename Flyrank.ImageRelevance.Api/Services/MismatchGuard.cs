using System.Text.RegularExpressions;
using Flyrank.ImageRelevance.Api.Domain.Models;

namespace Flyrank.ImageRelevance.Api.Services;

public sealed partial class MismatchGuard : IMismatchGuard
{
    private static readonly string[][] AnimalAliases =
    [
        ["fox", "foxes", "vixen"],
        ["wolf", "wolves"],
        ["dog", "dogs", "canine"],
        ["cat", "cats", "feline"],
        ["bear", "bears"],
        ["lion", "lions"],
        ["tiger", "tigers"],
        ["horse", "horses"],
        ["deer"],
        ["rabbit", "hare"],
        ["eagle", "eagles"],
        ["owl", "owls"]
    ];

    public string? GetRejectionReason(ImageMetadata image, Post post)
    {
        var imageText = $"{image.Subject} {string.Join(' ', image.Attributes)}";
        var postText = $"{post.Title} {post.Content}";
        var imageAnimals = FindAnimals(imageText);
        var postAnimals = FindAnimals(postText);

        if (imageAnimals.Count == 0 || postAnimals.Count == 0)
            return null;

        var matchingAnimal = imageAnimals.Intersect(postAnimals, StringComparer.OrdinalIgnoreCase).FirstOrDefault();
        if (matchingAnimal is not null)
            return null;

        var expected = postAnimals.First();
        var detected = imageAnimals.First();
        return $"Animal category mismatch: expected {expected}, detected {detected}";
    }

    private static HashSet<string> FindAnimals(string text)
    {
        var normalized = WordRegex().Replace(text.ToLowerInvariant(), " ");
        var words = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var aliases in AnimalAliases)
        {
            if (aliases.Any(alias => words.Contains(alias, StringComparer.OrdinalIgnoreCase)))
                result.Add(aliases[0]);
        }

        return result;
    }

    [GeneratedRegex("[^a-z0-9]+", RegexOptions.CultureInvariant)]
    private static partial Regex WordRegex();
}
