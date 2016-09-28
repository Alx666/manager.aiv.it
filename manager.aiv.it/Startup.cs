using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(manager.aiv.it.Startup))]
namespace manager.aiv.it
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
