using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PROJEKT_PZ_NK_v3.Startup))]
namespace PROJEKT_PZ_NK_v3
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
