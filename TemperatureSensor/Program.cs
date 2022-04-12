using TemperatureSensor.WebUI.DAL.Repositories.Abstract;
using TemperatureSensor.WebUI.DAL.Repositories.Concrete;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Authentication;
using TemperatureSensor.WebUI.Authorization;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("TemperatureDb") ?? "Data Source=temperature.db;Cache=Shared";

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DI for application services
builder.Services.AddScoped<ITemperatureRepository, TemperatureRepository>();
builder.Services.AddScoped(_ => new SqliteConnection(connectionString));

// Add the whole configuration object here.
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Basic authentication
builder.Services.AddAuthentication("BasicAuthentication")
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>
                ("BasicAuthentication", null);
builder.Services.AddAuthorization();

var app = builder.Build();

// Create database
ServiceProvider? sp = builder.Services.BuildServiceProvider();
var temperatureRepository = sp.GetService<ITemperatureRepository>();
await temperatureRepository.CreateDb();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// global cors policy
app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
