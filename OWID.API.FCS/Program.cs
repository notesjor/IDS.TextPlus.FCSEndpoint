
namespace OWID.API.FCS
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);
      builder.WebHost.ConfigureKestrel(options =>
      {
        options.ListenLocalhost(16319);
      });

      // Add services to the container.

      builder.Services.AddControllers();

      // Add MCP Server with HTTP/SSE transport and tools from the current assembly
      builder.Services.AddMcpServer()
        .WithHttpTransport() // Verwendet den Streamable HTTP/SSE Transportkanal
        .WithToolsFromAssembly(typeof(Program).Assembly);

      // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
      builder.Services.AddOpenApi();

      var app = builder.Build();

      app.UsePathBase("/api/fcs");

      app.UseCors(policy =>
      {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
      });

      app.MapOpenApi();

      app.MapMcp("/api/fcs/mcp");

      app.MapControllers();

      app.MapFallbackToController("Get", "Fcs");

      app.Run();
    }
  }
}
