using ChatAppAPI.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Load user secrets in Development
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// 2️⃣ Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Get connection string
var connectionString = builder.Configuration.GetConnectionString("BaseConnection");

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

app.MapControllers();

app.Run();
