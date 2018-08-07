using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Volvox.Helios.Core.Modules.Common;

namespace Volvox.Helios.Core.Modules
{
    public class Metadata
    {
        public List<ModuleMetadata> FromJson { get; set; }
        private string JsonPath { get; set; }

        public Metadata()
        {
            JsonPath = "../Volvox.Helios.Core/Modules/metadata.json";
            
            if (File.Exists(JsonPath))
            {
                string json = File.ReadAllText(JsonPath);
                FromJson = JsonConvert.DeserializeObject<List<ModuleMetadata>>(json);
            }
            else
            {
                FromJson = new List<ModuleMetadata>();
                string json = JsonConvert.SerializeObject(FromJson);
                File.WriteAllText(JsonPath, json);
            }          
        }

        public struct ModuleMetadata
        {
            public string Name;
            public string Version;
            public string Synopsis;
            public ReleaseState ReleaseState;
        }
    }
}