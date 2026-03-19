using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using TestPlatform.Application;
using TestPlatform.Infrastructure.FileStorage;
using TestPlatform.Infrastructure.Postgres;
using TestPlatform.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "TestPlatform", Version = "v1" });

    options.EnableAnnotations();
    options.UseInlineDefinitionsForEnums();
});

builder.Services
    .AddTestPlatformPersistence(builder.Configuration)
    .AddTestPlatformApplication()
    .AddTestPlatformFileStorage(builder.Configuration);

builder.Services.AddProblemDetails();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5062);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TestPlatformDbContext>();
    db.Database.Migrate();

    await DbInitializer.InitializeAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseRouting();


app.UseCors(policy =>
{
    policy.AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .WithOrigins("https://localhost:5173");
});


app.UseAuthentication();
app.UseMiddleware<EnsureUserMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();