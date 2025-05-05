using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ActionFilters
{
  public class ResponseHeaderFilterFactoryAttribute : Attribute, IFilterFactory
  {
    public bool IsReusable => false;

    private  string Key {get; set;}
    private string Value { get;set; }

    public int Order { get; }


    public ResponseHeaderFilterFactoryAttribute(string key, string value, int order)
    {
      Key = key;
      Value = value;
      Order = order;
    }

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
      var filter=serviceProvider.GetService<ResponseHeaderActionFilter>();
      filter.Value=Value;
      filter.Order=Order;
      filter.Key=Key;
      return filter;
    }
  }
  public class ResponseHeaderActionFilter : IAsyncActionFilter,IOrderedFilter
  {
    private readonly ILogger<ResponseHeaderActionFilter> _logger;

    public string Key { get; set; }
    public string Value { get; set; }

    public int Order { get; set; }


    public ResponseHeaderActionFilter(ILogger<ResponseHeaderActionFilter>logger)
    {
      _logger = logger;
    }

   
    public  async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
      _logger.LogInformation("ResponseHeaderActionFilter before");
      context.HttpContext.Response.Headers[Key] = Value;
      await next();
      _logger.LogInformation("ResponseHeaderActionFilter After");

    }
  }
}
