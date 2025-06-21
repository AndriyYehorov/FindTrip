using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.RegularExpressions;

namespace WebApplication1.Filters
{
    public class EdgeFilterAttribute : Attribute, IResourceFilter
    {
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            
        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            string userBrowser = context.HttpContext.Request.Headers.UserAgent.ToString();

            if (Regex.IsMatch(userBrowser.ToLower(), "edg")) 
            {
                context.Result = new ContentResult { Content = "Cкачайте хром)))" };
            }            
        }
    }
}
