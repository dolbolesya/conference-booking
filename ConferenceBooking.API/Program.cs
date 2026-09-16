using ConferenceBooking.API.Infrastructure;
using ConferenceBooking.Application;
using ConferenceBooking.Infrastructure;
using ConferenceBooking.Infrastructure.Persistence;
using Microsoft.OpenApi;

using System.Reflection;
using System.Text.Encodings.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>());

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Conference Booking API",
        Version = "v1",
        Description = "API для управління конференц-залами, бронюваннями та розрахунком вартості оренди."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
});

var app = builder.Build();

await app.Services.InitializeAsync();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Conference Booking API v1");
        options.DocumentTitle = "Conference Booking API";
    });
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();