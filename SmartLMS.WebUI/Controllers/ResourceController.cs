using System.Collections.Generic;
using SmartLMS.WebUI.ActionResults;
using SmartLMS.WebUI.Models;
using System.Configuration;
using System.Web.Mvc;

namespace SmartLMS.WebUI.Controllers
{
    public class ResourceController : Controller
    {
        // GET: Resource

        public ActionResult BrowserConfig()
        {
            var config = new Browserconfig
            {
                Msapplication = new Msapplication
                {
                    Tile = new Tile
                    {
                        Square150x150logo = new Square150x150logo { Src = "/Content/icons/mstile-150x150.png" },
                        TileColor = ConfigurationManager.AppSettings["ThemeColor"]
                    }
                }
            };

            return new XmlActionResult<Browserconfig>(config);
        }

        public ActionResult WebManifest()
        {
            var model = new WebManifestModel
            {
                name = "Smart LMS",
                short_name = "Smart LMS",
                background_color = ConfigurationManager.AppSettings["ThemeColor"],
                icons = new List<Icon>
                {
                   new Icon
                   {
                       sizes = "192x192",
                       src = "/Content/icons/android-chrome-192x192.png",
                       type = "image/png",
                   },
                    new Icon
                    {
                        sizes = "384x384",
                        src = "/Content/icons/android-chrome-384x384.png",
                        type = "image/png",
                    },
                },
                theme_color = ConfigurationManager.AppSettings["ThemeColor"],
            };

            return Json(model, JsonRequestBehavior.AllowGet);
        }
    }
}