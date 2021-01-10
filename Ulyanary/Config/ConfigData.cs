using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Ulyanary.Config
{
    public class ConfigData
    {
        [XmlElement]
        public RestlessFalcon RestlessFalcon;
        [XmlElement]
        public Ouman Ouman;
   }
    [XmlRoot("ConfigData")]
    public class RestlessFalcon
    {
        [XmlElement]
        public string url;
        [XmlElement]
        public string key;
        [XmlElement]
        public string sslThumbprint;
    }
    [XmlRoot("ConfigData")]
    public class Ouman
    {
        [XmlElement]
        public string url;
    }
}
