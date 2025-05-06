using CRUDExample.Filters;
using CRUDExample.Filters.ActionFilters;
using CRUDExample.Filters.AuthorizationFilter;
using CRUDExample.Filters.ExceptionFilters;
using CRUDExample.Filters.ResourceFilters;
using CRUDExample.Filters.ResultFilters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.AspNetCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDExample.Controllers
{
  [Route("[controller]")]
  //[TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { "My-Key-from-Controller", "my-Custom-Value-from-Controller", 3 })]
  [ResponseHeaderFilterFactoryAttribute( "My-Key-from-Controller", "my-Custom-Value-from-Controller", 3 )]
  [TypeFilter(typeof(HandleExceptionFilter))]
  [TypeFilter(typeof(PersonAlwaysRunResultFilter))]
  
  public class PersonsController : Controller
  {

    private readonly ICountriesGetterService _countriesGetterService;

    //private fields

    private readonly ILogger<PersonsController> _logger;
    private readonly IPersonsAdderService _personsAdderService;
    private readonly IPersonsDeleterService _personsDeleterService;
    private readonly IPersonsSorterService _personsSorterService;
    private readonly IPersonsUpdaterService _personsUpdaterService;
    private readonly IPersonsGetterService _personsGetterService;

    //constructor
    public PersonsController(ICountriesGetterService countriesGetterService, ILogger<PersonsController> logger,IPersonsAdderService personsAdderService,IPersonsDeleterService personsDeleterService,IPersonsSorterService personsSorterService,IPersonsUpdaterService personsUpdaterService,IPersonsGetterService personsGetterService)
    { 
     _countriesGetterService = countriesGetterService;
      _logger = logger;
      _personsAdderService = personsAdderService;
      _personsDeleterService = personsDeleterService;
      _personsSorterService = personsSorterService;
      _personsUpdaterService = personsUpdaterService;
      _personsGetterService = personsGetterService;
    }

    //Url: persons/index
    [Route("[action]")]
    [Route("/")]
    [ServiceFilter(typeof(PersonListActionFilter), Order = 4)]
    //[TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { "My-Key-from-Actiom", "my-Custom-Value-from-Action", 1 })]
    [ResponseHeaderFilterFactory("My-Key-from-Actiom", "my-Custom-Value-from-Action", 1)]
    [TypeFilter(typeof(PersonsListResultFilter))]
    [TypeFilter(typeof(SkipFilter))]
    public async Task<IActionResult> Index(string searchBy, string? searchString, string sortBy = nameof(PersonResponse.PersonName), SortOrderOptions sortOrder = SortOrderOptions.ASC)
    {
      _logger.LogInformation("Index method of Person Controller");
      _logger.LogDebug($"searchBy: {searchBy}, searchString: {searchString}, sortBy: {sortBy}, sortOrder: {sortOrder} ");

      //Search
      List<PersonResponse> persons = await _personsGetterService.GetFilteredPersons(searchBy, searchString);

      //Sort
      List<PersonResponse> sortedPersons = await _personsSorterService.GetSortedPersons(persons, sortBy, sortOrder);

      return View(sortedPersons); //Views/Persons/Index.cshtml
    }


    //Executes when the user clicks on "Create Person" hyperlink (while opening the create view)
    //Url: persons/create
    [Route("[action]")]
    [HttpGet]
    [ResponseHeaderFilterFactory("My-Key", "My-Value",2)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
      List<CountryResponse> countries = await _countriesGetterService.GetAllCountries();
      ViewBag.Countries = countries.Select(temp =>
        new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() }
      );

      //new SelectListItem() { Text="Harsha", Value="1" }
      //<option value="1">Harsha</option>
      return View();
    }

    [HttpPost]
    //Url: persons/create
    [Route("[action]")]
    [TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(PersonAddRequest personRequest)
    {
      //call the service method
      PersonResponse personResponse = await _personsAdderService.AddPerson(personRequest);

      //navigate to Index() action method (it makes another get request to "persons/index"
      return RedirectToAction("Index", "Persons");
    }

    [HttpGet]
    [Route("[action]/{personID}")] //Eg: /persons/edit/1
    [TypeFilter(typeof(TokenResultFilter))]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(Guid personID)
    {
      PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonID(personID);
      if (personResponse == null)
      {
        return RedirectToAction("Index");
      }

      PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();

      List<CountryResponse> countries = await _countriesGetterService.GetAllCountries();
      ViewBag.Countries = countries.Select(temp =>
      new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() });

      return View(personUpdateRequest);
    }


    [HttpPost]
    [Route("[action]/{personID}")]
    [TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]
    //[TypeFilter(typeof(FeatureDisabledResourceFilter), Arguments = new object[]{false})]
    [TypeFilter(typeof(TokenAuthorizationFilter))]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(PersonUpdateRequest personRequest)
    {
      PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonID(personRequest.PersonID);

      if (personResponse == null)
      {
        return RedirectToAction("Index");
      }

        PersonResponse updatedPerson = await _personsUpdaterService.UpdatePerson(personRequest);
        return RedirectToAction("Index");
     
     
    }


    [HttpGet]
    [Route("[action]/{personID}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid? personID)
    {
      PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonID(personID);
      if (personResponse == null)
        return RedirectToAction("Index");

      return View(personResponse);
    }

    [HttpPost]
    [Route("[action]/{personID}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(PersonUpdateRequest personUpdateResult)
    {
      PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonID(personUpdateResult.PersonID);
      if (personResponse == null)
        return RedirectToAction("Index");

      await _personsDeleterService.DeletePerson(personUpdateResult.PersonID);
      return RedirectToAction("Index");
    }
    [Route("[action]")]
    public async Task<IActionResult> PersonsPDF()
    {
      List<PersonResponse> persons = await _personsGetterService.GetAllPersons();

      return new ViewAsPdf("PersonsPDF", persons, ViewData)
      {
        PageMargins = new Rotativa.AspNetCore.Options.Margins() { Bottom = 20, Top = 20, Left = 20, Right = 20 },
        PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape
      };
    }
    [Route("[action]")]
    public async Task<IActionResult> PersonsCSV()
    {
      MemoryStream persons = await _personsGetterService.GetPersonsCSV();

      return File(persons, "application/octet-stream", "Persons.csv");
    }
    [Route("[action]")]
    public async Task<IActionResult> PersonsExcel()
    {
      MemoryStream persons = await _personsGetterService.GetPersonsExcel();

      return File(persons, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Persons.xlsx");
    }
  }
}
