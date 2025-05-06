using ServiceContracts;
using Services;
using Microsoft.EntityFrameworkCore;
using Entities;
using RepositoryContracts;
using ServiceContracts.DTO;
using Repositories;
using Microsoft.AspNetCore.HttpLogging;
using Serilog;
using Serilog.Events;
using CRUDExample.Filters.ActionFilters;
using CRUDExample;
using CRUDExample.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfiguration) =>
{
  loggerConfiguration
      .ReadFrom.Configuration(context.Configuration)
      .MinimumLevel.Debug() 
      .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) 
      .Enrich.FromLogContext() 
      .WriteTo.Console()
      .WriteTo.Seq("http://localhost:5341"); 
});

builder.Services.ConfigureSevices(builder.Configuration);

var app = builder.Build();
if (builder.Environment.IsDevelopment())
{
  app.UseDeveloperExceptionPage();
}
else
{
  app.UseExceptionHandler("/Error");
  app.UseExceptionHandlingMiddleware();
}
app.UseHsts();
app.UseHttpsRedirection();


app.UseSession();
app.UseSerilogRequestLogging();
app.UseHttpLogging();

if (!builder.Environment.IsEnvironment("Test"))
Rotativa.AspNetCore.RotativaConfiguration.Setup("wwwroot", wkhtmltopdfRelativePath : "Rotativa");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseEndpoints(endpoints =>
{
  endpoints.MapControllerRoute(name:"area",pattern:"{area:exists}/{controller=Home}/{action=Index}");
  endpoints.MapControllerRoute(name:"Default",pattern:"{controller}/{action}/{id?}");
});
app.Run();

public partial class Program { }