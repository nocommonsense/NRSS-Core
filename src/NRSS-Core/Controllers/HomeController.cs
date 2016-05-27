using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NRSSCore.ViewModels.Home;
using NRSSCore.Services;

namespace NRSSCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRSSService _rssService;
        
        public HomeController(IRSSService rssService)
        {
            _rssService = rssService;
        }

        public IActionResult Index()
        {
            var vm = new RSSViewModel();

            _rssService.PopulateRSSInfoViewModel(vm);

            return View(vm);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(RSSViewModel vm)
        {
            if(ModelState.IsValid)
            {
                if(!_rssService.AddRSSInfo(vm))
                {
                    return View(vm);
                }
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
                if (!_rssService.AddRSSItem(vm))
                {
                    return View(vm);
                }
            }

            return View(vm);

        }
        
        public IActionResult Error()
        {
            return View();
        }
    }
}
