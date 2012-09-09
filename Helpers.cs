using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;
using System.Linq.Expressions;
using System.Web.Mvc.Html;
using System.ComponentModel;
using System.Globalization;
using System.Web.Mvc.Ajax;
using MiBu.Web.Security;
using Newtonsoft.Json;
using System.Security.Principal;

namespace MiBu.Web.Helpers
{
    public static class Helpers
    {
        public enum NotificationType
        {
            success,
            alert,
            warning
        }

        public static MvcHtmlString Json<TModel, TObject>(this HtmlHelper<TModel> html, TObject obj)
        {
            return MvcHtmlString.Create(JsonConvert.SerializeObject(obj));
        }

        #region User extension methods

        public static bool HasAnyRole(this IPrincipal user, params string[] roles)
        {
            return roles.Any(user.IsInRole);
        }

        /// <summary>
        /// Check if user has any of given roles.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roles">Roles as string, separated with ","</param>
        /// <returns></returns>
        public static bool HasAnyRoleString(this IPrincipal user, string roles)
        {
            var rolesArray = roles.Split(',');
            return rolesArray.Any(user.IsInRole);
        }

        /// <summary>
        /// Extension for controllers to get current user ID. In controller just type this.UserID()
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static int UserID(this Controller controller)
        {
            return ((Security.Identity)controller.User.Identity).ID;
        }

        #endregion

        #region action links

        /// <summary>
        /// Renders actionlinks (for nav menu) and sets class active to currently active link.
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="linkText"></param>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        /// <returns></returns>
        public static MvcHtmlString MenuLink(this HtmlHelper helper,
        string linkText, string actionName,
        string controllerName)
        {
            string currentAction = helper.ViewContext.RouteData.GetRequiredString("action");
            string currentController = helper.ViewContext.RouteData.GetRequiredString("controller");

            TagBuilder builder = new TagBuilder("li");
            string actionLink;
            //Note: Current if statement must match controller and action from link to set it active, change  it to commented if you only need controller to match
            //if (controllerName == currentController)
            if (actionName == currentAction && controllerName == currentController)
            
            {
                builder.AddCssClass("active");
                actionLink = helper.ActionLink(
                    linkText,
                    actionName,
                    controllerName,
                    null,
                    new
                    {
                        @class = "active"
                    }).ToString();
            }
            actionLink = helper.ActionLink(linkText, actionName, controllerName).ToString();

            builder.InnerHtml = actionLink;
            return MvcHtmlString.Create(builder.ToString());
        }

        /// <summary>
        /// Conditional actionlink, if condition is not met, you can show disabled div with message, or render empty.
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="linkText"></param>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        /// <param name="routeValues"></param>
        /// <param name="htmlAttributes"></param>
        /// <param name="showActionLinkAsEnabled"></param>
        /// <param name="message"></param>
        /// <param name="hide"></param>
        /// <returns></returns>
        public static MvcHtmlString ActionLinkConditional(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName, object routeValues, object htmlAttributes, bool showActionLinkAsEnabled = true, string message = null, bool hide = false)
        {
            if (showActionLinkAsEnabled)
            {
                return htmlHelper.ActionLink(linkText, actionName, controllerName, routeValues, htmlAttributes);
            }
            else if (!showActionLinkAsEnabled && !hide)
            {
                TagBuilder tagBuilder = new TagBuilder("div");
                tagBuilder.AddCssClass("disabled");
                tagBuilder.Attributes.Add("Title", message);
                tagBuilder.InnerHtml = linkText;
                return MvcHtmlString.Create(tagBuilder.ToString());
            }
            else
            {
                return MvcHtmlString.Empty;
            }
        }

        #endregion

        #region Notification helper

        /// <summary>
        /// Tempdata extension for passing notification content to page.
        /// </summary>
        /// <param name="tempData">current tempdata</param>
        /// <param name="message">message content</param>
        /// <param name="css">additional css</param>
        public static void Notification(this TempDataDictionary tempData, string message, string title=null, NotificationType type = NotificationType.success)
        {
            tempData["notification"] = message;
            tempData["notificationTitle"] = title;
            tempData["notificationType"] = type.ToString();
;
        }

        /// <summary>
        /// Renders notification(classes from twitter bootstrap) on top of the page. Just type in layout @Html.Notification()
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <returns></returns>
        public static MvcHtmlString Notification(this HtmlHelper htmlHelper)
        {

            var notification = htmlHelper.ViewContext.TempData["notification"] as string;
            var notificationTitle = htmlHelper.ViewContext.TempData["notificationTitle"] as string;
            var type = htmlHelper.ViewContext.TempData["notificationType"] as string;

            if (string.IsNullOrEmpty(notification))
                return MvcHtmlString.Empty;

            return formatNotification(notification, notificationTitle, type);
        }

