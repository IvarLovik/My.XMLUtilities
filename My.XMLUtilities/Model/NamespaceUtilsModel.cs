using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using My.XMLUtilities.Interface;

namespace My.XMLUtilities.Model
{
    [XmlRoot("xmlutilities")]
    public class XMLUtilitiesModel : IModelSerialized
    {
        [XmlIgnore]
        public bool Loaded { get; set; }

        [XmlElement("appinfo")]
        public appinfo AppInfo { get; set; }

        [XmlElement("appdata")]
        public AppData_XMLUtilities AppData { get; set; }


        public XMLUtilitiesModel()
        {
            AppInfo = new appinfo();
            AppData = new AppData_XMLUtilities();
        }

        public static XMLUtilitiesModel GetDefaultContent()
        {
            XMLUtilitiesModel param = new XMLUtilitiesModel();
            param.AppData.NamespaceUtils.NamespaceDocs.Add(
                    new NamespaceDoc()
                    {
                        RootNamespace = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2",
                        Namespaces = new NamespaceUriAliasList
                        {
                            new NamespaceUriAlias() {Alias = "", Uri = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2" },
                            new NamespaceUriAlias() {Alias = "cbc", Uri = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2" },
                            new NamespaceUriAlias() {Alias = "cec", Uri = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2" },
                            new NamespaceUriAlias() {Alias = "cac", Uri = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2" }
                        }
                    });

            param.AppData.NamespaceUtils.NamespaceDocs.Add(
                    new NamespaceDoc()
                    {
                        RootNamespace = "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2",
                        Namespaces = new NamespaceUriAliasList
                         {
                            new NamespaceUriAlias() {Alias = "", Uri = "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2" },
                            new NamespaceUriAlias() {Alias = "cbc", Uri = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2" },
                            new NamespaceUriAlias() {Alias = "cec", Uri = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2" },
                            new NamespaceUriAlias() {Alias = "cac", Uri = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2" }
                         }
                    });
            return param;
        }

        public bool PreLoadUpdate(string appFile, out Version orgVersion)
        {
            orgVersion = ApplicationValue.ApplicationVersion;
            return true;
        }

        public bool PostLoadUpdate(Version orgVersion)
        {
            return false;
        }
    }

    public class AppData_XMLUtilities
    {

        [XmlElement("namespaceutils")]
        public NamespaceUtilsModel NamespaceUtils { get; set; }


        public AppData_XMLUtilities()
        {
            NamespaceUtils = new NamespaceUtilsModel();
        }
    }



    public class NamespaceUtilsModel
    {

        [XmlArray("namespacedocs"), XmlArrayItem(typeof(NamespaceDoc), ElementName = "namespacedoc")]
        public NamespaceDocList NamespaceDocs { get; set; }

        public NamespaceUtilsModel()
        {
          NamespaceDocs = new NamespaceDocList();
        }
    }



    public class NamespaceDocList : List<NamespaceDoc>
    {

    }

    public class NamespaceDoc
    {
        [XmlElement("rootnamespace")]
        public string RootNamespace { get; set; }

        [XmlArray("namespaces"), XmlArrayItem(typeof(NamespaceUriAlias), ElementName = "namespace")]
        public NamespaceUriAliasList Namespaces { get; set; }
        public NamespaceDoc()
        {
            Namespaces = new NamespaceUriAliasList();
        }
    }


    public class NamespaceUriAliasList : List<NamespaceUriAlias>
    { }


    public class NamespaceUriAlias
    {
        [XmlAttribute("alias")]
        public string Alias { get; set; }
        [XmlAttribute("uri")]
        public string Uri { get; set; }
    }




 



}
