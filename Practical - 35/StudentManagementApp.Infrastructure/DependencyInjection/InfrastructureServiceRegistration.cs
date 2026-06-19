using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StudentManagementApp.Application.DTOs;
using StudentManagementApp.Application.Interfaces;
using StudentManagementApp.Application.Mappings;
using StudentManagementApp.Application.Services;
using StudentManagementApp.Application.Validators;
using StudentManagementApp.Infrastructure.Logging;
using StudentManagementApp.Infrastructure.Repositories;

namespace StudentManagementApp.Infrastructure.DependencyInjection;

/// <summary>
/// Extension method that wires the entire dependency graph.
/// Program.cs calls this once; it never needs to know about concrete types.
/// </summary>
public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string logFilePath,
        string promptLogFilePath)
    {
        // Repository — singleton because the in-memory list must survive across service calls.
        services.AddSingleton<IStudentRepository, StudentRepository>();

        // Application service
        services.AddScoped<IStudentService, StudentService>();

        // AutoMapper — scans StudentMappingProfile in Application assembly.
        services.AddAutoMapper(typeof(StudentMappingProfile));

        // FluentValidation
        services.AddScoped<IValidator<StudentDto>, StudentValidator>();

        // Prompt logger — singleton so the file handle is shared.
        services.AddSingleton<IPromptLogger>(new PromptLogger(promptLogFilePath));

        // Logging: console + custom file sink.
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();
            builder.AddProvider(new FileLoggerProvider(logFilePath));
            builder.SetMinimumLevel(LogLevel.Information);
        });

        return services;
    }
}
