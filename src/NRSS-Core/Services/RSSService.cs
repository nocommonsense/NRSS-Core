using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.PlatformAbstractions;
using NRSSCore.ViewModels.Home;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace NRSSCore.Services
{
    public class RSSService : IRSSService
    {

        private readonly IHostingEnvironment _appEnvironment;

        public RSSService(IHostingEnvironment appEnv)
        {
            _appEnvironment = appEnv;
        }

        public void PopulateRSSInfoViewModel(RSSViewModel vm)
        {
            IFileProvider provider = _appEnvironment.ContentRootFileProvider;
            IDirectoryContents contents = provider.GetDirectoryContents("wwwroot\\content");
            
            if (!contents.Any(c => c.Name == "feed.xml") || !XmlFilePopulated())
            {
                vm.RSSCreated = false;
                return;
            }

            XDocument xdoc = XDocument.Load(contents.Where(c => c.Name == "feed.xml").FirstOrDefault().CreateReadStream());

            if(xdoc != null)
            {
                vm.Title = xdoc.Element("title").Value;
                vm.Description = xdoc.Element("description").Value;
                vm.Link = xdoc.Element("link").Value;

                string pubDate = xdoc.Element("pubDate").Value;
                DateTime pubDateTime;
                if (DateTime.TryParse(pubDate, out pubDateTime))
                {
                    vm.PublishedDate = pubDateTime;
                }

                string updateDate = xdoc.Element("lastBuildDate").Value;
                DateTime updateDateTime;
                if (DateTime.TryParse(updateDate, out updateDateTime))
                {
                    vm.UpdatedDate = updateDateTime;
                }

                int ttl;
                if(int.TryParse(xdoc.Element("ttl").Value, out ttl))
                {
                    vm.TimeToLive = ttl;
                }                

                vm.RSSCreated = true;
            }
                        
        }

        public bool AddRSSInfo(RSSViewModel vm)
        {

            XDocument xdoc = new XDocument(
                new XElement("rss", new XAttribute("version", "2.0"),
                new XElement("channel", new XElement("title", vm.Title),
                new XElement("description", vm.Description),
                new XElement("link", vm.Link),
                new XElement("pubDate", vm.PublishedDate.HasValue ? vm.PublishedDate.Value.ToString() : string.Empty),
                new XElement("lastBuildDate", vm.UpdatedDate.HasValue ? vm.UpdatedDate.Value.ToString() : string.Empty),
                new XElement("ttl", vm.TimeToLive)))
                
                );

            try
            {
                using (var fileStream = new FileStream(_appEnvironment.ContentRootPath + "\\wwwroot\\content\\feed.xml", FileMode.Create))
                {
                    xdoc.Save(fileStream);
                }
            }
            catch(Exception e)
            {
                return false;
            }
                                                
            return true;


        }
        
        public bool AddRSSItem(RSSItemViewModel vm)
        {
            IFileProvider provider = _appEnvironment.ContentRootFileProvider;
            IDirectoryContents contents = provider.GetDirectoryContents("wwwroot\\content");
            
            if (!contents.Any(c => c.Name == "feed.xml"))
            {
                return false;
            }

            if (!XmlFilePopulated())
                return false;

            XDocument xdoc = XDocument.Load(contents.Where(c => c.Name == "feed.xml").FirstOrDefault().CreateReadStream());

            var eleList = xdoc.Elements("item");
            var ttlEle = xdoc.Element("ttl");

            if (ttlEle == null)
                return false;

            if(eleList.Count() == 0)
            {
                //No RSS Items found.
                xdoc.Element("ttl").AddAfterSelf(new XElement("item"), 
                                                new XElement("title", vm.Title),
                                                new XElement("description", vm.Description),
                                                new XElement("link", vm.Link),
                                                new XElement("guid", new XAttribute("isPermaLink", "true"), vm.GUID.ToString()),
                                                new XElement("pubDate", vm.PublishedDate.HasValue ? vm.PublishedDate.Value.ToString() : string.Empty));
            }
            else
            {
                xdoc.Elements("item").LastOrDefault().AddAfterSelf(new XElement("item"),
                                                new XElement("title", vm.Title),
                                                new XElement("description", vm.Description),
                                                new XElement("link", vm.Link),
                                                new XElement("guid", new XAttribute("isPermaLink", "true"), vm.GUID.ToString()),
                                                new XElement("pubDate", vm.PublishedDate.HasValue ? vm.PublishedDate.Value.ToString() : string.Empty));
            }

                        
            return true;
        }

        private bool XmlFilePopulated()
        {
            IFileProvider provider = _appEnvironment.ContentRootFileProvider;
            IDirectoryContents contents = provider.GetDirectoryContents("wwwroot\\content");

            try
            {
                XDocument xdoc = XDocument.Load(contents.Where(c => c.Name == "feed.xml").FirstOrDefault().CreateReadStream());

                if (xdoc != null)
                {
                    if (xdoc.Elements("title").Count() > 0)
                        return true;
                }
            }
            catch(Exception e)
            {
                return false;
            }

            return false;
                        
        }
        

    }
}

