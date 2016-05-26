using RSS_Feed.ViewModels.Home;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RSS_Feed.Services
{
    public interface IRSSService
    {
        void PopulateRSSInfoViewModel(RSSViewModel vm);

        void AddRSSInfo(RSSViewModel vm);

        bool AddRSSItem(RSSItemViewModel vm);
    }
}
