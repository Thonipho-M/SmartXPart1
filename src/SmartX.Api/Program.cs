using SmartX.Api.Data;
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

// Each list preserves its packet's original telemetry value type.
var floatTelemetry = new List<TelemetryPacket<float>>();
var integerTelemetry = new List<TelemetryPacket<int>>();
var booleanTelemetry = new List<TelemetryPacket<bool>>();

MockTelemetrySeeder.Seed(
    registeredSensors,
    floatTelemetry,
    integerTelemetry,
    booleanTelemetry);

var telemetry = app.MapGroup("/api/telemetry")
    .WithTags("Telemetry");

// The three POST routes deliberately accept different generic packet types.
telemetry.MapPost("/float", (TelemetryPacket<float> packet) =>
    StoreTelemetryPacket(packet, floatTelemetry))
    .WithName("AddFloatTelemetry");

telemetry.MapPost("/integer", (TelemetryPacket<int> packet) =>
    StoreTelemetryPacket(packet, integerTelemetry))
    .WithName("AddIntegerTelemetry");

telemetry.MapPost("/boolean", (TelemetryPacket<bool> packet) =>
    StoreTelemetryPacket(packet, booleanTelemetry))
    .WithName("AddBooleanTelemetry");

// GET /api/telemetry returns all packets, grouped by their value type.
telemetry.MapGet("/", () => Results.Ok(new TelemetryHistory
{
    FloatPackets = floatTelemetry,
    IntegerPackets = integerTelemetry,
    BooleanPackets = booleanTelemetry
}))
.WithName("GetAllTelemetry");

// GET /api/telemetry/{deviceId} returns the history for one sensor.
telemetry.MapGet("/{deviceId}", (string deviceId) =>
{
    var sensorExists = registeredSensors.Any(sensor =>
        sensor.DeviceId.Equals(deviceId, StringComparison.OrdinalIgnoreCase));

    if (!sensorExists)
    {
        return Results.NotFound(new { message = "The requested sensor is not registered." });
    }

    return Results.Ok(new TelemetryHistory
    {
        FloatPackets = floatTelemetry.Where(packet =>
            packet.DeviceId.Equals(deviceId, StringComparison.OrdinalIgnoreCase)).ToList(),
        IntegerPackets = integerTelemetry.Where(packet =>
            packet.DeviceId.Equals(deviceId, StringComparison.OrdinalIgnoreCase)).ToList(),
        BooleanPackets = booleanTelemetry.Where(packet =>
            packet.DeviceId.Equals(deviceId, StringComparison.OrdinalIgnoreCase)).ToList()
    });
})
.WithName("GetSensorTelemetry");

IResult StoreTelemetryPacket<T>(TelemetryPacket<T> packet, List<TelemetryPacket<T>> packetList)
{
    if (string.IsNullOrWhiteSpace(packet.DeviceId) || string.IsNullOrWhiteSpace(packet.MetricName))
    {
        return Results.BadRequest(new { message = "Device ID and metric name are required." });
    }

    var sensorExists = registeredSensors.Any(sensor =>
        sensor.DeviceId.Equals(packet.DeviceId, StringComparison.OrdinalIgnoreCase));

    if (!sensorExists)
    {
        return Results.NotFound(new { message = "Register the sensor before sending telemetry." });
    }

    packetList.Add(packet);
    return Results.Created($"/api/telemetry/{packet.DeviceId}", packet);
}

app.Run();
