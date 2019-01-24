using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using My.XMLUtilities.Interface;


namespace My.XMLUtilities.Utilities
{
    
    public static class SerializeCData
    {

        static private string cdataCharacters = "<>&'\"";

        static public XmlCharacterData GetCData(string content)
        {
            if (content == null) return null;
            return new XmlDocument().CreateCDataSection(content);
        }

        static public XmlCharacterData GetCDataByContent(string content)
        {
            return GetCDataByContent(content, false);
        }

        static public XmlCharacterData GetCDataByContent(string content, bool forceCDATA)
        {
            if (content == null) return null;
            if (String.IsNullOrEmpty(content)) return new XmlDocument().CreateTextNode("");
            if (forceCDATA) return new XmlDocument().CreateCDataSection(content);

            foreach (char _char in cdataCharacters.ToCharArray())
            {   // return CDATA
                if (content.Contains(_char)) return new XmlDocument().CreateCDataSection(content);
            }
            // return string (not CDATA)
            return new XmlDocument().CreateTextNode(content);
        }

        static public string SetCData(XmlCharacterData content)
        {
            return content == null ? "" : content.Value.ToString();
        }
    }

    public static class SerializeFiles
    {
        static public bool FileExists<T>()
        {
            if (File.Exists(GetFileName<T>())) return true;
            return false;
        }

        static public bool FileDelete<T>()
        {
            string sFile = GetFileName<T>();
            if (File.Exists(sFile)) File.Delete(sFile);
            return !File.Exists(sFile);
        }

        static public string GetFileName<T>()
        {
            string appName = GetApplicationName<T>();
            string fileSuffix = GetFileSuffix<T>();
            string appFolder = "";
            if (ApplicationValue.UseCommonAppDataFolder)
            {
                string commonAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                appFolder = commonAppDataFolder + Path.DirectorySeparatorChar + appName;
            }
            else
            {
                appFolder = ApplicationValue.EntryExecPath;
            }
            return appFolder + Path.DirectorySeparatorChar + appName + "_" + fileSuffix + ".xml";
        }

        static public string GetApplicationName<T>()
        {
            T objectOut = GetNewInstance<T>();
            var refObjectOut = objectOut as IModelSerialized;
            return refObjectOut.AppInfo.AppName;
        }

        static public string GetFileSuffix<T>()
        {
            T objectOut = GetNewInstance<T>();
            return objectOut.GetType().Name;
        }

        static public T GetNewInstance<T>()
        {
            T objectOut = (T)Activator.CreateInstance(typeof(T), new object[] { });
            return objectOut;
        }

        static public bool Save<T>(T classContent)
        {
            return Save<T>(classContent, false);
        }
        static public bool Save<T>(T classContent, bool overrideChangeCache)
        {
            string appFile = GetFileName<T>();
            return SaveHandler<T>(classContent, appFile, overrideChangeCache);
        }

        static public bool SaveHandler<T>(T classContent, string appFile)
        {
            return SaveHandler<T>(classContent, appFile, false);
        }
        static public bool SaveHandler<T>(T classContent, string appFile, bool overrideChangeCache)
        {
            try
            {
                if (overrideChangeCache) ChangeCache.Instance.RemoveFromCache(appFile, "file");
                return SaveObjectToFile<T>(classContent, appFile);
            }
            catch 
            {
                throw;
            }
        }

        static private bool SaveObjectToFile<T>(T classContent, string appFile)
        {
            //remove from changecache when file not on disk during save
            if (!File.Exists(appFile)) ChangeCache.Instance.RemoveFromCache(appFile, "file");

            //skip serializing if object hasn't changed
            if (!ChangeCache.Instance.ObjectHasChanged<T>(appFile, "file", classContent)) return true;

            //create any missing folder 
            string appFolder = Path.GetDirectoryName(appFile);
            Encoding enc = new UTF8Encoding(false);

            if (!Directory.Exists(appFolder)) CreateFolderAndACL(appFolder);
            if (!Directory.Exists(appFolder)) throw new Exception(String.Format("Unable to create folder '{0}'", appFolder));

            //Serialize to XML and save default class content
            XmlSerializerNamespaces noNS = new XmlSerializerNamespaces();
            noNS.Add("", "");

            using (MemoryStream memStream = new MemoryStream())
            {
                XmlWriterSettings xmlWriterSettings = new System.Xml.XmlWriterSettings()
                {
                    // If set to true XmlWriter would close MemoryStream automatically and using would then do double dispose
                    // Code analysis does not understand that. That's why there is a suppress message.
                    CloseOutput = false,
                    Encoding = enc,
                    OmitXmlDeclaration = false,
                    Indent = true
                };
                using (XmlWriter xmlWrite = XmlWriter.Create(memStream, xmlWriterSettings))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(xmlWrite, classContent, noNS);
                    xmlWrite.Close();
                }

                File.WriteAllBytes(appFile, memStream.ToArray());
                memStream.Close();
            }

