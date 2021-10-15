using System;
using System.Collections.Generic;
using System.Linq;

namespace Homo.Bet.Api
{
    public class OrganizationDataservice
    {
        public static List<Organization> GetList(BargainingChipDBContext dbContext, long ownerId, int page, int limit, string name)
        {
            return dbContext.Organization
                .Where(x =>
                    x.DeletedAt == null
                    && x.OwnerId == ownerId
                    && (name == null || x.Name.Contains(name))
                )
                .OrderByDescending(x => x.Id)
                .Skip(limit * (page - 1))
                .Take(limit)
                .ToList();
        }

        public static List<Organization> GetAll(BargainingChipDBContext dbContext, long ownerId, string name)
        {
            return dbContext.Organization
                .Where(x =>
                    x.DeletedAt == null
                    && x.OwnerId == ownerId
                    && (name == null || x.Name.Contains(name))
                )
                .OrderByDescending(x => x.Id)
                .ToList();
        }
        public static int GetRowNum(BargainingChipDBContext dbContext, long ownerId, string name)
        {
            return dbContext.Organization
                .Where(x =>
                    x.DeletedAt == null
                    && x.OwnerId == ownerId
                    && (name == null || x.Name.Contains(name))
                )
                .Count();
        }

        public static Organization GetOne(BargainingChipDBContext dbContext, long ownerId, long id)
        {
            return dbContext.Organization
                .FirstOrDefault(x =>
                x.DeletedAt == null
                && x.OwnerId == ownerId
                && x.Id == id
            );
        }

        public static Organization Create(BargainingChipDBContext dbContext, long ownerId, long createdBy, DTOs.Organization dto)
        {
            Organization record = new Organization();
            foreach (var propOfDTO in dto.GetType().GetProperties())
            {
                var value = propOfDTO.GetValue(dto);
                var prop = record.GetType().GetProperty(propOfDTO.Name);
                prop.SetValue(record, value);
            }
            record.OwnerId = ownerId;
            record.CreatedBy = createdBy;
            record.CreatedAt = DateTime.Now;
            dbContext.Organization.Add(record);
            dbContext.SaveChanges();
            return record;
        }

        public static void BatchDelete(BargainingChipDBContext dbContext, long ownerId, long editedBy, List<long> ids)
        {
            foreach (int id in ids)
            {
                Organization record = new Organization { Id = id, OwnerId = ownerId };
                dbContext.Attach<Organization>(record);
                record.DeletedAt = DateTime.Now;
                record.EditedBy = editedBy;
            }
            dbContext.SaveChanges();
        }

        public static void Update(BargainingChipDBContext dbContext, long ownerId, int id, long editedBy, DTOs.Organization dto)
        {
            Organization record = dbContext.Organization.Where(x => x.Id == id && x.OwnerId == ownerId).FirstOrDefault();
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

        public static void Delete(BargainingChipDBContext dbContext, long ownerId, long id, long editedBy)
        {
            Organization record = dbContext.Organization.Where(x => x.Id == id && x.OwnerId == ownerId).FirstOrDefault();
            record.DeletedAt = DateTime.Now;
            record.EditedBy = editedBy;
            dbContext.SaveChanges();
        }
    }
}
