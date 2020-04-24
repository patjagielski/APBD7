using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tutorial_3._1.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            string bodyStr = "";
            
            
            if(httpContext.Request != null)
            {
                var req = httpContext.Request;
                req.EnableBuffering();
                var method = httpContext.Request.Method;
                var path = httpContext.Request.Path;
                using (StreamReader reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    bodyStr = await reader.ReadToEndAsync();
                    req.Body.Position = 0;
                }
                var queryStr = httpContext.Request.QueryString.ToString();

                using (StreamWriter sw = new StreamWriter(@"requestLog.txt"))
                {
                    sw.WriteLine(method);
                    sw.WriteLine(path);
                    sw.WriteLine(bodyStr);
                    sw.WriteLine(queryStr);
                }
            }
            


            await _next(httpContext);
        }
    }
}
