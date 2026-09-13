using System.Net.Http.Json;
using SmartX.Shared.Models;

namespace SmartX.Client.Services;

// Keeps HTTP calls out of the page so the user interface stays easy to read.
public class SensorApiClient(HttpClient httpClient)
{
    public async Task<List<SensorProfile>> GetSensorsAsync()
    {
        return await httpClient.GetFromJsonAsync<List<SensorProfile>>("api/sensors/") ?? [];
    }

    public Task<HttpResponseMessage> RegisterSensorAsync(SensorProfile sensor)
    {
        return httpClient.PostAsJsonAsync("api/sensors/", sensor);
    }
}
