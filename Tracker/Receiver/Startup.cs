using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Receiver.Startup))]
namespace Receiver
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
