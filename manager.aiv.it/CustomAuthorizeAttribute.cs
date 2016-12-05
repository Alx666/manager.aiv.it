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
                return httpContext.Session.GetUser().Roles.Select(r => (RoleType)r.Id).Any(x => this.allowedroles.Contains(x));                
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