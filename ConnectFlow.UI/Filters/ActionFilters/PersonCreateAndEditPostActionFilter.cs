using CRUDExample.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceContracts;
using ServiceContracts.DTO;

namespace CRUDExample.Filters.ActionFilters
{
  public class PersonCreateAndEditPostActionFilter : IAsyncActionFilter
  {
    private readonly ICountriesGetterService _countriesGetterService;
    private readonly ILogger<PersonCreateAndEditPostActionFilter> _logger;

    public PersonCreateAndEditPostActionFilter(ICountriesGetterService countriesService, ILogger<PersonCreateAndEditPostActionFilter> logger)
    {
      _countriesGetterService = countriesService;
      _logger = logger;
    }
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
      if (context.Controller is PersonsController personsController)
      {

        if (!personsController.ModelState.IsValid)
        {
          List<CountryResponse> countries = await _countriesGetterService.GetAllCountries();
          personsController.ViewBag.Countries = countries.Select(temp =>
          new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() });

          personsController.ViewBag.Errors = personsController.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
          context.Result = personsController.View(context.ActionArguments["personAddRequest"]);
        }
        else
          await next();
      }
      else
        await next();

      _logger.LogInformation("In after logic of PersonCreateAndEdit Action Filter");
    }
  }
}
