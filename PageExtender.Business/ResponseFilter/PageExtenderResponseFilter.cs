using Sitecore.Mvc.Pipelines;
using Sitecore.Mvc.Presentation;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;

namespace PageExtender.Business.ResponseFilter
{
    internal class PageExtenderResponseFilter : Stream
    {
        private readonly MemoryStream internalStream;

        private readonly Stream responseStream;

        private string extendersHtml;

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public virtual Encoding Encoding
        {
            get
            {
                PageContext currentOrNull = PageContext.CurrentOrNull;
                if (currentOrNull == null)
                {
                    return Encoding.UTF8;
                }
                return currentOrNull.RequestContext.HttpContext.Response.ContentEncoding;
            }
        }

        public virtual string ExtendersHtml
        {
            get
            {
                string html;
                if ((html = this.extendersHtml) == null)
                {
                    html = (this.extendersHtml = this.GetExtendersHtml());
                }
                return html;
            }
            set
            {
                Sitecore.Diagnostics.Assert.IsNotNull(value, "value");
                this.extendersHtml = value;
            }
        }

        public override long Length
        {
            get
            {
                return 0L;
            }
        }

        public override long Position
        {
            get;
            set;
        }

        public virtual Stream ResponseStream
        {
            get
            {
                return this.responseStream;
            }
        }

        public PageExtenderResponseFilter(Stream stream)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull(stream, "stream");
            this.responseStream = stream;
            this.internalStream = new MemoryStream();
        }

        public override void Close()
        {
            this.ResponseStream.Close();
            this.internalStream.Close();
        }

        public override void Flush()
        {
            byte[] array = this.internalStream.ToArray();
            HttpContext current = HttpContext.Current;
            if (current != null)
            {
                Exception lastError = current.Server.GetLastError();
                if (lastError != null)
                {
                    Sitecore.Diagnostics.Log.SingleWarn("Page extenders were not added, because an error occurred during the request execution", this);
                    this.TransmitData(array);
                    return;
                }
            }
            if (string.IsNullOrEmpty(this.ExtendersHtml))
            {
                this.TransmitData(array);
                return;
            }
            string text = this.Encoding.GetString(array);
            text = this.AddExtendersHtml(text);
            byte[] bytes = this.Encoding.GetBytes(text);
            this.TransmitData(bytes);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull(buffer, "buffer");
            return this.ResponseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.ResponseStream.Seek(offset, origin);
        }

        public override void SetLength(long length)
        {
            this.ResponseStream.SetLength(length);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull(buffer, "buffer");
            this.internalStream.Write(buffer, offset, count);
        }

        protected virtual string AddExtendersHtml(string html)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull(html, "html");
            Regex regex = new Regex("(<BODY\\b[^>]*?>)", RegexOptions.IgnoreCase);
            html = regex.Replace(html, "$1" + this.ExtendersHtml, 1);
            return html;
        }

        protected virtual string GetExtendersHtml()
        {
            HtmlTextWriter htmlTextWriter = new HtmlTextWriter(new StringWriter());
            RenderPageExtendersArgs renderPageExtendersArgs = new RenderPageExtendersArgs(htmlTextWriter);
            PipelineService.Get().RunPipeline<RenderPageExtendersArgs>("PageExtender.RenderPageExtenders", renderPageExtendersArgs);
            if (!renderPageExtendersArgs.IsRendered)
            {
                return string.Empty;
            }
            return htmlTextWriter.InnerWriter.ToString();
        }

        protected virtual void TransmitData(byte[] data)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull(data, "data");
            this.ResponseStream.Write(data, 0, data.Length);
            this.ResponseStream.Flush();
            this.internalStream.SetLength(0L);
            this.Position = 0L;
        }
    }
}
