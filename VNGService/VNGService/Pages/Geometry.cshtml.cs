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
            var modelsDir = GetModelsDir();
            if (string.IsNullOrEmpty(modelsDir))
            {
                _logger.LogError("Models path is not configured.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Models path is not configured.");
            }

            long owlModel = Engine.CreateModel();
            Engine.SetOverrideFileIO(owlModel, Engine.flagbit0 + Engine.flagbit4, Engine.flagbit0 + Engine.flagbit4);

            long owlInstanceInput = ImportBase64Model(owlModel, modelsDir, base64Content);

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
            var modelsDir = GetModelsDir();
            if (string.IsNullOrEmpty(modelsDir))
            {
                _logger.LogError("Models path is not configured.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Models path is not configured.");
            }

            long owlModel = Engine.CreateModel();
            Engine.SetOverrideFileIO(owlModel, Engine.flagbit0 + Engine.flagbit4, Engine.flagbit0 + Engine.flagbit4);

            long owlInstanceInput = ImportBase64Model(owlModel, modelsDir, base64Content);

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

            using var memoryStream = new MemoryStream();
            Engine.SaveInstanceTreeS(owlInstanceShadow, (value, size) =>
            {
                byte[] buffer = new byte[size];
                Marshal.Copy(value, buffer, 0, (int)size);

                memoryStream.Write(buffer, 0, (int)size);
            }, 20000);

            Engine.CloseModel(owlModel);            

            return new JsonResult(new { Projection = System.Text.Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length) });
        }

        public IActionResult OnPostTopologicalRelation([FromForm] string base64Content1, [FromForm] string base64Content2)
        {
            var modelsDir = GetModelsDir();
            if (string.IsNullOrEmpty(modelsDir))
            {
                _logger.LogError("Models path is not configured.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Models path is not configured.");
            }

            string topologicalRelation = "DISJOINT";

            long owlModel = Engine.CreateModel(); 
            Engine.SetOverrideFileIO(owlModel, Engine.flagbit0 + Engine.flagbit4, Engine.flagbit0 + Engine.flagbit4);

            try
            {
                long owlInstanceInputOne = ImportBase64Model(owlModel, modelsDir, base64Content1);
                long owlInstanceInputTwo = ImportBase64Model(owlModel, modelsDir, base64Content2);

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
            var modelsDir = GetModelsDir();
            if (string.IsNullOrEmpty(modelsDir))
            {
                _logger.LogError("Models path is not configured.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Models path is not configured.");
            }

            string topologicalRelation = "DISJOINT";

            long owlModel = Engine.CreateModel(); 
            Engine.SetOverrideFileIO(owlModel, Engine.flagbit0 + Engine.flagbit4, Engine.flagbit0 + Engine.flagbit4);

            try
            {
                long owlInstanceInputOne = ImportBase64Model(owlModel, modelsDir, base64Content1);

                var base64Datas = base64Content2.Split('|');
                if (base64Datas.Length > 1)
                {
                    for (int i = 0; i < base64Datas.Length; i++)
                    {
                        long owlInstanceInputTwo = ImportBase64Model(owlModel, modelsDir, base64Content2);

                        topologicalRelation = GetTopologicalRelation(owlModel, owlInstanceInputOne, owlInstanceInputTwo);
                        if (topologicalRelation != "DISJOINT")
                        {
                            break;
                        }
                    }
                }
                else
                {
                    long owlInstanceInputTwo = ImportBase64Model(owlModel, modelsDir, base64Content2);

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
            string topologicalRelation = "DISJOINT";

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
                        topologicalRelation = "OVERLAP";
                    }
                    else
                    {
                        topologicalRelation = "CONTAINS";
                    }
                }
                else
                {
                    topologicalRelation = "CONTAINED BY";
                }
            }

            return topologicalRelation;
        }

        public IActionResult OnPostArea([FromForm] string base64Content)
        {
            var modelsDir = GetModelsDir();
            if (string.IsNullOrEmpty(modelsDir))
            {
                _logger.LogError("Models path is not configured.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Models path is not configured.");
            }

            long owlModel = Engine.CreateModel();
            Engine.SetOverrideFileIO(owlModel, Engine.flagbit0 + Engine.flagbit4, Engine.flagbit0 + Engine.flagbit4);

            try
            {
                long owlInstance = ImportBase64Model(owlModel, modelsDir, base64Content);
                double area = Engine.GetArea(owlInstance);

                return new JsonResult(new { Area = area });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during area calculation.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error during area calculation.");
            }
            finally
            {
                Engine.CloseModel(owlModel);
            }
        }

        public IActionResult OnPostVolume([FromForm] string base64Content)
        {
            var modelsDir = GetModelsDir();
            if (string.IsNullOrEmpty(modelsDir))
            {
                _logger.LogError("Models path is not configured.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Models path is not configured.");
            }

            long owlModel = Engine.CreateModel();
            Engine.SetOverrideFileIO(owlModel, Engine.flagbit0 + Engine.flagbit4, Engine.flagbit0 + Engine.flagbit4);

            try
            {
                long owlInstance = ImportBase64Model(owlModel, modelsDir, base64Content);
                double volume = Engine.GetVolume(owlInstance);

                return new JsonResult(new { Volume = volume });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during volume calculation.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error during volume calculation.");
            }
            finally
            {
                Engine.CloseModel(owlModel);
            }
        }

        public IActionResult OnPostContent([FromForm] string base64Content)
        {
            var modelsDir = GetModelsDir();
            if (string.IsNullOrEmpty(modelsDir))
            {
                _logger.LogError("Models path is not configured.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Models path is not configured.");
            }

            long owlModel = Engine.CreateModel();
            Engine.SetOverrideFileIO(owlModel, Engine.flagbit0 + Engine.flagbit4, Engine.flagbit0 + Engine.flagbit4);

            try
            {
                long owlInstance = ImportBase64Model(owlModel, modelsDir, base64Content);

                var content = string.Empty;

                // bit 16:   Contains (Closed) Solid(s)
                content += "(Closed) Solid(s): ";
                content += Engine.CheckInstanceConsistency(owlInstance, Engine.flagbit16) > 0 ? "YES" : "-";
                content += ";";

                // bit 18:   Contains (Closed) Infinite Solid(s)
                content += "(Closed) Infinite Solid(s): ";
                content += Engine.CheckInstanceConsistency(owlInstance, Engine.flagbit18) > 0 ? "YES" : "-";
                content += ";";

                // bit 20:   Contains Closed Surface(s)
                content += "Closed Surface(s): ";
                content += Engine.CheckInstanceConsistency(owlInstance, Engine.flagbit20) > 0 ? "YES" : "-";
                content += ";";

                // bit 21:   Contains Open Surface(s)
                content += "Open Surface(s): ";
                content += Engine.CheckInstanceConsistency(owlInstance, Engine.flagbit21) > 0 ? "YES" : "-";
                content += ";";

                // bit 22:   Contains Closed Infinite Surface(s)
                content += "Closed Infinite Surface(s): ";
                content += Engine.CheckInstanceConsistency(owlInstance, Engine.flagbit22) > 0 ? "YES" : "-";
                content += ";";

                // bit 23:   Contains Open Infinite Surface(s)
                content += "Open Infinite Surface(s): ";
                content += Engine.CheckInstanceConsistency(owlInstance, Engine.flagbit23) > 0 ? "YES" : "-";
                content += ";";

                // bit 25:   Contains Open Line(s)
                content += "Open Line(s): ";
                content += Engine.CheckInstanceConsistency(owlInstance, Engine.flagbit25) > 0 ? "YES" : "-";
                content += ";";

                // bit 26:   Contains Closed Infinite Line(s) [i.e. both ends in infinity]
                content += "Closed Infinite Line(s): ";
                content += Engine.CheckInstanceConsistency(owlInstance, Engine.flagbit26) > 0 ? "YES" : "-";
                content += ";";

                // bit 27:   Contains Open Infinite Line(s) [i.e. one end in infinity]
                content += "Open Infinite Line(s): ";
                content += Engine.CheckInstanceConsistency(owlInstance, Engine.flagbit27) > 0 ? "YES" : "-";
                content += ";";

                // bit 28:   Contains (Closed) Point(s)
                content += "(Closed) Point(s): ";
                content += Engine.CheckInstanceConsistency(owlInstance, Engine.flagbit28) > 0 ? "YES" : "-";

                return new JsonResult(new { Content = content });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during content calculation.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error during volume calculation.");
            }
            finally
            {
                Engine.CloseModel(owlModel);
            }
        }

        public IActionResult OnPostDistance([FromForm] string base64Content1, [FromForm] string base64Content2)
        {
            var modelsDir = GetModelsDir();
            if (string.IsNullOrEmpty(modelsDir))
            {
                _logger.LogError("Models path is not configured.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Models path is not configured.");
            }

            long owlModel = Engine.CreateModel();
            Engine.SetOverrideFileIO(owlModel, Engine.flagbit0 + Engine.flagbit4, Engine.flagbit0 + Engine.flagbit4);

            try
            {
                long owlInstanceOne = ImportBase64Model(owlModel, modelsDir, base64Content1);
                long owlInstanceTwo = ImportBase64Model(owlModel, modelsDir, base64Content2);

                double distance = Engine.GetDistance(owlInstanceOne, owlInstanceTwo);

                return new JsonResult(new { Distance = distance });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during distance calculation.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error during distance calculation.");
            }
            finally
            {
                Engine.CloseModel(owlModel);
            }
        }

        private string? GetModelsDir()
        {
            var fileStorage = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "FileStorageLinux" : "FileStorage";
            return _configuration[$"{fileStorage}:ModelsDir"];
        }

        private static long ImportBase64Model(long owlModel, string modelsDir, string base64Content)
        {
            var modelPath = Path.Combine(modelsDir, $"{Guid.NewGuid():N}.bin");
            try
            {
                System.IO.File.WriteAllBytes(modelPath, Convert.FromBase64String(base64Content));
                return Engine.ImportModel(owlModel, modelPath);
            }
            finally
            {
                System.IO.File.Delete(modelPath);
            }
        }
    }
}