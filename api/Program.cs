using System.Data;
using Dapper;
using Db;
using Microsoft.Data.Sqlite;
using Repositories;
using Extensions;
using FluentValidation;
using Middlewares;

SqlMapper.AddTypeHandler(new GuidTypeHandler());

var builder = WebApplication.CreateBuilder(args);


{
    var Services = builder.Services;
    Services.ConfigureLogging(builder.Configuration, builder.Environment.EnvironmentName);
    var connectionString =
        builder.Configuration.GetConnectionString("ReservationsDb")
        ?? "Data Source=reservations.db;Cache=Shared";

    Services.AddScoped(_ => new SqliteConnection(connectionString));
    Services.AddScoped<IDbConnection>(sp => sp.GetRequiredService<SqliteConnection>());
    Services.AddScoped<GuestRepository>();
    Services.AddScoped<RoomRepository>();
    Services.AddScoped<ReservationRepository>();
    Services.AddMvc(opt =>
    {
        opt.EnableEndpointRouting = false;
    });
    Services.AddValidatorsFromAssemblyContaining<Program>();
    Services.AddCors();
    Services.AddEndpointsApiExplorer();
    Services.AddSwaggerGen();
}

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

{
    try
    {
        var db = app.Services.GetRequiredService<SqliteConnection>();
        await Setup.EnsureDb(db);

        if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Testing")
        {
            await Seed.SeedData(db);
        }
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Failed to setup the database, aborting");
        Environment.Exit(1);
        return;
    }

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.UsePathBase("/api")
        .UseMvc()
        .UseCors(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader())
        .UseSwagger()
        .UseSwaggerUI();
}

app.Run();

public partial class Program { }
