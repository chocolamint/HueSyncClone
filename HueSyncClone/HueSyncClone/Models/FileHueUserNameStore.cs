using System.IO;
using System.Reflection;

namespace HueSyncClone.Models
{
    public class FileHueUserNameStore : IHueUserNameStore
    {
        private static string AuthPath => Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "auth");

        public string Load() => File.Exists(AuthPath) ? File.ReadAllText(AuthPath) : null;

        public void Save(string userName) => File.WriteAllText(AuthPath, userName);
    }
}