using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace v29.Controllers
{
    public class LogsController : Controller
    {
        // GET: LogsController
        public ActionResult Index()
        {
            return View();
        }

        // GET: LogsController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: LogsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LogsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LogsController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LogsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LogsController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LogsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
