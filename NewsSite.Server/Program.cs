using NewsSite.Server.Data;
using NewsSite.Server.Interfaces;
using Microsoft.AspNetCore.RateLimiting;
using FluentValidation;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using NewsSite.Server.Middleware;
using NewsSite.Server.Configuration;
using NewsSite.Server.Models;
using NewsSite.Server.HealthChecks;
using NewsSite.Server.Services.Caching;
using NewsSite.Server.Services.Pipelines;
using NewsSite.Server.Services.OpenAi;
using NewsSite.Server.Services.SerpAi;
using System.Threading.RateLimiting;
using NewsSite.Server.Monitoring;
using NewsSite.Server.Services.Pipeline;
using NewsSite.Server.Services.Pipeline.Steps;
using NewsSite.Server.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using OpenAI.Extensions;
using NewsSite.Server.Services;
using NewsSite.Server.Services.News;
using NewsSite.Server.Models.Auth;
using Microsoft.Extensions.FileProviders;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "NewsSite API", 
        Version = "v1",
        Description = "API for managing news pipelines and article processing"
    });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<GoogleAuthSettings>(builder.Configuration.GetSection("GoogleAuth"));
builder.Services.Configure<RedditAuthSettings>(builder.Configuration.GetSection("RedditAuth"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = false,
        RequireExpirationTime = false,
        RequireSignedTokens = false
    };
});

builder.Services.AddScoped<IArticleRepository, MongoArticleRepository>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromSeconds(1)
            }));
});
builder.Services.AddScoped<IArticleCacheService, ArticleCacheService>();
builder.Services.AddHostedService<BackgroundPipelineService>();
builder.Services.AddSingleton<ITelemetryInitializer>(new CustomTelemetryInitializer());

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck<OpenAIHealthCheck>("OpenAI");

// Add metrics
builder.Services.AddSingleton<IMetricsService, MetricsService>();

// Add news analysis services

// Add configuration
builder.Services.Configure<OpenAISettings>(
    builder.Configuration.GetSection("OpenAI"));
builder.Services.Configure<TwitterSettings>(
    builder.Configuration.GetSection("Twitter"));
builder.Services.Configure<InstagramSettings>(
    builder.Configuration.GetSection("Instagram"));
builder.Services.Configure<YouTubeSettings>(
    builder.Configuration.GetSection("YouTube"));

// Register OpenAI service
builder.Services.AddOpenAIService(settings => 
{
    settings.ApiKey = builder.Configuration.GetSection("OpenAI:ApiKey").Value;
});

//builder.Services.AddHostedService<CleanupService>();

// Configure MongoDB
NewsSite.Server.Configuration.MongoDbConfiguration.Configure();

// Add services to the container
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

// Register MongoDB services
builder.Services.AddSingleton<IMongoDbConnectionService, MongoDbConnectionService>();
builder.Services.AddScoped<IArticleRepository, MongoArticleRepository>();

// Register MongoDB collections
builder.Services.AddScoped(sp =>
{
    var connectionService = sp.GetRequiredService<IMongoDbConnectionService>();
    var database = connectionService.GetDatabaseAsync().GetAwaiter().GetResult();
    return database.GetCollection<ProcessedArticle>("ProcessedArticles");
});

// Register pipeline services
builder.Services.AddScoped<IPipelineService, PipelineService>();
builder.Services.AddScoped<NewsSite.Server.Services.Pipeline.IPipelineExecutionService, NewsSite.Server.Services.Pipeline.PipelineExecutionService>();
builder.Services.AddScoped<IStepFactory, StepFactory>();

// Register news services
builder.Services.AddScoped<INewsCategoryService, NewsCategoryService>();

// Register pipeline steps
builder.Services.AddScoped<OpenAICompletionStep>();
builder.Services.AddScoped<SerpApiStep>();
builder.Services.AddScoped<DatabaseStep>();
builder.Services.AddScoped<TwitterStep>();
builder.Services.AddScoped<InstagramStep>();
builder.Services.AddScoped<YouTubeStep>();
builder.Services.AddScoped<NewsCategoryStep>();
builder.Services.AddScoped<IPipelineStep, DallEStep>();

// Register step dependencies
builder.Services.AddScoped<ISerpApiService, SerpApiService>();
builder.Services.Configure<SerpApiSettings>(
    builder.Configuration.GetSection("SerpApi"));

// Register database services
builder.Services.AddSingleton<IApplicationDbContext, ApplicationDbContext>();
builder.Services.AddScoped<IDatabaseOperations, DatabaseOperations>();

// Register auth services
builder.Services.AddScoped<IAuthService, AuthService>();

// Add CORS configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(origin => true) // Be careful with this in production!
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ITrendingService, TrendingService>();

// Register repositories and services
builder.Services.AddScoped<IUserRepository, MongoUserRepository>();

var app = builder.Build();

// Ensure wwwroot directories exist
var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
var imagesPath = Path.Combine(wwwrootPath, "images");
var dallePath = Path.Combine(imagesPath, "dalle");

Directory.CreateDirectory(wwwrootPath);
Directory.CreateDirectory(imagesPath);
Directory.CreateDirectory(dallePath);

app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(wwwrootPath),
    RequestPath = ""
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSwagger(c =>
{
    c.RouteTemplate = "api-docs/{documentname}/swagger.json";
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/api-docs/v1/swagger.json", "NewsSite API V1");
    c.RoutePrefix = "api-docs";
    // Enable basic auth for Swagger UI in production
    if (!app.Environment.IsDevelopment())
    {
        c.ConfigObject.AdditionalItems["persistAuthorization"] = true;
        c.ConfigObject.AdditionalItems["filter"] = true;
    }
});

app.UseHttpsRedirection();

// Add request logging middleware
app.UseMiddleware<RequestLoggingMiddleware>();

// Add performance monitoring middleware
app.UseMiddleware<PerformanceMonitoringMiddleware>();

// Add error handling middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

// Add CORS middleware
app.UseCors();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.UseRateLimiter();

// Configure health check endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
});

app.Run();
