using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace v29.ViewComponents
{
    [ViewComponent(Name = "SidebarMenu")]
    public class CategoryViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("SidebarMenu");
        }

    }
}