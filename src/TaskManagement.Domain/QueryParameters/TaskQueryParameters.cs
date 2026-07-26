using TaskManagement.Domain.Enums;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Domain.QueryParameters;

public class TaskQueryParameters
{
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 10;

    public TaskStatus? Status { get; set; }
    public TaskPriority? Priority { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public string? Search { get; set; }

    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "asc";
}
