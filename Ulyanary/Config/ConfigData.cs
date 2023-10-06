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
        [XmlArray("Shelly"), XmlArrayItem(typeof(Device))]
        public List<Device> ShellyDevices;
        [XmlArray("Fronius"), XmlArrayItem(typeof(Device))]
        public List<Device> FroniusDevices;
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
    [XmlRoot("ConfigData")]
    public class Device
    {
        [XmlElement]
        public string Model;
        [XmlElement]
        public string Name;
        [XmlElement]
        public string IP;
        public double CounterValue;
        public bool InitialPoll = true;

    }
}
