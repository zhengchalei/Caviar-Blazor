﻿using Caviar.AntDesignPages.Helper;
using Caviar.Models.SystemData;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caviar.AntDesignPages.Pages.Menu
{
    public partial class Update : ITableTemplate
    {
        [Inject]
        HttpHelper Http { get; set; }
        [Parameter]
        public int Id { get; set; }
        /// <summary>
        /// 数据源
        /// </summary>
        public ViewMenu DataSource { get; set; }
        /// <summary>
        /// 模板
        /// </summary>
        DataTemplate TemplateRef { get; set; }
        /// <summary>
        /// 数据初始化
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            var result = await Http.GetJson<ViewMenu>("Menu/GetEntity?Id=" + Id);
            if (result.Status != 200) return;
            DataSource = result.Data;
        }
        /// <summary>
        /// 数据提交
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Submit()
        {
            return await TemplateRef.Submit();
        }
    }
}
