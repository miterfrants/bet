using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Homo.Api;

namespace Homo.Bet.Api
{
    public class AuthApiErrorHandlingMiddleware : ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;
        public AuthApiErrorHandlingMiddleware(RequestDelegate next) : base(next)
        {
            this.next = next;
        }

        protected override ActionResult<dynamic> HandleExceptionAsync(HttpContext context, Exception ex, IOptions<IAppSettings> config, ErrorMessageLocalizer localizer)
        {
            IOptions<Homo.Bet.Api.AppSettings> _config = _serviceProvider.GetService<IOptions<Homo.Bet.Api.AppSettings>>();
            return base.HandleExceptionAsync(context, ex, _config, localizer);
        }
    }
}
