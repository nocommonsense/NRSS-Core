using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.PlatformAbstractions;
using RSS_Feed.ViewModels.Home;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RSS_Feed.Services
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
            IDirectoryContents contents = provider.GetDirectoryContents("content");
            
            if (!contents.Any(c => c.Name == "feed.xml") || !XmlFilePopulated())
            {
                vm.RSSCreated = false;
                return;
            }
            
            using (XmlReader reader = XmlReader.Create(contents.Where(c => c.Name == "feed.xml").FirstOrDefault().CreateReadStream()))
            {
                bool completed = false;
                //File exists, check if populated with RSS Info. If not, add it.
                while (reader.Read())
                {
                    if (completed)
                    {
                        break;
                    }

                    switch (reader.Name)
                    {
                        case "title":
                            //Found start element. Assume info is populated.
                            vm.Title = reader.ReadElementContentAsString();
                            break;
                        case "description":
                            vm.Description = reader.ReadElementContentAsString();
                            break;
                        case "link":
                            vm.Link = reader.ReadElementContentAsString();
                            break;
                        case "pubDate":
                            DateTime pubDate;
                            if (DateTime.TryParse(reader.ReadElementContentAsString(), out pubDate))
                            {
                                vm.PublishedDate = pubDate;
                            }
                            break;
                        case "lastBuildDate":
                            DateTime updateDate;
                            if (DateTime.TryParse(reader.ReadElementContentAsString(), out updateDate))
                            {
                                vm.UpdatedDate = updateDate;
                            }
                            break;
                        case "item":
                            completed = true;
                            break;
                    }
                }
            }
        }

        public void AddRSSInfo(RSSViewModel vm)
        {
            
            
           
        }
        
        public bool AddRSSItem(RSSItemViewModel vm)
        {
            IFileProvider provider = _appEnvironment.ContentRootFileProvider;
            IDirectoryContents contents = provider.GetDirectoryContents("content");
            
            if (!contents.Any(c => c.Name == "feed.xml"))
            {
                return false;
            }
            StringBuilder sb = new StringBuilder();

            XmlReader reader = XmlReader.Create(contents.Where(c => c.Name == "feed.xml").FirstOrDefault().CreateReadStream());
            XmlWriter writer = XmlWriter.Create(sb);
                        
            return true;
        }

        private bool XmlFilePopulated()
        {
            IFileProvider provider = _appEnvironment.ContentRootFileProvider;
            IDirectoryContents contents = provider.GetDirectoryContents("content");

            try
            {
                bool foundStartElement = false;

                using (XmlReader reader = XmlReader.Create(contents.Where(c => c.Name == "feed.xml").FirstOrDefault().CreateReadStream()))
                {
                    //File exists, check if populated with RSS Info. If not, add it.
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            switch (reader.Name)
                            {
                                case "rss":
                                    //Found start element. Assume info is populated.
                                    foundStartElement = true;
                                    break;
                            }
                        }
                    }
                }

                if (!foundStartElement)
                {
                    return false;
                }

            }
            catch (FileNotFoundException e)
            {
                return false;
            }

            return true;
        }

        private bool PopulateRSSInfo(bool create)
        {            
            if (create)
            {
                try
                {
                    File.Create(_appEnvironment.ContentRootPath + "/content/feed.xml");
                }
                catch
                {
                    return false;
                }
            }

            StringBuilder sb = new StringBuilder();

            XmlWriter writer = XmlWriter.Create(sb);
            writer.WriteStartDocument();

            writer.WriteStartElement("rss");
            writer.WriteAttributeString("version", "2.0");

            writer.WriteEndElement();
            writer.WriteEndDocument();

            using (var fileStream = new FileStream(_appEnvironment.ContentRootPath + "/content/feed.xml", FileMode.Open))
            {
                byte[] ba = Encoding.ASCII.GetBytes(sb.ToString());
                fileStream.Write(ba, 0, ba.Length);
            }

            return true;

        }


    }
}

