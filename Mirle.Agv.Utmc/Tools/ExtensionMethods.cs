using Newtonsoft.Json;

namespace Mirle.Agv.Utmc.Tools
{
    public static class ExtensionMethods
    {
        public static string GetJsonInfo(this object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }
    }
}
