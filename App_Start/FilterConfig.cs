using System.Web;
using System.Web.Mvc;

namespace PROJEKT_PZ_NK_v3
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
