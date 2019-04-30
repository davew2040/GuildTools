using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GuildTools.Tests
{
    public class Utilities
    {
        public static string ReadFromAssembly(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(
                assembly.GetManifestResourceNames().Single(r => r.EndsWith(resourceName))))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
