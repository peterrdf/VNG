using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RDF;
using System;

namespace VNGService.Pages
{
    public class CSGModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DSOModel> _logger;

        public CSGModel(IConfiguration configuration, ILogger<DSOModel> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public IActionResult OnGetCreateCylinder(double length, double radius, long segmentationParts)
        {
            var base64Content = string.Empty;

            long owlModel = engine.CreateModel();
            try
            {
                long owlClassCylinder = engine.GetClassByName(owlModel, "Cylinder");
                long owlInstanceCylinder = engine.CreateInstance(owlClassCylinder, "");
                engine.SetDatatypeProperty(owlInstanceCylinder, engine.GetPropertyByName(owlModel, "length"), length);
                engine.SetDatatypeProperty(owlInstanceCylinder, engine.GetPropertyByName(owlModel, "radius"), radius);
                engine.SetDatatypeProperty(owlInstanceCylinder, engine.GetPropertyByName(owlModel, "segmentationParts"), segmentationParts);

                var modelPath = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.bin");
                engine.SaveModel(owlModel, modelPath);
                base64Content = Convert.ToBase64String(System.IO.File.ReadAllBytes(modelPath));
                System.IO.File.Delete(modelPath);

                return new JsonResult(new { Geometry = base64Content });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cylinder model.");
                throw;
            }
            finally
            {
                engine.CloseModel(owlModel);
            }
        }
    }
}
