using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Server.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=users.db"));

builder.Services.AddControllers();

// 添加CORS支持
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("your-super-secret-key-that-is-at-least-32-characters-long"))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream",
    OnPrepareResponse = ctx =>
    {
        if (ctx.File.Name.EndsWith(".wasm") || ctx.File.Name.EndsWith(".pdb") || ctx.File.Name.EndsWith(".dat"))
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "no-cache");
        }
    }
});

app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value;
    if (path != null)
    {
        if (path.EndsWith(".wasm"))
        {
            context.Response.ContentType = "application/wasm";
        }
        else if (path.EndsWith(".pdb"))
        {
            context.Response.ContentType = "application/octet-stream";
        }
        else if (path.EndsWith(".dat"))
        {
            context.Response.ContentType = "application/octet-stream";
        }
    }
    await next();
});

app.MapFallbackToFile("index.html");

app.Run();
