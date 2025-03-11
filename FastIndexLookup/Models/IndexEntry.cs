namespace IndexService;

[GenerateSerializer]
[Serializable]
public sealed record IndexEntry(
    [property: Id(0)] IdentifierType Type,
    [property: Id(1)] string Key,
    [property: Id(2)] string Value,
    [property: Id(3)] Guid[] IDs
);