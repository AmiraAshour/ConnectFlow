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
  public class PersonsGetterService : IPersonsGetterService
  {
    //private field
    private readonly IPersonsRepository _personsRepository;
    private readonly ILogger<PersonsGetterService> _logger;
    private readonly IDiagnosticContext _diagnosticContext;

    //constructor
    public PersonsGetterService(IPersonsRepository perosnsRepository, ILogger<PersonsGetterService> logger, IDiagnosticContext diagnosticContext )
    {
      _personsRepository = perosnsRepository;
      _logger = logger;
      _diagnosticContext = diagnosticContext;
    }



    public virtual async Task<List<PersonResponse>> GetAllPersons()
    {
      _logger.LogInformation("GetAllPersons method of PersonService");
      List<Person>persons=await _personsRepository.GetAllPersons();
      return persons.Select(temp => temp.ToPersonResponse()).ToList();
     //return _db.sp_GetAllPersons().Select(temp => ConvertPersonToPersonResponse(temp)).ToList();
    }


    public virtual async Task<PersonResponse?> GetPersonByPersonID(Guid? personID)
    {
      if (personID == null)
        return null;

      Person? person = await _personsRepository.GetPersonByPersonId(personID.Value);
      if (person == null)
        return null;

      return person.ToPersonResponse();
    }

    public virtual async Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString)
    {
      _logger.LogInformation("GetFilteredPersons method of PersonService");
      List<Person> persons;
      if (string.IsNullOrEmpty(searchString))
        return  (await _personsRepository.GetAllPersons()).Select(temp => temp.ToPersonResponse()).ToList();
      using (Operation.Time("Time for filter person form database"))
      {
       persons = searchBy switch
        {
          nameof(PersonResponse.PersonName) =>
           await _personsRepository.GetFilteredPersons(temp =>
           temp.PersonName.Contains(searchString)),

          nameof(PersonResponse.Email) =>
           await _personsRepository.GetFilteredPersons(temp =>
           temp.Email.Contains(searchString)),

          nameof(PersonResponse.DateOfBirth) when DateTime.TryParse(searchString, out DateTime searchDate) =>
           await _personsRepository.GetFilteredPersons(temp =>
               temp.DateOfBirth.HasValue && temp.DateOfBirth.Value.Date == searchDate.Date),


          nameof(PersonResponse.Gender) =>
           await _personsRepository.GetFilteredPersons(temp =>
           temp.Gender.Contains(searchString)),

          nameof(PersonResponse.CountryID) =>
           await _personsRepository.GetFilteredPersons(temp =>
           temp.Country.CountryName.Contains(searchString)),

          nameof(PersonResponse.Address) =>
          await _personsRepository.GetFilteredPersons(temp =>
          temp.Address.Contains(searchString)),

          _ => await _personsRepository.GetAllPersons()
        };
      }
        _diagnosticContext.Set("Persons", persons);
        return persons.Select(temp => temp.ToPersonResponse()).ToList();
      }
    



    public virtual async Task<MemoryStream> GetPersonsCSV()
    {
      MemoryStream memoryStream = new MemoryStream();
      StreamWriter writer = new StreamWriter(memoryStream);
      CsvConfiguration csvConfiguration=new CsvConfiguration(CultureInfo.InvariantCulture);
      CsvWriter csvWriter =new CsvWriter(writer,csvConfiguration);

      //csvWriter.WriteHeader<PersonResponse>();
      csvWriter.WriteField(nameof(PersonResponse.PersonName));
      csvWriter.WriteField(nameof(PersonResponse.Email));
      csvWriter.WriteField(nameof(PersonResponse.DateOfBirth));
      csvWriter.WriteField(nameof(PersonResponse.Age));
      csvWriter.WriteField(nameof(PersonResponse.Gender));
      csvWriter.WriteField(nameof(PersonResponse.Address));
      csvWriter.WriteField(nameof(PersonResponse.Country));
      csvWriter.WriteField(nameof(PersonResponse.ReceiveNewsLetters));
      csvWriter.NextRecord();

      List<PersonResponse> persons=await GetAllPersons();
      foreach (var person in persons)
      {
        csvWriter.WriteField(person.PersonName);
        csvWriter.WriteField(person.Email);
        csvWriter.WriteField((person.DateOfBirth != null)?person.DateOfBirth.Value.ToString("yyyy-MM-dd"):"");
        csvWriter.WriteField(person.Age);
        csvWriter.WriteField(person.Gender);
        csvWriter.WriteField(person.Address);
        csvWriter.WriteField(person.Country);
        csvWriter.WriteField(person.ReceiveNewsLetters);
        csvWriter.NextRecord();
        csvWriter.Flush();
      }
      //await csvWriter.WriteRecordsAsync(persons);

      memoryStream.Position = 0;

      return memoryStream;
    }

    public virtual async Task<MemoryStream> GetPersonsExcel()
    {
      MemoryStream memoryStream=new MemoryStream();
      using (ExcelPackage package = new ExcelPackage(memoryStream))
      {
        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("PersonsSheet");
        worksheet.Cells["A1"].Value="Person Name";
        worksheet.Cells["B1"].Value="Email";
        worksheet.Cells["C1"].Value="Country";
        worksheet.Cells["D1"].Value="Address";
        worksheet.Cells["E1"].Value="Age";
        worksheet.Cells["F1"].Value="Date of Birth";
        worksheet.Cells["G1"].Value="Gender";
        worksheet.Cells["H1"].Value="Receive News Letters";

        using (ExcelRange headerCells = worksheet.Cells["A1:H1"])
        {
          headerCells.Style.Fill.PatternType=OfficeOpenXml.Style.ExcelFillStyle.Solid;
          headerCells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);
          headerCells.Style.Font.Bold = true;

        }
        int row = 2;
        List<PersonResponse> persons = await GetAllPersons();

        foreach (var person in persons)
        {
          worksheet.Cells[row, 1].Value = person.PersonName;
          worksheet.Cells[row, 2].Value = person.Email;
          worksheet.Cells[row, 3].Value = person.Country;
          worksheet.Cells[row, 4].Value = person.Address;
          worksheet.Cells[row, 5].Value = person.Age;
          worksheet.Cells[row, 6].Value = (person.DateOfBirth !=null)?person.DateOfBirth.Value.ToString("yyyy-MM-dd"):null;
          worksheet.Cells[row, 7].Value = person.Gender;
          worksheet.Cells[row, 8].Value = person.ReceiveNewsLetters;
          row++;
        }

        worksheet.Cells[$"A1:H{row}"].AutoFitColumns();
        await package.SaveAsync();

      }
      memoryStream.Position = 0;
      return memoryStream;
    }
  }
}
