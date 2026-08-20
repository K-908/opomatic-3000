using OpoMatic3000.Api.Configuration;
using OpoMatic3000.Api.ErrorHandling;
using OpoMatic3000.Application.Topics;
using OpoMatic3000.Application.Questions;
using OpoMatic3000.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddApiProblemDetails();
builder.Services.AddApiCors(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<TopicService>();
builder.Services.AddScoped<QuestionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors(CorsConfiguration.FrontendPolicy);
app.MapControllers();

app.Run();

public partial class Program;
