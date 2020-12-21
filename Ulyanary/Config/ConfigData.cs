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
}
