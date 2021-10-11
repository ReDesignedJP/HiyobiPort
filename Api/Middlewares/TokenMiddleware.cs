using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Api.Lib;
using Api.Models;

namespace Api.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class TokenMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            httpContext.Session.LoadAsync().Wait();
            if (httpContext.Session.IsAvailable)
            {
                if (httpContext.Session.GetUser().Result == null && httpContext.Request.Headers.ContainsKey("Authorization")) //로그인 세션이 없고 토큰이 설정되있으면 토큰으로 로그인
                {
                    string token = httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
                    if (string.IsNullOrEmpty(token)) return _next(httpContext); //토큰이 공백이면 넘김
                    User user = UserLib.GetUserByTokenAsync(token).Result;
                    if (user == null)
                    {
                        httpContext.Session.Clear();
                        httpContext.Session.CommitAsync().Wait();
                        return _next(httpContext);
                    }

                    httpContext.Session.SetUser(user).Wait();
                }
            }
            return _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class TokenMiddlewareExtensions
    {
        public static IApplicationBuilder UseTokenMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TokenMiddleware>();
        }
    }
}
