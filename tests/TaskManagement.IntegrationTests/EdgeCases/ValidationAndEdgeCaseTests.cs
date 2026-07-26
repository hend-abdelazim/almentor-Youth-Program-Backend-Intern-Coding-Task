using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using TaskManagement.Application.DTOs.Projects;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Domain.Enums;
using TaskManagement.IntegrationTests.Helpers;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.IntegrationTests.EdgeCases;

public class ValidationAndEdgeCaseTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ValidationAndEdgeCaseTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DuplicateProjectName_ShouldReturn409Conflict()
    {
        var userEmail = $"dup_{Guid.NewGuid()}@test.com";
        var userName = $"dup_user_{Guid.NewGuid():N}";
        await AuthHelper.RegisterAndLoginAsync(_client, userEmail, userName, "TestPass123!");

        const string projectName = "Duplicate Name Test";
        var req1 = new CreateProjectRequestDto { Name = projectName };
        var resp1 = await _client.PostAsJsonAsync("/api/projects", req1);
        resp1.EnsureSuccessStatusCode();

        var req2 = new CreateProjectRequestDto { Name = projectName };
        var resp2 = await _client.PostAsJsonAsync("/api/projects", req2);
        Assert.Equal(HttpStatusCode.Conflict, resp2.StatusCode);

        var problem = await ReadProblemDetails(resp2);
        Assert.NotNull(problem);
        Assert.Equal(409, problem.Status);
    }

    [Fact]
    public async Task CreateTask_PastDueDate_ShouldReturnValidationError()
    {
        var userEmail = $"pastdue_{Guid.NewGuid()}@test.com";
        var userName = $"pastdue_user_{Guid.NewGuid():N}";
        await AuthHelper.RegisterAndLoginAsync(_client, userEmail, userName, "TestPass123!");

        var createProjectResponse = await _client.PostAsJsonAsync("/api/projects",
            new CreateProjectRequestDto { Name = $"Past Due Project {Guid.NewGuid()}" });
        var project = await createProjectResponse.Content.ReadFromJsonAsync<ProjectResponseDto>();
        Assert.NotNull(project);

        var taskRequest = new CreateTaskRequestDto
        {
            Title = "Past Due Task",
            DueDate = DateTime.UtcNow.AddDays(-5)
        };

        var response = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/tasks", taskRequest);
        Assert.True(response.StatusCode == HttpStatusCode.UnprocessableEntity ||
                    response.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task InvalidProjectId_ShouldReturn404()
    {
        var userEmail = $"invproj_{Guid.NewGuid()}@test.com";
        var userName = $"invproj_user_{Guid.NewGuid():N}";
        await AuthHelper.RegisterAndLoginAsync(_client, userEmail, userName, "TestPass123!");

        var fakeId = Guid.NewGuid();

        var getResponse = await _client.GetAsync($"/api/projects/{fakeId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

        var deleteResponse = await _client.DeleteAsync($"/api/projects/{fakeId}");
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);

        var updateResponse = await _client.PutAsJsonAsync($"/api/projects/{fakeId}",
            new UpdateProjectRequestDto { Name = "Test" });
        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);
    }

    [Fact]
    public async Task InvalidTaskId_ShouldReturn404()
    {
        var userEmail = $"invtask_{Guid.NewGuid()}@test.com";
        var userName = $"invtask_user_{Guid.NewGuid():N}";
        await AuthHelper.RegisterAndLoginAsync(_client, userEmail, userName, "TestPass123!");

        var fakeId = Guid.NewGuid();

        var getResponse = await _client.GetAsync($"/api/tasks/{fakeId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

        var deleteResponse = await _client.DeleteAsync($"/api/tasks/{fakeId}");
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);

        var updateResponse = await _client.PutAsJsonAsync($"/api/tasks/{fakeId}",
            new UpdateTaskRequestDto
            {
                Title = "Test",
                Status = TaskStatus.Todo,
                Priority = TaskPriority.Medium
            });
        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);
    }

    [Fact]
    public async Task UnauthorizedAccess_ToProjectAndTaskEndpoints_ShouldReturn401()
    {
        AuthHelper.ClearAuthToken(_client);

        var projectResponses = new[]
        {
            await _client.GetAsync("/api/projects"),
            await _client.PostAsJsonAsync("/api/projects", new CreateProjectRequestDto { Name = "Unauth" }),
            await _client.GetAsync($"/api/projects/{Guid.NewGuid()}"),
            await _client.PutAsJsonAsync($"/api/projects/{Guid.NewGuid()}", new UpdateProjectRequestDto { Name = "x" }),
            await _client.DeleteAsync($"/api/projects/{Guid.NewGuid()}"),
        };

        var taskResponses = new[]
        {
            await _client.GetAsync("/api/tasks"),
            await _client.PostAsJsonAsync($"/api/projects/{Guid.NewGuid()}/tasks", new CreateTaskRequestDto { Title = "x" }),
            await _client.GetAsync($"/api/projects/{Guid.NewGuid()}/tasks"),
            await _client.GetAsync($"/api/tasks/{Guid.NewGuid()}"),
            await _client.PutAsJsonAsync($"/api/tasks/{Guid.NewGuid()}", new UpdateTaskRequestDto { Title = "x", Status = TaskStatus.Todo, Priority = TaskPriority.Medium }),
            await _client.DeleteAsync($"/api/tasks/{Guid.NewGuid()}"),
        };

        Assert.All(projectResponses, r => Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode));
        Assert.All(taskResponses, r => Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode));
    }

    [Fact]
    public async Task AccessAnotherUsersProject_ShouldReturn403()
    {
        var user1Email = $"user1_{Guid.NewGuid()}@test.com";
        var user1Name = $"user1_{Guid.NewGuid():N}";
        var user1Auth = await AuthHelper.RegisterAndLoginAsync(_client, user1Email, user1Name, "TestPass123!");

        var createProjectResp = await _client.PostAsJsonAsync("/api/projects",
            new CreateProjectRequestDto { Name = $"User1's Private Project {Guid.NewGuid()}" });
        var user1Project = await createProjectResp.Content.ReadFromJsonAsync<ProjectResponseDto>();
        Assert.NotNull(user1Project);

        var createTaskResp = await _client.PostAsJsonAsync($"/api/projects/{user1Project.Id}/tasks",
            new CreateTaskRequestDto { Title = "User1's task" });
        var user1Task = await createTaskResp.Content.ReadFromJsonAsync<TaskResponseDto>();
        Assert.NotNull(user1Task);

        var user2Email = $"user2_{Guid.NewGuid()}@test.com";
        var user2Name = $"user2_{Guid.NewGuid():N}";
        _ = await AuthHelper.RegisterAndLoginAsync(_client, user2Email, user2Name, "TestPass123!");

        var getProjectResp = await _client.GetAsync($"/api/projects/{user1Project.Id}");
        Assert.Equal(HttpStatusCode.Forbidden, getProjectResp.StatusCode);

        var getProjectTasksResp = await _client.GetAsync($"/api/projects/{user1Project.Id}/tasks");
        Assert.Equal(HttpStatusCode.Forbidden, getProjectTasksResp.StatusCode);

        var createTaskForAnotherProjectResp = await _client.PostAsJsonAsync(
            $"/api/projects/{user1Project.Id}/tasks",
            new CreateTaskRequestDto { Title = "Hacker task" });
        Assert.Equal(HttpStatusCode.Forbidden, createTaskForAnotherProjectResp.StatusCode);

        var getTaskResp = await _client.GetAsync($"/api/tasks/{user1Task.Id}");
        Assert.Equal(HttpStatusCode.Forbidden, getTaskResp.StatusCode);

        var updateTaskResp = await _client.PutAsJsonAsync($"/api/tasks/{user1Task.Id}",
            new UpdateTaskRequestDto
            {
                Title = "Hacked",
                Status = TaskStatus.Done,
                Priority = TaskPriority.High
            });
        Assert.Equal(HttpStatusCode.Forbidden, updateTaskResp.StatusCode);

        var deleteTaskResp = await _client.DeleteAsync($"/api/tasks/{user1Task.Id}");
        Assert.Equal(HttpStatusCode.Forbidden, deleteTaskResp.StatusCode);

        var updateProjectResp = await _client.PutAsJsonAsync($"/api/projects/{user1Project.Id}",
            new UpdateProjectRequestDto { Name = "Hacked" });
        Assert.Equal(HttpStatusCode.Forbidden, updateProjectResp.StatusCode);

        var deleteProjectResp = await _client.DeleteAsync($"/api/projects/{user1Project.Id}");
        Assert.Equal(HttpStatusCode.Forbidden, deleteProjectResp.StatusCode);

        var listProjects = await _client.GetFromJsonAsync<PagedResponseWrapper<ProjectResponseDto>>("/api/projects");
        Assert.NotNull(listProjects);
        Assert.DoesNotContain(listProjects.Items, p => p.Id == user1Project.Id);

        var listTasks = await _client.GetFromJsonAsync<PagedResponseWrapper<TaskResponseDto>>("/api/tasks");
        Assert.NotNull(listTasks);
        Assert.DoesNotContain(listTasks.Items, t => t.Id == user1Task.Id);
    }

    [Fact]
    public async Task Register_InvalidInput_ShouldReturnValidationErrors()
    {
        var shortPasswordRequest = new RegisterRequestDto
        {
            Username = "test",
            Email = "test@test.com",
            Password = "123",
            ConfirmPassword = "123"
        };
        var response = await _client.PostAsJsonAsync("/api/auth/register", shortPasswordRequest);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var mismatchedPasswordRequest = new RegisterRequestDto
        {
            Username = "test",
            Email = "test@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!!"
        };
        response = await _client.PostAsJsonAsync("/api/auth/register", mismatchedPasswordRequest);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task DuplicateUsername_Register_ShouldReturn409()
    {
        var userEmail = $"dupuser_{Guid.NewGuid()}@test.com";
        var userName = $"dupuser_{Guid.NewGuid():N}";

        await AuthHelper.RegisterAndLoginAsync(_client, userEmail, userName, "TestPass123!");
        AuthHelper.ClearAuthToken(_client);

        var dupRequest = new RegisterRequestDto
        {
            Username = userName,
            Email = $"different_{Guid.NewGuid()}@test.com",
            Password = "TestPass123!",
            ConfirmPassword = "TestPass123!"
        };
        var response = await _client.PostAsJsonAsync("/api/auth/register", dupRequest);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task WrongCredentials_Login_ShouldReturn401()
    {
        var loginRequest = new LoginRequestDto
        {
            UsernameOrEmail = "nonexistentuser",
            Password = "wrongpassword"
        };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAllTasks_ShouldIncludeProjectName()
    {
        var userEmail = $"incproj_{Guid.NewGuid()}@test.com";
        var userName = $"incproj_user_{Guid.NewGuid():N}";
        await AuthHelper.RegisterAndLoginAsync(_client, userEmail, userName, "TestPass123!");

        const string projectName = "My Awesome Project";
        var createProjectResponse = await _client.PostAsJsonAsync("/api/projects",
            new CreateProjectRequestDto { Name = projectName });
        var project = await createProjectResponse.Content.ReadFromJsonAsync<ProjectResponseDto>();
        Assert.NotNull(project);

        await _client.PostAsJsonAsync($"/api/projects/{project.Id}/tasks",
            new CreateTaskRequestDto { Title = "Test Task" });

        var tasks = await _client.GetFromJsonAsync<PagedResponseWrapper<TaskResponseDto>>("/api/tasks");
        Assert.NotNull(tasks);
        Assert.NotEmpty(tasks.Items);
        var task = tasks.Items.First(t => t.Title == "Test Task");
        Assert.Equal(projectName, task.ProjectName);
    }

    [Fact]
    public async Task InvalidPriorityEnum_ShouldReturn422()
    {
        var userEmail = $"invprio_{Guid.NewGuid()}@test.com";
        var userName = $"invprio_user_{Guid.NewGuid():N}";
        await AuthHelper.RegisterAndLoginAsync(_client, userEmail, userName, "TestPass123!");

        var jsonPayload = "{\"title\":\"Test\",\"status\":\"invalid_priority_value\",\"priority\":\"invalid_priority\"}";
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var createProjectResponse = await _client.PostAsJsonAsync("/api/projects",
            new CreateProjectRequestDto { Name = $"Priority Test {Guid.NewGuid()}" });
        var project = await createProjectResponse.Content.ReadFromJsonAsync<ProjectResponseDto>();
        Assert.NotNull(project);

        var response = await _client.PostAsync($"/api/projects/{project.Id}/tasks", content);
        var statusCode = (int)response.StatusCode;
        Assert.True(statusCode == 400 || statusCode == 422,
            $"Expected 400 or 422, got {statusCode}");
    }

    private static async Task<ProblemDetailsResponse?> ReadProblemDetails(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        try
        {
            return JsonSerializer.Deserialize<ProblemDetailsResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            return null;
        }
    }

    private class ProblemDetailsResponse
    {
        public int Status { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
    }
}
