using Microsoft.AspNetCore.Mvc;

namespace Homo.Bet.Api
{
    [Route("v1/me")]
    [AuthorizeFactory]
    public class MeController : ControllerBase
    {

        private readonly BargainingChipDBContext _dbContext;
        public MeController(BargainingChipDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public dynamic getMe(DTOs.JwtExtraPayload extraPayload)
        {
            User user = UserDataservice.GetOne(_dbContext, extraPayload.Id);
            return new
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Profile = user.Profile,
                PseudonymousAddress = user.PseudonymousAddress,
                PseudonymousPhone = user.PseudonymousPhone
            };
        }
    }
}
