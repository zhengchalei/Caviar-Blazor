﻿using Caviar.AntDesignUI.Core;
using Caviar.SharedKernel.Entities;
using Caviar.SharedKernel.Entities.View;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Caviar.AntDesignUI.Pages.UserGroup
{
    public partial class SysUserGroupDataRange : ITableTemplate
    {

        [Parameter]
        public ApplicationRoleView DataSource { get; set; }
        [Inject]
        UserConfig UserConfig { get; set; }
        [Inject]
        HttpService HttpService { get; set; }
        [Parameter]
        public string CurrentUrl {get;set;}

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            UserGroups = await GetMenus();
            if(DataSource.Entity.DataList != null && DataSource.Entity.DataList.Count() > 0)
            {
                var ids = DataSource.Entity.DataList.Split(CurrencyConstant.CustomDataSeparator);
                List<SysUserGroupView> userGroups = new List<SysUserGroupView>();
                UserGroups.TreeToList(userGroups);
                foreach (var item in userGroups)
                {
                    item.IsPermission = ids.Contains(item.Id.ToString());
                }
            }
            
        }

        private List<SysUserGroupView> UserGroups = new List<SysUserGroupView>();


        async Task<List<SysUserGroupView>> GetMenus()
        {
            var result = await HttpService.GetJson<PageData<SysUserGroupView>>($"{UrlConfig.UserGroupIndex}?pageSize={Config.MaxPageSize}");
            if (result.Status != HttpStatusCode.OK) return null;
            return result.Data.Rows;
        }


        public Task<bool> Validate()
        {
            List<SysUserGroupView> userGroups = new List<SysUserGroupView>();
            UserGroups.TreeToList(userGroups);
            var ids = userGroups.Where(u => u.IsPermission).Select(u => u.Id).ToList();
            StringBuilder dataList = new StringBuilder();
            foreach (var item in ids)
            {
                dataList.Append($"{item.ToString()}{CurrencyConstant.CustomDataSeparator}");
            }
            DataSource.Entity.DataList = dataList.ToString();
            return Task.FromResult(true);
        }
    }
}
