using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using SmartX.Shared.Models;

namespace SmartX.Client.Services;

// Keeps HTTP calls out of the page so the user interface stays easy to read.
public class SensorApiClient(HttpClient httpClient)
{
    // The API sends enum values such as "Environmental" as text.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<List<SensorProfile>> GetSensorsAsync()
    {
        return await httpClient.GetFromJsonAsync<List<SensorProfile>>("api/sensors/", JsonOptions) ?? [];
    }

    public Task<HttpResponseMessage> RegisterSensorAsync(SensorProfile sensor)
    {
        return httpClient.PostAsJsonAsync("api/sensors/", sensor, JsonOptions);
    }

    public async Task<TelemetryHistory> GetTelemetryHistoryAsync(string deviceId)
    {
        return await httpClient.GetFromJsonAsync<TelemetryHistory>(
            $"api/telemetry/{Uri.EscapeDataString(deviceId)}", JsonOptions) ?? new TelemetryHistory();
    }

    public async Task<List<TelemetryAlert>> GetAlertsAsync()
    {
        return await httpClient.GetFromJsonAsync<List<TelemetryAlert>>("api/alerts", JsonOptions) ?? [];
    }

    public async Task<HttpResponseMessage> UploadAttachmentAsync(
        string deviceId,
        string fileName,
        string contentType,
        Stream fileStream)
    {
        using var form = new MultipartFormDataContent();
        using var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(contentType)
            ? "application/octet-stream"
            : contentType);
        form.Add(fileContent, "file", fileName);

        return await httpClient.PostAsync(
            $"api/sensors/{Uri.EscapeDataString(deviceId)}/attachments",
            form);
    }

    public async Task<List<SensorAttachment>> GetAttachmentsAsync(string deviceId)
    {
        return await httpClient.GetFromJsonAsync<List<SensorAttachment>>(
            $"api/sensors/{Uri.EscapeDataString(deviceId)}/attachments", JsonOptions) ?? [];
    }
}
