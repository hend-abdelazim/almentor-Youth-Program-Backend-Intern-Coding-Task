using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using TaskManagement.Application.DTOs.Auth;

namespace TaskManagement.IntegrationTests.Helpers;

public static class AuthHelper
{
    public static async Task<AuthResponseDto> RegisterAndLoginAsync(HttpClient client, string email, string username, string password)
    {
        var registerRequest = new RegisterRequestDto
        {
            Username = username,
            Email = email,
            Password = password,
            ConfirmPassword = password
        };

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        var loginRequest = new LoginRequestDto
        {
            UsernameOrEmail = username,
            Password = password
        };

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrEmpty(auth.Token));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        return auth;
    }

    public static async Task<AuthResponseDto> LoginAsync(HttpClient client, string usernameOrEmail, string password)
    {
        var loginRequest = new LoginRequestDto
        {
            UsernameOrEmail = usernameOrEmail,
            Password = password
        };

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrEmpty(auth.Token));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        return auth;
    }

    public static void SetAuthToken(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public static void ClearAuthToken(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
    }
}
