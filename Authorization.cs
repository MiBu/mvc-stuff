using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Project.Web.Security
{
    public static class Authorization
    {
        /// <summary>
        /// Checks if user has enough permissions to action from controller.
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="actionName">Name of the action.</param>
        /// <param name="controllerName">Name of controller. If empty, uses current controller.</param>
        /// <returns></returns>
        public static bool ActionAuthorized(this HtmlHelper htmlHelper, string actionName, string controllerName)
        {
            ControllerBase controllerBase = string.IsNullOrEmpty(controllerName) ? htmlHelper.ViewContext.Controller : htmlHelper.GetControllerByName(controllerName);
            ControllerContext controllerContext = new ControllerContext(htmlHelper.ViewContext.RequestContext, controllerBase);
            ControllerDescriptor controllerDescriptor = new ReflectedControllerDescriptor(controllerContext.Controller.GetType());
            ActionDescriptor actionDescriptor = controllerDescriptor.FindAction(controllerContext, actionName);
            if (actionDescriptor == null)
            {
                actionDescriptor = controllerDescriptor.GetCanonicalActions().Where(a => a.ActionName == actionName).FirstOrDefault();
            }
            if (actionDescriptor == null)
                return false;

            FilterInfo filters = new FilterInfo(FilterProviders.Providers.GetFilters(controllerContext, actionDescriptor));

            AuthorizationContext authorizationContext = new AuthorizationContext(controllerContext, actionDescriptor);
            foreach (IAuthorizationFilter authorizationFilter in filters.AuthorizationFilters)
            {
                authorizationFilter.OnAuthorization(authorizationContext);
                if (authorizationContext.Result != null)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Gets controller by name.
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="controllerName"></param>
        /// <returns></returns>
        public static ControllerBase GetControllerByName(this HtmlHelper htmlHelper, string controllerName)
        {
            IControllerFactory factory = ControllerBuilder.Current.GetControllerFactory();
            IController controller = factory.CreateController(htmlHelper.ViewContext.RequestContext, controllerName);
            if (controller == null)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, "The IControllerFactory '{0}' did not return a controller for the name '{1}'.", factory.GetType(), controllerName));
            }
            return (ControllerBase)controller;
        }

    }
}