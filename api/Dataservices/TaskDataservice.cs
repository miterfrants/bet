using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Homo.Bet.Api
{
    public class TaskDataservice
    {
        public static List<Task> GetList(BargainingChipDBContext dbContext, long organizationId, long projectId, int page, int limit, string name)
        {
            return dbContext.Task
                .Where(x =>
                    x.DeletedAt == null
                    && x.ProjectId == projectId
                    && x.Project.OrganizationId == organizationId
                    && (name == null || x.Name.Contains(name))
                )
                .OrderByDescending(x => x.Id)
                .Skip(limit * (page - 1))
                .Take(limit)
                .ToList();
        }

        public static List<Task> GetAll(BargainingChipDBContext dbContext, long organizationId, long projectId, string name, List<long> ids, List<string> externalIds)
        {
            return dbContext.Task
                .Include(item => item.Assignee)
                .Where(x =>
                    x.DeletedAt == null
                    && x.ProjectId == projectId
                    && x.Project.OrganizationId == organizationId
                    && (name == null || x.Name.Contains(name))
                    && (ids == null || ids.Contains(x.Id))
                    && (externalIds == null || externalIds.Contains(x.ExternalId))
                )
                .OrderByDescending(x => x.Id)
                .ToList();
        }

        public static List<Task> GetBeingExpiredTask(BargainingChipDBContext dbContext, int days)
        {
            return dbContext.Task
                .Where(x =>
                    x.DeletedAt == null
                    && x.ExpectedFinishAt.GetValueOrDefault().AddDays(days) <= DateTime.Now
                )
                .OrderByDescending(x => x.Id)
                .ToList();
        }

        public static int GetRowNum(BargainingChipDBContext dbContext, long organizationId, long projectId, string name)
        {
            return dbContext.Task
                .Where(x =>
                    x.DeletedAt == null
                    && x.ProjectId == projectId
                    && x.Project.OrganizationId == organizationId
                    && (name == null || x.Name.Contains(name))
                )
                .Count();
        }

        public static Task GetOne(BargainingChipDBContext dbContext, long organizationId, long projectId, long id)
        {
            return dbContext.Task
            .FirstOrDefault(x =>
                x.DeletedAt == null
                && x.ProjectId == projectId
                && x.Project.OrganizationId == organizationId
                && x.Id == id
            );
        }

        public static Task Create(BargainingChipDBContext dbContext, long projectId, long createdBy, DTOs.Task dto)
        {
            Task record = new Task();
            foreach (var propOfDTO in dto.GetType().GetProperties())
            {
                var value = propOfDTO.GetValue(dto);
                var prop = record.GetType().GetProperty(propOfDTO.Name);
                prop.SetValue(record, value);
            }
            record.ProjectId = projectId;
            record.CreatedBy = createdBy;
            record.CreatedAt = DateTime.Now;
            dbContext.Task.Add(record);
            dbContext.SaveChanges();
            return record;
        }

        public static void BatchDelete(BargainingChipDBContext dbContext, long editedBy, List<long> ids)
        {
            foreach (long id in ids)
            {
                Task record = new Task { Id = id };
                dbContext.Attach<Task>(record);
                record.DeletedAt = DateTime.Now;
                record.EditedBy = editedBy;
            }
            dbContext.SaveChanges();
        }

        public static void Delete(BargainingChipDBContext dbContext, long id, long editedBy)
        {
            Task record = dbContext.Task
                .Where(x =>
                    x.Id == id
                )
                .FirstOrDefault();
            record.DeletedAt = DateTime.Now;
            record.EditedBy = editedBy;
            dbContext.SaveChanges();
        }

        public static Task GetOneByExternalId(BargainingChipDBContext dbContext, long projectId, string externalId)
        {
            Task record = dbContext.Task
                .Where(x =>
                    x.DeletedAt == null
                    && x.ProjectId == projectId
                    && x.ExternalId == externalId
                )
                .Include(x => x.Assignee)
                .FirstOrDefault();
            return record;
        }

        public static void Assign(BargainingChipDBContext dbContext, Task task, long assigneeId, int WorkDays)
        {
            task.AssigneeId = assigneeId;
            task.ExpectedFinishAt = DateTime.Now.AddDays(WorkDays);
            dbContext.SaveChanges();
        }

        public static void MarkFinish(BargainingChipDBContext dbContext, Task task)
        {
            task.Status = TASK_STATUS.BE_MARK_FINSIH;
            task.MarkFinishAt = DateTime.Now;
            dbContext.SaveChanges();
        }

        public static void Done(BargainingChipDBContext dbContext, long approverId, Task task)
        {
            task.Status = TASK_STATUS.DONE;
            task.EditedAt = DateTime.Now;
            task.EditedBy = approverId;
            dbContext.SaveChanges();
        }
    }
}
