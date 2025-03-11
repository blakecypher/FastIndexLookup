namespace IndexService.Tests;

public static class TestHelpers
{
    public static List<IndexEntry> LoadTesting(int count)
    {
        return Enumerable.Range(1, count).Select(i =>
        {
            var type = (IdentifierType)(i % 3);
            return new IndexEntry(
                type,
                $"Name{i % 2 + 1}", // 1-2 names per type
                $"Value{i}",
                i % 100 == 0 
                    ? [Guid.NewGuid(), Guid.NewGuid()] 
                    : [Guid.NewGuid()]);
        }).ToList();
    }
}