
using Fizzler.Systems.HtmlAgilityPack;
using FluentAssertions;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CRUDTests
{
  public class PersonControllerIntegrationText:IClassFixture<CustomWebApplicationFactory>
  {
    private readonly HttpClient _client;
    public PersonControllerIntegrationText(CustomWebApplicationFactory factory)
    {
      _client=factory.CreateClient();
    }

    #region Index
    [Fact]
    public async Task Index_ToReturnView()
    {
      HttpResponseMessage response = await _client.GetAsync("Persons/Index");

      response.Should().BeSuccessful();

      string resposeBody = await response.Content.ReadAsStringAsync();

      HtmlDocument htmlDocument = new HtmlDocument();
      htmlDocument.LoadHtml(resposeBody);

       var document =htmlDocument.DocumentNode;
      document.QuerySelectorAll("table.person").Should().NotBeNull();
    }

    #endregion
  }
}