        /// <summary>
        /// Formating notification into html element, classes are from twitter bootstrap.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static MvcHtmlString formatNotification(string message, string title, string type)
        {
            string cssClass = "clear margl20 alert alert-block alert-" + type;
            var div = new TagBuilder("div");
            div.Attributes.Add("id", "notification");
            if (!string.IsNullOrEmpty(title))
            {
                var h4 = new TagBuilder("h4");
                h4.SetInnerText(title);
                div.InnerHtml = h4.ToString();
            }
            div.AddCssClass(cssClass);
            div.SetInnerText(message);
            return MvcHtmlString.Create(div.ToString());
        }

        #endregion

        //Authorized actionlinks
        #region ActionLink authorized helpers

        public static MvcHtmlString ActionLinkAuthorized(this HtmlHelper htmlHelper, string linkText, string actionName, bool hide = true)
        {
            return htmlHelper.ActionLinkAuthorized(linkText, actionName, null, new RouteValueDictionary(), new RouteValueDictionary(), hide);
        }

        public static MvcHtmlString ActionLinkAuthorized(this HtmlHelper htmlHelper, string linkText, string actionName, object routeValues, bool hide = true)
        {
            return htmlHelper.ActionLinkAuthorized(linkText, actionName, null, new RouteValueDictionary(routeValues), new RouteValueDictionary(), hide);
        }

        public static MvcHtmlString ActionLinkAuthorized(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName, bool hide = true)
        {
            return htmlHelper.ActionLinkAuthorized(linkText, actionName, controllerName, new RouteValueDictionary(), new RouteValueDictionary(), hide);
        }

        public static MvcHtmlString ActionLinkAuthorized(this HtmlHelper htmlHelper, string linkText, string actionName, RouteValueDictionary routeValues, bool hide = true)
        {
            return htmlHelper.ActionLinkAuthorized(linkText, actionName, null, routeValues, new RouteValueDictionary(), hide);
        }

        public static MvcHtmlString ActionLinkAuthorized(this HtmlHelper htmlHelper, string linkText, string actionName, object routeValues, object htmlAttributes, bool hide = true)
        {
            return htmlHelper.ActionLinkAuthorized(linkText, actionName, null, new RouteValueDictionary(routeValues), new RouteValueDictionary(htmlAttributes), hide);
        }

        public static MvcHtmlString ActionLinkAuthorized(this HtmlHelper htmlHelper, string linkText, string actionName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes, bool hide = true)
        {
            return htmlHelper.ActionLinkAuthorized(linkText, actionName, null, routeValues, htmlAttributes, hide);
        }

        public static MvcHtmlString ActionLinkAuthorized(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName, object routeValues, object htmlAttributes, bool hide = true)
        {
            return htmlHelper.ActionLinkAuthorized(linkText, actionName, controllerName, new RouteValueDictionary(routeValues), new RouteValueDictionary(htmlAttributes), hide);
        }

        public static MvcHtmlString ActionLinkAuthorized(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes, bool hide = true)
        {
            //method ActionAuthorized is in security
            if (htmlHelper.ActionAuthorized(actionName, controllerName))
            {
                return htmlHelper.ActionLink(linkText, actionName, controllerName, routeValues, htmlAttributes);
            }
            else
            {
                return ActionLinkConditional(htmlHelper, linkText, actionName, controllerName, routeValues, htmlAttributes, false, "Not enough permissions to execute this!", hide);
            }
        }

        #endregion

        /// <summary>
        /// Extension for enumerating list of items on view. Invoke like T.List(**template**).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="template">Template for rendering ie @Html.Partial("view", item)</param>
        /// <returns></returns>
        public static HelperResult List<T>(this IEnumerable<T> items,
          Func<T, HelperResult> template)
        {
            return new HelperResult(writer =>
            {
                if (!items.IsNullOrEmpty())
                    foreach (var item in items)
                        template(item).WriteTo(writer);
            });
        }

        /// <summary>
        /// Checks if list is null, or has no items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> items)
        {
            return (items == null || items.Count() == 0);
        }

        /// <summary>
        /// Extension on any Hrml helper, you can insert condition when to render certain helper.
        /// </summary>
        /// <param name="value">Helper</param>
        /// <param name="evaluation">Condition</param>
        /// <param name="falseValue">If condition is not met, render falseValue. If null nothing renders..</param>
        /// <returns></returns>
        public static MvcHtmlString If(this MvcHtmlString value, bool evaluation, MvcHtmlString falseValue = default(MvcHtmlString))
        {
            return evaluation ? value : falseValue;
        }
    }
}