using System.Data;
using Db;
using Microsoft.Data.Sqlite;
using Repositories;
using Extensions;
using Middlewares;

var builder = WebApplication.CreateBuilder(args);


{
    var Services = builder.Services;
    Services.ConfigureLogging(builder.Configuration, builder.Environment.EnvironmentName);
    var connectionString =
        builder.Configuration.GetConnectionString("ReservationsDb")
        ?? "Data Source=reservations.db;Cache=Shared";

    Services.AddSingleton(_ => new SqliteConnection(connectionString));
    Services.AddSingleton<IDbConnection>(sp => sp.GetRequiredService<SqliteConnection>());
    Services.AddSingleton<GuestRepository>();
    Services.AddSingleton<RoomRepository>();
    Services.AddSingleton<ReservationRepository>();
    Services.AddMvc(opt =>
    {
        opt.EnableEndpointRouting = false;
    });
    Services.AddCors();
    Services.AddEndpointsApiExplorer();
    Services.AddSwaggerGen();
}

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

{
    try
    {
        Setup.EnsureDb(app.Services.CreateScope());
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
