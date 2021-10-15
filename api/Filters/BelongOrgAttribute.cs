using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using System.Net;
using System.Linq;
using Homo.Core.Constants;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System;

namespace Homo.Bet.Api
{
    public class BelongOrgAttribute : ActionFilterAttribute
    {
        private string _dbc { get; set; }
        private string _jwtKey { get; set; }
        public BelongOrgAttribute(string dbc, string jwtKey)
        {
            _dbc = dbc;
            _jwtKey = jwtKey;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            long orgId = -1;
            var optionsBuilder = new DbContextOptionsBuilder<BargainingChipDBContext>();
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 25));
            optionsBuilder.UseMySql(_dbc, serverVersion);
            BargainingChipDBContext dbContext = new BargainingChipDBContext(optionsBuilder.Options);
            long.TryParse(context.RouteData.Values["organizationId"].ToString(), out orgId);
            var extraPayload = (Homo.Bet.Api.DTOs.JwtExtraPayload)context.ActionArguments["extraPayload"];
            List<RelationOfOrganizationAndUser> list = RelationOfOrganizationAndUserDataservice.GetListByOrganization(dbContext, orgId);
            bool isBelongOrg = list.Where(x => x.UserId == extraPayload.Id).Count() > 0;
            if (!isBelongOrg)
            {
                throw new CustomException(ERROR_CODE.NOT_BELONG_ORG, HttpStatusCode.Forbidden);
            }
        }
    }
}