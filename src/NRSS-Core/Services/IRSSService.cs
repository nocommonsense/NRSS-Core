using NRSSCore.ViewModels.Home;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NRSSCore.Services
{
    public interface IRSSService
    {
        void PopulateRSSInfoViewModel(RSSViewModel vm);

        void PopulateRSSItemsViewModel(RSSItemsViewModel vm);

        void PopulateEditRSSItemViewModel(EditRSSItemViewModel vm, string id);

        bool AddRSSInfo(RSSViewModel vm);

        bool AddRSSItem(RSSItemViewModel vm);

        bool EditRSSItem(RSSItemViewModel vm, string id);

        bool DeleteRSSItem(string id);
    }
}
