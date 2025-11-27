using ChatAppAPI.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Load User Secrets in development
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Get connection string BEFORE build
var connectionString = builder.Configuration.GetConnectionString("BaseConnection");
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(connectionString)
);

var app = builder.Build();

// Use dynamic port from Render
var port = Environment.GetEnvironmentVariable("PORT");
if (!app.Environment.IsDevelopment() && port != null)
{
    app.Urls.Add($"http://*:{port}");
}

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

// Root test endpoint
app.MapGet("/", () => "API is running on Render 🚀");

// Controllers
app.MapControllers();

app.Run();