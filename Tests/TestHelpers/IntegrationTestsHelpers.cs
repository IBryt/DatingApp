
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
}
