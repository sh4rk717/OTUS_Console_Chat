namespace FirstInteract.Helpers;

public static class EnumerableExtension
{
    public static IEnumerable<T> GetBatchByNumber<T>(this IEnumerable<T> source, int batchSize, int batchNumber)
    {
        // Проверка граничных значений
        ArgumentNullException.ThrowIfNull(source);
        if (batchSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(batchSize), "Batch size must be positive");
        if (batchNumber < 0)
            throw new ArgumentOutOfRangeException(nameof(batchNumber), "Batch number must be non-negative");

        return source.Skip(batchNumber * batchSize).Take(batchSize);
    }
}