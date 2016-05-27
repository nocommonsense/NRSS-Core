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

        bool AddRSSInfo(RSSViewModel vm);

        bool AddRSSItem(RSSItemViewModel vm);
    }
}
