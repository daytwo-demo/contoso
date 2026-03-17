using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

// OpenShift S2I requirement: listen on port 8080
builder.WebHost.UseUrls("http://*:8080");

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.UseCors();
app.UseStaticFiles();

// --- In-memory data store ---
var devices = new ConcurrentDictionary<int, Device>();
var nextId = 1;

// Seed data
var seedDevices = new[]
{
    new Device(nextId++, "Surface Pro 9", "Asignado"),
    new Device(nextId++, "ThinkPad X1 Carbon", "Disponible"),
    new Device(nextId++, "MacBook Pro M3", "Asignado"),
    new Device(nextId++, "Dell XPS 15", "Disponible"),
    new Device(nextId++, "HP EliteBook 840", "Asignado"),
};

foreach (var d in seedDevices)
    devices[d.Id] = d;

// --- API Endpoints ---

// GET /api/info - Pod name for load-balancing "wow effect"
app.MapGet("/api/info", () => Results.Ok(new
{
    podName = Environment.MachineName,
    timestamp = DateTimeOffset.UtcNow
}));

// GET /api/devices - List all devices
app.MapGet("/api/devices", () =>
    Results.Ok(devices.Values.OrderBy(d => d.Id)));

// POST /api/devices - Add a new device
app.MapPost("/api/devices", (DeviceRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Name))
        return Results.BadRequest(new { error = "El nombre del equipo es requerido." });

    var estado = string.IsNullOrWhiteSpace(req.Estado) ? "Disponible" : req.Estado;
    var id = Interlocked.Increment(ref nextId);
    var device = new Device(id, req.Name.Trim(), estado.Trim());
    devices[id] = device;

    return Results.Created($"/api/devices/{id}", device);
});

// DELETE /api/devices/{id} - Remove a device
app.MapDelete("/api/devices/{id:int}", (int id) =>
{
    if (devices.TryRemove(id, out _))
        return Results.NoContent();

    return Results.NotFound(new { error = $"Equipo con ID {id} no encontrado." });
});

// Fallback: serve index.html for any non-API route (SPA support)
app.MapFallbackToFile("index.html");

app.Run();

// --- Models ---
record Device(int Id, string Name, string Estado);
record DeviceRequest(string Name, string? Estado);
