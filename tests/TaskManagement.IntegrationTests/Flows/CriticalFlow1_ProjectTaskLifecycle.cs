using System.Net;
using System.Net.Http.Json;
using TaskManagement.Application.DTOs.Projects;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Persistence;
using TaskManagement.IntegrationTests.Helpers;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;
using Microsoft.EntityFrameworkCore;

namespace TaskManagement.IntegrationTests.Flows;

public class CriticalFlow1_ProjectTaskLifecycle : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CriticalFlow1_ProjectTaskLifecycle(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task FullLifecycle_Register_CreateProject_CreateTask_MarkDone_DeleteProject_VerifySoftDelete()
    {
        var userEmail = $"flow1_{Guid.NewGuid()}@test.com";
        var userName = $"flow1_user_{Guid.NewGuid():N}";
        const string password = "TestPass123!";

        var auth = await AuthHelper.RegisterAndLoginAsync(_client, userEmail, userName, password);

        var createProjectRequest = new CreateProjectRequestDto
        {
            Name = $"Flow1 Project {Guid.NewGuid()}",
            Description = "Test project for lifecycle flow"
        };
        var createProjectResponse = await _client.PostAsJsonAsync("/api/projects", createProjectRequest);
        Assert.Equal(HttpStatusCode.Created, createProjectResponse.StatusCode);
        var project = await createProjectResponse.Content.ReadFromJsonAsync<ProjectResponseDto>();
        Assert.NotNull(project);
        var projectId = project.Id;
        Assert.Equal(createProjectRequest.Name, project.Name);

        var createTaskRequest = new CreateTaskRequestDto
        {
            Title = "Implement feature X",
            Description = "Full implementation of feature X",
            Status = TaskStatus.Todo,
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(7)
        };
        var createTaskResponse = await _client.PostAsJsonAsync($"/api/projects/{projectId}/tasks", createTaskRequest);
        Assert.Equal(HttpStatusCode.Created, createTaskResponse.StatusCode);
        var task = await createTaskResponse.Content.ReadFromJsonAsync<TaskResponseDto>();
        Assert.NotNull(task);
        var taskId = task.Id;
        Assert.Equal(createTaskRequest.Title, task.Title);
        Assert.Equal(TaskStatus.Todo, task.Status);

        var updateTaskRequest = new UpdateTaskRequestDto
        {
            Title = createTaskRequest.Title,
            Description = createTaskRequest.Description,
            Status = TaskStatus.Done,
            Priority = createTaskRequest.Priority ?? TaskPriority.Medium,
            DueDate = createTaskRequest.DueDate
        };
        var updateTaskResponse = await _client.PutAsJsonAsync($"/api/tasks/{taskId}", updateTaskRequest);
        Assert.Equal(HttpStatusCode.OK, updateTaskResponse.StatusCode);
        var updatedTask = await updateTaskResponse.Content.ReadFromJsonAsync<TaskResponseDto>();
        Assert.NotNull(updatedTask);
        Assert.Equal(TaskStatus.Done, updatedTask.Status);

        var deleteProjectResponse = await _client.DeleteAsync($"/api/projects/{projectId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteProjectResponse.StatusCode);

        var getProjectResponse = await _client.GetAsync($"/api/projects/{projectId}");
        Assert.Equal(HttpStatusCode.NotFound, getProjectResponse.StatusCode);

        var getProjectsResponse = await _client.GetFromJsonAsync<PagedResponseWrapper<ProjectResponseDto>>("/api/projects");
        Assert.NotNull(getProjectsResponse);
        Assert.DoesNotContain(getProjectsResponse.Items, p => p.Id == projectId);

        var getTasksResponse = await _client.GetFromJsonAsync<PagedResponseWrapper<TaskResponseDto>>("/api/tasks");
        Assert.NotNull(getTasksResponse);
        Assert.DoesNotContain(getTasksResponse.Items, t => t.Id == taskId);

        using (var db = _factory.GetDbContext())
        {
            var dbProject = await db.Projects
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == projectId);
            Assert.NotNull(dbProject);
            Assert.NotNull(dbProject.DeletedAt);

            var dbTask = await db.Tasks
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Id == taskId);
            Assert.NotNull(dbTask);
            Assert.NotNull(dbTask.DeletedAt);
        }
    }
}

