using PageExtender.Business.Models;
using PageExtender.Business.Pipelines.RequestEnd;
using Sitecore;
using Sitecore.Analytics;
using Sitecore.Analytics.Model;
using Sitecore.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace PageExtender.Business.Pipelines.PageExtender.RenderPageExtenders
{
    public class GoogleAnalyticsRenderPageExtender : BasePageExtenderProcessor
    {        
        public override void DoProcess(RenderPageExtendersArgs args)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull(args, "args");
            if (args.IsRendered)
            {
                return;
            }
            args.IsRendered = this.Render(args.Writer);           
        }
        private class PersonalizationController : Controller
        {

        }

        private bool Render(TextWriter writer)
        {


            var result = false;
            List<string> appliedPersonalizations = new List<string>();
            IList<PersonalizationRuleData> exposedRules = null;

            if (Tracker.Current != null && Tracker.Enabled && Tracker.Current.CurrentPage != null && Tracker.Current.CurrentPage.Personalization != null)
            {
                var personalization = Tracker.Current.CurrentPage.Personalization;
                exposedRules = personalization.ExposedRules;
                foreach (var exposedRule in exposedRules)
                {
                    var a = exposedRule.RuleId;
                }
                //var appliedRules = Tracker.CurrentPage.Personalization;
            }

            if (exposedRules != null)
            {
                var defaultRuleId = new ID("{00000000-0000-0000-0000-000000000000}");
                var filteredExposedRules = exposedRules.Where(x => x.RuleId != defaultRuleId).ToList();
                if (filteredExposedRules.Count > 0)
                {
                    if (Context.Device != null)
                    {
                        var renderingReferences = Context.Item.Visualization.GetRenderings(Context.Device, true);
                        if (renderingReferences != null)
                        {
                            foreach (var renderingReference in renderingReferences)
                            {
                                if (renderingReference.Settings.Rules != null && renderingReference.Settings.Rules.Rules != null)
                                {
                                    var renderingReferenceRules = renderingReference.Settings.Rules.Rules;
                                    if (renderingReferenceRules.Count() > 0)
                                    {
                                        var filteredRulesList = renderingReferenceRules.Where(x => x.UniqueId != defaultRuleId).ToList();
                                        if (filteredRulesList.Count > 0)
                                        {

                                            var appliedRule = from rules in filteredRulesList
                                                              join exp in filteredExposedRules
                                                                on rules.UniqueId equals exp.RuleId
                                                              select rules.Name;
                                            if (appliedRule != null && appliedRule.Count() > 0)
                                            {
                                                appliedPersonalizations.Add(appliedRule.FirstOrDefault());
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
           
            result = true;

            /// via view - more maintainable
            string partialName = this.razorView;
            HttpContextWrapper httpContext = new HttpContextWrapper(HttpContext.Current);
            ControllerContext controllerContext = new ControllerContext(new RequestContext(httpContext, new RouteData
            {
                Values =
                {
                    {
                        "Controller",
                        "PersonalizationController"
                    }
                }
            }), new PersonalizationController());

            IPersonalization personalizationModel = new Personalization();
            personalizationModel.PersonalizationNames = appliedPersonalizations;

            IView view = ViewEngines.Engines.FindPartialView(controllerContext, partialName).View;
            ViewContext viewContext = new ViewContext(controllerContext, view, new ViewDataDictionary { Model = personalizationModel }, new TempDataDictionary(), writer);
            view.Render(viewContext, writer);

            return result;
        }
    }

}
