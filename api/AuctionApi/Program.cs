using System.Globalization;
using AuctionApi;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173" };

builder.Services
    .AddPersistence(builder.Configuration)
    .AddDomain()
    .AddPresentation()
    .AddAuth(builder.Configuration)
    .AddSwagger()
    .AddCors(allowedOrigins);

var app = builder.Build();

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(CultureInfo.InvariantCulture),
    SupportedCultures = new[] { CultureInfo.InvariantCulture },
    SupportedUICultures = new[] { CultureInfo.InvariantCulture },
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Serve the React build (when copied into wwwroot/) and fall back to
// index.html so the SPA's client-side router keeps working on deep links.
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("AllowClient");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("index.html");
app.Run();
