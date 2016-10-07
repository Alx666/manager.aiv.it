using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace manager.aiv.it
{
    public class CustomAuthorizeAttribute : System.Web.Mvc.AuthorizeAttribute
    {  
        private AivEntities context = new AivEntities();

        private readonly RoleType[] allowedroles;  
        public CustomAuthorizeAttribute(params RoleType[] roles)
        {  
            this.allowedroles = roles;  
        }  

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            try
            {
                int iUserId = (int)httpContext.Session["UserId"];
                List<RoleType> hRoles = httpContext.Session["Roles"] as List<RoleType>;
                return hRoles.Any(r => allowedroles.Contains(r));
            }
            catch (Exception)
            {
                return false;
            }
        }  
        protected override void HandleUnauthorizedRequest(System.Web.Mvc.AuthorizationContext filterContext)
        {  
            filterContext.Result = new HttpUnauthorizedResult();  
        }  
    }  
}