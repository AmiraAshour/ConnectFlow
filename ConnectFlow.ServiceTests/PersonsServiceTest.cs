using System;
using System.Collections.Generic;
using Xunit;
using ServiceContracts;
using Entities;
using ServiceContracts.DTO;
using Services;
using ServiceContracts.Enums;
using Xunit.Abstractions;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using static System.Collections.Specialized.BitVector32;
using Moq;
using RepositoryContracts;
using FluentAssertions.Execution;
using System.Linq.Expressions;
using Serilog;
using Microsoft.Extensions.Logging;

namespace CRUDTests
{
  public class PersonsServiceTest
  {
    //private fields
    private readonly Mock<IPersonsRepository> _personRepositoryMock;
    private readonly IPersonsRepository _personsRepository;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly IFixture _fixture;

    private readonly IPersonsAdderService _personsAdderService;
    private readonly IPersonsDeleterService _personsDeleterService;
    private readonly IPersonsSorterService _personsSorterService;
    private readonly IPersonsUpdaterService _personsUpdaterService;
    private readonly IPersonsGetterService _personsGetterService;

    //constructor
    public PersonsServiceTest(ITestOutputHelper testOutputHelper)
    {
      _fixture = new Fixture();
      _personRepositoryMock = new Mock<IPersonsRepository>();
      _personsRepository = _personRepositoryMock.Object;

      var diagnosticContextMoq = new Mock<IDiagnosticContext>();
      var loggerMiq = new Mock<ILogger<PersonsGetterService>>();

      _personsAdderService = new PersonsAdderService(_personsRepository,loggerMiq.Object,diagnosticContextMoq.Object);
      _personsGetterService = new PersonsGetterService(_personsRepository,loggerMiq.Object,diagnosticContextMoq.Object);
      _personsSorterService = new PersonsSorterService(_personsRepository,loggerMiq.Object,diagnosticContextMoq.Object);
      _personsUpdaterService = new PersonsUpdaterService(_personsRepository,loggerMiq.Object,diagnosticContextMoq.Object);
      _personsDeleterService = new PersonsDeleterService(_personsRepository,loggerMiq.Object,diagnosticContextMoq.Object);
      _testOutputHelper = testOutputHelper;
    }

    #region AddPerson

    //When we supply null value as PersonAddRequest, it should throw ArgumentNullException
    [Fact]
    public async Task AddPerson_NullPerson_ToBeArgumentNullException()
    {
      //Arrange
      PersonAddRequest? personAddRequest = null;

      //Act
      Func<Task> action = async () =>
      {
        await _personsAdderService.AddPerson(personAddRequest);
      };
      await action.Should().ThrowAsync<ArgumentNullException>();
      // await Assert.ThrowsAsync<ArgumentNullException>(       );
    }


    //When we supply null value as PersonName, it should throw ArgumentException
    [Fact]
    public async Task AddPerson_PersonNameIsNull_ToBeArgumentException()
    {
      //Arrange
      PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
        .With(x => x.PersonName, null as string).Create();

      Person person = personAddRequest.ToPerson();
      _personRepositoryMock.Setup(x => x.AddPerson(It.IsAny<Person>())).ReturnsAsync(person);

      //Act
      Func<Task> action = async () =>
        {
          await _personsAdderService.AddPerson(personAddRequest);
        };
      await action.Should().ThrowAsync<ArgumentException>();

    }

    //When we supply proper person details, it should insert the person into the persons list; and it should return an object of PersonResponse, which includes with the newly generated person id
    [Fact]
    public async Task AddPerson_FullPersonDetails_ToBeSuccessful()
    {
      //Arrange
      PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
        .With(temp => temp.Email, "amira@gmail.com").Create();

      Person person = personAddRequest.ToPerson();
      PersonResponse personResponseExpected = person.ToPersonResponse();

      _personRepositoryMock.Setup(x => x.AddPerson(It.IsAny<Person>())).ReturnsAsync(person);
      //Act
      PersonResponse person_response_from_add = await _personsAdderService.AddPerson(personAddRequest);

      personResponseExpected.PersonID = person_response_from_add.PersonID;
      //Assert
      person_response_from_add.PersonID.Should().NotBe(Guid.Empty);

      personResponseExpected.Should().Be(person_response_from_add);

    }

