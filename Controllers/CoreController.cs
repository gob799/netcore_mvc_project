using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
//using v29.Models;

namespace v29.Controllers
{
    public class CoreController : Controller
    {
        public CoreController()
        {
        }
        public IActionResult BanList()
        {
            return View();
        }

    }
}