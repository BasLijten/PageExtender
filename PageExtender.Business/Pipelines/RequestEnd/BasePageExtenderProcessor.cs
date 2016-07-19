using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using Sitecore;
using Sitecore.Xml;
using System;

namespace PageExtender.Business.Pipelines.RequestEnd
{
    public abstract class BasePageExtenderProcessor
    {
        protected string razorView = String.Empty;
        protected Collection<PageExtenderSiteFilter> SiteFilters { get; private set; }

        protected BasePageExtenderProcessor()
        {
            SiteFilters = new Collection<PageExtenderSiteFilter>();
        }

        public void AddFilter(XmlNode node)
        {
            if (node == null)
                return;

            string key = XmlUtil.GetAttribute("filterKey", node);
            string value = XmlUtil.GetAttribute("filterValue", node);
            string razorViewAttribute = XmlUtil.GetAttribute("razorViewName", node);

            if (String.IsNullOrEmpty(key))
                return;
            if (String.IsNullOrEmpty(value))
                return;
            if (String.IsNullOrEmpty(razorViewAttribute))
                return;

            SiteFilters.Add(new PageExtenderSiteFilter { Key = key, Value = value, RazorViewFilename = razorViewAttribute });
        }

        public void Process(RenderPageExtendersArgs args)
        {
            if (args == null)
                return;

            if (Context.Site == null)            
                return;

            PageExtenderSiteFilter filter = SiteFilters.Where(f => string.Equals(Context.Site.Properties[f.Key], f.Value) && !String.IsNullOrEmpty(f.RazorViewFilename)).FirstOrDefault();
            if (filter == null)
                return;

            if (filter!=null)
            {
                razorView = filter.RazorViewFilename;
                this.DoProcess(args);
            }            
        }

        public abstract void DoProcess(RenderPageExtendersArgs args);
    }
}
