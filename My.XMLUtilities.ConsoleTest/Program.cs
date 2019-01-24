using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.XMLUtilities.Model;

namespace My.XMLUtilities.ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {

            /*
            Change this....
                xmlns="urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2" 
                xmlns:ns2="urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2" 
                xmlns:ns3="urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2" 
                xmlns:ns4="urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2"

            To this....
                xmlns:cbc="urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2" 
                xmlns:cec="urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2" 
                xmlns:cac="urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2" 
                xmlns="urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2"

             */

            try
            {

                //manually setting parameter NamespaceDocList
                NamespaceDocList nsDocs = new NamespaceDocList
                {
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
                    },
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
                    },
                };

                
                string outputSuffix = "_output";
                List<string> testFiles = Directory.GetFiles(@".\TestFiles", "*.xml", SearchOption.TopDirectoryOnly).ToList();

                //ChangeNameSpaceAlias using NamespaceDocList parameter
                foreach (var testFile in testFiles)
                {
                    if (Path.GetFileNameWithoutExtension(testFile).EndsWith(outputSuffix, StringComparison.InvariantCultureIgnoreCase)) continue;

                    string inputXML = testFile;
                    string outputXML = Path.Combine(Path.GetDirectoryName(testFile), Path.GetFileNameWithoutExtension(testFile) + outputSuffix + Path.GetExtension(testFile));

                    File.Delete(outputXML);
                    NamespaceUtils.ChangeNameSpaceAlias(inputXML, outputXML, nsDocs);
                }


                testFiles = Directory.GetFiles(@".\TestFiles", "*.xml", SearchOption.TopDirectoryOnly).ToList();
                //ChangeNameSpaceAlias using saved parameter file
                //When file is missing a default file will be created in the application folder. Filename, [running application]_XMLUtilitiesModel.xml
                foreach (var testFile in testFiles)
                {
                    if (Path.GetFileNameWithoutExtension(testFile).EndsWith(outputSuffix, StringComparison.InvariantCultureIgnoreCase)) continue;

                    string inputXML = testFile;
                    string outputXML = Path.Combine(Path.GetDirectoryName(testFile), Path.GetFileNameWithoutExtension(testFile) + outputSuffix + Path.GetExtension(testFile));

                    File.Delete(outputXML);
                    NamespaceUtils.ChangeNameSpaceAlias(inputXML, outputXML);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }

        }
    }
}
