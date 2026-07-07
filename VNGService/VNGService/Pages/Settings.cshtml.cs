using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RDF;
using System.Runtime.InteropServices;
using System.Xml;

namespace VNGService.Pages
{
    [IgnoreAntiforgeryToken]
    public class SettingsModel : PageModel
    {
        private readonly ILogger<SettingsModel> _logger;
        private readonly IConfiguration _configuration;

        public SettingsModel(ILogger<SettingsModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPostAPIKey([FromForm] string service, [FromForm] string key)
        {
            if (string.IsNullOrEmpty(service) || string.IsNullOrEmpty(key))
            {
                _logger.LogError("Service or key is null or empty.");
                return new JsonResult(new { Success = false, Result = "Error: Service or key is null or empty." });
            }

            try
            {
                switch (service.ToLower())
                {
                    case "dso":
                        UpdateAPIKey(service, key);
                        break;
                    case "bag":
                        UpdateAPIKey(service, key);
                        break;                    
                    default:
                        throw new ArgumentException($"Unknown service: {service}");
                }

                return new JsonResult(new { Success = true, Result = "API key updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during API key update.");
                return new JsonResult(new { Success = false, Result = "Error: An unexpected error occurred." });
            }
        }

        private void UpdateAPIKey(string service, string key)
        {
            var FileStorage = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "FileStorageLinux" : "FileStorage";
            var settingsDir = _configuration[$"{FileStorage}:SettingsDir"];
            if (string.IsNullOrEmpty(settingsDir))
            {
                throw new InvalidOperationException("Settings path is not configured.");
            }

            SettingsManager settingsManager = new SettingsManager(_configuration, _logger);
            Dictionary<string, string> dicSettings = settingsManager.LoadAPISettings();

            switch (service.ToLower())
            {
                case "dso":
                    if (dicSettings.ContainsKey("dso"))
                    {
                        dicSettings["dso"] = key;
                    }
                    else
                    {
                        dicSettings.Add("dso", key);
                    }
                    break;
                case "bag":
                    if (dicSettings.ContainsKey("bag"))
                    {
                        dicSettings["bag"] = key;
                    }
                    else
                    {
                        dicSettings.Add("bag", key);
                    }
                    break;                
                default:
                    throw new ArgumentException($"Unknown service: {service}");
            }
            
            settingsManager.SaveAPISettings(dicSettings);
        }
    }
}
