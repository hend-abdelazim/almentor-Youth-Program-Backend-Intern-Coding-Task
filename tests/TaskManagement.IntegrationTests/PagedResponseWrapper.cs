namespace TaskManagement.IntegrationTests;

public class PagedResponseWrapper<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int Limit { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
