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
  public class PersonsSorterService : IPersonsSorterService
  {
    //private field
    private readonly IPersonsRepository _personsRepository;
    private readonly ILogger<PersonsGetterService> _logger;
    private readonly IDiagnosticContext _diagnosticContext;

    //constructor
    public PersonsSorterService(IPersonsRepository perosnsRepository, ILogger<PersonsGetterService> logger, IDiagnosticContext diagnosticContext )
    {
      _personsRepository = perosnsRepository;
      _logger = logger;
      _diagnosticContext = diagnosticContext;
    }



   
    public async Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, SortOrderOptions sortOrder)
    {
      _logger.LogInformation("GetSortedPersons method of PersonService");

      if (string.IsNullOrEmpty(sortBy))
        return allPersons;

      PropertyInfo? property = typeof(PersonResponse).GetProperty(sortBy);
      if (property == null)
        return allPersons; 

      Func<PersonResponse, object> keySelector = person =>
      {
        var value = property.GetValue(person);
        return value ?? string.Empty; 
      };

      return sortOrder == SortOrderOptions.ASC
          ? allPersons.OrderBy(keySelector).ToList()
          : allPersons.OrderByDescending(keySelector).ToList();
    }


  }
}
