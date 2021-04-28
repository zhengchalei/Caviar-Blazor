﻿using Caviar.Models;
using Caviar.Models.SystemData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Caviar.Control
{
    public partial class SysDataContext : IDataContext
    {

        public SysDataContext(DataContext dataContext,BaseControllerModel baseControllerModel)
        {
            _dataContext = dataContext;
            _baseControllerModel = baseControllerModel;
            if (IsDataInit)//判断数据库是否初始化
            {
                IsDataInit = DataInit().Result;
            }
        }
        DataContext _dataContext;

        private DataContext Base_DataContext => _dataContext;

        IBaseControllerModel _baseControllerModel;

        /// <summary>
        /// 添加实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="isSaveChange">默认为立刻保存</param>
        /// <returns></returns>
        public virtual async Task<int> AddEntityAsync<T>(T entity, bool isSaveChange = true) where T : class, IBaseModel
        {
            Base_DataContext.Entry(entity).State = EntityState.Added;
            if (isSaveChange)
            {
                return await SaveChangesAsync();
            }
            return 0;
        }
        /// <summary>
        /// 添加多个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <param name="isSaveChange">默认为立刻保存</param>
        /// <returns></returns>
        public virtual async Task<int> AddRangeAsync<T>(List<T> entities, bool isSaveChange = true) where T : class, IBaseModel
        {
            var set = Base_DataContext.Set<T>();
            await set.AddRangeAsync(entities);
            if (isSaveChange)
            {
                return await SaveChangesAsync();
            }
            return 0;
        }
        /// <summary>
        /// 保存所有更改
        /// </summary>
        /// <returns></returns>
        public virtual async Task<int> SaveChangesAsync()
        {
            Base_DataContext.ChangeTracker.DetectChanges(); // Important!
            Base_DataContext.ChangeTracker
                .Entries()
                .Where(u => u.State == EntityState.Modified)
                .Select(u => u.Entity)
                .ToList()
                .ForEach(u =>
                {
                    var baseEntity = u as IBaseModel;
                    if (baseEntity != null)
                    {
                        baseEntity.UpdateTime = DateTime.Now;
                        baseEntity.OperatorUp = _baseControllerModel.UserName;
                    }
                });
            Base_DataContext.ChangeTracker
                .Entries()
                .Where(u => u.State == EntityState.Added)
                .Select(u => u.Entity)
                .ToList()
                .ForEach(u =>
                {
                    var baseEntity = u as IBaseModel;
                    if (baseEntity != null)
                    {
                        baseEntity.CreatTime = DateTime.Now;
                        baseEntity.OperatorCare = _baseControllerModel.UserName;
                    }
                });
            return await Base_DataContext.SaveChangesAsync();
        }
        /// <summary>
        /// 修改指定实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="isSaveChange">默认为立刻保存</param>
        /// <returns></returns>
        public virtual async Task<int> UpdateEntityAsync<T>(T entity, bool isSaveChange = true) where T : class, IBaseModel
        {
            Base_DataContext.Entry(entity).State = EntityState.Modified;
            if (isSaveChange)
            {
                return await SaveChangesAsync();
            }
            return 0;
        }
        /// <summary>
        /// 修改部分实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="fieldExp"></param>
        /// <param name="isSaveChange">默认为立刻保存</param>
        /// <returns></returns>
        public virtual async Task<int> UpdateEntityAsync<T>(T entity, Expression<Func<T, object>> fieldExp, bool isSaveChange = true) where T : class, IBaseModel
        {
            Base_DataContext.Entry(entity).Property(fieldExp).IsModified = true;
            if (isSaveChange)
            {
                return await SaveChangesAsync();
            }
            return 0;
        }
        /// <summary>
        /// 删除实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="isSaveChange">默认立刻保存</param>
        /// <param name="IsDelete">是否立刻删除，默认不删除，只修改IsDelete,设为true则立刻删除</param>
        /// <returns></returns>
        public virtual async Task<int> DeleteEntityAsync<T>(T entity, bool isSaveChange = true, bool IsDelete = false) where T : class, IBaseModel
        {
            if (entity.IsDelete || IsDelete)
            {
                var set = Base_DataContext.Set<T>();
                set.Remove(entity);
                if (isSaveChange)
                {
                    return await SaveChangesAsync();
                }
            }
            else
            {
                entity.IsDelete = true;
                return await UpdateEntityAsync(entity,isSaveChange);
            }
            return 0;
        }
        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual IQueryable<T> GetAllAsync<T>() where T : class, IBaseModel
        {
            var set = Base_DataContext.Set<T>();
            return set.Where(u => u.IsDelete == false);
        }
        /// <summary>
        /// 获取指定页数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="whereLambda"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="isOrder"></param>
        /// <param name="isNoTracking"></param>
        /// <returns></returns>
        public virtual async Task<PageData<T>> GetPageAsync<T, TKey>(Expression<Func<T, bool>> whereLambda, Expression<Func<T, TKey>> orderBy, int pageIndex, int pageSize, bool isOrder = true, bool isNoTracking = true) where T : class, IBaseModel
        {
            var set = Base_DataContext.Set<T>();
            IQueryable<T> data = isOrder ?
                set.OrderBy(orderBy) :
                set.OrderByDescending(orderBy);
            data.Where(u => u.IsDelete == false);
            if (whereLambda != null)
            {
                data = isNoTracking ? data.Where(whereLambda).AsNoTracking() : data.Where(whereLambda);
            }
            PageData<T> pageData = new PageData<T>
            {
                Total = await data.CountAsync(),
                Rows = await data.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync()
            };
            return pageData;
        }
        /// <summary>
        /// 获取指定实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public IQueryable<T> GetEntityAsync<T>(Expression<Func<T, bool>> where) where T : class, IBaseModel
        {
            var set = Base_DataContext.Set<T>();
            return set.Where(u => u.IsDelete == false).Where(where);
        }
        /// <summary>
        /// 根据id获取实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<T> GetEntityAsync<T>(int id) where T : class, IBaseModel
        {
            var set = Base_DataContext.Set<T>();
            return set.Where(u => u.IsDelete == false).FirstOrDefaultAsync(u => u.Id == id);
        }
        /// <summary>
        /// 根据guid获取实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uid"></param>
        /// <returns></returns>
        public Task<T> GetEntityAsync<T>(Guid uid) where T : class, IBaseModel
        {
            var set = Base_DataContext.Set<T>();
            return set.Where(u => u.IsDelete == false).FirstOrDefaultAsync(u => u.Uid == uid);
        }

        /// <summary>
        /// 开启事务
        /// </summary>
        /// <returns></returns>
        public IDbContextTransaction BeginTransaction()
        {
            var transaction = Base_DataContext.Database.BeginTransaction();
            return transaction;
        }


        static bool IsDataInit = true;
        /// <summary>
        /// 数据初始化
        /// </summary>
        /// <param name="allModules"></param>
        /// <param name="IsSpa"></param>
        /// <returns>返回true表示需要进行初始化数据操作，返回false即数据库已经存在或不需要初始化数据</returns>
        public virtual async Task<bool> DataInit()
        {
            bool IsExistence = await Base_DataContext.Database.EnsureCreatedAsync();
            if (IsExistence)
            {
                //创建初始角色
                SysUserLogin Login = new SysUserLogin()
                {
                    UserName = "admin",
                    Password = CommonHelper.SHA256EncryptString("123456"),
                    PhoneNumber = "11111111111",
                };
                await AddEntityAsync(Login);
                //创建基础角色
                var NoLoginRole = new SysRole
                {
                    RoleName = "未登录角色",
                    Uid = CaviarConfig.NoLoginRoleGuid
                };
                await AddEntityAsync(NoLoginRole);
                var role = new SysRole()
                {
                    RoleName = "系统管理员",
                    Uid = CaviarConfig.SysAdminRoleGuid
                };
                await AddEntityAsync(role);
                //默认角色加入管理员角色
                SysRoleLogin sysRoleLogin = new SysRoleLogin()
                {
                    RoleId = role.Id,
                    UserId = Login.Id
                };
                await AddEntityAsync(sysRoleLogin);
                //创建基础访问页面
                SysPowerMenu homePage = new SysPowerMenu()
                {
                    MenuType = MenuType.Menu,
                    TargetType = TargetType.CurrentPage,
                    MenuName = "首页",
                    Icon ="home",
                    Url = "/"
                };
                await AddEntityAsync(homePage);
                //创建基础菜单
                SysPowerMenu management = new SysPowerMenu()
                {
                    MenuType = MenuType.Catalog,
                    TargetType = TargetType.CurrentPage,
                    MenuName = "系统管理",
                    Icon = "windows",
                    Number = "999"
                };
                await AddEntityAsync(management);
                SysPowerMenu menuManage = new SysPowerMenu()
                {
                    MenuType = MenuType.Menu,
                    TargetType = TargetType.CurrentPage,
                    MenuName = "菜单管理",
                    Url = "/Menu/Index",
                    Icon = "menu",
                    UpLayerId = management.Id,
                    Number = "999"
                };
                await AddEntityAsync(menuManage);
                SysPowerMenu AddButton = new SysPowerMenu()
                {
                    MenuType = MenuType.Button,
                    TargetType = TargetType.EjectPage,
                    MenuName = "新增",
                    ButtonPosition = ButtonPosition.Header,
                    Url = "/Menu/Add",
                    Icon = "menu",
                    UpLayerId = menuManage.Id,
                    IsDoubleTrue = false,
                    Number = "999"
                };
                await AddEntityAsync(AddButton);
            }
            return IsExistence;
        }
    }
}
