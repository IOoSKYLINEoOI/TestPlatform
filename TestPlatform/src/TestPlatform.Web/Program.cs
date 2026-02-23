using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using TestPlatform.Application;
using TestPlatform.Infrastructure.Postgres;

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
    .AddTestPlatformApplication();

builder.Services.AddProblemDetails();

/*builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5195); // только HTTP
});*/

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TestPlatformDbContext>();
    db.Database.Migrate();

    // await DbInitializer.InitializeAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseCors(policy =>
{
    policy.AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .WithOrigins("https://localhost:5173");
});


/*app.UseAuthentication();
app.UseAuthorization();*/

app.MapControllers();

app.Run();