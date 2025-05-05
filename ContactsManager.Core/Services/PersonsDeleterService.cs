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
  public class PersonsDeleterService : IPersonsDeleterService
  {
    //private field
    private readonly IPersonsRepository _personsRepository;
    private readonly ILogger<PersonsGetterService> _logger;
    private readonly IDiagnosticContext _diagnosticContext;

    //constructor
    public PersonsDeleterService(IPersonsRepository perosnsRepository, ILogger<PersonsGetterService> logger, IDiagnosticContext diagnosticContext )
    {
      _personsRepository = perosnsRepository;
      _logger = logger;
      _diagnosticContext = diagnosticContext;
    }


    public async Task<bool> DeletePerson(Guid? personID)
    {
      if (personID == null)
      {
        throw new ArgumentNullException(nameof(personID));
      }

      Person? person =await _personsRepository.GetPersonByPersonId( personID.Value);
      if (person == null)
        return false;

      return await _personsRepository.DeletePersonByPerosnID(person.PersonID);
    }

   
  }
}
