using TaskManagement.Domain.Enums;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Application.DTOs.Tasks;

public class CreateTaskRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus? Status { get; set; } = TaskStatus.Todo;
    public TaskPriority? Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
}
