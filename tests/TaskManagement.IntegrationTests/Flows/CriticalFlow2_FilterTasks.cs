using System.Net;
using System.Net.Http.Json;
using TaskManagement.Application.DTOs.Projects;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Domain.Enums;
using TaskManagement.IntegrationTests.Helpers;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.IntegrationTests.Flows;

public class CriticalFlow2_FilterTasks : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CriticalFlow2_FilterTasks(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task FilterTasks_ByStatusAndPriority_ShouldReturnCorrectResults()
    {
        var userEmail = $"flow2_{Guid.NewGuid()}@test.com";
        var userName = $"flow2_user_{Guid.NewGuid():N}";
        const string password = "TestPass123!";

        await AuthHelper.RegisterAndLoginAsync(_client, userEmail, userName, password);

        var createProjectRequest = new CreateProjectRequestDto { Name = $"Filter Test Project {Guid.NewGuid()}" };
        var createProjectResponse = await _client.PostAsJsonAsync("/api/projects", createProjectRequest);
        var project = await createProjectResponse.Content.ReadFromJsonAsync<ProjectResponseDto>();
        Assert.NotNull(project);
        var projectId = project.Id;

        var tasksToCreate = new[]
        {
            new { Title = "Todo High", Status = TaskStatus.Todo, Priority = TaskPriority.High },
            new { Title = "Todo Medium", Status = TaskStatus.Todo, Priority = TaskPriority.Medium },
            new { Title = "Todo Low", Status = TaskStatus.Todo, Priority = TaskPriority.Low },
            new { Title = "InProgress High", Status = TaskStatus.InProgress, Priority = TaskPriority.High },
            new { Title = "InProgress Medium", Status = TaskStatus.InProgress, Priority = TaskPriority.Medium },
            new { Title = "Done High", Status = TaskStatus.Done, Priority = TaskPriority.High },
            new { Title = "Done Medium", Status = TaskStatus.Done, Priority = TaskPriority.Medium },
            new { Title = "Done Low", Status = TaskStatus.Done, Priority = TaskPriority.Low },
        };

        foreach (var t in tasksToCreate)
        {
            var createTaskRequest = new CreateTaskRequestDto
            {
                Title = t.Title,
                Status = t.Status,
                Priority = t.Priority
            };
            var resp = await _client.PostAsJsonAsync($"/api/projects/{projectId}/tasks", createTaskRequest);
            resp.EnsureSuccessStatusCode();
        }

        var allTasks = await _client.GetFromJsonAsync<PagedResponseWrapper<TaskResponseDto>>("/api/tasks?limit=50");
        Assert.NotNull(allTasks);
        Assert.Equal(tasksToCreate.Length, allTasks.TotalCount);

        var todoTasks = await _client.GetFromJsonAsync<PagedResponseWrapper<TaskResponseDto>>($"/api/tasks?status=Todo&limit=50");
        Assert.NotNull(todoTasks);
        Assert.Equal(3, todoTasks.TotalCount);
        Assert.All(todoTasks.Items, t => Assert.Equal(TaskStatus.Todo, t.Status));

        var doneTasks = await _client.GetFromJsonAsync<PagedResponseWrapper<TaskResponseDto>>($"/api/tasks?status=Done&limit=50");
        Assert.NotNull(doneTasks);
        Assert.Equal(3, doneTasks.TotalCount);
        Assert.All(doneTasks.Items, t => Assert.Equal(TaskStatus.Done, t.Status));

        var highPriorityTasks = await _client.GetFromJsonAsync<PagedResponseWrapper<TaskResponseDto>>($"/api/tasks?priority=High&limit=50");
        Assert.NotNull(highPriorityTasks);
        Assert.Equal(3, highPriorityTasks.TotalCount);
        Assert.All(highPriorityTasks.Items, t => Assert.Equal(TaskPriority.High, t.Priority));

        var lowPriorityTasks = await _client.GetFromJsonAsync<PagedResponseWrapper<TaskResponseDto>>($"/api/tasks?priority=Low&limit=50");
        Assert.NotNull(lowPriorityTasks);
        Assert.Equal(2, lowPriorityTasks.TotalCount);
        Assert.All(lowPriorityTasks.Items, t => Assert.Equal(TaskPriority.Low, t.Priority));

        var inProgressHighTasks = await _client.GetFromJsonAsync<PagedResponseWrapper<TaskResponseDto>>($"/api/tasks?status=InProgress&priority=High&limit=50");
        Assert.NotNull(inProgressHighTasks);
        Assert.Equal(1, inProgressHighTasks.TotalCount);
        var combinedTask = Assert.Single(inProgressHighTasks.Items);
        Assert.Equal(TaskStatus.InProgress, combinedTask.Status);
        Assert.Equal(TaskPriority.High, combinedTask.Priority);
    }
}
