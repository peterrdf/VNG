using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RDF;
using System;
using System.Runtime.InteropServices;

namespace VNGService.Pages
{
    [IgnoreAntiforgeryToken]
    public class GeometryModelingModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DSOModel> _logger;

        public GeometryModelingModel(IConfiguration configuration, ILogger<DSOModel> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public IActionResult OnGetCreateCylinder(double length, double radius, long? segmentationParts)
        {
            long owlModel = Engine.CreateModel(); 
            Engine.SetOverrideFileIO(owlModel, Engine.flagbit0 + Engine.flagbit4, Engine.flagbit0 + Engine.flagbit4);

            try
            {
                long owlClassCylinder = Engine.GetClassByName(owlModel, "Cylinder");
                long owlInstanceCylinder = Engine.CreateInstance(owlClassCylinder, "");
                Engine.SetDatatypeProperty(owlInstanceCylinder, Engine.GetPropertyByName(owlModel, "length"), length);
                Engine.SetDatatypeProperty(owlInstanceCylinder, Engine.GetPropertyByName(owlModel, "radius"), radius);
                Engine.SetDatatypeProperty(owlInstanceCylinder, Engine.GetPropertyByName(owlModel, "segmentationParts"), segmentationParts ?? 36);

                using var memoryStream = new MemoryStream();
                Engine.SaveInstanceTreeS(owlInstanceCylinder, (value, size) =>
                {
                    byte[] buffer = new byte[size];
                    Marshal.Copy(value, buffer, 0, (int)size);

                    memoryStream.Write(buffer, 0, (int)size);
                }, 20000);

                return new JsonResult(new { Geometry = System.Text.Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Cylinder model.");
                throw;
            }
            finally
            {
                Engine.CloseModel(owlModel);
            }
        }

        public IActionResult OnGetCreateSphere(double radius, long? segmentationParts)
        {
            long owlModel = Engine.CreateModel(); 
            Engine.SetOverrideFileIO(owlModel, Engine.flagbit0 + Engine.flagbit4, Engine.flagbit0 + Engine.flagbit4);

            try
            {
                long owlClassSphere = Engine.GetClassByName(owlModel, "Sphere");
                long owlInstanceSphere = Engine.CreateInstance(owlClassSphere, "");
                Engine.SetDatatypeProperty(owlInstanceSphere, Engine.GetPropertyByName(owlModel, "radius"), radius);
                Engine.SetDatatypeProperty(owlInstanceSphere, Engine.GetPropertyByName(owlModel, "segmentationParts"), segmentationParts ?? 36);

                using var memoryStream = new MemoryStream();
                Engine.SaveInstanceTreeS(owlInstanceSphere, (value, size) =>
                {
                    byte[] buffer = new byte[size];
                    Marshal.Copy(value, buffer, 0, (int)size);

                    memoryStream.Write(buffer, 0, (int)size);
                }, 20000);

                return new JsonResult(new { Geometry = System.Text.Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Sphere model.");
                throw;
            }
            finally
            {
                Engine.CloseModel(owlModel);
            }
        }

        public IActionResult OnGetCreateBox(double length, double width, double height)
        {
            long owlModel = Engine.CreateModel(); 
            Engine.SetOverrideFileIO(owlModel, Engine.flagbit0 + Engine.flagbit4, Engine.flagbit0 + Engine.flagbit4);

            try
            {
                long owlClassBox = Engine.GetClassByName(owlModel, "Box");
                long owlInstanceBox = Engine.CreateInstance(owlClassBox, "");
                Engine.SetDatatypeProperty(owlInstanceBox, Engine.GetPropertyByName(owlModel, "length"), length);
                Engine.SetDatatypeProperty(owlInstanceBox, Engine.GetPropertyByName(owlModel, "width"), width);
                Engine.SetDatatypeProperty(owlInstanceBox, Engine.GetPropertyByName(owlModel, "height"), height);

                using var memoryStream = new MemoryStream();
                Engine.SaveInstanceTreeS(owlInstanceBox, (value, size) =>
                {
                    byte[] buffer = new byte[size];
                    Marshal.Copy(value, buffer, 0, (int)size);

                    memoryStream.Write(buffer, 0, (int)size);
                }, 20000);

                return new JsonResult(new { Geometry = System.Text.Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Box model.");
                throw;
            }
            finally
            {
                Engine.CloseModel(owlModel);
            }
        }

        public IActionResult OnGetCreatePlane(double a, double b, double c, double d)
        {
            long owlModel = Engine.CreateModel();
            Engine.SetOverrideFileIO(owlModel, Engine.flagbit0 + Engine.flagbit4, Engine.flagbit0 + Engine.flagbit4);

            try
            {
                long owlClassPlane = Engine.GetClassByName(owlModel, "Plane");
                long owlInstancePlane = Engine.CreateInstance(owlClassPlane, "");
                Engine.SetDatatypeProperty(owlInstancePlane, Engine.GetPropertyByName(owlModel, "A"), a);
                Engine.SetDatatypeProperty(owlInstancePlane, Engine.GetPropertyByName(owlModel, "B"), b);
                Engine.SetDatatypeProperty(owlInstancePlane, Engine.GetPropertyByName(owlModel, "C"), c);
                Engine.SetDatatypeProperty(owlInstancePlane, Engine.GetPropertyByName(owlModel, "D"), d);

                using var memoryStream = new MemoryStream();
                Engine.SaveInstanceTreeS(owlInstancePlane, (value, size) =>
                {
                    byte[] buffer = new byte[size];
                    Marshal.Copy(value, buffer, 0, (int)size);

                    memoryStream.Write(buffer, 0, (int)size);
                }, 20000);

                return new JsonResult(new { Geometry = System.Text.Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Plane model.");
                throw;
            }
            finally
            {
                Engine.CloseModel(owlModel);
            }
        }
        public IActionResult OnPostCreateProjection2D([FromForm] string base64Content1, [FromForm] string base64Content2)
        {
            var modelPath = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.bin");
            System.IO.File.WriteAllBytes(modelPath, Convert.FromBase64String(base64Content1));

            var planePath = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.bin");
            System.IO.File.WriteAllBytes(planePath, Convert.FromBase64String(base64Content2));

            long owlModel = Engine.CreateModel();
            Engine.SetOverrideFileIO(owlModel, Engine.flagbit0 + Engine.flagbit4, Engine.flagbit0 + Engine.flagbit4);

            try
            {
                long owlInstanceInput = Engine.ImportModel(owlModel, modelPath);
                System.IO.File.Delete(modelPath);

                long owlInstancePlane = Engine.ImportModel(owlModel, planePath);
                System.IO.File.Delete(planePath);

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

                return new JsonResult(new { Geometry = System.Text.Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Projection model.");
                throw;
            }
            finally
            {
                Engine.CloseModel(owlModel);
            }
        }


        public IActionResult OnPostCreateTranslation([FromForm] string base64Content, [FromForm] double _41, [FromForm] double _42, [FromForm] double _43)
        {
            var modelPath = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.bin");
            System.IO.File.WriteAllBytes(modelPath, Convert.FromBase64String(base64Content));

            long owlModel = Engine.CreateModel();
            Engine.SetOverrideFileIO(owlModel, Engine.flagbit0 + Engine.flagbit4, Engine.flagbit0 + Engine.flagbit4);

            try
            {
                long owlRootInstance = Engine.ImportModel(owlModel, modelPath);

                //
                // Translate
                //
                long owlMatrixInstance = Engine.CreateInstance(Engine.GetClassByName(owlModel, "Matrix"));
                List<double> vecMatrix = new List<double>
                {
                    1.0,    // _11
                    0.0,    // _12
                    0.0,    // _13
                    0.0,    // _21
                    1.0,    // _22
                    0.0,    // _23
                    0.0,    // _31
                    0.0,    // _32
                    1.0,    // _33
                    _41,    // _41
                    _42,    // _42
                    _43,    // _43
                };
                Engine.SetDatatypeProperty(
                    owlMatrixInstance,
                    Engine.GetPropertyByName(owlModel, "coordinates"),
                    vecMatrix.ToArray(),
                    vecMatrix.Count);

                long owlTransformationInstance = Engine.CreateInstance(Engine.GetClassByName(owlModel, "Transformation"));
                Engine.SetObjectProperty(
                    owlTransformationInstance,
                    Engine.GetPropertyByName(owlModel, "matrix"),
                    owlMatrixInstance);

                Engine.SetObjectProperty(
                    owlTransformationInstance,
                    Engine.GetPropertyByName(owlModel, "object"),
                    owlRootInstance);

                using var memoryStream = new MemoryStream();
                Engine.SaveInstanceTreeS(owlTransformationInstance, (value, size) =>
                {
                    byte[] buffer = new byte[size];
                    Marshal.Copy(value, buffer, 0, (int)size);

                    memoryStream.Write(buffer, 0, (int)size);
                }, 20000);

                return new JsonResult(new { Geometry = System.Text.Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Translating model.");
                throw;
            }
            finally
            {
                Engine.CloseModel(owlModel);
                System.IO.File.Delete(modelPath);
            }
        }

        public IActionResult OnPostCreateRotation([FromForm] string base64Content, [FromForm] double alpha, [FromForm] double beta, [FromForm] double gamma)
        {
            var modelPath = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.bin");
            System.IO.File.WriteAllBytes(modelPath, Convert.FromBase64String(base64Content));

            long owlModel = Engine.CreateModel();
            Engine.SetOverrideFileIO(owlModel, Engine.flagbit0 + Engine.flagbit4, Engine.flagbit0 + Engine.flagbit4);

            try
            {
                long owlRootInstance = Engine.ImportModel(owlModel, modelPath);

                //
                // Translate
                //
                long owlMatrixInstance = Engine.CreateInstance(Engine.GetClassByName(owlModel, "Matrix"));
                double[] vecMatrix = MatrixRotateByEulerAngles(alpha, beta, gamma);
                Engine.SetDatatypeProperty(
                    owlMatrixInstance,
                    Engine.GetPropertyByName(owlModel, "coordinates"),
                    vecMatrix,
                    vecMatrix.Length);

                long owlTransformationInstance = Engine.CreateInstance(Engine.GetClassByName(owlModel, "Transformation"));
                Engine.SetObjectProperty(
                    owlTransformationInstance,
                    Engine.GetPropertyByName(owlModel, "matrix"),
                    owlMatrixInstance);

                Engine.SetObjectProperty(
                    owlTransformationInstance,
                    Engine.GetPropertyByName(owlModel, "object"),
                    owlRootInstance);

                using var memoryStream = new MemoryStream();
                Engine.SaveInstanceTreeS(owlTransformationInstance, (value, size) =>
                {
                    byte[] buffer = new byte[size];
                    Marshal.Copy(value, buffer, 0, (int)size);

                    memoryStream.Write(buffer, 0, (int)size);
                }, 20000);

                return new JsonResult(new { Geometry = System.Text.Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Translating model.");
                throw;
            }
            finally
            {
                Engine.CloseModel(owlModel);
                System.IO.File.Delete(modelPath);
            }
        }

        public IActionResult OnPostCreateBooleanOperation([FromForm] string base64Content1, [FromForm] string base64Content2, long type)
        {
            var modelPath1 = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.bin");
            System.IO.File.WriteAllBytes(modelPath1, Convert.FromBase64String(base64Content1));

            var modelPath2 = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.bin");
            System.IO.File.WriteAllBytes(modelPath2, Convert.FromBase64String(base64Content2));

            long owlModel = Engine.CreateModel();
            Engine.SetOverrideFileIO(owlModel, Engine.flagbit0 + Engine.flagbit4, Engine.flagbit0 + Engine.flagbit4);

            try
            {
                long owlRootInstance1 = Engine.ImportModel(owlModel, modelPath1);
                long owlRootInstance2 = Engine.ImportModel(owlModel, modelPath2);

                //
                // Union
                //
                long owlInstanceBooleanOperation = Engine.CreateInstance(Engine.GetClassByName(owlModel, "BooleanOperation"));
                Engine.SetObjectProperty(owlInstanceBooleanOperation, Engine.GetPropertyByName(owlModel, "firstObject"), owlRootInstance1);
                Engine.SetObjectProperty(owlInstanceBooleanOperation, Engine.GetPropertyByName(owlModel, "secondObject"), owlRootInstance2);
                Engine.SetDatatypeProperty(owlInstanceBooleanOperation, Engine.GetPropertyByName(owlModel, "type"), type);
                Engine.CalculateInstance(owlInstanceBooleanOperation, out long _, out long _);

                using var memoryStream = new MemoryStream();
                Engine.SaveInstanceTreeS(owlInstanceBooleanOperation, (value, size) =>
                {
                    byte[] buffer = new byte[size];
                    Marshal.Copy(value, buffer, 0, (int)size);

                    memoryStream.Write(buffer, 0, (int)size);
                }, 20000);

                return new JsonResult(new { Geometry = System.Text.Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Translating model.");
                throw;
            }
            finally
            {
                Engine.CloseModel(owlModel);
                System.IO.File.Delete(modelPath1);
                System.IO.File.Delete(modelPath2);
            }
        }

        public IActionResult OnPostCreateColor([FromForm] string base64Content, [FromForm] double R, [FromForm] double G, [FromForm] double B, [FromForm] double? A)
        {
            var modelPath = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.bin");
            System.IO.File.WriteAllBytes(modelPath, Convert.FromBase64String(base64Content));

            long owlModel = Engine.CreateModel();
            Engine.SetOverrideFileIO(owlModel, Engine.flagbit0 + Engine.flagbit4, Engine.flagbit0 + Engine.flagbit4);

            try
            {
                long owlRootInstance = Engine.ImportModel(owlModel, modelPath);

                AssignMaterialToInstance(owlModel, owlRootInstance, R, G, B, A);

                using var memoryStream = new MemoryStream();
                Engine.SaveInstanceTreeS(owlRootInstance, (value, size) =>
                {
                    byte[] buffer = new byte[size];
                    Marshal.Copy(value, buffer, 0, (int)size);

                    memoryStream.Write(buffer, 0, (int)size);
                }, 20000);

                return new JsonResult(new { Geometry = System.Text.Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Coloring model.");
                throw;
            }
            finally
            {
                Engine.CloseModel(owlModel);
                System.IO.File.Delete(modelPath);
            }
        }

        private static void AssignMaterialToInstance(long owlModel, long owlInstance, double? R, double? G, double? B, double? A)
        {
            if (R.HasValue && G.HasValue && B.HasValue)
            {
                long owlClassColorComponent = Engine.GetClassByName(owlModel, "ColorComponent");
                long owlInstanceColorComponent = Engine.CreateInstance(owlClassColorComponent, "");
                Engine.SetDatatypeProperty(owlInstanceColorComponent, Engine.GetPropertyByName(owlModel, "R"), R.Value);
                Engine.SetDatatypeProperty(owlInstanceColorComponent, Engine.GetPropertyByName(owlModel, "G"), G.Value);
                Engine.SetDatatypeProperty(owlInstanceColorComponent, Engine.GetPropertyByName(owlModel, "B"), B.Value);

                long owlClassColor = Engine.GetClassByName(owlModel, "Color");
                long owlInstanceColor = Engine.CreateInstance(owlClassColor, "");
                Engine.SetObjectProperty(owlInstanceColor, Engine.GetPropertyByName(owlModel, "ambient"), owlInstanceColorComponent);
                Engine.SetDatatypeProperty(owlInstanceColor, Engine.GetPropertyByName(owlModel, "transparency"), (A ?? 1.0));

                long owlClassMaterial = Engine.GetClassByName(owlModel, "Material");
                long owlInstanceMaterial = Engine.CreateInstance(owlClassMaterial, "");
                Engine.SetObjectProperty(owlInstanceMaterial, Engine.GetPropertyByName(owlModel, "color"), owlInstanceColor);

                Engine.SetObjectProperty(owlInstance, Engine.GetPropertyByName(owlModel, "material"), owlInstanceMaterial);
            }
        }

        /// <summary>
        /// Builds a rotation matrix from Euler angles (radians).
        /// https://en.wikipedia.org/wiki/Rotation_matrix
        /// Returns the coefficients in the order: _11, _12, _13, _21, _22, _23, _31, _32, _33, _41, _42, _43.
        /// </summary>
        private static double[] MatrixRotateByEulerAngles(double alpha, double beta, double gamma)
        {
            double cosAlpha = Math.Cos(alpha), sinAlpha = Math.Sin(alpha);
            double cosBeta = Math.Cos(beta), sinBeta = Math.Sin(beta);
            double cosGamma = Math.Cos(gamma), sinGamma = Math.Sin(gamma);

            return
                [
                    // _11, _12, _13
                    cosBeta * cosGamma,
                    cosBeta * sinGamma,
                    -sinBeta,

                    // _21, _22, _23
                    sinAlpha * sinBeta * cosGamma - cosAlpha * sinGamma,
                    sinAlpha * sinBeta * sinGamma + cosAlpha * cosGamma,
                    sinAlpha * cosBeta,

                    // _31, _32, _33
                    cosAlpha * sinBeta * cosGamma + sinAlpha * sinGamma,
                    cosAlpha * sinBeta * sinGamma - sinAlpha * cosGamma,
                    cosAlpha * cosBeta,

                    // _41, _42, _43
                    0.0,
                    0.0,
                    0.0,
                ];
        }
    }
}
