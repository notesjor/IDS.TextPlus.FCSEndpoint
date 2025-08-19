using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Routing.Constraints;

namespace OWID.API.FCS
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateSlimBuilder(args);

      builder.Services.ConfigureHttpJsonOptions(options =>
      {
        options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
      });

      builder.Services.Configure<RouteOptions>(options =>
          options.SetParameterPolicy<RegexInlineRouteConstraint>("regex"));

      // OpenAPI/Swagger-Dokumentation aktivieren
      builder.Services.AddEndpointsApiExplorer();
      builder.Services.AddSwaggerGen();

      var app = builder.Build();

      // Swagger-Middleware aktivieren
      app.UseSwagger();
      app.UseSwaggerUI();

      var sampleTodos = new Todo[] {
              new(1, "Walk the dog"),
              new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
              new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
              new(4, "Clean the bathroom"),
              new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
          };

      var todosApi = app.MapGroup("/todos");
      todosApi.MapGet("/", () => sampleTodos);
      todosApi.MapGet("/{id}", (int id) =>
          sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
              ? Results.Ok(todo)
              : Results.NotFound());
      todosApi.MapPost("/", (Todo todo) =>
      {
        if (todo.Id == 0)
        {
          todo = new Todo(sampleTodos.Length + 1, todo.Title, todo.DueBy, todo.IsComplete);
          return Results.Created($"/todos/{todo.Id}", todo);
        }
        else
        {
          var index = Array.FindIndex(sampleTodos, a => a.Id == todo.Id);
          if (index > -1)
          {
            sampleTodos[index] = todo;
          }
          return Results.Accepted($"/todos/{todo.Id}", todo);
        }
      });

      app.Run();
    }
  }

  public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

  [JsonSerializable(typeof(Todo[]))]
  internal partial class AppJsonSerializerContext : JsonSerializerContext
  {

  }
}
