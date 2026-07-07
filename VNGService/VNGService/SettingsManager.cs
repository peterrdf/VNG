using System.Runtime.InteropServices;
using System.Xml;

namespace VNGService
{
    public class SettingsManager
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public SettingsManager(IConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public Dictionary<string, string> LoadAPISettings()
        {
            var FileStorage = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "FileStorageLinux" : "FileStorage";
            var settingsDir = _configuration[$"{FileStorage}:SettingsDir"];
            if (string.IsNullOrEmpty(settingsDir))
            {
                throw new InvalidOperationException("Settings path is not configured.");
            }

            var settingsFilePath = Path.Combine(settingsDir, "api_settings.xml");
            if (!System.IO.File.Exists(settingsFilePath))
            {
                return new Dictionary<string, string>();
            }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(settingsFilePath);

                var apiKeyNodes = xmlDocument.SelectNodes("//services/service/apikey");
                if (apiKeyNodes == null)
                {
                    return new Dictionary<string, string>();
                }

                Dictionary<string, string> dicSettings = new();
                for (int i = 0; i < apiKeyNodes.Count; i++)
                {
                    var serviceNode = apiKeyNodes[i]?.ParentNode;
                    if (serviceNode != null)
                    {
                        var serviceName = serviceNode.Attributes?["name"]?.Value;
                        var apiKey = apiKeyNodes[i]?.InnerText;
                        if (!string.IsNullOrEmpty(serviceName) && !string.IsNullOrEmpty(apiKey))
                        {
                            dicSettings[serviceName] = apiKey;
                        }
                    }
                }

                return dicSettings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading API settings file.");

            }

            return new Dictionary<string, string>();
        }

        public void SaveAPISettings(Dictionary<string, string> dicSettings)
        {
            var FileStorage = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "FileStorageLinux" : "FileStorage";
            var settingsDir = _configuration[$"{FileStorage}:SettingsDir"];
            if (string.IsNullOrEmpty(settingsDir))
            {
                throw new InvalidOperationException("Settings path is not configured.");
            }

            XmlDocument xmlDocument = new XmlDocument();
            XmlElement rootElement = xmlDocument.CreateElement("services");
            xmlDocument.AppendChild(rootElement);

            foreach (var kvp in dicSettings)
            {
                XmlElement serviceElement = xmlDocument.CreateElement("service");
                serviceElement.SetAttribute("name", kvp.Key);

                XmlElement apiKeyElement = xmlDocument.CreateElement("apikey");
                apiKeyElement.InnerText = kvp.Value;
                serviceElement.AppendChild(apiKeyElement);

                rootElement.AppendChild(serviceElement);
            }

            var settingsFilePath = Path.Combine(settingsDir, "api_settings.xml");
            xmlDocument.Save(settingsFilePath);
        }
    }
}
