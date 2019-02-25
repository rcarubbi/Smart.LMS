using System.Collections.Generic;

namespace SmartLMS.WebUI.Models
{

    public class Icon
    {
        public string src { get; set; }
        public string sizes { get; set; }
        public string type { get; set; }
    }

    public class WebManifestModel
    {
        public string name { get; set; }
        public string short_name { get; set; }
        public List<Icon> icons { get; set; }
        public string theme_color { get; set; }
        public string background_color { get; set; }
    }

}