using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using My.XMLUtilities;
using System.ComponentModel;
using System.Security.Principal;


namespace My.XMLUtilities.Model
{

    public class appinfo
    {
        [XmlAttribute("appname")]
        public string AppName { get; set; }
        [XmlAttribute("major")]
        public int Major { get; set; }
        [XmlAttribute("minor")]
        public int Minor { get; set; }
        [XmlAttribute("build")]
        public int Build { get; set; }
        [XmlAttribute("revision")]
        public int Revision { get; set; }

        private Version _version;
        [XmlIgnore]
        public Version Version { get { return _version; } set { _version = value; } }


        public appinfo()
        {
            AppName = ApplicationValue.ApplicationName;
            _version = ApplicationValue.ApplicationVersion;
            Major = _version.Major;
            Minor = _version.Minor;
            Build = _version.Build;
            Revision = _version.Revision;
        }
    }

    public class IdNameDescActive
    {

        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlElement("active")]
        public bool Active { get; set; }

        public IdNameDescActive(string parentClassName)
        {
            parentClassName = parentClassName == null ? "Object" : parentClassName;
            Id = Guid.NewGuid().ToString().ToLower();
            Name = "[New " + parentClassName + "]";
            Description = "";
            Active = false;
        }
    }


}
