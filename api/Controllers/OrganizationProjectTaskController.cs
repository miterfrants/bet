using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Homo.Core;

namespace Homo.Bet.Api
{
    [AuthorizeFactory]
    [BelongOrgFactory]
    [Route("v1/organizations/{organizationId}/projects/{projectId}/tasks")]
    public class TaskController : ControllerBase
    {
        private readonly BargainingChipDBContext _dbContext;
        public TaskController(BargainingChipDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public ActionResult<dynamic> getList([FromRoute] long organizationId, [FromRoute] long projectId, [FromQuery] string name, [FromQuery] int limit, [FromQuery] int page, Homo.Bet.Api.DTOs.JwtExtraPayload extraPayload)
        {
            long ownerId = extraPayload.Id;
            List<Task> records = TaskDataservice.GetList(_dbContext, organizationId, projectId, page, limit, name);
            return new
            {
                tasks = records,
                rowNums = TaskDataservice.GetRowNum(_dbContext, organizationId, projectId, name)
            };
        }

        [HttpPost]
        public ActionResult<dynamic> create([FromRoute] long organizationId, [FromRoute] long projectId, [FromBody] DTOs.Task dto, Homo.Bet.Api.DTOs.JwtExtraPayload extraPayload)
        {
            long ownerId = extraPayload.Id;
            Task task = TaskDataservice.Create(_dbContext, projectId, ownerId, dto);
            return new { Id = task.Id, Qty = 0 };
        }

        [HttpGet]
        [Route("all")]
        public ActionResult<dynamic> getAll([FromRoute] long organizationId, [FromRoute] long projectId, [FromQuery] string name, [FromQuery] int limit, [FromQuery] int page, [FromQuery] string externalIds, Homo.Bet.Api.DTOs.JwtExtraPayload extraPayload)
        {
            long ownerId = extraPayload.Id;
            List<string> listOfExternalId = externalIds.Split(",").ToList<string>();
            return TaskDataservice.GetAll(_dbContext, organizationId, projectId, null, null, listOfExternalId);
        }

        [HttpGet]
        [Route("by-external-id/{extId}")]
        public ActionResult<dynamic> getOneByExternalId([FromRoute] long organizationId, [FromRoute] long projectId, [FromRoute] string extId, Homo.Bet.Api.DTOs.JwtExtraPayload extraPayload)
        {
            var task = TaskDataservice.GetOneByExternalId(_dbContext, projectId, extId);
            if (task == null)
            {
                throw new Homo.Core.Constants.CustomException(ERROR_CODE.DATA_NOT_FOUND, System.Net.HttpStatusCode.NotFound);
            }
            int totalQty = System.Math.Abs(CoinsLogDataService.GetTaskBetCoins(_dbContext, task.Id));
            int ownerFreeBet = System.Math.Abs(CoinsLogDataService.GetTaskOwnerFreeBetCoins(_dbContext, task.Id, extraPayload.Id));
            int ownerLockedBet = System.Math.Abs(CoinsLogDataService.GetTaskOwnerFreeBetCoins(_dbContext, task.Id, extraPayload.Id));
            CoinLog log = CoinsLogDataService.GetFreeOneByTaskIdAndOwnerId(_dbContext, task.Id, extraPayload.Id);

            return new
            {
                Id = task.Id,
                excludeOwnerBet = totalQty - ownerFreeBet - ownerLockedBet,
                ownerLockedBet = ownerLockedBet,
                ownerFreeBet = ownerFreeBet,
                assigneeId = task.AssigneeId,
                assignee = task.Assignee,
                expectedFinishAt = task.ExpectedFinishAt,
                currentCoinLogId = log == null ? 0 : log.Id,
                status = task.Status
            };
        }

        [HttpPost]
        [Route("{id}/update-current-coin-log")]
        public ActionResult<dynamic> updateCurrentCoinLogs([FromRoute] long organizationId, [FromRoute] long projectId, [FromRoute] long id, Homo.Bet.Api.DTOs.JwtExtraPayload extraPayload, [FromBody] DTOs.CoinLog dto)
        {
            var task = TaskDataservice.GetOne(_dbContext, organizationId, projectId, id);
            if (task == null)
            {
                throw new Homo.Core.Constants.CustomException(ERROR_CODE.DATA_NOT_FOUND, System.Net.HttpStatusCode.NotFound);
            }
            // first one bet give author of issue one coin
            int taskBetCoins = CoinsLogDataService.GetTaskBetCoins(_dbContext, task.Id);
            List<CoinLog> logs = CoinsLogDataService.GetAll(_dbContext, task.Id, task.CreatedBy, COIN_LOG_TYPE.EARN);
            if (taskBetCoins == 0 && logs.Count == 0)
            {
                CoinsLogDataService.Create(_dbContext, task.CreatedBy, task.Id, extraPayload.Id, COIN_LOG_TYPE.EARN, new DTOs.CoinLog() { Qty = 1 });
            }

            CoinLog log = CoinsLogDataService.GetFreeOneByTaskIdAndOwnerId(_dbContext, task.Id, extraPayload.Id, false);
            if (log == null)
            {
                log = CoinsLogDataService.Create(_dbContext, extraPayload.Id, task.Id, extraPayload.Id, COIN_LOG_TYPE.BET, dto);
            }
            else
            {
                if (task.AssigneeId != null && System.Math.Abs(dto.Qty) < System.Math.Abs(log.Qty))
                {
                    throw new Homo.Core.Constants.CustomException(ERROR_CODE.TASK_HAS_CLAIMED, System.Net.HttpStatusCode.Forbidden);
                }
                CoinsLogDataService.Update(_dbContext, log, extraPayload.Id, dto);
            }
            return new { status = Homo.Core.Constants.CUSTOM_RESPONSE.OK };
        }

        [HttpPost]
        [Route("{id}/assign")]
        public ActionResult<dynamic> assign([FromRoute] long organizationId, [FromRoute] long projectId, [FromRoute] long id, Homo.Bet.Api.DTOs.JwtExtraPayload extraPayload, [FromBody] DTOs.TaskWorkDays dto)
        {
            var task = TaskDataservice.GetOne(_dbContext, organizationId, projectId, id);
            if (task == null)
            {
                throw new Homo.Core.Constants.CustomException(ERROR_CODE.DATA_NOT_FOUND, System.Net.HttpStatusCode.NotFound);
            }

            if (task.AssigneeId != null)
            {
                throw new Homo.Core.Constants.CustomException(ERROR_CODE.TASK_HAS_CLAIMED, System.Net.HttpStatusCode.NotFound);
            }
            TaskDataservice.Assign(_dbContext, task, extraPayload.Id, dto.WorkDays);
            return new { status = Homo.Core.Constants.CUSTOM_RESPONSE.OK };
        }

        [HttpPost]
        [Route("{id}/mark-finish")]
        public ActionResult<dynamic> markFinish([FromRoute] long organizationId, [FromRoute] long projectId, [FromRoute] long id, Homo.Bet.Api.DTOs.JwtExtraPayload extraPayload)
        {
            var task = TaskDataservice.GetOne(_dbContext, organizationId, projectId, id);
            if (task == null)
            {
                throw new Homo.Core.Constants.CustomException(ERROR_CODE.DATA_NOT_FOUND, System.Net.HttpStatusCode.NotFound);
            }

            if (task.AssigneeId != extraPayload.Id)
            {
                throw new Homo.Core.Constants.CustomException(ERROR_CODE.TASK_HAS_CLAIMED, System.Net.HttpStatusCode.NotFound);
            }
            TaskDataservice.MarkFinish(_dbContext, task);
            return new { status = Homo.Core.Constants.CUSTOM_RESPONSE.OK };
        }

        [HttpPost]
        [Route("{id}/done")]
        public ActionResult<dynamic> done([FromRoute] long organizationId, [FromRoute] long projectId, [FromRoute] long id, Homo.Bet.Api.DTOs.JwtExtraPayload extraPayload)
        {
            var task = TaskDataservice.GetOne(_dbContext, organizationId, projectId, id);
            if (task == null)
            {
                throw new Homo.Core.Constants.CustomException(ERROR_CODE.DATA_NOT_FOUND, System.Net.HttpStatusCode.NotFound);
            }
            if (task.Status != TASK_STATUS.BE_MARK_FINSIH)
            {
                throw new Homo.Core.Constants.CustomException(ERROR_CODE.TASK_STATUS_ERROR, System.Net.HttpStatusCode.NotFound);
            }

            if (task.AssigneeId == extraPayload.Id)
            {
                throw new Homo.Core.Constants.CustomException(ERROR_CODE.ASSIGNEE_NOT_ALLOW_APPROVE, System.Net.HttpStatusCode.NotFound);
            }
            TaskDataservice.Done(_dbContext, extraPayload.Id, task);
            int coins = CoinsLogDataService.GetTaskBetCoins(_dbContext, task.Id);
            CoinsLogDataService.Create(_dbContext, task.AssigneeId.GetValueOrDefault(), task.Id, extraPayload.Id, COIN_LOG_TYPE.EARN, new DTOs.CoinLog() { Qty = -coins });
            int bonus = (int)System.Math.Ceiling(((decimal)coins / (decimal)5));
            CoinsLogDataService.Create(_dbContext, extraPayload.Id, task.Id, extraPayload.Id, COIN_LOG_TYPE.EARN, new DTOs.CoinLog() { Qty = -bonus });
            return new { status = Homo.Core.Constants.CUSTOM_RESPONSE.OK };
        }
    }
}
