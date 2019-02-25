using System.Web;
using System.Web.Mvc;
using Carubbi.Utils.Persistence;

namespace SmartLMS.WebUI.ActionResults
{
    public class XmlActionResult<T> : ActionResult
    {
        public XmlActionResult(T data)
        {
            Data = data;
        }

        public T Data { private get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            HttpContextBase httpContextBase = context.HttpContext;
            httpContextBase.Response.Buffer = true;
            httpContextBase.Response.Clear();

            httpContextBase.Response.ContentType = "text/xml";


            Serializer<T> serializer = new Serializer<T>();
            string xml = serializer.XmlSerialize(Data);
            httpContextBase.Response.Write(xml);
        }
    }
}