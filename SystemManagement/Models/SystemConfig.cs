using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SystemManagement.Models
{
    [XmlRoot("SystemConfig")]
    public class SystemConfig
    {
        [XmlArray("envKeys")]
        [XmlArrayItem("envKey")]
        public List<string> EnvKeys { get; set; } = new();

        [XmlArray("connections")]
        [XmlArrayItem("connection")]
        public List<ConnectionItem> Connections { get; set; } = new();

        [XmlArray("sites")]
        [XmlArrayItem("site")]
        public List<SiteItem> Sites { get; set; } = new();
    }

    public class ConnectionItem
    {
        [XmlAttribute("name")]
        public string Name { get; set; } = string.Empty;

        [XmlElement("host")]
        public string Host { get; set; } = string.Empty;

        [XmlElement("database")]
        public string Database { get; set; } = string.Empty;

        [XmlElement("username")]
        public string Username { get; set; } = string.Empty;

        [XmlElement("password")]
        public string Password { get; set; } = string.Empty;

        [XmlElement("trustCertifficate")] // Kept original XML typo: trustCertifficate
        public string TrustCertificate { get; set; } = string.Empty;
    }

    public class SiteItem
    {
        [XmlAttribute("name")]
        public string Name { get; set; } = string.Empty;

        [XmlArray("modules")]
        [XmlArrayItem("module")]
        public List<ModuleItem> Modules { get; set; } = new();
    }

    public class ModuleItem
    {
        [XmlAttribute("name")]
        public string Name { get; set; } = string.Empty;

        [XmlElement("folderPath")]
        public string FolderPath { get; set; } = string.Empty;

        [XmlElement("configurationPath")]
        public string ConfigurationPath { get; set; } = string.Empty;

        [XmlElement("configurationFileType")]
        public string ConfigurationFileType { get; set; } = string.Empty;

        [XmlElement("iisName")]
        public string IisName { get; set; } = string.Empty;

        [XmlElement("iisPort")]
        public string IisPort { get; set; } = string.Empty;
    }
}
