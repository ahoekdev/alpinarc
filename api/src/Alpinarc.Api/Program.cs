using Alpinarc.Api.Data;
using Microsoft.EntityFrameworkCore;

const string WebCorsPolicy = "Web";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AlpinarcDbContext>(options => options
    .UseNpgsql(builder.Configuration.GetConnectionString("Alpinarc"))
    .UseSnakeCaseNamingConvention());

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy(WebCorsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors(WebCorsPolicy);
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
