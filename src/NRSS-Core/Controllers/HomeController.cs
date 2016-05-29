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

            return RedirectToAction("Index", "Home");

        }

        public IActionResult Items()
        {
            var vm = new RSSItemsViewModel();
            var itemVm = new RSSItemViewModel();

            vm.RSSItem = itemVm;

            _rssService.PopulateRSSItemsViewModel(vm);
            
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Items(RSSItemsViewModel vm)
        {
            if(ModelState.IsValid)
            {
                if (!_rssService.AddRSSItem(vm.RSSItem))
                {
                    return View(vm);
                }
            }

            return RedirectToAction("Items", "Home");

        }

        public IActionResult EditItem(string id)
        {
            EditRSSItemViewModel vm = new EditRSSItemViewModel();
                        
            _rssService.PopulateEditRSSItemViewModel(vm, id);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditItem(EditRSSItemViewModel vm, string id)
        {
            _rssService.EditRSSItem(vm.RSSItem, id);

            return RedirectToAction("Items", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteItem(string id)
        {
            _rssService.DeleteRSSItem(id);

            return RedirectToAction("Items", "Home");
        }
        
        public IActionResult Error()
        {
            return View();
        }
    }
}
