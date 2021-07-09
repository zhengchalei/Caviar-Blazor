﻿using Caviar.Models.SystemData;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Caviar.Control.Permission
{
    /// <summary>
    /// 字段权限
    /// </summary>
    public partial class PermissionController
    {

        /// <summary>
        /// 获取所有model
        /// </summary>
        [HttpGet]
        public IActionResult GetModels(bool isView = false)
        {
            List<ViewModelFields> viewModels = new List<ViewModelFields>();
            var types = CommonHelper.GetModelList(isView);
            foreach (var item in types)
            {
                var displayName = item.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
                viewModels.Add(new ViewModelFields() { TypeName = item.Name, DisplayName = displayName, FullName = item.FullName.Replace("." + item.Name, "") });
            }
            ResultMsg.Data = viewModels;
            return ResultOK();
        }
        /// <summary>
        /// 只能获取自身字段
        /// </summary>
        /// <param name="modelName"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetFields(string modelName)
        {
            if (string.IsNullOrEmpty(modelName)) return ResultError("请输入需要获取的数据名称");
            var result = await _Action.GetFieldsData(CavAssembly, modelName);
            return Ok(result);
        }

        /// <summary>
        /// 获取指定角色字段
        /// </summary>
        /// <param name="modelName"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> RoleFields(string modelName, int roleId)
        {
            if (string.IsNullOrEmpty(modelName)) return ResultError("请输入需要获取的数据名称");
            var result = await _Action.GetFieldsData(CavAssembly, modelName, roleId);
            return Ok(result);
        }
        /// <summary>
        /// 设置角色字段
        /// </summary>
        /// <param name="viewModelFields"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> RoleFields(string fullName, int roleId, List<ViewModelFields> viewModelFields)
        {
            await _Action.SetRoleFields(fullName, roleId, viewModelFields);
            return Ok();
        }
    }
}
