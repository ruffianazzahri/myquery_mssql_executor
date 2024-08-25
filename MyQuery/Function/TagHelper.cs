﻿using Microsoft.AspNetCore.Mvc.Rendering;

namespace Synergy_Test.Function
{
    public static class TagHelper
    {
        public static string IsActive(this IHtmlHelper helper, string controller, string action)
        {
            ViewContext context = helper.ViewContext;

            RouteValueDictionary values = context.RouteData.Values;

            string _controller = values["controller"].ToString();

            string _action = values["action"].ToString();

            if ((action == _action) && (controller == _controller))
            {
                return "active";
            }
            else
            {
                return "";
            }
        }

        public static string IsMenuopen(this IHtmlHelper helper, string controller, string action)
        {
            ViewContext context = helper.ViewContext;

            RouteValueDictionary values = context.RouteData.Values;
            string _controller = values["controller"].ToString();

            string _action = values["action"].ToString();
            if ((action == _action) && (controller == _controller))
            {
                return "menu-open";
            }
            else
            {
                return "";
            }
        }
    }
}
