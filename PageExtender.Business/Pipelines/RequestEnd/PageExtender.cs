using PageExtender.Business.ResponseFilter;
using Sitecore;
using Sitecore.Mvc.Extensions;
using Sitecore.Mvc.Pipelines.Request.RequestEnd;
using Sitecore.Mvc.Presentation;
using System.IO;
using System.Web.Routing;

namespace PageExtender.Business.Pipelines.RequestEnd
{
    public class PageExtender : RequestEndProcessor
    {
        public override void Process(RequestEndArgs args)
        {
            if (Context.Site == null)
            {
                return;
            }
            PageContext pageContext = args.PageContext;
            if (pageContext == null)
            {
                return;
            }
            if (!Context.PageMode.IsNormal || Context.PageMode.IsExperienceEditor || Context.PageMode.IsExperienceEditorEditing || Context.PageMode.IsPreview || Context.PageMode.IsProfiling)
            {
                return;
            }

            RequestContext requestContext = pageContext.RequestContext;
            Stream filter = requestContext.HttpContext.Response.Filter;
            if (filter == null)
            {
                return;
            }
            if (args.PageContext.Item == null)
            {
                return;
            }
            PageExtenderResponseFilter pageExtenderResponseFilter = new PageExtenderResponseFilter(filter);
            if (pageExtenderResponseFilter.ExtendersHtml.IsWhiteSpaceOrNull())
            {
                return;
            }
            requestContext.HttpContext.Response.Filter = pageExtenderResponseFilter;
        }
    }
}
