using SmartX.Shared.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddCors(options =>
    options.AddPolicy("BlazorClient", policy =>
        policy.WithOrigins("http://localhost:5025", "https://localhost:7169")
            .AllowAnyHeader()
            .AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Keep local testing simple; deployed versions should always redirect to HTTPS.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("BlazorClient");

// This list is temporary storage for Part 1. It is cleared when the API stops.
var registeredSensors = new List<SensorProfile>();

var sensors = app.MapGroup("/api/sensors")
    .WithTags("Sensors");

// GET /api/sensors returns every sensor registered during the current run.
sensors.MapGet("/", () => Results.Ok(registeredSensors))
    .WithName("GetSensors");

// POST /api/sensors adds one sensor registration record.
sensors.MapPost("/", (SensorProfile profile) =>
{
    if (string.IsNullOrWhiteSpace(profile.DeviceId) ||
        string.IsNullOrWhiteSpace(profile.MacAddress) ||
        string.IsNullOrWhiteSpace(profile.DeploymentLocation))
    {
        return Results.BadRequest(new
        {
            message = "Device ID, MAC address and deployment location are required."
        });
    }

    var sensorAlreadyExists = registeredSensors.Any(sensor =>
        sensor.DeviceId.Equals(profile.DeviceId, StringComparison.OrdinalIgnoreCase) ||
        sensor.MacAddress.Equals(profile.MacAddress, StringComparison.OrdinalIgnoreCase));

    if (sensorAlreadyExists)
    {
        return Results.Conflict(new
        {
            message = "A sensor with this device ID or MAC address is already registered."
        });
    }

    registeredSensors.Add(profile);

    return Results.Created($"/api/sensors/{profile.DeviceId}", profile);
})
.WithName("RegisterSensor");

app.Run();
