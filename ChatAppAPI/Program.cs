using ChatAppAPI.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Load configuration

// Load user secrets in Development
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Environment variables are automatically loaded in ASP.NET Core
// Render environment variable example:
// ConnectionStrings__BaseConnection

// 2️⃣ Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Get connection string
var connectionString = builder.Configuration.GetConnectionString("BaseConnection");

// Optional: print connection string to verify it's loaded
Console.WriteLine("CONNECTION STRING: " + connectionString);

// Add DbContext
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(connectionString)
);

var app = builder.Build();

// 3️⃣ Configure app URLs for Render
var port = Environment.GetEnvironmentVariable("PORT");
if (!app.Environment.IsDevelopment() && port != null)
{
    app.Urls.Add($"http://*:{port}");
}

// 4️⃣ Middleware
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

// 5️⃣ Test endpoint
app.MapGet("/", () => "API is running on Render 🚀");

// 6️⃣ Controllers
app.MapControllers();

// 7️⃣ Run the app
app.Run();
