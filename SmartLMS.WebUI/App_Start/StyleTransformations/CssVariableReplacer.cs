using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Text;
using System.Web.Optimization;

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
                content = content.Replace("#ThemeColor0.1Transparency#", ToRgba(ConfigurationManager.AppSettings["ThemeColor"], "0.1"));
                content = content.Replace("#ThemeColor0.12Transparency#", ToRgba(ConfigurationManager.AppSettings["ThemeColor"], "0.12"));
                content = content.Replace("#ThemeColor0.14Transparency#", ToRgba(ConfigurationManager.AppSettings["ThemeColor"], "0.14"));
                content = content.Replace("#ThemeColor0.2Transparency#", ToRgba(ConfigurationManager.AppSettings["ThemeColor"], "0.2"));
                content = content.Replace("#ThemeColor0.5Transparency#", ToRgba(ConfigurationManager.AppSettings["ThemeColor"], "0.5"));
                content = content.Replace("#ThemeColor0.7Transparency#", ToRgba(ConfigurationManager.AppSettings["ThemeColor"], "0.7"));
                content = content.Replace("#ThemeColor0.9Transparency#", ToRgba(ConfigurationManager.AppSettings["ThemeColor"], "0.9"));
                content = content.Replace("#ThemeColorBrighter20#", ChangeColorBright(ConfigurationManager.AppSettings["ThemeColor"], 20));
                content = content.Replace("#ThemeColorBrighter40#", ChangeColorBright(ConfigurationManager.AppSettings["ThemeColor"], 40));
                content = content.Replace("#ThemeColorDarker20#", ChangeColorBright(ConfigurationManager.AppSettings["ThemeColor"], -20));
                stbOutput.Append(content);
            }
            context.HttpContext.Response.Cache.SetLastModifiedFromFileDependencies();
          
            response.Content = stbOutput.ToString();
            response.ContentType = "text/css";

        }

        private string ChangeColorBright(string color, double bright)
        {
            var rgb = Enum.TryParse<KnownColor>(color, true, out var knownColor) ? Color.FromKnownColor(knownColor) : ColorTranslator.FromHtml(color);
            var newRed = Math.Max(Math.Min((int) (rgb.R * (1 + bright / 100)), 255), 0);
            var newGreen  = Math.Max(Math.Min((int) (rgb.G * (1 + bright / 100)), 255), 0);
            var newBlue = Math.Max(Math.Min((int) (rgb.B * (1 + bright / 100)), 255), 0);
            return $"rgb({newRed},{newGreen},{newBlue})";
        }

        private string ToRgba(string color, string transparency)
        {
            var rgb = Enum.TryParse<KnownColor>(color, true, out var knownColor) ? Color.FromKnownColor(knownColor) : ColorTranslator.FromHtml(color);
            return $"rgba({rgb.R},{rgb.G},{rgb.B},{transparency})";
        }
    }
}