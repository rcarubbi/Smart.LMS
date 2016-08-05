using System.Web;
using System.Web.Optimization;

namespace SmartLMS.WebUI
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery.unobtrusive-ajax.js",
                        "~/Scripts/jquery-ui-{version}.js",
                        "~/Scripts/jquery.dropdown/jquery.dropdown.js",
                        "~/Scripts/handlebars.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js",
                      "~/Scripts/material-kit/material.min.js",
                      "~/Scripts/material-kit/nouislider.min.js",
                      "~/Scripts/material-kit/material-kit.js",
                      "~/Scripts/toastr/toastr.min.js",
                      "~/Scripts/slimscroll/jquery.slimscroll.min.js",
                      "~/Scripts/app.js"));
                      

            bundles.Add(new StyleBundle("~/Content/css").Include(
               "~/Content/font-awesome.css",
               "~/Content/bootstrap.css",
               "~/Content/themes/notheme/jquery-ui.css",
               "~/Content/themes/notheme/jquery-ui.structure.css",
               "~/Content/material-kit/material-kit.css",
               "~/Content/toastr/toastr.min.css",
               "~/Content/jquery.dropdown/jquery.dropdown.css",
               "~/Content/multi-carousel.css",
               "~/Content/figure-caption.css",
               "~/Content/listgroup.css",
               "~/Content/Site.css"));

        }
    }
}