    #endregion


    #region GetPersonByPersonID

    //If we supply null as PersonID, it should return null as PersonResponse
    [Fact]
    public async Task GetPersonByPersonID_NullPersonID_ToBeNull()
    {
      //Arrange
      Guid? personID = null;

      //Act
      PersonResponse? person_response_from_get = await _personsGetterService.GetPersonByPersonID(personID);

      //Assert
      //Assert.Null(person_response_from_get);
      person_response_from_get.Should().BeNull();
    }


    //If we supply a valid person id, it should return the valid person details as PersonResponse object
    [Fact]
    public async Task GetPersonByPersonID_WithPersonID_ToBeSucceeful()
    {
      //Arange

      Person person = _fixture.Build<Person>().With(x => x.Email, "amira@gmail.com")
        .With(x => x.Country, null as Country).Create();

      PersonResponse personResponseExpected = person.ToPersonResponse();

      _personRepositoryMock.Setup(x => x.GetPersonByPersonId(It.IsAny<Guid>())).ReturnsAsync(person);

      PersonResponse? person_response_from_get = await _personsGetterService.GetPersonByPersonID(person.PersonID);

      //Assert
      //Assert.Equal(person_response_from_add, person_response_from_get);
      person_response_from_get.Should().Be(personResponseExpected);
    }

    #endregion


    #region GetAllPersons

    //The GetAllPersons() should return an empty list by default
    [Fact]
    public async Task GetAllPersons_EmptyList()
    {
      _personRepositoryMock.Setup(x => x.GetAllPersons()).ReturnsAsync(new List<Person>());

      //Act
      List<PersonResponse> persons_from_get = await _personsGetterService.GetAllPersons();

      //Assert
      Assert.Empty(persons_from_get);
      persons_from_get.Should().BeEmpty();
    }


    //First, we will add few persons; and then when we call GetAllPersons(), it should return the same persons that were added
    [Fact]
    public async Task GetAllPersons_WithFewPersons_ToBeSuccessful()
    {
      //Arrange
      List<Person> persons = new List<Person>() {
        _fixture.Build<Person>().With(x => x.Email, "amira1@gmail.com")
        .With(x=>x.Country,null as Country).Create(),
        _fixture.Build<Person>().With(x => x.Email, "amira2@gmail.com")
        .With(x=>x.Country,null as Country).Create(),
        _fixture.Build<Person>().With(x => x.Email, "amira3@gmail.com")
        .With(x=>x.Country,null as Country).Create()
        };


      List<PersonResponse> person_response_list_expected = persons.Select(x => x.ToPersonResponse()).ToList();

      //print person_response_list_from_add
      _testOutputHelper.WriteLine("Expected:");
      foreach (PersonResponse person_response_from_add in person_response_list_expected)
      {
        _testOutputHelper.WriteLine(person_response_from_add.ToString());
      }
      _personRepositoryMock.Setup(x => x.GetAllPersons()).ReturnsAsync(persons);

      //Act
      List<PersonResponse> persons_list_from_get = await _personsGetterService.GetAllPersons();

      //print persons_list_from_get
      _testOutputHelper.WriteLine("Actual:");
      foreach (PersonResponse person_response_from_get in persons_list_from_get)
      {
        _testOutputHelper.WriteLine(person_response_from_get.ToString());
      }

      //Assert
      //foreach (PersonResponse person_response_from_add in person_response_list_from_add)
      //{
      //  Assert.Contains(person_response_from_add, persons_list_from_get);
      //}
      persons_list_from_get.Should().BeEquivalentTo(person_response_list_expected);
    }
    #endregion


    #region GetFilteredPersons

    

