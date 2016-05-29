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
            
            if (!contents.Any(c => c.Name == "feed.xml"))
            {
                CreateXMLFile();
                vm.RSSCreated = false;
                return;
            }

            if(!XmlFilePopulated())            
                return;
            

            XDocument xdoc = XDocument.Load(contents.Where(c => c.Name == "feed.xml").FirstOrDefault().CreateReadStream());

            if(xdoc != null)
            {
                vm.Title = xdoc.Descendants("title").FirstOrDefault()?.Value;
                vm.Description = xdoc.Descendants("description").FirstOrDefault()?.Value;
                vm.Link = xdoc.Descendants("link").FirstOrDefault()?.Value;

                string pubDate = xdoc.Descendants("pubDate").FirstOrDefault()?.Value;
                DateTime pubDateTime;
                if (DateTime.TryParse(pubDate, out pubDateTime))
                {
                    vm.PublishedDate = pubDateTime;
                }

                string updateDate = xdoc.Descendants("lastBuildDate").FirstOrDefault()?.Value;
                DateTime updateDateTime;
                if (DateTime.TryParse(updateDate, out updateDateTime))
                {
                    vm.UpdatedDate = updateDateTime;
                }

                int ttl;
                if(int.TryParse(xdoc.Descendants("ttl").FirstOrDefault()?.Value, out ttl))
                {
                    vm.TimeToLive = ttl;
                }                

                vm.RSSCreated = true;
            }                        
        }

        public void PopulateRSSItemsViewModel(RSSItemsViewModel vm)
        {
            List<RSSItemViewModel> vmList = new List<RSSItemViewModel>();

            IFileProvider provider = _appEnvironment.ContentRootFileProvider;
            IDirectoryContents contents = provider.GetDirectoryContents("wwwroot\\content");

            if (!contents.Any(c => c.Name == "feed.xml"))
            {
                if (!CreateXMLFile())
                    return;
            }
            
            if(!XmlFilePopulated())                                       
                return;
            
            XDocument xdoc = XDocument.Load(contents.Where(c => c.Name == "feed.xml").FirstOrDefault().CreateReadStream());

            if (xdoc != null)
            {
                List<XElement> elements = xdoc.Descendants("item").ToList();

                foreach(var element in elements)
                {
                    RSSItemViewModel tempVm = new RSSItemViewModel();
                    tempVm.Title = element.Descendants("title").FirstOrDefault()?.Value;
                    tempVm.Description = element.Descendants("description").FirstOrDefault()?.Value;
                    tempVm.Link = element.Descendants("link").FirstOrDefault()?.Value;

                    string pubDate = element.Descendants("pubDate").FirstOrDefault()?.Value;
                    DateTime pubDateTime;
                    if (DateTime.TryParse(pubDate, out pubDateTime))
                    {
                        tempVm.PublishedDate = pubDateTime;
                    }

                    tempVm.GUID = element.Descendants("guid").FirstOrDefault()?.Value;

                    vmList.Add(tempVm);               
                }                
            }

            vm.RSSItems = vmList;            
        }

        public void PopulateEditRSSItemViewModel(EditRSSItemViewModel vm, string id)
        {
            RSSItemViewModel itemVm = new RSSItemViewModel();

            if (string.IsNullOrEmpty(id))
                return;

            IFileProvider provider = _appEnvironment.ContentRootFileProvider;
            IDirectoryContents contents = provider.GetDirectoryContents("wwwroot\\content");

            if (!contents.Any(c => c.Name == "feed.xml"))
            {
                if (!CreateXMLFile())
                    return;
            }
                
            if (!XmlFilePopulated())
                return;

            XDocument xdoc = XDocument.Load(contents.Where(c => c.Name == "feed.xml").FirstOrDefault().CreateReadStream());

            if (xdoc != null)
            {
                var eleList = xdoc.Descendants("item").ToList();

                var item = eleList.Where(c => c.Element("guid").Value == id).FirstOrDefault();

                if (item != null)
                {
                    itemVm.Title = item.Descendants("title").FirstOrDefault()?.Value;
                    itemVm.Description = item.Descendants("description").FirstOrDefault()?.Value;
                    itemVm.Link = item.Descendants("link").FirstOrDefault()?.Value;
                    itemVm.GUID = item.Descendants("guid").FirstOrDefault()?.Value;
                }
            }

            vm.RSSItem = itemVm;

        }

        public bool AddRSSInfo(RSSViewModel vm)
        {
            //pubdate format from http://www.dotnetperls.com/pubdate

            IFileProvider provider = _appEnvironment.ContentRootFileProvider;
            IDirectoryContents contents = provider.GetDirectoryContents("wwwroot\\content");

            if (!contents.Any(c => c.Name == "feed.xml"))
            {
                if (!CreateXMLFile())
                    return false;
            }
                
            XDocument xdoc;
            DateTime now = DateTime.Now;
            if (!XmlFilePopulated())
            {
                xdoc = new XDocument(
                new XElement("rss", new XAttribute("version", "2.0"),
                new XElement("channel", new XElement("title", vm.Title),
                new XElement("description", vm.Description),
                new XElement("link", vm.Link),
                new XElement("pubDate", now.ToString("ddd',' d MMM yyyy HH':'mm':'ss") +
                                        " " +
                                        now.ToString("zzzz").Replace(":", "")),
                new XElement("lastBuildDate", now.ToString("ddd',' d MMM yyyy HH':'mm':'ss") +
                                        " " +
                                        now.ToString("zzzz").Replace(":", "")),
                new XElement("ttl", vm.TimeToLive))));
            }
            else
            {
                xdoc = XDocument.Load(contents.Where(c => c.Name == "feed.xml").FirstOrDefault().CreateReadStream());

                xdoc.Descendants("title").FirstOrDefault()?.SetValue(vm.Title);
                xdoc.Descendants("description").FirstOrDefault()?.SetValue(vm.Description);
                xdoc.Descendants("link").FirstOrDefault()?.SetValue(vm.Link);
                xdoc.Descendants("lastBuildDate").FirstOrDefault()?.SetValue(now.ToString("ddd',' d MMM yyyy HH':'mm':'ss") +
                                                                            " " +
                                                                            now.ToString("zzzz").Replace(":", ""));
                xdoc.Descendants("ttl").FirstOrDefault()?.SetValue(vm.TimeToLive);
            }
                                   
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
            return ModifyRSSItem(ModifyType.Create, vm, null);                        
        }

        public bool EditRSSItem(RSSItemViewModel vm, string id)
        {
            return ModifyRSSItem(ModifyType.Edit, vm, id);
        }

        public bool DeleteRSSItem(string id)
        {
            return ModifyRSSItem(ModifyType.Delete, null, id);
        }

        private bool CreateXMLFile()
        {
            try
            {                
                File.Create(_appEnvironment.ContentRootPath + "\\wwwroot\\content\\feed.xml");                                           
                return true;
            }
            catch(Exception e)
            {
                return false;
            }            
        }

        private bool ModifyRSSItem(ModifyType type, RSSItemViewModel vm, string id)
        {
            IFileProvider provider = _appEnvironment.ContentRootFileProvider;
            IDirectoryContents contents = provider.GetDirectoryContents("wwwroot\\content");

            if (!contents.Any(c => c.Name == "feed.xml"))
            {
                if(!CreateXMLFile())
                    return false;
            }

            if (!XmlFilePopulated())
                return false;

            XDocument xdoc = XDocument.Load(contents.Where(c => c.Name == "feed.xml").FirstOrDefault().CreateReadStream());

            var eleList = xdoc.Descendants("item").ToList();
            var ttlEle = xdoc.Descendants("ttl").LastOrDefault();

            if (ttlEle == null)
                return false;

            DateTime now = DateTime.Now;

            if (type == ModifyType.Create)
            {
                if (eleList.Count() == 0)
                {
                    //No RSS Items found.
                    xdoc.Descendants("ttl").LastOrDefault()?.AddAfterSelf(new XElement("item", new XElement("title", vm.Title),
                                                    new XElement("description", vm.Description),
                                                    new XElement("link", vm.Link),
                                                    new XElement("guid", new XAttribute("isPermaLink", "true"), Guid.NewGuid().ToString()),
                                                    new XElement("pubDate", now.ToString("ddd',' d MMM yyyy HH':'mm':'ss") +
                                                                            " " +
                                                                            now.ToString("zzzz").Replace(":", ""))));
                }
                else
                {
                    xdoc.Descendants("item").LastOrDefault()?.AddAfterSelf(new XElement("item", new XElement("title", vm.Title),
                                                    new XElement("description", vm.Description),
                                                    new XElement("link", vm.Link),
                                                    new XElement("guid", new XAttribute("isPermaLink", "true"), Guid.NewGuid().ToString()),
                                                    new XElement("pubDate", now.ToString("ddd',' d MMM yyyy HH':'mm':'ss") +
                                                                            " " +
                                                                            now.ToString("zzzz").Replace(":", ""))));
                }
            }
            else if(type == ModifyType.Edit)
            {
                var item = eleList.Where(c => c.Element("guid").Value == id).FirstOrDefault();
               
                if(item != null)
                {
                    item.Descendants("title").FirstOrDefault()?.SetValue(vm.Title);
                    item.Descendants("description").FirstOrDefault()?.SetValue(vm.Description);
                    item.Descendants("link").FirstOrDefault()?.SetValue(vm.Link);                    
                }
            }
            else if(type == ModifyType.Delete)
            {
                var item = eleList.Where(c => c.Element("guid").Value == id).FirstOrDefault();

                if (item != null)
                {
                    item.Remove();
                }
            }

            try
            {
                using (var fileStream = new FileStream(_appEnvironment.ContentRootPath + "\\wwwroot\\content\\feed.xml", FileMode.Create))
                {
                    xdoc.Save(fileStream);

                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }

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
                    if (xdoc.Descendants("title").Count() > 0)
                        return true;
                }
            }
            catch(Exception e)
            {
                return false;
            }

            return false;
                        
        }

        enum ModifyType
        {
            Create, Edit, Delete
        }       

    }
}

