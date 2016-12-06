using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Watcher.Startup))]
namespace Watcher
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
