using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PetStore.Filters
{
    /// <summary>
    /// Custom authorization filter for admin-only routes
    /// UPDATED: Now redirects to /Account/Login (single login page)
    /// </summary>
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Check if user has admin role in session
            var userRole = context.HttpContext.Session.GetString("UserRole");

            // If not admin, redirect to main login page
            if (string.IsNullOrEmpty(userRole) || userRole != "Admin")
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }

            base.OnActionExecuting(context);
        }
    }
}