using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Core.Startup))]
namespace Core
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
