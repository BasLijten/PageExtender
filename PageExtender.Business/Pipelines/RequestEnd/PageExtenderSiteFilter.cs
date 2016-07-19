using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageExtender.Business.Pipelines.RequestEnd
{
    public class PageExtenderSiteFilter
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public string RazorViewFilename { get; set; }
    }
}
