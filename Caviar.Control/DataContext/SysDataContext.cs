﻿using Caviar.Models;
using Caviar.Models.SystemData;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
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

        private DataContext DC => _dataContext;

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
            DC.Entry(entity).State = EntityState.Added;
            if (isSaveChange)
            {
                return await SaveChangesAsync();
            }
            return 0;
        }
        /// <summary>
        /// 批量添加
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="isSaveChange"></param>
        /// <returns></returns>
        public virtual async Task<int> AddEntityAsync<T>(List<T> entity, bool isSaveChange) where T : class, IBaseModel
        {
            var transaction = BeginTransaction();
            foreach (var item in entity)
            {
                await AddEntityAsync(item, false);
            }
            var count = 0;
            if (isSaveChange)
            {
                count = await SaveChangesAsync();
            }
            transaction.Commit();
            return count;
        }
        /// <summary>
        /// 保存所有更改
        /// </summary>
        /// <returns></returns>
        public virtual async Task<int> SaveChangesAsync()
        {
            DC.ChangeTracker.DetectChanges(); // Important!
            DC.ChangeTracker
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
            DC.ChangeTracker
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
            return await DC.SaveChangesAsync();
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
            DC.Entry(entity).State = EntityState.Modified;
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
            DC.Entry(entity).Property(fieldExp).IsModified = true;
            if (isSaveChange)
            {
                return await SaveChangesAsync();
            }
            return 0;
        }
        /// <summary>
        /// 批量修改
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="fieldExp"></param>
        /// <param name="isSaveChange"></param>
        /// <returns></returns>
        public virtual async Task<int> UpdateEntityAsync<T>(List<T> entity, bool isSaveChange = true) where T : class, IBaseModel
        {
            var transaction = BeginTransaction();
            foreach (var item in entity)
            {
                await UpdateEntityAsync(item, false);
            }
            var count = 0;
            if (isSaveChange)
            {
                count = await SaveChangesAsync();
            }
            transaction.Commit();
            return count;
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
                DC.Entry(entity).State = EntityState.Deleted;
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
        /// 批量删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="isSaveChange"></param>
        /// <param name="IsDelete"></param>
        /// <returns></returns>
        public virtual async Task<int> DeleteEntityAsync<T>(List<T> entity, bool isSaveChange, bool IsDelete) where T : class, IBaseModel
        {
            var transaction = BeginTransaction();
            foreach (var item in entity)
            {
                await DeleteEntityAsync(item, false, IsDelete);
            }
            var count = 0;
            if (isSaveChange)
            {
                count = await SaveChangesAsync();
            }
            transaction.Commit();
            return count;
        }
        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual IQueryable<T> GetAllAsync<T>() where T : class, IBaseModel
        {
            var set = DC.Set<T>();
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
        public virtual async Task<PageData<T>> GetPageAsync<T, TKey>(Expression<Func<T, bool>> whereLambda, Expression<Func<T, TKey>> orderBy, int pageIndex, int pageSize, bool isOrder = true, bool isNoTracking = false) where T : class, IBaseModel
        {
            var set = DC.Set<T>();
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
                Rows = await data.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(),
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            return pageData;
        }
        /// <summary>
        /// 获取指定实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public virtual IQueryable<T> GetEntityAsync<T>(Expression<Func<T, bool>> where) where T : class, IBaseModel
        {
            var set = DC.Set<T>();
            return set.Where(u => u.IsDelete == false).Where(where);
        }
        /// <summary>
        /// 根据id获取实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual Task<T> GetEntityAsync<T>(int id) where T : class, IBaseModel
        {
            var set = DC.Set<T>();
            return set.Where(u => u.IsDelete == false).FirstOrDefaultAsync(u => u.Id == id);
        }
        /// <summary>
        /// 根据guid获取实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uid"></param>
        /// <returns></returns>
        public virtual Task<T> GetEntityAsync<T>(Guid uid) where T : class, IBaseModel
        {
            var set = DC.Set<T>();
            return set.Where(u => u.IsDelete == false).FirstOrDefaultAsync(u => u.Uid == uid);
        }

        /// <summary>
        /// 开启事务
        /// </summary>
        /// <returns></returns>
        public virtual IDbContextTransaction BeginTransaction()
        {
            var transaction = DC.Database.BeginTransaction();
            return transaction;
        }
        /// <summary>
        /// 执行sql，请注意参数的检查防止sql注入
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataTable SqlQuery(string sql, params object[] parameters)
        {
            return DC.Database.SqlQuery(sql, parameters);
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
            bool IsExistence = await DC.Database.EnsureCreatedAsync();
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
                    Icon = "appstore-add",
                    UpLayerId = menuManage.Id,
                    IsDoubleTrue = false,
                    Number = "999"
                };
                await AddEntityAsync(AddButton);
                AddButton = new SysPowerMenu()
                {
                    MenuType = MenuType.Button,
                    MenuName = "删除",
                    ButtonPosition = ButtonPosition.Row,
                    Icon = "delete",
                    UpLayerId = menuManage.Id,
                    IsDoubleTrue = true,
                    Number = "999"
                };
                await AddEntityAsync(AddButton);





                SysPowerMenu codePage = new SysPowerMenu()
                {
                    MenuType = MenuType.Menu,
                    TargetType = TargetType.CurrentPage,
                    MenuName = "代码生成",
                    Url = "/Code/Index",
                    Icon = "code",
                    UpLayerId = management.Id,
                    Number = "999"
                };
                await AddEntityAsync(codePage);
            }
            return IsExistence;
        }




    }
}
