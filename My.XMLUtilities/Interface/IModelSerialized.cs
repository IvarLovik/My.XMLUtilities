using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using My.XMLUtilities.Model;

namespace My.XMLUtilities.Interface
{
    public interface IModelSerialized
    {
        appinfo AppInfo { get; set; }
        bool Loaded { get; set; }
        bool PreLoadUpdate(string appFile, out Version orgVersion);
        bool PostLoadUpdate(Version orgVersion);
    }
}
