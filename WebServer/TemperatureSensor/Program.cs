using TemperatureSensor.WebUI.DAL.Repositories.Abstract;
using TemperatureSensor.WebUI.DAL.Repositories.Concrete;
using Microsoft.Data.Sqlite;
using TemperatureSensor.WebUI.Authorization;
using AspNetCore.Authentication.Basic;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("TemperatureDb") ?? "Data Source=temperature.db;Cache=Shared";

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DI for application services
builder.Services.AddScoped<ITemperatureRepository, TemperatureRepository>();
builder.Services.AddScoped<ISettings, SettingsRepository>();
builder.Services.AddScoped(_ => new SqliteConnection(connectionString));

// Add the whole configuration object here.
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddAuthentication(BasicDefaults.AuthenticationScheme)

    // The below AddBasic without type parameter will require OnValidateCredentials delegete on options.Events to be set unless an implementation of IBasicUserValidationService interface is registered in the dependency register.
    // Please note if both the delgate and validation server are set then the delegate will be used instead of BasicUserValidationService.
    //.AddBasic(options =>

    // The below AddBasic with type parameter will add the BasicUserValidationService to the dependency register. 
    // Please note if OnValidateCredentials delegete on options.Events is also set then this delegate will be used instead of BasicUserValidationService.
    .AddBasic<BasicUserValidationService>(options =>
    {
        options.Realm = "Temperature Web API";
        options.Events = new BasicEvents
        {
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

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

app.UseHttpsRedirection();
app.UseStaticFiles();
//app.UseDefaultFiles();
app.UseRouting();

// global cors policy
app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
