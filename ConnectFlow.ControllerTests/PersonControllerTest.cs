using AutoFixture;
using CRUDExample.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using Serilog.Extensions.Hosting;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CRUDTests
{
  public class PersonControllerTest
  {
    private readonly IPersonsSorterService _personsService;
    private readonly ICountriesGetterService _countriesGetterService;
    private readonly ILogger<PersonsController> _logger;

    private readonly IPersonsAdderService _personsAdderService;
    private readonly IPersonsDeleterService _personsDeleterService;
    private readonly IPersonsSorterService _personsSorterService;
    private readonly IPersonsUpdaterService _personsUpdaterService;
    private readonly IPersonsGetterService _personsGetterService;


    private readonly Mock<ILogger<PersonsController>> _loggerMock;

    private readonly Mock<IPersonsAdderService> _personsAdderServiceMock;
    private readonly Mock<IPersonsUpdaterService> _personsUpdaterServiceMock;
    private readonly Mock<IPersonsSorterService> _personsSorterServiceMock;
    private readonly Mock<IPersonsGetterService> _personsGetterServiceMock;
    private readonly Mock<IPersonsDeleterService> _personsDeleterServiceMock;
    private readonly Mock<ICountriesGetterService> _countriesGetterServiceMock;
    private readonly Fixture _fixture;
    public PersonControllerTest()
    {
      _fixture = new Fixture();

      _countriesGetterServiceMock = new Mock<ICountriesGetterService>();
      _personsSorterServiceMock= new Mock<IPersonsSorterService>();
      _personsGetterServiceMock= new Mock<IPersonsGetterService>();
      _personsAdderServiceMock= new Mock<IPersonsAdderService>();
      _personsUpdaterServiceMock= new Mock<IPersonsUpdaterService>();
      _personsDeleterServiceMock= new Mock<IPersonsDeleterService>();

      var diagnosticContextMoq = new Mock<IDiagnosticContext>();
      _loggerMock = new Mock<ILogger<PersonsController>>();

      _personsSorterService = _personsSorterServiceMock.Object;
      _personsDeleterService = _personsDeleterServiceMock.Object;
      _personsUpdaterService = _personsUpdaterServiceMock.Object;
      _personsAdderService = _personsAdderServiceMock.Object;
      _personsGetterService = _personsGetterServiceMock.Object;

      _countriesGetterService = _countriesGetterServiceMock.Object;
      _logger = _loggerMock.Object;

    }

    #region Index 

    [Fact]
    public async Task Index_ShouldReturnIndexViewWithPersonList()
    {
      List<PersonResponse> persons=_fixture.Create<List<PersonResponse>>();

      PersonsController personsController = new PersonsController( _countriesGetterService,_logger,_personsAdderService,_personsDeleterService,_personsSorterService,_personsUpdaterService,_personsGetterService);
      
      _personsGetterServiceMock.Setup(x=>x.GetFilteredPersons(It.IsAny<string>(),It.IsAny<string>())).ReturnsAsync(persons);
      _personsSorterServiceMock.Setup(x=>x.GetSortedPersons(It.IsAny<List<PersonResponse>>(),It.IsAny<string>(),It.IsAny<SortOrderOptions>())).ReturnsAsync(persons);

      //act
    IActionResult actionResult= await personsController.Index(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<SortOrderOptions>());

      //Assert
     ViewResult viewResult= Assert.IsType<ViewResult>(actionResult);
      viewResult.ViewData.Model.Should().BeAssignableTo<IEnumerable< PersonResponse>>();
      viewResult.ViewData.Model.Should().Be(persons);
    }
    #endregion

    #region Create

    

    [Fact]
    public async void Create_IfNoModelErrors_ToReturnRedirectToIndex()
    {
      //Arrange
      PersonAddRequest person_add_request = _fixture.Create<PersonAddRequest>();

      PersonResponse person_response = _fixture.Create<PersonResponse>();

      List<CountryResponse> countries = _fixture.Create<List<CountryResponse>>();

      _countriesGetterServiceMock
       .Setup(temp => temp.GetAllCountries())
       .ReturnsAsync(countries);

      _personsAdderServiceMock
       .Setup(temp => temp.AddPerson(It.IsAny<PersonAddRequest>()))
       .ReturnsAsync(person_response);

      PersonsController personsController = new PersonsController(_countriesGetterService, _logger, _personsAdderService, _personsDeleterService, _personsSorterService, _personsUpdaterService, _personsGetterService);


      //Act
      IActionResult result = await personsController.Create(person_add_request);

      //Assert
      RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);

      redirectResult.ActionName.Should().Be("Index");
    }

    #endregion
  }
}
