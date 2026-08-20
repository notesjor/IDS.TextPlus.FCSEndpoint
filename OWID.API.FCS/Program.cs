
namespace OWID.API.FCS
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);

      // Add services to the container.

      builder.Services.AddControllers();

      // Add MCP Server with HTTP/SSE transport and tools from the current assembly
      builder.Services.AddMcpServer()
        .WithHttpTransport() // Verwendet den Streamable HTTP/SSE Transportkanal
        .WithToolsFromAssembly(typeof(Program).Assembly);

      // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
      builder.Services.AddOpenApi();

      var app = builder.Build();

      // Configure the HTTP request pipeline.
      if (app.Environment.IsDevelopment())
      {
        app.MapOpenApi();
      }

      app.MapControllers();

      app.Run();
    }
  }
}
