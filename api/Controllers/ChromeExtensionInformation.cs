using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Homo.Core.Constants;

namespace Homo.Bet.Api
{
    [Route("v1/chrome-extension")]
    [AuthorizeFactory]
    public class ChromeExtensionInformation : ControllerBase
    {

        private readonly BargainingChipDBContext _dbContext;
        public ChromeExtensionInformation(BargainingChipDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("users")]
        public ActionResult<dynamic> getUsers()
        {
            return UserDataservice.GetAllByIds(null, _dbContext).Select(item => new
            {
                item.Id,
                item.Username,
            }).ToList();
        }

    }
}