    // search based on person name with some search string. It should return the matching persons
    [Fact]
    public async Task GetFilteredPersons_SearchByPersonName()
    {
      //Arrange
      List<Person> persons = new List<Person>() {
        _fixture.Build<Person>().With(x => x.Email, "amira1@gmail.com")
        .With(x=>x.Country,null as Country).Create(),
        _fixture.Build<Person>().With(x => x.Email, "amira2@gmail.com")
        .With(x=>x.Country,null as Country).Create(),
        _fixture.Build<Person>().With(x => x.Email, "amira3@gmail.com")
        .With(x=>x.Country,null as Country).Create()
        };


      List<PersonResponse> person_response_list_expected = persons.Select(x => x.ToPersonResponse()).ToList();


      //print person_response_list_from_add
      _testOutputHelper.WriteLine("Expected:");
      foreach (PersonResponse person_response_from_add in person_response_list_expected)
      {
        _testOutputHelper.WriteLine(person_response_from_add.ToString());
      }
      _personRepositoryMock.Setup(x => x.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>())).ReturnsAsync(persons);

      //Act
      List<PersonResponse> persons_list_from_search = await _personsGetterService.GetFilteredPersons(nameof(Person.PersonName), "sa");

      //print persons_list_from_get
      _testOutputHelper.WriteLine("Actual:");
      foreach (PersonResponse person_response_from_get in persons_list_from_search)
      {
        _testOutputHelper.WriteLine(person_response_from_get.ToString());
      }

      //Assert
      //foreach (PersonResponse person_response_from_add in person_response_list_from_add)
      //{
      //  Assert.Contains(person_response_from_add, persons_list_from_search);
      //}
      persons_list_from_search.Should().BeEquivalentTo(person_response_list_expected);
    }

    #endregion


    #region GetSortedPersons

    //When we sort based on PersonName in DESC, it should return persons list in descending on PersonName
    [Fact]
    public async Task GetSortedPersons()
    {
      //Arrange
      List<Person> persons = new List<Person>() {
       _fixture.Build<Person>().With(x => x.Email, "amira1@gmail.com")
       .With(x=>x.Country,null as Country).Create(),
       _fixture.Build<Person>().With(x => x.Email, "amira2@gmail.com")
       .With(x=>x.Country,null as Country).Create(),
       _fixture.Build<Person>().With(x => x.Email, "amira3@gmail.com")
       .With(x=>x.Country,null as Country).Create()
        };


      List<PersonResponse> person_response_list_expected = persons.Select(x => x.ToPersonResponse()).ToList();

     _personRepositoryMock.Setup(x => x.GetAllPersons()).ReturnsAsync(persons);

      //print person_response_list_from_add
      _testOutputHelper.WriteLine("Expected:");
      foreach (PersonResponse person_response_from_add in person_response_list_expected)
      {
        _testOutputHelper.WriteLine(person_response_from_add.ToString());
      }
      List<PersonResponse> allPersons = await _personsGetterService.GetAllPersons();

      //Act
      List<PersonResponse> persons_list_from_sort = await _personsSorterService.GetSortedPersons(allPersons, nameof(Person.PersonName), SortOrderOptions.DESC);

      //print persons_list_from_get
      _testOutputHelper.WriteLine("Actual:");
      foreach (PersonResponse person_response_from_get in persons_list_from_sort)
      {
        _testOutputHelper.WriteLine(person_response_from_get.ToString());
      }
      //person_response_list_from_add = person_response_list_from_add.OrderByDescending(temp => temp.PersonName).ToList();

      //Assert
      //for (int i = 0; i < person_response_list_from_add.Count; i++)
      //{
      //  Assert.Equal(person_response_list_from_add[i], persons_list_from_sort[i]);
      //}
      //persons_list_from_sort.Should().BeEquivalentTo(person_response_list_from_add);
      persons_list_from_sort.Should().BeInDescendingOrder(x => x.PersonName);
    }
    #endregion


    #region UpdatePerson

