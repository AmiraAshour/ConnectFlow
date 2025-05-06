using System;
using Entities;
using ServiceContracts.DTO;
using ServiceContracts;
using System.ComponentModel.DataAnnotations;
using Services.Helpers;
using ServiceContracts.Enums;
using CsvHelper;
using System.Globalization;
using System.IO;
using CsvHelper.Configuration;
using OfficeOpenXml;
using RepositoryContracts;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Serilog;
using SerilogTimings;

namespace Services
{
  public class PersonsUpdaterService : IPersonsUpdaterService
  {
    //private field
    private readonly IPersonsRepository _personsRepository;
    private readonly ILogger<PersonsGetterService> _logger;
    private readonly IDiagnosticContext _diagnosticContext;

    //constructor
    public PersonsUpdaterService(IPersonsRepository perosnsRepository, ILogger<PersonsGetterService> logger, IDiagnosticContext diagnosticContext )
    {
      _personsRepository = perosnsRepository;
      _logger = logger;
      _diagnosticContext = diagnosticContext;
    }



    public async Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest)
    {
      if (personUpdateRequest == null)
        throw new ArgumentNullException(nameof(Person));

      //validation
      ValidationHelper.ModelValidation(personUpdateRequest);

      //get matching person object to update
      Person? matchingPerson =await _personsRepository.UpdatePerson(personUpdateRequest.ToPerson());
      if (matchingPerson == null)
      {
        throw new ArgumentException("Given person id doesn't exist");
      }

      //update all details
     
      return matchingPerson.ToPersonResponse();
    }

  }
}
