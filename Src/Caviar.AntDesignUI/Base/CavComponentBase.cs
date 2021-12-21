﻿using AntDesign;
using Caviar.AntDesignUI.Helper;
using Caviar.SharedKernel.Entities.View;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caviar.AntDesignUI
{
    public partial class CavComponentBase : ComponentBase
    {
        #region 属性注入
        /// <summary>
        /// HttpClient
        /// </summary>
        [Inject]
        protected HttpHelper HttpService { get; set; }
        /// <summary>
        /// 全局提示
        /// </summary>
        [Inject]
        protected MessageService MessageService { get; set; }
        /// <summary>
        /// 导航管理器
        /// </summary>
        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        /// <summary>
        /// API组
        /// </summary>
        [Parameter]
        public List<SysMenuView> APIList { get; set; } = new List<SysMenuView>();
        /// <summary>
        /// url读取器
        /// </summary>
        protected UrlAccessor Url { get; set; }
        /// <summary>
        /// 需要获取url的控制器集合
        /// </summary>
        public List<string> ControllerList = new List<string>();
        /// <summary>
        /// 加载等待
        /// </summary>
        public bool Loading = false;
        /// <summary>
        /// 当前url
        /// </summary>
        [Parameter]
        public string CurrentUrl { get; set; }

        [Inject]
        public UserConfig UserConfig { get; set; }
        #endregion

        /// <summary>
        /// 获取API
        /// 获取该页面下的API
        /// </summary>
        /// <returns></returns>
        protected virtual async Task<List<SysMenuView>> GetApiList()
        {
            var splicing = "";
            foreach (var item in ControllerList)
            {
                splicing += item + "|";
            }
            var result = await HttpService.GetJson<List<SysMenuView>>($"{Config.PathList.GetApiList}?url={CurrentUrl}&splicing={splicing}");
            if (result.Status != StatusCodes.Status200OK) return null;
            return result.Data;
        }

        protected override void OnParametersSet()
        {
            UserConfig.RefreshCurrentPage = Refresh;
            base.OnParametersSet();
        }

        protected override async Task OnInitializedAsync()
        {
            Loading = true;
            if (string.IsNullOrEmpty(CurrentUrl))
            {
                CurrentUrl = NavigationManager.Uri.Replace(NavigationManager.BaseUri, "");
            }
            APIList = await GetApiList();
            Url = new UrlAccessor(APIList);
            Loading = false;
            await base.OnInitializedAsync();
        }

        /// <summary>
        /// 刷新
        /// </summary>
        /// <returns></returns>
        public virtual async void Refresh()
        {
            await OnInitializedAsync();
            StateHasChanged();
        }
    }

    public class UrlAccessor
    {
        public UrlAccessor(List<SysMenuView> apiList)
        {
            APIList = apiList;
        }

        public List<SysMenuView> APIList { get; set; }


        public string this[string name] { 
            get 
            {
                var url = APIList?.FirstOrDefault(u => u.Entity.Key.ToLower() == name.ToLower())?.Entity.Url;
                return url; 
            } 
        }

        public string this[string name, string controller]
        {
            get
            {
                var url = APIList?.SingleOrDefault(u => u.Entity.Key.ToLower() == name.ToLower() && u.Entity.ControllerName.ToLower() == controller.ToLower())?.Entity.Url;
                return url;
            }
        }
    }
}
