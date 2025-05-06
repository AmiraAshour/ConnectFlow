using CRUDExample.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using ServiceContracts.DTO;

namespace CRUDExample.Filters.ActionFilters
{
  public class PersonListActionFilter : IActionFilter
  {
    private readonly ILogger<PersonListActionFilter> _logger;

    public PersonListActionFilter(ILogger<PersonListActionFilter> logger)
    {
      _logger = logger;
    }
    public void OnActionExecuted(ActionExecutedContext context)
    {
      _logger.LogInformation("{FilterName}.{MethodName} method",nameof(PersonListActionFilter),nameof( OnActionExecuted) );

      PersonsController personsController =(PersonsController) context.Controller;

      IDictionary<string, object?>? parameters = (IDictionary<string, object?>?) context.HttpContext.Items["arguments"];
      if (parameters is not null)
      {
        if(parameters.ContainsKey("searchBy"))
         personsController.ViewBag.CurrentSearchBy = Convert.ToString(parameters["searchBy"]);
        if(parameters.ContainsKey("searchString"))
         personsController.ViewBag.CurrentSearchString = Convert.ToString(parameters["searchString"]);
        if(parameters.ContainsKey("sortBy"))
         personsController.ViewBag.CurrentSortBy = Convert.ToString(parameters["sortBy"]);
        if(parameters.ContainsKey("sortOrder"))
         personsController.ViewBag.CurrentSortOrder = Convert.ToString(parameters["sortOrder"]);
      }
     personsController.ViewBag.SearchFields = new Dictionary<string, string>()
      {
        { nameof(PersonResponse.PersonName), "Person Name" },
        { nameof(PersonResponse.Email), "Email" },
        { nameof(PersonResponse.DateOfBirth), "Date of Birth" },
        { nameof(PersonResponse.Gender), "Gender" },
        { nameof(PersonResponse.CountryID), "Country" },
        { nameof(PersonResponse.Address), "Address" }
      };
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
      context.HttpContext.Items["arguments"] = context.ActionArguments;
      //To do: add before logic here
      _logger.LogInformation("{FilterName}.{MethodName} method", nameof(PersonListActionFilter), nameof(OnActionExecuting));



      if (context.ActionArguments.ContainsKey("searchBy"))
      {
        string? searchBy = Convert.ToString(context.ActionArguments["searchBy"]);

        //validate the searchBy parameter value
        if (!string.IsNullOrEmpty(searchBy))
        {
          var searchByOptions = new List<string>() {
            nameof(PersonResponse.PersonName),
            nameof(PersonResponse.Email),
            nameof(PersonResponse.DateOfBirth),
            nameof(PersonResponse.Gender),
            nameof(PersonResponse.CountryID),
            nameof(PersonResponse.Address)
           };

          //reset the searchBy paramer value
          if (searchByOptions.Any(temp => temp == searchBy) == false)
          {
            _logger.LogInformation("searchBy actual value {searchBy}", searchBy);
            context.ActionArguments["searchBy"] = nameof(PersonResponse.PersonName);
            _logger.LogInformation("searchBy updated value {searchBy}", searchBy);
          }
        }
      }
    }
  }
}
