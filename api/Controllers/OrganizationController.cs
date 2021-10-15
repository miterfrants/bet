using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Homo.Core.Constants;

namespace Homo.Bet.Api
{
    [AuthorizeFactory]
    [Route("v1/organizations")]
    public class OrganizationController : ControllerBase
    {
        private readonly BargainingChipDBContext _dbContext;
        public OrganizationController(BargainingChipDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public ActionResult<dynamic> getList([FromQuery] int limit, [FromQuery] int page, [FromQuery] string name, Homo.Bet.Api.DTOs.JwtExtraPayload extraPayload)
        {
            long ownerId = extraPayload.Id;
            List<Organization> records = OrganizationDataservice.GetList(_dbContext, ownerId, page, limit, name);
            return new
            {
                organizations = records,
                rowNums = OrganizationDataservice.GetRowNum(_dbContext, ownerId, name)
            };
        }

        [HttpGet]
        [Route("all")]
        public ActionResult<dynamic> getAll([FromQuery] string name, Homo.Bet.Api.DTOs.JwtExtraPayload extraPayload)
        {
            long ownerId = extraPayload.Id;
            return OrganizationDataservice.GetAll(_dbContext, ownerId, name);
        }

        [HttpPost]
        public ActionResult<dynamic> create([FromBody] DTOs.Organization dto, Homo.Bet.Api.DTOs.JwtExtraPayload extraPayload)
        {
            long ownerId = extraPayload.Id;
            Organization rewRecord = OrganizationDataservice.Create(_dbContext, ownerId, ownerId, dto);
            return rewRecord;
        }

        [HttpDelete]
        public ActionResult<dynamic> batchDelete([FromBody] List<long> ids, Homo.Bet.Api.DTOs.JwtExtraPayload extraPayload)
        {
            long ownerId = extraPayload.Id;
            OrganizationDataservice.BatchDelete(_dbContext, ownerId, ownerId, ids);
            return new { status = CUSTOM_RESPONSE.OK };
        }

        [HttpGet]
        [Route("{id}")]
        public ActionResult<dynamic> get([FromRoute] int id, Homo.Bet.Api.DTOs.JwtExtraPayload extraPayload)
        {
            long ownerId = extraPayload.Id;
            Organization record = OrganizationDataservice.GetOne(_dbContext, ownerId, id);
            if (record == null)
            {
                throw new CustomException(ERROR_CODE.DATA_NOT_FOUND, System.Net.HttpStatusCode.NotFound);
            }
            return record;
        }

        [HttpPatch]
        [Route("{id}")]
        public ActionResult<dynamic> update([FromRoute] int id, [FromBody] DTOs.Organization dto, Homo.Bet.Api.DTOs.JwtExtraPayload extraPayload)
        {
            long ownerId = extraPayload.Id;
            OrganizationDataservice.Update(_dbContext, ownerId, id, ownerId, dto);
            return new { status = CUSTOM_RESPONSE.OK };
        }

        [HttpDelete]
        [Route("{id}")]
        public ActionResult<dynamic> delete([FromRoute] long id, Homo.Bet.Api.DTOs.JwtExtraPayload extraPayload)
        {
            long ownerId = extraPayload.Id;
            OrganizationDataservice.Delete(_dbContext, ownerId, id, ownerId);
            return new { status = CUSTOM_RESPONSE.OK };
        }

    }
}
