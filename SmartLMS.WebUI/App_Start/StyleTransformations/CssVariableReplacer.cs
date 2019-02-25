using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Text;
using System.Web.Optimization;
using Carubbi.Extensions;

namespace SmartLMS.WebUI.App_Start.StyleTransformations
{
    public class CssVariableReplacer : IBundleTransform
    {
        public void Process(BundleContext context, BundleResponse response)
        {
            response.Content = string.Empty;
            StringBuilder stbOutput = new StringBuilder();
            foreach (var file in response.Files)
            {
                var content = File.ReadAllText(context.HttpContext.Server.MapPath(file.VirtualFile.VirtualPath));
                content = content.Replace("#ThemeColor#", ConfigurationManager.AppSettings["ThemeColor"]);
                content = content.Replace("#ThemeColor0.9Transparency#", ToRgba(ConfigurationManager.AppSettings["ThemeColor"], "0.9"));
                stbOutput.Append(content);
            }
            context.HttpContext.Response.Cache.SetLastModifiedFromFileDependencies();
          
            response.Content = stbOutput.ToString();
            response.ContentType = "text/css";

        }

        private string ToRgba(string color, string transparency)
        {
            var knownColor = color.To<KnownColor>();
            var rgb = knownColor.HasValue? Color.FromKnownColor(knownColor.Value) : ColorTranslator.FromHtml(color);
            return $"rgba({rgb.R},{rgb.G},{rgb.B},{transparency})";
        }
    }
}