using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.HttpOverrides;
using MudBlazor.Services;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);
var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
logger.LogInformation("Environment: {Environment}", builder.Environment.EnvironmentName);
logger.LogInformation("Content Root: {ContentRoot}", builder.Environment.ContentRootPath);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMudServices();

if (builder.Environment.IsDevelopment())
{
    // Configures data protection to persist encryption keys to a local file system directory.
    // This is used for shared cookie authentication in local development environments.
    // The application name ensures keys are isolated for this specific app.
    // This setup is primarily for local testing and should be wrapped in an IsDevelopment check to avoid being used in production.
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(builder.Configuration["LocalOnlyConfig:SharedKeyPath"]))
        .SetApplicationName("BoardGameTrackerSSO");

    // Configures Kestrel to listen on localhost (127.0.0.1) at port 5001.
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Listen(System.Net.IPAddress.Parse("127.0.0.1"), 5001, listenOptions =>
        {
            listenOptions.UseHttps(
                builder.Configuration["LocalOnlyConfig:Certificates:Path"],
                builder.Configuration["LocalOnlyConfig:Certificates:Password"]);
        });
    });
}

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
})
.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    // LocalHost development needs custom domains set in local "Hosts" file 
    // in order to have the same shared-domain cookie functionality for auto authentication.
    // For example, add the following line to your Hosts file: 127.0.0.1 App1.localtest.me
    // If not, you will be requested to login each time for first time you access an app.
    options.Cookie.Name = "BoardGameTrackerCookie";
    options.Cookie.Domain = builder.Configuration["Cookie:Domain"];
    options.Cookie.Path = "/";
    // Use Lax for cross-site cookies
    options.Cookie.SameSite = SameSiteMode.Lax; 
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    // options.Cookie.SameSite = SameSiteMode.None; // Use None for cross-site cookies in production
})
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Authority = builder.Configuration["Cognito:Authority"];
    options.ClientId = builder.Configuration["Cognito:ClientId"];
    options.ClientSecret = builder.Configuration["Cognito:ClientSecret"];
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.CallbackPath = "/signin-oidc";
    // options.SignedOutCallbackPath = "/authentication/loggedout";
    options.RequireHttpsMetadata = true;
    // Save the access and ID tokens in the authentication session/cookie.
    options.SaveTokens = true; //TODO Not sure if still needed
    
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("email");
    options.Scope.Add("profile");
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("WarehouseUser", policy =>
    {
        policy.RequireAssertion(context =>
            context.User.HasClaim("cognito:groups", "Warehouse")
        );
    });

    options.AddPolicy("CustomerUser", policy =>
    {
        policy.RequireAssertion(context =>
            context.User.HasClaim("cognito:groups", "Customer")
        );
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Enables support for forwarded headers in reverse proxy scenarios to correctly
// determine the original client IP and protocol (HTTP/HTTPS).
// I am using AWS which is using Nginx for Reverse Proxy, and I believe
// Nginx is terminating https and forwarding requests to my app over http.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession(); 
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapStaticAssets();

app.Run();