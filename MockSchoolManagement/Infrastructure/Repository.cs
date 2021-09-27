﻿using Microsoft.EntityFrameworkCore;
using MockSchoolManagement.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MockSchoolManagement.Infrastructure
{
    public class Repository<TEntity, TPrimaryKey> : IRepository<TEntity, TPrimaryKey>
        where TEntity:class
    {
        protected readonly AppDbContext _dbContext;
        public virtual DbSet<TEntity> Table => _dbContext.Set<TEntity>();
        public Repository(AppDbContext appDbContext)
        {
            _dbContext = appDbContext;
        }

        #region 查询

        public IQueryable<TEntity> GetAll()
        {
            return Table.AsQueryable();
        }
        public List<TEntity> GetAllList()
        {
            return Table.ToList();
        }
        public async Task<List<TEntity>> GetAllListAsync()
        {
            return await GetAll().ToListAsync();
        }

        public List<TEntity> GetAllList(Expression<Func<TEntity, bool>> predicate)
        {
            return GetAll().Where(predicate).ToList();
        }

        public async Task<List<TEntity>> GetAllListAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await GetAll().Where(predicate).ToListAsync();
        }

        public TEntity Single(Expression<Func<TEntity, bool>> predicate)
        {
            return GetAll().Single(predicate);
        }
        public async Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await GetAll().SingleAsync(predicate);
        }

        public TEntity FirstOrDefalult(Expression<Func<TEntity, bool>> predicate)
        {
            return GetAll().FirstOrDefault(predicate);
        }
        public async Task<TEntity> FirstOrDefalultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await GetAll().FirstOrDefaultAsync(predicate);
        }

        #endregion



        #region 插入
        public TEntity Insert(TEntity entity)
        {
            var newEntity = Table.Add(entity).Entity;
            this.Save();
            return newEntity;
        }

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            var newEntity =await Table.AddAsync(entity);
            await this.SaveAsync();
            return newEntity.Entity;
        }
        #endregion

        #region 更新
        public TEntity Update(TEntity entity)
        {
            AttachIfNot(entity);
            _dbContext.Entry(entity).State = EntityState.Modified;
            this.Save();
            return entity;
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            AttachIfNot(entity);
            _dbContext.Entry(entity).State = EntityState.Modified;
            await this.SaveAsync();
            return entity;
        }
        #endregion

        #region 删除
        public void Delete(TEntity entity)
        {
            AttachIfNot(entity);
            Table.Remove(entity);
            this.Save();
        }

        public async Task DeleteAsync(TEntity entity)
        {
            AttachIfNot(entity);
            Table.Remove(entity);
            await this.SaveAsync();
        }

        public void Delete(Expression<Func<TEntity, bool>> predicate)
        {
            foreach (var entity in GetAll().Where(predicate).ToList())
            {
                Delete(entity);
            }
        }

        public async Task DeleteAsync(Expression<Func<TEntity, bool>> predicate)
        {
            foreach (var entity in GetAll().Where(predicate).ToList())
            {
                await DeleteAsync(entity);
            }
        }
        #endregion

        #region 统计
        public int Count()
        {
            return GetAll().Count();
        }
        public async Task<int> CountAsync()
        {
            return await GetAll().CountAsync();
        }
        public int Count(Expression<Func<TEntity, bool>> predicate)
        {
            return GetAll().Where(predicate).Count();
        }
        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await GetAll().Where(predicate).CountAsync();
        }

        public long LongCount()
        {
            return GetAll().LongCount();
        }
        public async Task<long> LongCountAsync()
        {
            return await GetAll().LongCountAsync();
        }
        public long LongCount(Expression<Func<TEntity, bool>> predicate)
        {
            return GetAll().Where(predicate).LongCount();
        }
        public async Task<long> LongCountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await GetAll().Where(predicate).LongCountAsync();
        }
        #endregion

        #region 工具类方法
        /// <summary>
        /// 检查实体是否处于跟踪状态，如果是则返回；如果不是则添加跟踪状态
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void AttachIfNot(TEntity entity)
        {
            var entry = _dbContext.ChangeTracker.Entries()
                .FirstOrDefault(ent => ent.Entity == entity);
            if (entity!=null)
            {
                return;
            }
            Table.Attach(entity);
        }
        protected void Save()
        {
            _dbContext.SaveChanges();
        }
        protected async Task SaveAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}