using System.Web.Optimization;
using SmartLMS.WebUI.App_Start.StyleTransformations;

namespace SmartLMS.WebUI
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/jquery.unobtrusive-ajax.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                "~/Scripts/jquery-ui-{version}.js",
                "~/Scripts/jquery.dropdown/jquery.dropdown.js",
                "~/Scripts/jquery.sortable/jquery.sortable.js",
                "~/Scripts/jquery.touchSwipe/jquery.touchSwipe.js"));

            bundles.Add(new ScriptBundle("~/Bundles/data").Include(
                "~/Scripts/handlebars.js",
                "~/Scripts/momentjs/moment-with-locales.js",
                "~/Scripts/jQuery.mask/jquery.mask.js"));


            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/dropzone").Include(
                "~/Scripts/dropzone/dropzone.js"));


            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.js",
                "~/Scripts/respond.js",
                "~/Scripts/bootstrap-datepicker/bootstrap-datepicker.js",
                "~/Scripts/bootstrap-multiselect/bootstrap-multiselect.js",
                "~/Scripts/material-kit/material.min.js",
                "~/Scripts/material-kit/nouislider.min.js",
                "~/Scripts/material-kit/material-kit.js",
                "~/Scripts/toastr/toastr.min.js",
                "~/Scripts/slimscroll/jquery.slimscroll.min.js",
                "~/Scripts/app.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/font-awesome.css",
                "~/Content/bootstrap.css",
                "~/Content/bootstrap-datepicker/bootstrap-datepicker.css",
                "~/Content/bootstrap-multiselect/bootstrap-multiselect.css",
                "~/Content/themes/notheme/jquery-ui.css",
                "~/Content/themes/notheme/jquery-ui.structure.css",
                "~/Content/addtohomescreen/addtohomescreen.css"
            ));
            var cssCustom = new StyleBundle("~/Content/csscustom").Include(
                "~/Content/material-kit/material-kit.css",
                "~/Content/toastr/toastr.min.css",
                "~/Content/jquery.dropdown/jquery.dropdown.css",
                "~/Content/multi-carousel.css",
                "~/Content/figure-caption.css",
                "~/Content/listgroup.css",
                "~/Content/Site.css");
            cssCustom.Transforms.Add(new CssVariableReplacer());
            bundles.Add(cssCustom);

            // dropZone styles
            bundles.Add(new StyleBundle("~/Content/dropzone/dropZoneStyles").Include(
                "~/Content/dropzone/basic.css",
                "~/Content/dropzone/dropzone.css"));
        }
    }
}