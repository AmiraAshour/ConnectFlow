using Microsoft.AspNetCore.Http;
using ServiceContracts.DTO;

namespace ServiceContracts
{
  /// <summary>
  /// Represents business logic for manipulating Country entity
  /// </summary>
  public interface ICountriesUploaderService
  {

    /// <summary>
    /// Upload Countries from excel file into database
    /// </summary>
    /// <param name="formFile">Excel file with list of countries</param>
    /// <returns>return number of countries upload in database</returns>

    Task<int> UploadCountriesFromExcelFile(IFormFile formFile);
  }
}
