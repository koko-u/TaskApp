using System;
using System.Threading.Tasks;
using MicroElements.AspNetCore.OpenApi.FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;
using Serilog;
using TaskApp.Application;
using TaskApp.Infrastructure;

// Bootstrap Logger
Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);
    // Configure Serilog
    builder.Host.UseSerilog(
        (context, provider, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(provider)
                .Enrich.FromLogContext();
        }
    );
    // Configure DI Validation
    builder.Host.UseDefaultServiceProvider(opts =>
    {
        opts.ValidateScopes = true;
        opts.ValidateOnBuild = true;
    });

    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddProblemDetails();
    builder.Services.AddFluentValidationRulesToOpenApi();
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddInfrastructure(builder.Configuration);

    // configuring OpenAPI
    builder.Services.AddOpenApi(opts =>
    {
        opts.AddOperationTransformer(
            (operation, _, _) =>
            {
                operation.Summary = null;
                operation.Description = null;
                return Task.CompletedTask;
            }
        );
        opts.AddFluentValidationRules();
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(opts =>
        {
            opts.EnableDarkMode()
                .WithTitle("Tasks Api Reference")
                .WithTheme(ScalarTheme.BluePlanet)
                .ShowOperationId()
                .WithDefaultHttpClient(ScalarTarget.Shell, ScalarClient.Curl)
                .WithDocumentDownloadType(DocumentDownloadType.Json)
                .WithJsonDocumentDownload()
                .PreserveSchemaPropertyOrder();
        });
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler();
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception e)
{
    Log.Fatal(e, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
