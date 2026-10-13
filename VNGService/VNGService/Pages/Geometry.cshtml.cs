using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RDF;
using System.Runtime.InteropServices;
using VNGService.Models;

namespace VNGService.Pages
{
    [IgnoreAntiforgeryToken]
    public class GeometryModel : PageModel
    {
        private readonly ILogger<LandRegistryModel> _logger;
        private readonly IConfiguration _configuration;

        public GeometryModel(IConfiguration configuration, ILogger<LandRegistryModel> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPostBBox([FromForm] string base64Content)
        {
            var isLinuxPlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            var FileStorage = isLinuxPlatform ? "FileStorageLinux" : "FileStorage";

            var modelsDir = _configuration[$"{FileStorage}:ModelsDir"];
            if (string.IsNullOrEmpty(modelsDir))
            {
                _logger.LogError("Models path is not configured.");
                return new JsonResult(new { Projection = "" });
            }

            var tempId = Guid.NewGuid().ToString("N");
            var modelPath = Path.Combine(modelsDir, $"{tempId}.bin");
            System.IO.File.WriteAllBytes(modelPath, Convert.FromBase64String(base64Content));

            long owlModel = Engine.CreateModel(); 
            Engine.SetOverrideFileIO(owlModel, Engine.flagbit0 + Engine.flagbit4, Engine.flagbit0 + Engine.flagbit4);

            long owlInstanceInput = Engine.ImportModel(owlModel, modelPath);

            System.IO.File.Delete(modelPath);

            double[]? transformationMatrix = null;
            double[] startVector = new double[3];
            double[] endVector = new double[3];
#pragma warning disable CS8604 // Possible null reference argument.
            Engine.GetBoundingBox(owlInstanceInput, transformationMatrix, startVector, endVector);
#pragma warning restore CS8604 // Possible null reference argument.

            Engine.CloseModel(owlModel);

            return new JsonResult(new[] { startVector[0], startVector[1], startVector[2], endVector[0], endVector[1], endVector[2] });
        }

        public IActionResult OnPostProjection([FromForm] string base64Content)
        {
            var isLinuxPlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            var FileStorage = isLinuxPlatform ? "FileStorageLinux" : "FileStorage";

            var modelsDir = _configuration[$"{FileStorage}:ModelsDir"];
            if (string.IsNullOrEmpty(modelsDir))
            {
                _logger.LogError("Models path is not configured.");
                return new JsonResult(new { Projection = "" });
            }

            var tempId = Guid.NewGuid().ToString("N");
            var modelPath = Path.Combine(modelsDir, $"{tempId}.bin");
            System.IO.File.WriteAllBytes(modelPath, Convert.FromBase64String(base64Content));

            long owlModel = Engine.CreateModel(); 
            Engine.SetOverrideFileIO(owlModel, Engine.flagbit0 + Engine.flagbit4, Engine.flagbit0 + Engine.flagbit4);

            long owlInstanceInput = Engine.ImportModel(owlModel, modelPath);

            System.IO.File.Delete(modelPath);

            long owlInstancePlane = Engine.CreateInstance(Engine.GetClassByName(owlModel, "Plane"));
            Engine.SetDatatypeProperty(owlInstancePlane, Engine.GetPropertyByName(owlModel, "A"), 0.0);
            Engine.SetDatatypeProperty(owlInstancePlane, Engine.GetPropertyByName(owlModel, "B"), 0.0);
            Engine.SetDatatypeProperty(owlInstancePlane, Engine.GetPropertyByName(owlModel, "C"), 1.0);
            Engine.SetDatatypeProperty(owlInstancePlane, Engine.GetPropertyByName(owlModel, "D"), 0.0);

            long owlInstanceVector3 = Engine.CreateInstance(Engine.GetClassByName(owlModel, "Vector3"));
            Engine.SetDatatypeProperty(owlInstanceVector3, Engine.GetPropertyByName(owlModel, "z"), 1.0);

            long owlInstanceShadow = Engine.CreateInstance(Engine.GetClassByName(owlModel, "Shadow"));
            Engine.SetObjectProperty(owlInstanceShadow, Engine.GetPropertyByName(owlModel, "plane"), owlInstancePlane);
            Engine.SetObjectProperty(owlInstanceShadow, Engine.GetPropertyByName(owlModel, "lightDirection"), owlInstanceVector3);
            Engine.SetObjectProperty(owlInstanceShadow, Engine.GetPropertyByName(owlModel, "object"), owlInstanceInput);
            Engine.SetDatatypeProperty(owlInstanceShadow, Engine.GetPropertyByName(owlModel, "type"), (long)1);            

            modelPath = Path.Combine(modelsDir, $"projection_{tempId}.bin");
            Engine.SaveInstanceTree(owlInstanceShadow, modelPath);

            var projectionBase64Content = Convert.ToBase64String(System.IO.File.ReadAllBytes(modelPath));

            System.IO.File.Delete(modelPath);

            Engine.CloseModel(owlModel);            

            return new JsonResult(new { Projection = projectionBase64Content });
        }

        public IActionResult OnPostTopologicalRelation([FromForm] string base64Content1, [FromForm] string base64Content2)
        {
            var isLinuxPlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            var FileStorage = isLinuxPlatform ? "FileStorageLinux" : "FileStorage";

            var modelsDir = _configuration[$"{FileStorage}:ModelsDir"];
            if (string.IsNullOrEmpty(modelsDir))
            {
                _logger.LogError("Models path is not configured.");
                return new JsonResult(new { TopologicalRelation = "error" });
            }

            string topologicalRelation = "DISJOINT";

            long owlModel = Engine.CreateModel(); 
            Engine.SetOverrideFileIO(owlModel, Engine.flagbit0 + Engine.flagbit4, Engine.flagbit0 + Engine.flagbit4);

            try
            {
                var tempId = Guid.NewGuid().ToString("N");

                var modelPath1 = Path.Combine(modelsDir, $"{tempId}_1.bin");
                System.IO.File.WriteAllBytes(modelPath1, Convert.FromBase64String(base64Content1));

                long owlInstanceInputOne = Engine.ImportModel(owlModel, modelPath1);

                System.IO.File.Delete(modelPath1);

                var modelPath2 = Path.Combine(modelsDir, $"{tempId}_2.bin");
                System.IO.File.WriteAllBytes(modelPath2, Convert.FromBase64String(base64Content2));

                long owlInstanceInputTwo = Engine.ImportModel(owlModel, modelPath2);

                System.IO.File.Delete(modelPath2);

                topologicalRelation = GetTopologicalRelation(owlModel, owlInstanceInputOne, owlInstanceInputTwo);

                Engine.CloseModel(owlModel);

                return new JsonResult(new { TopologicalRelation = topologicalRelation });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during topological relation calculation.");
                return new JsonResult(new { TopologicalRelation = "error" });
            }            
        }

        public IActionResult OnPostTopologicalRelation2([FromForm] string base64Content1, [FromForm] string base64Content2)
        {
            var isLinuxPlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            var FileStorage = isLinuxPlatform ? "FileStorageLinux" : "FileStorage";

            var modelsDir = _configuration[$"{FileStorage}:ModelsDir"];
            if (string.IsNullOrEmpty(modelsDir))
            {
                _logger.LogError("Models path is not configured.");
                return new JsonResult(new { TopologicalRelation = "error" });
            }

            string topologicalRelation = "DISJOINT";

            long owlModel = Engine.CreateModel(); 
            Engine.SetOverrideFileIO(owlModel, Engine.flagbit0 + Engine.flagbit4, Engine.flagbit0 + Engine.flagbit4);

            try
            {
                var tempId = Guid.NewGuid().ToString("N");

                var modelPath1 = Path.Combine(modelsDir, $"{tempId}_1.bin");
                System.IO.File.WriteAllBytes(modelPath1, Convert.FromBase64String(base64Content1));

                long owlInstanceInputOne = Engine.ImportModel(owlModel, modelPath1);

                System.IO.File.Delete(modelPath1);

                var base64Datas = base64Content2.Split('|');
                if (base64Datas.Length > 1)
                {
                    for (int i = 0; i < base64Datas.Length; i++)
                    {
                        var modelPath2 = Path.Combine(modelsDir, $"{tempId}_2_{i}.bin");
                        System.IO.File.WriteAllBytes(modelPath2, Convert.FromBase64String(base64Datas[i]));

                        long owlInstanceInputTwo = Engine.ImportModel(owlModel, modelPath2);

                        System.IO.File.Delete(modelPath2);

                        topologicalRelation = GetTopologicalRelation(owlModel, owlInstanceInputOne, owlInstanceInputTwo);
                        if (topologicalRelation != "DISJOINT")
                        {
                            break;
                        }
                    }
                }
                else
                {
                    var modelPath2 = Path.Combine(modelsDir, $"{tempId}_2.bin");
                    System.IO.File.WriteAllBytes(modelPath2, Convert.FromBase64String(base64Content2));

                    long owlInstanceInputTwo = Engine.ImportModel(owlModel, modelPath2);

                    System.IO.File.Delete(modelPath2);

                    topologicalRelation = GetTopologicalRelation(owlModel, owlInstanceInputOne, owlInstanceInputTwo);
                }                

                Engine.CloseModel(owlModel);

                return new JsonResult(new { TopologicalRelation = topologicalRelation });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during topological relation calculation.");
                return new JsonResult(new { TopologicalRelation = "error" });
            }
        }

        private string GetTopologicalRelation(long owlModel, long owlInstanceInputOne, long owlInstanceInputTwo)
        {
            string topologicalRelation = "DISJOINT"; // "First AND Second DISJOINT";

            // Intersection
            long owlInstanceBooleanOperation2D_A_and_B = Engine.CreateInstance(Engine.GetClassByName(owlModel, "BooleanOperation2D"));
            Engine.SetObjectProperty(owlInstanceBooleanOperation2D_A_and_B, Engine.GetPropertyByName(owlModel, "firstObject"), owlInstanceInputOne);
            Engine.SetObjectProperty(owlInstanceBooleanOperation2D_A_and_B, Engine.GetPropertyByName(owlModel, "secondObject"), owlInstanceInputTwo);            
            Engine.SetDatatypeProperty(owlInstanceBooleanOperation2D_A_and_B, Engine.GetPropertyByName(owlModel, "type"), (long)3);

            Engine.CalculateInstance(owlInstanceBooleanOperation2D_A_and_B, out long vertexBufferSize1, out long indexBufferSize1);
            if ((vertexBufferSize1 != 0) && (indexBufferSize1 != 0))
            {
                // Difference
                long owlInstanceBooleanOperation2D_A_min_B = Engine.CreateInstance(Engine.GetClassByName(owlModel, "BooleanOperation2D"));
                Engine.SetObjectProperty(owlInstanceBooleanOperation2D_A_min_B, Engine.GetPropertyByName(owlModel, "firstObject"), owlInstanceInputOne);
                Engine.SetObjectProperty(owlInstanceBooleanOperation2D_A_min_B, Engine.GetPropertyByName(owlModel, "secondObject"), owlInstanceInputTwo);                
                Engine.SetDatatypeProperty(owlInstanceBooleanOperation2D_A_min_B, Engine.GetPropertyByName(owlModel, "type"), (long)1);

                Engine.CalculateInstance(owlInstanceBooleanOperation2D_A_min_B, out long vertexBufferSize2, out long indexBufferSize2);
                if ((vertexBufferSize2 != 0) && (indexBufferSize2 != 0))
                {
                    // Inverse Difference
                    long owlInstanceBooleanOperation2D_B_min_A = Engine.CreateInstance(Engine.GetClassByName(owlModel, "BooleanOperation2D"));
                    Engine.SetObjectProperty(owlInstanceBooleanOperation2D_B_min_A, Engine.GetPropertyByName(owlModel, "firstObject"), owlInstanceInputTwo);
                    Engine.SetObjectProperty(owlInstanceBooleanOperation2D_B_min_A, Engine.GetPropertyByName(owlModel, "secondObject"), owlInstanceInputOne);                    
                    Engine.SetDatatypeProperty(owlInstanceBooleanOperation2D_B_min_A, Engine.GetPropertyByName(owlModel, "type"), (long)2);

                    Engine.CalculateInstance(owlInstanceBooleanOperation2D_B_min_A, out long vertexBufferSize3, out long indexBufferSize3);
                    if ((vertexBufferSize3 != 0) && (indexBufferSize3 != 0))
                    {
                        topologicalRelation = "OVERLAP"; // "First AND Second OVERLAP";
                    }
                    else
                    {
                        topologicalRelation = "CONTAINS"; // "First CONTAINS Second";
                    }
                }
                else
                {
                    topologicalRelation = "CONTAINED BY"; // "Second CONTAINS First";
                }
            }

            return topologicalRelation;
        }
    }
}