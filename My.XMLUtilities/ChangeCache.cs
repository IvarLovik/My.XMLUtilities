using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Timers;

namespace My.XMLUtilities.Utilities
{
    public class ChangeCache
    {
        public class CacheObj
        {
            public string Key { get; set; }
            public string Type { get; set; }
            public DateTime Time { get; set; }

            private string hash;
            public string Hash { get { return hash; } set { hash = value; Time = DateTime.Now; } }

            public CacheObj()
            {
                Time = DateTime.Now;
            }

        }

        private static readonly object _locker = new object();
        private static volatile ChangeCache _instance;
        private static List<CacheObj> _model;
        private static Timer cacheCleanup = null;

        public static ChangeCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_locker)
                    {
                        if (_instance == null) _instance = new ChangeCache();
                    }
                }
                return _instance;
            }
        }

        public static List<CacheObj> ObjectHash
        {
            get
            {
                if (_model == null)
                {
                    lock (_locker)
                    {
                        if (_model == null) _model = new List<CacheObj>();
                    }
                }
                return _model;
            }
        }

        public ChangeCache()
        {
            //Start timer
            cacheCleanup = new Timer();
            cacheCleanup.Interval = 1000 * 60 * 1; //1 min
            cacheCleanup.Elapsed += new ElapsedEventHandler(cacheCleanup_Tick);

            //Start heartbeat
            cacheCleanup.Start();
        }

        private void cacheCleanup_Tick(object sender, EventArgs e)
        {
            //check if all processes are alive
            cacheCleanup.Enabled = false;
            try
            {
                List<CacheObj> toBeDeleted = new List<CacheObj>();
                foreach (CacheObj cacheObj in ObjectHash.FindAll(a => a.Time.AddMinutes(5) < DateTime.Now))
                {
                    //test files
                    if (cacheObj.Type.ToLower() == "file")
                    {
                        if (!File.Exists(cacheObj.Key)) toBeDeleted.Add(cacheObj);
                    }
                }

                foreach (CacheObj deleteItem in toBeDeleted) ObjectHash.Remove(deleteItem);

            }
            catch
            { }
            finally
            {
                cacheCleanup.Enabled = true;
            }
        }



        public bool ObjectHasChanged<T>(string key, string type, T obj)
        {
            if (obj == null) return true;
            key = key.ToLower();
            string objSer = obj.Serialize<T>();
            if (String.IsNullOrEmpty(objSer)) return true;

            CacheObj foundCacheObj = ObjectHash.Find(a => a.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase) && a.Type.Equals(type, StringComparison.InvariantCultureIgnoreCase));

            if (foundCacheObj == null)
            {
                ObjectHash.Add(new CacheObj() { Key = key, Type = type, Hash = GetSHA1Hash(objSer, Encoding.UTF8).ToString() });
                return true;
            }
            else if (foundCacheObj.Hash != GetSHA1Hash(objSer, Encoding.UTF8).ToString())
            {
                foundCacheObj.Hash = GetSHA1Hash(objSer, Encoding.UTF8).ToString();
                return true;
            }
            return false;
        }
        public bool RemoveFromCache(string key, string type)
        {
            CacheObj foundCacheObj = ObjectHash.Find(a => a.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase) && a.Type.Equals(type, StringComparison.InvariantCultureIgnoreCase));
            if (foundCacheObj != null) ObjectHash.Remove(foundCacheObj);
            return true;
        }


        public static string GetSHA1Hash(string content, Encoding encoding)
        {
            string retVal;
            byte[] byteContent = encoding.GetBytes(content);
            using (var cryptoProvider = new SHA1CryptoServiceProvider())
            {
                retVal = BitConverter.ToString(cryptoProvider.ComputeHash(byteContent));
            }
            return retVal;
        }

    }





}
