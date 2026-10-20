namespace Flyrank.ImageRelevance.Api.Services;

public static class CosineSimilarity
{
    public static double Calculate(IReadOnlyList<float> left, IReadOnlyList<float> right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        if (left.Count == 0 || left.Count != right.Count)
            throw new ArgumentException("Embedding vectors must be non-empty and have equal dimensions.");

        double dotProduct = 0;
        double leftMagnitude = 0;
        double rightMagnitude = 0;

        for (var index = 0; index < left.Count; index++)
        {
            dotProduct += left[index] * right[index];
            leftMagnitude += left[index] * left[index];
            rightMagnitude += right[index] * right[index];
        }

        if (leftMagnitude == 0 || rightMagnitude == 0)
            return 0;

        return dotProduct / (Math.Sqrt(leftMagnitude) * Math.Sqrt(rightMagnitude));
    }
}
