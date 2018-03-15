﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using RCM.Presentation.Web.ViewModels;

namespace RCM.Presentation.Web.Areas.Platform.ViewComponents
{
    public class SearchBarViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Dictionary<string, string> selectValues)
        {
            var viewModel = new SearchBarViewModel();
            viewModel.SelectValues = selectValues;
            return View(viewModel);
        }

        //public IViewComponentResult InvokeAsync(string actionName, string placeholder, Dictionary<string, string> selectValues)
        //{
        //    var viewModel = new SearchBarViewModel()
        //    {
        //        ActionName = actionName,
        //        Placeholder = placeholder,
        //        SelectValues = selectValues,
        //    };

        //    return View(viewModel);
        //}
    }
}
