using ChatAppAPI.Data;
using ChatAppAPI.Models;
using ChatAppAPI.Repository;
using ChatAppAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

// Add AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
// Register repositories
builder.Services.AddScoped<IUserReporitory, UserRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IChatUserRepository, ChatUserRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped(typeof(IChatApiRepository<>), typeof(ChatApiRepository<>));
// Adding JwtService
builder.Services.AddSingleton<JwtService>();

// JWT settings
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

// Swagger settings
builder.Services.AddSwaggerGen(c =>
{
    c.TagActionsBy(api =>
    {
        return new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] };
    });

    c.DocInclusionPredicate((_, _) => true);
});

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

app.UseAuthentication();
app.UseAuthorization();

// 5️⃣ Test endpoint
app.MapGet("/", () => "API is running on Render 🚀");

app.MapControllers();

app.Run();
