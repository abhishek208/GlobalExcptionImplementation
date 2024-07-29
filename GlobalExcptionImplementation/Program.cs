using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Text.Json.Serialization;

namespace GlobalExcptionImplementation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();


            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.Use(async (context, next) =>
            {
                var logDetails = new
                {
                    RequestPath = context.Request.Path,
                    RequestMethod = context.Request.Method,
                    QueryString = context.Request.QueryString.ToString(),
                    Headers = context.Request.Headers
                };
                // Do work that can write to the Response.
               Console.WriteLine($"Testing what happens to the request that comes in the middleware:::{JsonConvert.SerializeObject(logDetails)}");
                await next.Invoke();
                // Do logging or other work that doesn't write to the Response.
            });
            var summaries = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };

            app.UseExceptionHandler(configure => {
                configure.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        var problemDetails = new ProblemDetails
                        {
                            Status = context.Response.StatusCode,
                            Title = "An error occurred while processing your request.",
                            Detail = contextFeature.Error.Message,
                            Instance = context.Request.Path
                        };

                        Console.WriteLine($"Something went wrong: {contextFeature.Error}");

                        await context.Response.WriteAsJsonAsync(problemDetails);
                    }
                });



            });
            app.MapGet("/weatherforecast", (HttpContext httpContext) =>
            {
                throw new Exception("Testing the new Global Exception Error Handleing");
                var forecast = Enumerable.Range(1, 5).Select(index =>
                    new WeatherForecast
                    {
                        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        TemperatureC = Random.Shared.Next(-20, 55),
                        Summary = summaries[Random.Shared.Next(summaries.Length)]
                    })
                    .ToArray();
                return forecast;
            });

            app.Run();
        }
    }
}
