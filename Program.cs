using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// Two JWT schemes are registered as an example. Endpoints stay public unless
// they are decorated with [Authorize(AuthenticationSchemes = "Auth0App1")].
// No default scheme on purpose: otherwise every request would be authenticated against it.
// Settings come from appsettings or environment variables (Auth0App1__Issuer, Auth0App1__Audience, ...).
builder.Services.AddAuthentication()
    .AddJwtBearer("Auth0App1", options => ConfigureJwt(options, configuration.GetSection("Auth0App1")))
    .AddJwtBearer("Auth0App2", options => ConfigureJwt(options, configuration.GetSection("Auth0App2")));

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();

static void ConfigureJwt(JwtBearerOptions options, IConfigurationSection section)
{
    options.Audience = section["Audience"];
    options.Authority = section["Issuer"];
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = section["Issuer"]
    };
}
