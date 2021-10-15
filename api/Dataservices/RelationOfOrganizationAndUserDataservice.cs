using System;
using System.Linq;
using System.Collections.Generic;

namespace Homo.Bet.Api
{
    public class RelationOfOrganizationAndUserDataservice
    {
        public static List<RelationOfOrganizationAndUser> GetListByOrganization(BargainingChipDBContext dbContext, long organizationId)
        {
            return dbContext.RelationOfOrganizationAndUser
                .Where(x =>
                    x.DeletedAt == null
                    && x.OrganizationId == organizationId
                )
                .ToList();
        }
    }
}