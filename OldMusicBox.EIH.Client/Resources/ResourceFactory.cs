using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Resources
{
    /// <summary>
    /// Embedded resources
    /// </summary>
    public class ResourceFactory
    {
        public enum EmbeddedResource
        {
            RequestPostBinding,
            ResponsePostBinding
        }

        private Dictionary<EmbeddedResource, string> resourceNames =
            new Dictionary<EmbeddedResource, string>()
            {
                {
                    EmbeddedResource.RequestPostBinding, "RequestPostBinding.html"
                },
                {
                    EmbeddedResource.ResponsePostBinding, "ResponsePostBinding.html"
                }
            };

        public string Create( EmbeddedResource resource )
        {
            if (resourceNames.ContainsKey(resource))
            {
                return Create(resourceNames[resource]);
            }
            else
            {
                return string.Empty;
            }
        }

        private string Create( string resource )
        {
            var assembly     = typeof(ResourceFactory).Assembly;
            var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(name => name.EndsWith(resource));

            if (string.IsNullOrEmpty(resourceName))
                throw new ArgumentException(string.Format("Resource {0} not found", resource));

            var resourceStream = assembly.GetManifestResourceStream(resourceName);

            using (var reader = new StreamReader(resourceStream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
