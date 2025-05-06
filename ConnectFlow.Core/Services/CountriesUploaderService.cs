using Entities;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
  public class CountriesUploaderService : ICountriesUploaderService
  {
    //private field
    private readonly ICountriesRepository _countriesRepository;
    private readonly ICountriesAdderService _countriesAdderService;

    //constructor
    public CountriesUploaderService(ICountriesRepository countriesRepository,ICountriesAdderService countriesAdderService)
    {
      _countriesRepository = countriesRepository;
      _countriesAdderService = countriesAdderService;
    }

   
    public async Task<int> UploadCountriesFromExcelFile(IFormFile formFile)
    {
      MemoryStream stream = new MemoryStream();
      await formFile.CopyToAsync(stream);
      int countriesInserted = 0;
      using (ExcelPackage package = new ExcelPackage(stream))
      {
        ExcelWorksheet worksheet = package.Workbook.Worksheets["Countries"];
        int rows = worksheet.Dimension.Rows;
        
        for (int i = 2; i <= rows; i++)
        {
          string? countryName = Convert.ToString(worksheet.Cells[i, 1].Value);
          if (!string.IsNullOrEmpty(countryName))
          {
            if (await _countriesRepository.GetCountryByCountryName( countryName) is  null)
            {
              CountryAddRequest countryAddRequest = new CountryAddRequest() { CountryName = countryName, };
              await _countriesAdderService.AddCountry(countryAddRequest);
              countriesInserted++;
            }
          }
        }


      }
      return countriesInserted;
    }
  }
}

