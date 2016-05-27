using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NRSSCore.ViewModels.Home
{
  
    public class RSSViewModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public string Link { get; set; }

        public bool RSSCreated { get; set; }

        public DateTime? PublishedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public int TimeToLive { get; set; }
        
    }

    public class RSSItemViewModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public string Link { get; set; }

        public DateTime? PublishedDate { get; set; }

        public Guid GUID { get; set; }


    }
}
