
using API.DTOs;
using System.Text;
using System.Text.Json;

namespace Tests.TestHelpers;

public static  class IntegrationTestsHelpers
{
    public static async Task<T> DeserializeResponse<T>(HttpResponseMessage httpResponse)
    {
        httpResponse.EnsureSuccessStatusCode();
        var stringResponse = await httpResponse.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        return JsonSerializer.Deserialize<T>(stringResponse, options);
    }

    public static async Task<string> GetToken(string login, string password, CustomWebApplicationFactory<Program> factory, HttpClient client)
    {
        var RequestUri = "api/account";

        var loginDto = new LoginDto
        {
            Username = login,
            Password = password,
        };
        var content = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");

        var httpResponse = await client.PostAsync(RequestUri + "/login", content);

        var actual = await DeserializeResponse<UserDto>(httpResponse);

        return actual.Token;
    }
}
