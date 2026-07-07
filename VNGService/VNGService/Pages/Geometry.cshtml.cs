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
        private const long flagbit0 = 1;   // 2^^0
        private const long flagbit1 = 2;   // 2^^1
        private const long flagbit2 = 4;   // 2^^2
        private const long flagbit3 = 8;   // 2^^3
        private const long flagbit4 = 16;  // 2^^4

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

            long owlModel = engine.CreateModel();

            long setting = flagbit0 + flagbit4;
            long mask = flagbit0 + flagbit4;
            engine.SetOverrideFileIO(owlModel, setting, mask);

            long owlInstanceInput = engine.ImportModel(owlModel, modelPath);

            System.IO.File.Delete(modelPath);

            double[]? transformationMatrix = null;
            double[] startVector = new double[3];
            double[] endVector = new double[3];
#pragma warning disable CS8604 // Possible null reference argument.
            engine.GetBoundingBox(owlInstanceInput, transformationMatrix, startVector, endVector);
#pragma warning restore CS8604 // Possible null reference argument.

            engine.CloseModel(owlModel);

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

            long owlModel = engine.CreateModel();

            long setting = flagbit0 + flagbit4;
            long mask = flagbit0 + flagbit4;
            engine.SetOverrideFileIO(owlModel, setting, mask);

            long owlInstanceInput = engine.ImportModel(owlModel, modelPath);

            System.IO.File.Delete(modelPath);

            long owlInstancePlane = engine.CreateInstance(engine.GetClassByName(owlModel, "Plane"));
            engine.SetDatatypeProperty(owlInstancePlane, engine.GetPropertyByName(owlModel, "A"), 0.0);
            engine.SetDatatypeProperty(owlInstancePlane, engine.GetPropertyByName(owlModel, "B"), 0.0);
            engine.SetDatatypeProperty(owlInstancePlane, engine.GetPropertyByName(owlModel, "C"), 1.0);
            engine.SetDatatypeProperty(owlInstancePlane, engine.GetPropertyByName(owlModel, "D"), 0.0);

            long owlInstanceVector3 = engine.CreateInstance(engine.GetClassByName(owlModel, "Vector3"));
            engine.SetDatatypeProperty(owlInstanceVector3, engine.GetPropertyByName(owlModel, "z"), 1.0);

            long owlInstanceShadow = engine.CreateInstance(engine.GetClassByName(owlModel, "Shadow"));
            engine.SetObjectProperty(owlInstanceShadow, engine.GetPropertyByName(owlModel, "plane"), owlInstancePlane);
            engine.SetObjectProperty(owlInstanceShadow, engine.GetPropertyByName(owlModel, "lightDirection"), owlInstanceVector3);
            engine.SetObjectProperty(owlInstanceShadow, engine.GetPropertyByName(owlModel, "object"), owlInstanceInput);
            engine.SetDatatypeProperty(owlInstanceShadow, engine.GetPropertyByName(owlModel, "type"), (long)1);            

            modelPath = Path.Combine(modelsDir, $"projection_{tempId}.bin");
            engine.SaveInstanceTree(owlInstanceShadow, modelPath);

            var projectionBase64Content = Convert.ToBase64String(System.IO.File.ReadAllBytes(modelPath));

            System.IO.File.Delete(modelPath);

            engine.CloseModel(owlModel);            

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

            long owlModel = engine.CreateModel();

            long setting = flagbit0 + flagbit4;
            long mask = flagbit0 + flagbit4;
            engine.SetOverrideFileIO(owlModel, setting, mask);

            try
            {
                var tempId = Guid.NewGuid().ToString("N");

                var modelPath1 = Path.Combine(modelsDir, $"{tempId}_1.bin");
                System.IO.File.WriteAllBytes(modelPath1, Convert.FromBase64String(base64Content1));

                long owlInstanceInputOne = engine.ImportModel(owlModel, modelPath1);

                System.IO.File.Delete(modelPath1);

                var modelPath2 = Path.Combine(modelsDir, $"{tempId}_2.bin");
                System.IO.File.WriteAllBytes(modelPath2, Convert.FromBase64String(base64Content2));

                long owlInstanceInputTwo = engine.ImportModel(owlModel, modelPath2);

                System.IO.File.Delete(modelPath2);

                topologicalRelation = GetTopologicalRelation(owlModel, owlInstanceInputOne, owlInstanceInputTwo);

                engine.CloseModel(owlModel);

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

            long owlModel = engine.CreateModel();

            long setting = flagbit0 + flagbit4;
            long mask = flagbit0 + flagbit4;
            engine.SetOverrideFileIO(owlModel, setting, mask);

            try
            {
                var tempId = Guid.NewGuid().ToString("N");

                var modelPath1 = Path.Combine(modelsDir, $"{tempId}_1.bin");
                System.IO.File.WriteAllBytes(modelPath1, Convert.FromBase64String(base64Content1));

                long owlInstanceInputOne = engine.ImportModel(owlModel, modelPath1);

                System.IO.File.Delete(modelPath1);

                var base64Datas = base64Content2.Split('|');
                if (base64Datas.Length > 1)
                {
                    for (int i = 0; i < base64Datas.Length; i++)
                    {
                        var modelPath2 = Path.Combine(modelsDir, $"{tempId}_2_{i}.bin");
                        System.IO.File.WriteAllBytes(modelPath2, Convert.FromBase64String(base64Datas[i]));

                        long owlInstanceInputTwo = engine.ImportModel(owlModel, modelPath2);

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

                    long owlInstanceInputTwo = engine.ImportModel(owlModel, modelPath2);

                    System.IO.File.Delete(modelPath2);

                    topologicalRelation = GetTopologicalRelation(owlModel, owlInstanceInputOne, owlInstanceInputTwo);
                }                

                engine.CloseModel(owlModel);

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
            long owlInstanceBooleanOperation2D_A_and_B = engine.CreateInstance(engine.GetClassByName(owlModel, "BooleanOperation2D"));
            engine.SetObjectProperty(owlInstanceBooleanOperation2D_A_and_B, engine.GetPropertyByName(owlModel, "firstObject"), owlInstanceInputOne);
            engine.SetObjectProperty(owlInstanceBooleanOperation2D_A_and_B, engine.GetPropertyByName(owlModel, "secondObject"), owlInstanceInputTwo);            
            engine.SetDatatypeProperty(owlInstanceBooleanOperation2D_A_and_B, engine.GetPropertyByName(owlModel, "type"), (long)3);

            engine.CalculateInstance(owlInstanceBooleanOperation2D_A_and_B, out long vertexBufferSize1, out long indexBufferSize1);
            if ((vertexBufferSize1 != 0) && (indexBufferSize1 != 0))
            {
                // Difference
                long owlInstanceBooleanOperation2D_A_min_B = engine.CreateInstance(engine.GetClassByName(owlModel, "BooleanOperation2D"));
                engine.SetObjectProperty(owlInstanceBooleanOperation2D_A_min_B, engine.GetPropertyByName(owlModel, "firstObject"), owlInstanceInputOne);
                engine.SetObjectProperty(owlInstanceBooleanOperation2D_A_min_B, engine.GetPropertyByName(owlModel, "secondObject"), owlInstanceInputTwo);                
                engine.SetDatatypeProperty(owlInstanceBooleanOperation2D_A_min_B, engine.GetPropertyByName(owlModel, "type"), (long)1);

                engine.CalculateInstance(owlInstanceBooleanOperation2D_A_min_B, out long vertexBufferSize2, out long indexBufferSize2);
                if ((vertexBufferSize2 != 0) && (indexBufferSize2 != 0))
                {
                    // Inverse Difference
                    long owlInstanceBooleanOperation2D_B_min_A = engine.CreateInstance(engine.GetClassByName(owlModel, "BooleanOperation2D"));
                    engine.SetObjectProperty(owlInstanceBooleanOperation2D_B_min_A, engine.GetPropertyByName(owlModel, "firstObject"), owlInstanceInputTwo);
                    engine.SetObjectProperty(owlInstanceBooleanOperation2D_B_min_A, engine.GetPropertyByName(owlModel, "secondObject"), owlInstanceInputOne);                    
                    engine.SetDatatypeProperty(owlInstanceBooleanOperation2D_B_min_A, engine.GetPropertyByName(owlModel, "type"), (long)2);

                    engine.CalculateInstance(owlInstanceBooleanOperation2D_B_min_A, out long vertexBufferSize3, out long indexBufferSize3);
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