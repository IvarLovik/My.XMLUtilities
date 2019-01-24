using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using My.XMLUtilities.Utilities;
using My.XMLUtilities.Model;

namespace My.XMLUtilities
{



    public static class NamespaceUtils
    {
        public static bool ChangeNameSpaceAlias(string inputFileXML, string outputFileXML)
        {
            ApplicationValue.UseCommonAppDataFolder = false;
            string filename = SerializeFiles.GetFileName<XMLUtilitiesModel>();

            if (!File.Exists(filename))
                SerializeFiles.SaveHandler<XMLUtilitiesModel>(XMLUtilitiesModel.GetDefaultContent(), filename, true);

            XMLUtilitiesModel param = SerializeFiles.LoadHandler<XMLUtilitiesModel>(filename);
            List<NamespaceDoc> namespaceDocs = param.AppData.NamespaceUtils.NamespaceDocs;
            return ChangeNameSpaceAlias(inputFileXML, outputFileXML, namespaceDocs);
        }

        public static bool ChangeNameSpaceAlias(string inputFileXML, string outputFileXML, List<NamespaceDoc> namespaceDocs)
        {

            NamespaceUriAliasList currentDocListOfNSAlias = new NamespaceUriAliasList();

            try
            {
                if (String.IsNullOrEmpty(inputFileXML) || !File.Exists(inputFileXML)) throw new Exception(String.Format("Missing input file: '{0}'", String.IsNullOrEmpty(inputFileXML) ? "[none]" : inputFileXML));
                if (!String.IsNullOrEmpty(outputFileXML) && File.Exists(outputFileXML)) throw new Exception(String.Format("Output file exists: '{0}'", outputFileXML));

                //Validate input
                XDocument xDoc = XDocument.Load(inputFileXML);

                //find root namespace
                if (namespaceDocs.Exists(a=>a.RootNamespace == xDoc.Document.Root.Name.Namespace.NamespaceName))
                {
                    currentDocListOfNSAlias = namespaceDocs.Find(a => a.RootNamespace == xDoc.Document.Root.Name.Namespace.NamespaceName).Namespaces;
                }

                if (currentDocListOfNSAlias == null || currentDocListOfNSAlias.Count == 0)
                {
                    //No changes to do, return output as input
                    File.Copy(inputFileXML, outputFileXML);
                    return true;
                }

                //Include any missing namespace uri in file 
                //First all with names
                var allNamespaces = xDoc.Descendants().Attributes().Where(x => x.IsNamespaceDeclaration && x.Name.LocalName != "xmlns" && !String.IsNullOrEmpty(x.Name.LocalName));
                foreach (var ns in allNamespaces)
                    if (!currentDocListOfNSAlias.Exists(x => x.Uri == ns.Value)) currentDocListOfNSAlias.Add(new NamespaceUriAlias() { Alias = ns.Name.LocalName, Uri = ns.Value });
                
                //then all without names
                allNamespaces = xDoc.Descendants().Attributes().Where(x => x.IsNamespaceDeclaration && x.Name.LocalName != "xmlns");
                foreach (var ns in allNamespaces)
                    if (!currentDocListOfNSAlias.Exists(x => x.Uri == ns.Value)) currentDocListOfNSAlias.Add(new NamespaceUriAlias() { Alias = ns.Name.LocalName, Uri = ns.Value });

                //Ensure unique non blank namespace uris
                NamespaceUriAliasList tmpListOfNSAlias = new NamespaceUriAliasList();
                foreach (var ns in currentDocListOfNSAlias.FindAll(x => !String.IsNullOrEmpty(x.Uri)))
                    if (!tmpListOfNSAlias.Exists(x => x.Uri == ns.Uri)) tmpListOfNSAlias.Add(new NamespaceUriAlias() { Alias = ns.Alias, Uri = ns.Uri });
                currentDocListOfNSAlias = tmpListOfNSAlias;
                
                //Remove all namespacedeclarations
                xDoc.Descendants().Attributes().Where(x => x.IsNamespaceDeclaration).Remove();

                //Resolve common alias when alias = ""
                foreach (var nsAlias in currentDocListOfNSAlias)
                {
                    if (!String.IsNullOrEmpty(nsAlias.Alias)) continue;
                    if (nsAlias.Uri == "http://www.w3.org/2001/XMLSchema") { nsAlias.Alias = "xsd"; }
                    else if (nsAlias.Uri == "http://www.w3.org/2001/XMLSchema-instance") { nsAlias.Alias = "xsi"; }
                }

                //Set counter alias on duplicate aliases, only set counter alias from the secound in list (keep alias on the first occurence)
                string currItem = null;
                int counter = 0;
                foreach (var dupitem in currentDocListOfNSAlias.OrderBy(o => o.Alias))
                {
                    if (currItem == null || dupitem.Alias != currItem)
                    {
                        currItem = dupitem.Alias;
                    }
                    else
                    {
                        //number 2 of alias
                        counter++;
                        string newAlias = "ns" + counter.ToString();
                        while (currentDocListOfNSAlias.Exists(x => x.Alias == newAlias)) newAlias = "ns" + (counter++).ToString();
                        dupitem.Alias = newAlias;
                    }

                }

                //Load namespaces to root, start with the default if it exists, and procees in alias alphabetical order
                foreach (var nsAlias in currentDocListOfNSAlias.FindAll(x => String.IsNullOrEmpty(x.Alias)))
                    xDoc.Root.Add(new XAttribute("xmlns", nsAlias.Uri));

                foreach (var nsAlias in currentDocListOfNSAlias.FindAll(x => !String.IsNullOrEmpty(x.Alias)).OrderBy(o=>o.Alias.ToLower()))
                    xDoc.Root.Add(new XAttribute(XNamespace.Xmlns + nsAlias.Alias, nsAlias.Uri));

                //Save output file
                xDoc.Save(outputFileXML);

                return true;
            }
            catch 
            {
                throw;
            }

        }


    }
}
