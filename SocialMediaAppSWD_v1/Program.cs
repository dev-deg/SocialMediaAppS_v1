using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using SocialMediaAppSWD_v1.DataAccess;
using System.Security.Claims;
using SocialMediaAppSWD_v1.Interfaces;
using SocialMediaAppSWD_v1.Services;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", builder.Configuration["Authentication:Google:ServiceAccountCredentials"]);

// Create logger factory and logger for secret manager
builder.Configuration["GoogleCloud:ProjectId"] = "pftc-2025-s";
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().AddDebug());
var cloudLoggingService = new GoogleCloudLoggingService(
    builder.Configuration,
    loggerFactory.CreateLogger<GoogleCloudLoggingService>(),
    builder.Environment);

// Register cloud logging service first so other services can use it
builder.Services.AddSingleton<ICloudLoggingService>(cloudLoggingService);

// Initialize and load secrets using cloud logging
var projectId = "620707456996";
var secretManager = new GoogleSecretManagerService(projectId, cloudLoggingService);
await secretManager.LoadSecretsIntoConfigurationAsync(builder.Configuration);

// Make sure GoogleCloud:ProjectId is set for other services to use
builder.Configuration["GoogleCloud:ProjectId"] = projectId;
builder.Configuration["GoogleCloud:LogName"] = "social-media-app-log";

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    IConfigurationSection googleAuth = builder.Configuration.GetSection("Authentication:Google");
    options.ClientId = googleAuth["ClientId"];
    options.ClientSecret = googleAuth["ClientSecret"];
    options.Scope.Add("profile");
    options.Events.OnCreatingTicket = (context) =>
    {
        String email = context.User.GetProperty("email").GetString();
        String picture = context.User.GetProperty("picture").GetString();
        context.Identity.AddClaim(new Claim("email", email));
        context.Identity.AddClaim(new Claim("picture", picture));
        return Task.CompletedTask;
    };
});

//Add firestore to the DI container (Instantiate it when required)
builder.Services.AddScoped<FirestoreRepository>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();

// Register the secret manager service with DI
builder.Services.AddSingleton<ISecretManagerService>(sp => 
    new GoogleSecretManagerService(
        builder.Configuration["GoogleCloud:ProjectId"], 
        sp.GetRequiredService<ICloudLoggingService>()));

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Log application startup
var logger = app.Services.GetRequiredService<ICloudLoggingService>();
await logger.LogInformationAsync($"Application started in {app.Environment.EnvironmentName} environment");

app.Run();
