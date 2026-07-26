namespace TaskManagement.Application.DTOs.Projects;

public class UpdateProjectRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
