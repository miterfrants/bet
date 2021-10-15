using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Homo.Bet.Api
{
    public class ProjectDataservice
    {
        public static List<Project> GetList(BargainingChipDBContext dbContext, long organizationId, int page, int limit, string name)
        {
            return dbContext.Project
                .Where(x =>
                    x.DeletedAt == null
                    && x.OrganizationId == organizationId
                    && (name == null || x.Name.Contains(name))
                )
                .OrderByDescending(x => x.Id)
                .Skip(limit * (page - 1))
                .Take(limit)
                .ToList();
        }

        public static List<Project> GetAll(BargainingChipDBContext dbContext, long organizationId, string name, List<long> ids, bool asNoTracking = false)
        {
            IQueryable<Project> dbSet;
            if (asNoTracking)
            {
                dbSet = dbContext.Project.AsNoTracking();
            }
            else
            {
                dbSet = dbContext.Project;
            }
            return dbSet
                .Where(x =>
                    x.DeletedAt == null
                    && x.OrganizationId == organizationId
                    && (ids == null || ids.Contains(x.Id))
                    && (name == null || x.Name.Contains(name))
                )
                .OrderByDescending(x => x.Id)
                .ToList();
        }

        public static int GetRowNum(BargainingChipDBContext dbContext, long organizationId, string name)
        {
            return dbContext.Project
                .Where(x =>
                    x.DeletedAt == null
                    && x.OrganizationId == organizationId
                    && (name == null || x.Name.Contains(name))
                )
                .Count();
        }

        public static Project GetOne(BargainingChipDBContext dbContext, long organizationId, long id, bool asNoTracking = false)
        {
            IQueryable<Project> dbSet;
            if (asNoTracking)
            {
                dbSet = dbContext.Project.AsNoTracking();
            }
            else
            {
                dbSet = dbContext.Project;
            }
            return dbSet
                .FirstOrDefault(x =>
                    x.DeletedAt == null
                    && x.OrganizationId == organizationId
                    && x.Id == id
                );
        }

        public static Project Create(BargainingChipDBContext dbContext, long organizationId, long ownerId, long createdBy, DTOs.Project dto)
        {
            Project record = new Project();
            foreach (var propOfDTO in dto.GetType().GetProperties())
            {
                var value = propOfDTO.GetValue(dto);
                var prop = record.GetType().GetProperty(propOfDTO.Name);
                prop.SetValue(record, value);
            }
            record.OrganizationId = organizationId;
            record.OwnerId = ownerId;
            record.CreatedBy = createdBy;
            record.CreatedAt = DateTime.Now;
            dbContext.Project.Add(record);
            dbContext.SaveChanges();
            return record;
        }

        public static void BatchDelete(BargainingChipDBContext dbContext, long ownerId, long editedBy, List<long> ids)
        {
            foreach (long id in ids)
            {
                Project record = new Project { Id = id, OwnerId = ownerId };
                dbContext.Attach<Project>(record);
                record.DeletedAt = DateTime.Now;
                record.EditedBy = editedBy;
            }
            dbContext.SaveChanges();
        }

        public static void Update(BargainingChipDBContext dbContext, long id, long editedBy, DTOs.Project dto)
        {
            Project record = new Project { Id = id };
            dbContext.Attach<Project>(record);
            foreach (var propOfDTO in dto.GetType().GetProperties())
            {
                var value = propOfDTO.GetValue(dto);
                var prop = record.GetType().GetProperty(propOfDTO.Name);
                prop.SetValue(record, value);
            }
            record.EditedAt = DateTime.Now;
            record.EditedBy = editedBy;
            dbContext.SaveChanges();
        }

        public static void Delete(BargainingChipDBContext dbContext, long id, long editedBy)
        {
            Project record = new Project { Id = id };
            dbContext.Attach<Project>(record);
            record.DeletedAt = DateTime.Now;
            record.EditedBy = editedBy;
            dbContext.SaveChanges();
        }
    }
}