            return true;
        }

        static public T Load<T>()
        {
            string appFile = GetFileName<T>();
            return LoadHandler<T>(appFile);
        }

        static public T LoadHandler<T>(string appFile)
        {
            try
            {
                //Create missing file with default class data
                return LoadFileToObject<T>(appFile);
            }
            catch 
            {
                throw;
            }
        }

        static private T LoadFileToObject<T>(string appFile)
        {
            T objectOut = default(T);
            Version fileVer = null;

            if (string.IsNullOrEmpty(appFile) || !File.Exists(appFile)) { return objectOut; }

            objectOut = GetNewInstance<T>();
            var refObjectValidate = objectOut as IModelSerialized;
            if (refObjectValidate != null) refObjectValidate.PreLoadUpdate(appFile, out fileVer);

            using (FileStream fileStream = new FileStream(appFile, FileMode.Open, FileAccess.Read))
            {
                Type outType = typeof(T);

                XmlSerializer serializer = new XmlSerializer(outType);
                using (XmlReader reader = new XmlTextReader(fileStream))
                {
                    objectOut = (T)serializer.Deserialize(reader);
                    reader.Close();
                }
                fileStream.Close();
            }


            var refObjectLoaded = objectOut as IModelSerialized;
            if (objectOut != null && refObjectLoaded != null) refObjectLoaded.Loaded = true;
            //set initial serialized object
            ChangeCache.Instance.ObjectHasChanged<T>(appFile, "file", objectOut);


            //Do PostUpdate
            if (objectOut != null && refObjectLoaded != null)
            {
                //if a PostLoadUpdate occurred, remove file from cache so SaveObjectToFile is garanteed 
                if (refObjectLoaded.PostLoadUpdate(fileVer)) ChangeCache.Instance.RemoveFromCache(appFile, "file");
                //Save any changes done is post-load update
                SaveObjectToFile<T>(objectOut, appFile);
            }


            return objectOut;
        }

        static public bool CreateFolderAndACL(string appFolder)
        {
            //create any missing folder 
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
                // Get folder ACL
                DirectorySecurity appFolderSecurity = Directory.GetAccessControl(appFolder);

                try
                {
                    SecurityIdentifier builtinUsersSid = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
                    if (builtinUsersSid != null)
                    {
                        FileSystemAccessRule users = new FileSystemAccessRule(builtinUsersSid, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow);
                        appFolderSecurity.AddAccessRule(users);
                    }
                }
                catch (IdentityNotMappedException)
                {
                    //Do Nada                   
                }
                catch (ArgumentNullException)
                {
                    //Do Nada                   
                }
                catch
                {
                    throw; //else
                }

                try
                {
                    SecurityIdentifier accountDomainUsersSid = new SecurityIdentifier(WellKnownSidType.AccountDomainUsersSid, null);
                    if (accountDomainUsersSid != null)
                    {
                        FileSystemAccessRule domainUsers = new FileSystemAccessRule(accountDomainUsersSid, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow);
                        appFolderSecurity.AddAccessRule(domainUsers);
                    }
                }
                catch (IdentityNotMappedException)
                {
                    //Do Nada                   
                }
                catch (ArgumentNullException)
                {
                    //Do Nada                   
                }
                catch
                {
                    throw; //else
                }

                Directory.SetAccessControl(appFolder, appFolderSecurity);
            }
            return true;

        }
        




    }
}
