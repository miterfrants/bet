using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Homo.Core.Constants;

namespace Homo.Bet.Api
{
    [AuthorizeFactory]
    [BelongOrgFactory]
    [Route("v1/organizations/{organizationId}/projects")]
    public class ProjectController : ControllerBase
    {
        private readonly BargainingChipDBContext _dbContext;
        public ProjectController(BargainingChipDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public ActionResult<dynamic> getList([FromRoute] long organizationId, [FromQuery] string name, [FromQuery] int limit, [FromQuery] int page, Homo.Bet.Api.DTOs.JwtExtraPayload extraPayload)
        {
            List<Project> records = ProjectDataservice.GetList(_dbContext, organizationId, page, limit, name);
            return new
            {
                projects = records,
                rowNums = ProjectDataservice.GetRowNum(_dbContext, organizationId, name)
            };
        }

        [HttpPost]
        public ActionResult<dynamic> create([FromRoute] long organizationId, [FromBody] DTOs.Project dto, Homo.Bet.Api.DTOs.JwtExtraPayload extraPayload)
        {
            long ownerId = extraPayload.Id;
            Project rewRecord = ProjectDataservice.Create(_dbContext, organizationId, ownerId, ownerId, dto);
            return rewRecord;
        }

        [HttpDelete]
        public ActionResult<dynamic> batchDelete([FromRoute] long organizationId, [FromBody] List<long> ids, Homo.Bet.Api.DTOs.JwtExtraPayload extraPayload)
        {

            long ownerId = extraPayload.Id;
            var projects = ProjectDataservice.GetAll(_dbContext, organizationId, null, ids, true);
            if (projects.Count != ids.Count)
            {
                throw new CustomException(ERROR_CODE.DELETE_NOT_URS_PROJECT, System.Net.HttpStatusCode.BadRequest);
            }
            ProjectDataservice.BatchDelete(_dbContext, ownerId, ownerId, ids);
            return new { status = CUSTOM_RESPONSE.OK };
        }

        [HttpGet]
        [Route("{id}")]
        public ActionResult<dynamic> get([FromRoute] long organizationId, [FromRoute] int id, Homo.Bet.Api.DTOs.JwtExtraPayload extraPayload)
        {
            long ownerId = extraPayload.Id;
            Project record = ProjectDataservice.GetOne(_dbContext, organizationId, id);
            if (record == null)
            {
                throw new CustomException(ERROR_CODE.DATA_NOT_FOUND, System.Net.HttpStatusCode.NotFound);
            }
            if (record.OrganizationId != organizationId)
            {
                throw new CustomException(ERROR_CODE.DATA_NOT_FOUND, System.Net.HttpStatusCode.NotFound);
            }
            return record;
        }

        [HttpPatch]
        [Route("{id}")]
        public ActionResult<dynamic> update([FromRoute] long organizationId, [FromRoute] long id, [FromBody] DTOs.Project dto, Homo.Bet.Api.DTOs.JwtExtraPayload extraPayload)
        {
            long ownerId = extraPayload.Id;
            var project = ProjectDataservice.GetOne(_dbContext, organizationId, id, true);
            if (project.OrganizationId != organizationId)
            {
                throw new CustomException(ERROR_CODE.DATA_NOT_FOUND, System.Net.HttpStatusCode.NotFound);
            }
            ProjectDataservice.Update(_dbContext, id, ownerId, dto);
            return new { status = CUSTOM_RESPONSE.OK };
        }

        [HttpDelete]
        [Route("{id}")]
        public ActionResult<dynamic> delete([FromRoute] long organizationId, [FromRoute] long id, Homo.Bet.Api.DTOs.JwtExtraPayload extraPayload)
        {
            long ownerId = extraPayload.Id;
            var project = ProjectDataservice.GetOne(_dbContext, organizationId, id, true);
            if (project.OrganizationId != organizationId)
            {
                throw new CustomException(ERROR_CODE.DATA_NOT_FOUND, System.Net.HttpStatusCode.NotFound);
            }
            ProjectDataservice.Delete(_dbContext, id, ownerId);
            return new { status = CUSTOM_RESPONSE.OK };
        }
    }
}
