using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using Services;

namespace CRUDExample.Controllers
{
  [Route("[controller]")]
  public class CountriesController : Controller
  {
    private readonly ICountriesUploaderService _countriesService;

    public CountriesController(ICountriesUploaderService countriesService)
    {
      _countriesService = countriesService;
    }
    [HttpGet]
    [Route("[action]")]
    [Authorize(Roles = "Admin")]
    public IActionResult UploadFromExcel()
    {
      return View();
    }
    [HttpPost]
    [Route("[action]")]
    [Authorize(Roles = "Admin")]
    public async Task< IActionResult> UploadFromExcel( IFormFile excelfile)
    {
      if (excelfile == null||excelfile.Length==0)
      {
        ViewBag.ErrorMessage = "Please select xlsx file";
        return View();
        
      }
      if (!Path.GetExtension(excelfile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
      {
        ViewBag.ErrorMessage = "Unsupported file. 'xlsx' is expected";
        return View();
      }
     int countriesCountInserted= await _countriesService.UploadCountriesFromExcelFile(excelfile);

      ViewBag.Message = $"{countriesCountInserted} Countries Uploaded";
      return View();
    }
  }
}
