# Smart-X Part 1

Smart-X is a simulated Internet of Things management system for distributed environments such as hydroponic farms, smart-grid installations, and automated utility trackers. This Part 1 application provides a telemetry ingestion API and a Blazor WebAssembly dashboard for registering sensors, inspecting telemetry, and responding to detected problems.

## Project details

- **Developer:** Thonipho Mavhungu
- **Student number:** ST10121100
- **Module assessment:** PROG7312 Smart-X Part 1

## Implemented features

- Smart-X startup gateway with the three required architectural pillars.
- ASP.NET Core Minimal API for sensor registration and telemetry ingestion.
- Sensor profiles containing device ID, MAC address, deployment location, and category.
- Generic `TelemetryPacket<T>` model for `float`, `int`, and `bool` telemetry values.
- Typed telemetry endpoints that preserve the original value type.
- Mock sensor and telemetry seeding using jagged arrays before data is transferred to `List<T>` collections.
- Sensor registration form connected to the API.
- Timestamped telemetry history with metric filtering and reading drill-down.
- Proactive alerts for low soil moisture, power spikes, and sensor disconnects.
- Alert investigation view with an actionable recommendation and linked sensor history.
- In-app help guide explaining how to use the dashboard.

## Technology stack

- .NET 10
- ASP.NET Core Minimal API
- Blazor WebAssembly
- C#
- Bootstrap and custom CSS

## Project structure

```text
src/
  SmartX.Api/       ASP.NET Core API and mock telemetry seeding
  SmartX.Client/    Blazor WebAssembly dashboard
  SmartX.Shared/    Shared models used by the API and dashboard
```

## Running the project

Open two PowerShell windows in the project folder.

Start the API first:

```powershell
dotnet run --project src\SmartX.Api --launch-profile http
```

Then start the dashboard:

```powershell
dotnet run --project src\SmartX.Client --launch-profile http
```

Open [http://localhost:5025/telemetry](http://localhost:5025/telemetry) in a browser.

To compile the complete solution:

```powershell
dotnet build SmartX.slnx
```

## How to use the dashboard

1. Open the **Telemetry** workspace from the Smart-X gateway.
2. Review the proactive alerts and select **Investigate** when an alert needs attention.
3. Register a sensor using its device ID, MAC address, deployment location, and category.
4. Select a sensor to review its telemetry history.
5. Use the metric filter to focus on a specific measurement.
6. Select a reading to see its exact timestamp and source device.

## AI assistance acknowledgement

This project was developed and reviewed by **Thonipho Mavhungu**. ChatGPT was used as an assistive tool during development, particularly for UI-layout guidance, code explanation, debugging support, and refining the project documentation. The final implementation was reviewed, tested, and organised by the student.
