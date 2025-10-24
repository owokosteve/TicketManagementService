using Scalar.AspNetCore;
using ServicesUnitTest.Models;
using TicketManagerService.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "ServiceUnitTest API";
    config.Title = "ServiceUnitTest v1";
    config.Version = "v1";
});

// Register services
//builder.Services.AddSingleton<DemoOne>(provider => new DemoOne("Manual Testing"));
//builder.Services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<DemoOne>());

// Register logger as singleton (or scoped if needed)
// builder.Services.AddSingleton<ILogWriter>(new FileLogWriter(IntiationModels._log));

// 4. GROUPSMANAGER
// builder.Services.AddGroupManager(IntiationModels._psqlModel);
builder.Services.AddTicketManager(IntiationModels._psqlModel);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "ServiceUnitTestAPI";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}

app.UseAuthorization();

app.MapControllers();

app.MapRazorPages();

app.UseStaticFiles();

app.Run();
