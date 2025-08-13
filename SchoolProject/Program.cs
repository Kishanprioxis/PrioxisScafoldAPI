using System.Reflection;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Models.Models.ResponseModel;
using Models.Models.School;
using Models.Models.SpDbContext;
using Models.Models.ValidatorModel;
using SchoolProject.Helper;
using Service.Repository.Implementation;
using Service.Repository.Interface;
using Serilog;
namespace SchoolProject;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        // Get connection string from appsettings.json
        var connectionString = builder.Configuration.GetConnectionString("DBConnection");
        builder.Services.AddDbContext<SchoolDbContext>(options =>
            options.UseSqlServer(connectionString));
        // Add services to the container.
        builder.Services.AddScoped<IStudentRepository, StudentRepository>();
        //Automapper
        builder.Services.AddAutoMapper(typeof(StudentMapper));
    
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        //serilogger
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("log.txt",
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true)
            .CreateLogger();
        //Fluent Validation
        builder.Services.AddControllers()

            .AddFluentValidation(fv =>

            {

                fv.RegisterValidatorsFromAssembly(AppDomain.CurrentDomain.GetAssemblies()

                    .SingleOrDefault(assembly => assembly.GetName().Name == typeof(Program).Assembly.GetName().Name));

            });
        builder.Services.AddDbContext<SchoolDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
            {

            });
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            options.EnableSensitiveDataLogging(true);
        }, ServiceLifetime.Transient);

        builder.Services.AddDbContext<SchoolManagementSpContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
            {

                sqlOptions.EnableRetryOnFailure();

            });
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            options.EnableSensitiveDataLogging(true);
        }, ServiceLifetime.Transient);
        // Register the UserService
        UnitOfWorkServiceCollectionExtentions.AddUnitOfWork<SchoolDbContext>(builder.Services);


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        app.UseMiddleware<ExceptionMiddleware>();
        
        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}