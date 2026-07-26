using System.Net.Http.Json;
using TaskManagement.Application.DTOs.Projects;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Domain.Enums;
using TaskManagement.IntegrationTests.Helpers;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.IntegrationTests.Flows;

public class CriticalFlow3_SearchPagination : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CriticalFlow3_SearchPagination(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SearchTasks_CaseInsensitive_Pagination_TotalCount()
    {
        var userEmail = $"flow3_{Guid.NewGuid()}@test.com";
        var userName = $"flow3_user_{Guid.NewGuid():N}";
        const string password = "TestPass123!";

        await AuthHelper.RegisterAndLoginAsync(_client, userEmail, userName, password);

        var createProjectRequest = new CreateProjectRequestDto { Name = $"Search Test Project {Guid.NewGuid()}" };
        var createProjectResponse = await _client.PostAsJsonAsync("/api/projects", createProjectRequest);
        var project = await createProjectResponse.Content.ReadFromJsonAsync<ProjectResponseDto>();
        Assert.NotNull(project);
        var projectId = project.Id;

        var backendTasks = new[]
        {
            new { Title = "Backend API development", Desc = "Build REST backend endpoints" },
            new { Title = "Backend unit tests", Desc = "Test the backend code" },
            new { Title = "Fix Backend bug", Desc = "Login bug in backend" },
        };

        var frontendTasks = new[]
        {
            new { Title = "Frontend design", Desc = "Create UI mockups" },
            new { Title = "Frontend tests", Desc = "Write end-to-end tests" },
        };

        var otherTasks = new[]
        {
            new { Title = "Deploy to production", Desc = "Production deployment script" },
            new { Title = "Documentation", Desc = "Write API documentation" },
        };

        foreach (var t in backendTasks)
        {
            await _client.PostAsJsonAsync($"/api/projects/{projectId}/tasks",
                new CreateTaskRequestDto { Title = t.Title, Description = t.Desc });
        }
        foreach (var t in frontendTasks)
        {
            await _client.PostAsJsonAsync($"/api/projects/{projectId}/tasks",
                new CreateTaskRequestDto { Title = t.Title, Description = t.Desc });
        }
        foreach (var t in otherTasks)
        {
            await _client.PostAsJsonAsync($"/api/projects/{projectId}/tasks",
                new CreateTaskRequestDto { Title = t.Title, Description = t.Desc });
        }

        var searchUpper = await _client.GetFromJsonAsync<PagedResponseWrapper<TaskResponseDto>>("/api/tasks?q=BACKEND&limit=50");
        Assert.NotNull(searchUpper);
        Assert.Equal(3, searchUpper.TotalCount);

        var searchLower = await _client.GetFromJsonAsync<PagedResponseWrapper<TaskResponseDto>>("/api/tasks?q=backend&limit=50");
        Assert.NotNull(searchLower);
        Assert.Equal(3, searchLower.TotalCount);

        var searchMixed = await _client.GetFromJsonAsync<PagedResponseWrapper<TaskResponseDto>>("/api/tasks?q=BaCkEnD&limit=50");
        Assert.NotNull(searchMixed);
        Assert.Equal(3, searchMixed.TotalCount);

        var searchTitle = await _client.GetFromJsonAsync<PagedResponseWrapper<TaskResponseDto>>("/api/tasks?q=design&limit=50");
        Assert.NotNull(searchTitle);
        Assert.Equal(1, searchTitle.TotalCount);
        Assert.Equal("Frontend design", searchTitle.Items[0].Title);

        var searchDescription = await _client.GetFromJsonAsync<PagedResponseWrapper<TaskResponseDto>>("/api/tasks?q=endpoints&limit=50");
        Assert.NotNull(searchDescription);
        Assert.Equal(1, searchDescription.TotalCount);
        Assert.Contains("endpoints", searchDescription.Items[0].Description, StringComparison.OrdinalIgnoreCase);

        var page1 = await _client.GetFromJsonAsync<PagedResponseWrapper<TaskResponseDto>>("/api/tasks?page=1&limit=3");
        Assert.NotNull(page1);
        Assert.Equal(7, page1.TotalCount);
        Assert.Equal(3, page1.TotalPages);
        Assert.Equal(1, page1.Page);
        Assert.Equal(3, page1.Limit);
        Assert.Equal(3, page1.Items.Count);

        var page2 = await _client.GetFromJsonAsync<PagedResponseWrapper<TaskResponseDto>>("/api/tasks?page=2&limit=3");
        Assert.NotNull(page2);
        Assert.Equal(7, page2.TotalCount);
        Assert.Equal(3, page2.TotalPages);
        Assert.Equal(2, page2.Page);
        Assert.Equal(3, page2.Items.Count);

        var page3 = await _client.GetFromJsonAsync<PagedResponseWrapper<TaskResponseDto>>("/api/tasks?page=3&limit=3");
        Assert.NotNull(page3);
        Assert.Equal(7, page3.TotalCount);
        Assert.Equal(1, page3.Items.Count);

        var page4 = await _client.GetFromJsonAsync<PagedResponseWrapper<TaskResponseDto>>("/api/tasks?page=4&limit=3");
        Assert.NotNull(page4);
        Assert.Equal(7, page4.TotalCount);
        Assert.Empty(page4.Items);
    }

    [Fact]
    public async Task Sorting_ShouldWorkCorrectly()
    {
        var userEmail = $"flow3sort_{Guid.NewGuid()}@test.com";
        var userName = $"flow3sort_user_{Guid.NewGuid():N}";
        const string password = "TestPass123!";

        await AuthHelper.RegisterAndLoginAsync(_client, userEmail, userName, password);

        var createProjectRequest = new CreateProjectRequestDto { Name = $"Sort Test Project {Guid.NewGuid()}" };
        var createProjectResponse = await _client.PostAsJsonAsync("/api/projects", createProjectRequest);
        var project = await createProjectResponse.Content.ReadFromJsonAsync<ProjectResponseDto>();
        Assert.NotNull(project);
        var projectId = project.Id;

        var tasks = new[]
        {
            new { Title = "C Low", Priority = TaskPriority.Low, Due = DateTime.UtcNow.AddDays(5) },
            new { Title = "A High", Priority = TaskPriority.High, Due = DateTime.UtcNow.AddDays(1) },
            new { Title = "B Medium", Priority = TaskPriority.Medium, Due = DateTime.UtcNow.AddDays(3) },
        };

        foreach (var t in tasks)
        {
            await _client.PostAsJsonAsync($"/api/projects/{projectId}/tasks",
                new CreateTaskRequestDto { Title = t.Title, Priority = t.Priority, DueDate = t.Due });
        }

        var sortPriorityAsc = await _client.GetFromJsonAsync<PagedResponseWrapper<TaskResponseDto>>(
            "/api/tasks?sort_by=priority&sort_direction=asc&limit=50");
        Assert.NotNull(sortPriorityAsc);
        Assert.Equal(3, sortPriorityAsc.Items.Count);
        Assert.Equal(TaskPriority.Low, sortPriorityAsc.Items[0].Priority);
        Assert.Equal(TaskPriority.Medium, sortPriorityAsc.Items[1].Priority);
        Assert.Equal(TaskPriority.High, sortPriorityAsc.Items[2].Priority);

        var sortPriorityDesc = await _client.GetFromJsonAsync<PagedResponseWrapper<TaskResponseDto>>(
            "/api/tasks?sort_by=priority&sort_direction=desc&limit=50");
        Assert.NotNull(sortPriorityDesc);
        Assert.Equal(TaskPriority.High, sortPriorityDesc.Items[0].Priority);
        Assert.Equal(TaskPriority.Medium, sortPriorityDesc.Items[1].Priority);
        Assert.Equal(TaskPriority.Low, sortPriorityDesc.Items[2].Priority);

        var sortDueDateAsc = await _client.GetFromJsonAsync<PagedResponseWrapper<TaskResponseDto>>(
            "/api/tasks?sort_by=due_date&sort_direction=asc&limit=50");
        Assert.NotNull(sortDueDateAsc);
        Assert.Equal("A High", sortDueDateAsc.Items[0].Title);
        Assert.Equal("B Medium", sortDueDateAsc.Items[1].Title);
        Assert.Equal("C Low", sortDueDateAsc.Items[2].Title);
    }
}