    //When we supply null as PersonUpdateRequest, it should throw ArgumentNullException
    [Fact]
    public async Task UpdatePerson_NullPerson__ToBeArgumentNullException()
    {
      //Arrange
      PersonUpdateRequest? person_update_request = null;

      //Assert
      Func<Task> action = async () =>
      {
        //Act
        await _personsUpdaterService.UpdatePerson(person_update_request);
      };
      await action.Should().ThrowAsync<ArgumentNullException>();
    }


    //When we supply invalid person id, it should throw ArgumentException
    [Fact]
    public async Task UpdatePerson_InvalidPersonID_ToBeArgumentException()
    {
      //Arrange
      PersonUpdateRequest? person_update_request = _fixture.Build<PersonUpdateRequest>()
        .With(x => x.Email, "amira@gmail.com").Create();

      //Assert
      Func<Task> action = async () =>
      {
        //Act
        await _personsUpdaterService.UpdatePerson(person_update_request);
      };
      await action.Should().ThrowAsync<ArgumentException>();

    }


    //When PersonName is null, it should throw ArgumentException
    [Fact]
    public async void UpdatePerson_PersonNameIsNull()
    {
      //Arrange

      Person person_add_request = _fixture.Build<Person>()
        .With(x => x.PersonName, null as string)
        .With(x => x.Country, null as Country)
        .With(x => x.Email, "amira1@gmail.com")
        .With(x=>x.Gender, "Male").Create();

      PersonResponse person_response_from_add = person_add_request.ToPersonResponse();

      PersonUpdateRequest person_update_request = person_response_from_add.ToPersonUpdateRequest();


      //Assert
      Func<Task> action = async () =>
      {
        //Act
        await _personsUpdaterService.UpdatePerson(person_update_request);
      };
      await action.Should().ThrowAsync<ArgumentException>();


    }


    //First, add a new person and try to update the person name and email
    [Fact]
    public async Task UpdatePerson_PersonFullDetailsUpdation()
    {

      Person person = _fixture.Build<Person>()
        .With(x => x.Country,null as Country)
        .With(x => x.Email, "amira1@gmail.com")
         .With(x => x.Gender, "Male").Create();

      PersonResponse person_response_expected = person .ToPersonResponse();

      PersonUpdateRequest person_update_request = person_response_expected.ToPersonUpdateRequest();

      _personRepositoryMock.Setup(x => x.UpdatePerson(It.IsAny<Person>())).ReturnsAsync(person);
      _personRepositoryMock.Setup(x => x.GetPersonByPersonId(It.IsAny<Guid>())).ReturnsAsync(person);

      //Act
      PersonResponse person_response_from_update = await _personsUpdaterService.UpdatePerson(person_update_request);


      //Assert
      //Assert.Equal(person_response_from_get, person_response_from_update);
      person_response_from_update.Should().Be(person_response_expected);

    }

    #endregion


    #region DeletePerson

    //If you supply an valid PersonID, it should return true
    [Fact]
    public async Task DeletePerson_ValidPersonID()
    {
      //Arrange
      Person person = _fixture.Build<Person>()
        .With(x => x.PersonName, "mary")
        .With(x => x.Country,null as Country)
        .With(x => x.Email, "amira1@gmail.com")
         .With(x => x.Gender, "Male").Create();

      _personRepositoryMock.Setup(x => x.DeletePersonByPerosnID(It.IsAny<Guid>())).ReturnsAsync(true);
      _personRepositoryMock.Setup(x => x.GetPersonByPersonId(It.IsAny<Guid>())).ReturnsAsync(person);
      //Act
      bool isDeleted = await _personsDeleterService.DeletePerson(person.PersonID);

      //Assert
      //Assert.True(isDeleted);
      isDeleted.Should().BeTrue();
    }


    //If you supply an invalid PersonID, it should return false
    [Fact]
    public async Task DeletePerson_InvalidPersonID()
    {
      //Act
      bool isDeleted = await _personsDeleterService.DeletePerson(Guid.NewGuid());

      //Assert
      Assert.False(isDeleted);
      isDeleted.Should().BeFalse();
    }

    #endregion
  }
}
