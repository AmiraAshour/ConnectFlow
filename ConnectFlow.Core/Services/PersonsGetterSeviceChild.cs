using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using RepositoryContracts;
using Serilog;
using ServiceContracts.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
  public class PersonsGetterSeviceChild:PersonsGetterService
  {
    public PersonsGetterSeviceChild(IPersonsRepository perosnsRepository, ILogger<PersonsGetterService> logger, IDiagnosticContext diagnosticContext):base(perosnsRepository,logger,diagnosticContext)
    {
      
    }
    public override async Task<MemoryStream> GetPersonsExcel()
    {

      MemoryStream memoryStream = new MemoryStream();
      using (ExcelPackage package = new ExcelPackage(memoryStream))
      {
        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("PersonsSheet");
        worksheet.Cells["A1"].Value = "Person Name";
        worksheet.Cells["B1"].Value = "Age";
        worksheet.Cells["C1"].Value = "Gender";

        using (ExcelRange headerCells = worksheet.Cells["A1:C1"])
        {
          headerCells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
          headerCells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);
          headerCells.Style.Font.Bold = true;

        }
        int row = 2;
        List<PersonResponse> persons = await GetAllPersons();

        foreach (var person in persons)
        {
          worksheet.Cells[row, 1].Value = person.PersonName;
          worksheet.Cells[row, 2].Value = person.Age;
          worksheet.Cells[row, 3].Value = person.Gender;
          row++;
        }

        worksheet.Cells[$"A1:C{row}"].AutoFitColumns();
        await package.SaveAsync();

      }
      memoryStream.Position = 0;
      return memoryStream;
    }
  }
}
