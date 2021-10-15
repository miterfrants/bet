using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Homo.Bet.Api
{
    public class BelongOrgFactory : ActionFilterAttribute, IFilterFactory
    {
        public bool IsReusable => true;
        public BelongOrgFactory()
        {
        }

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            IOptions<AppSettings> config = serviceProvider.GetService<IOptions<AppSettings>>();
            var secrets = (Secrets)config.Value.Secrets;
            return new BelongOrgAttribute(secrets.DBConnectionString, secrets.JwtKey);
        }
    }
}