using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMyFrontend",
        policy =>
        {
            policy.WithOrigins("https://localhost:3000", "https://crowded-exams.onrender.com")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithExposedHeaders("Authorization")
                .AllowCredentials();
        });
});
builder.Services.AddSingleton<IEmailSender, SendGridEmailSender>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"])),

            ValidateIssuer = true,
            ValidIssuer = config["Jwt:Issuer"],

            ValidateAudience = true,
            ValidAudience = config["Jwt:Audience"],

            ValidateLifetime = true,
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["accessToken"];
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
string GetPgConnString(IConfiguration cfg)
{
    var raw =
        cfg.GetConnectionString("DefaultConnection")
        ?? cfg["DATABASE_INTERNAL_URL"]
        ?? cfg["DATABASE_URL"];

    if (string.IsNullOrWhiteSpace(raw))
        throw new InvalidOperationException("Postgres connection string not set.");

    if (!(raw.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
          raw.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)))
        return raw; // already key=value

    var uri = new Uri(raw);
    var userInfo = uri.UserInfo.Split(':', 2);
    var user = Uri.UnescapeDataString(userInfo[0]);
    var pass = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
    var db = uri.AbsolutePath.TrimStart('/');

    var qs = uri.Query.TrimStart('?')
        .Split('&', StringSplitOptions.RemoveEmptyEntries)
        .Select(p => p.Split('=', 2))
        .ToDictionary(a => a[0], a => a.Length > 1 ? a[1] : "", StringComparer.OrdinalIgnoreCase);

    var b = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Username = user,
        Password = pass,
        Database = db
    };

    if (qs.TryGetValue("sslmode", out var ssl) && ssl.Equals("require", StringComparison.OrdinalIgnoreCase))
    {
        b.SslMode = SslMode.Require;
    }

    return b.ToString();
}

builder.Services.AddDbContext<CrowdedExamsDb>(options =>
{
    var conn = GetPgConnString(builder.Configuration);
    options.UseNpgsql(conn);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CrowdedExamsDb>();
    db.Database.Migrate(); // or: await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowMyFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
