using ContactsManager.Core.Domain.IdentityEnitities;
using ContactsManager.Core.DTO;
using ContactsManager.Core.Services;
using ContactsManager.Core.ServicesContracts;
using CRUDExample.Filters.ActionFilters;
using Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Repositories;
using RepositoryContracts;
using ServiceContracts;
using Services;

namespace CRUDExample
{
  public static class ConfigureServicesExtension
  {
    public static IServiceCollection ConfigureSevices(this IServiceCollection services, IConfiguration configuration)
    {
      //add services into IoC container
      services.AddScoped<ICountriesGetterService, CountriesGetterService>();
      services.AddScoped<ICountriesAdderService, CountriesAdderService>();
      services.AddScoped<ICountriesUploaderService, CountriesUploaderService>();

      services.AddScoped<IPersonsGetterService, PersonsGetterServiceWithFewExcelFieleds>();
      services.AddScoped<PersonsGetterService, PersonsGetterService>();

      services.AddScoped<IPersonsUpdaterService, PersonsUpdaterService>();
      services.AddScoped<IPersonsDeleterService, PersonsDeleterService>();
      services.AddScoped<IPersonsAdderService, PersonsAdderService>();
      services.AddScoped<IPersonsSorterService, PersonsSorterService>();

      services.AddScoped<ICountriesRepository, CountriesRepositoey>();
      services.AddScoped<IPersonsRepository, PersonsRepository>();
      services.AddTransient<ResponseHeaderActionFilter>();

      services.AddTransient<IEmailSender, EmailSender>();
      services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));


      services.AddSession();

      services.AddControllersWithViews(options =>
      {
        //options.Filters.Add<ResponseHeaderActionFilter>();
        options.Filters.Add(new ResponseHeaderActionFilter(services.BuildServiceProvider()
          .GetRequiredService<ILogger<ResponseHeaderActionFilter>>())
        { Value = "my-key-global", Key = "My-value-global", Order = 2 });

        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
      });
      services.AddTransient<PersonListActionFilter>();

      services.AddDbContext<ApplicationDbContext>(options =>
      {
        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
      });

      services.AddIdentity<ApplicationUser, ApplicationRole>(option =>
      {
        option.Password.RequiredLength = 5;
        option.Password.RequiredUniqueChars = 3;
        option.Password.RequireLowercase = true;
        option.Password.RequireNonAlphanumeric = false;
        option.Password.RequireUppercase = false;
        option.Password.RequireDigit = false;

        option.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        option.Lockout.MaxFailedAccessAttempts = 5;
        option.Lockout.AllowedForNewUsers = true;
      })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders()
        .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()
        .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>();

     


      // add google login
      services.AddAuthentication(options =>
      {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
      
      })
      .AddCookie()
      .AddGoogle(options =>
      {
        options.ClientId = configuration["Authentication:Google:ClientId"];
        options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
      });


      services.AddAuthorization(option =>
      {
        option.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

        option.AddPolicy("NotAuthorized", policy =>
        {
          policy.RequireAssertion(context => !context.User.Identity.IsAuthenticated);
        });
      });

      services.ConfigureApplicationCookie(options =>
      {
        options.LoginPath = "/Account/Login";
      });

      services.AddHttpLogging(options =>
      {
        options.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders | HttpLoggingFields.ResponsePropertiesAndHeaders;
      });
      return services;
    }

  }
}
