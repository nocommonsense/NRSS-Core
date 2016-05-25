using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RSS_Feed.ViewModels.Home;

namespace RSS_Feed.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Info()
        {
            var vm = new RSSViewModel();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Info(RSSViewModel vm)
        {
            if(ModelState.IsValid)
            {

            }

            return View(vm);

        }

        public IActionResult AddItem()
        {
            var vm = new RSSItemViewModel();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddItem(RSSItemViewModel vm)
        {
            if(ModelState.IsValid)
            {

            }

            return View(vm);

        }
        
        public IActionResult Error()
        {
            return View();
        }
    }
}
