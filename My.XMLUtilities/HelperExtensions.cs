using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.Xml.Linq;

namespace My.XMLUtilities.Utilities
{
    public static class HelperExtensions
    {
        public static Boolean IsBase64String(this String value)
        {
            if (value == null || value.Length == 0 || value.Length % 4 != 0
                || value.Contains(" ") || value.Contains("\t") || value.Contains("\r") || value.Contains("\n"))
                return false;
            var index = value.Length - 1;
            if (value[index] == '=')
                index--;
            if (value[index] == '=')
                index--;
            for (var i = 0; i <= index; i++)
                if (IsInvalid(value[i]))
                    return false;
            return true;
        }
        // Make it private as there is the name makes no sense for an outside caller
        private static Boolean IsInvalid(char value)
        {
            var intValue = (Int32)value;
            if (intValue >= 48 && intValue <= 57)
                return false;
            if (intValue >= 65 && intValue <= 90)
                return false;
            if (intValue >= 97 && intValue <= 122)
                return false;
            return intValue != 43 && intValue != 47;
        }

        public static bool HasPropValue(object src, string propName)
        {
            return src.GetType().GetProperties().FirstOrDefault(a => a.Name.ToLower() == propName.ToLower()) != null;
        }

        public static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperties().FirstOrDefault(a => a.Name.ToLower() == propName.ToLower()).GetValue(src, null);
        }

        public static string Serialize<T>(this T value)
        {
            string retValue = string.Empty;
            if (value == null) return retValue;
            try
            {
                Encoding enc = new UTF8Encoding(false);
                XmlSerializerNamespaces noNS = new XmlSerializerNamespaces();
                noNS.Add("", "");

                using (MemoryStream memStream = new MemoryStream())
                {
                    XmlWriterSettings xmlWriterSettings = new XmlWriterSettings()
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
                        serializer.Serialize(xmlWrite, value, noNS);
                        xmlWrite.Close();
                    }

                    retValue = Encoding.UTF8.GetString(memStream.ToArray());
                    memStream.Close();
                }

                return retValue;
            }
            catch (Exception ex)
            {
                return retValue;
            }
        }

        public static DateTime Trim(this DateTime date, long ticks)
        {
            return new DateTime(date.Ticks - (date.Ticks % ticks), date.Kind);
        }

        



    }

}
