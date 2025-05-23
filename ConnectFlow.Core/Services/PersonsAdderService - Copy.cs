﻿using System;
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
  public class PersonsAdderService : IPersonsAdderService
  {
    //private field
    private readonly IPersonsRepository _personsRepository;
    private readonly ILogger<PersonsGetterService> _logger;
    private readonly IDiagnosticContext _diagnosticContext;

    //constructor
    public PersonsAdderService(IPersonsRepository perosnsRepository, ILogger<PersonsGetterService> logger, IDiagnosticContext diagnosticContext )
    {
      _personsRepository = perosnsRepository;
      _logger = logger;
      _diagnosticContext = diagnosticContext;
    }



    public async Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest)
    {
      //check if PersonAddRequest is not null
      if (personAddRequest == null)
      {
        throw new ArgumentNullException(nameof(personAddRequest));
      }

      //Model validation
      ValidationHelper.ModelValidation(personAddRequest);

      //convert personAddRequest into Person type
      Person person = personAddRequest.ToPerson();

      //generate PersonID
      person.PersonID = Guid.NewGuid();

      //add person object to persons list
     await _personsRepository.AddPerson(person);
      //_db.sp_InsertPerson(person);

      //convert the Person object into PersonResponse type
      return person.ToPersonResponse();
    }


 
  }
}
