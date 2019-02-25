using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace SmartLMS.WebUI.Models
{
    [XmlRoot(ElementName = "square150x150logo")]
    public class Square150x150logo
    {
        [XmlAttribute(AttributeName = "src")]
        public string Src { get; set; }
    }

    [XmlRoot(ElementName = "tile")]
    public class Tile
    {
        [XmlElement(ElementName = "square150x150logo")]
        public Square150x150logo Square150x150logo { get; set; }
        [XmlElement(ElementName = "TileColor")]
        public string TileColor { get; set; }
    }

    [XmlRoot(ElementName = "msapplication")]
    public class Msapplication
    {
        [XmlElement(ElementName = "tile")]
        public Tile Tile { get; set; }
    }

    [XmlRoot(ElementName = "browserconfig")]
    public class Browserconfig
    {
        [XmlElement(ElementName = "msapplication")]
        public Msapplication Msapplication { get; set; }
    }
}