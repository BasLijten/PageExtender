using Sitecore.Mvc.Pipelines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageExtender.Business
{
    public class RenderPageExtendersArgs : MvcPipelineArgs
    {
        private readonly TextWriter writer;

        public bool IsRendered
        {
            get;
            set;
        }

        public TextWriter Writer
        {
            get
            {
                return this.writer;
            }
        }

        public RenderPageExtendersArgs(TextWriter writer)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull(writer, "writer");
            this.writer = writer;
        }
    }
}
