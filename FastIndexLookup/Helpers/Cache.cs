namespace IndexService;

public class Cache(IndexEntry entry, DateTime Time)
{
    public IndexEntry Entry { get; } = entry;
    public DateTime Time { get; set; } = Time;
}