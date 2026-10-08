using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RDF;
using System;

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
                _logger.LogError(ex, "Error creating Cylinder model.");
                throw;
            }
            finally
            {
                engine.CloseModel(owlModel);
            }
        }

        public IActionResult OnGetCreateSphere(double radius, long segmentationParts)
        {
            var base64Content = string.Empty;

            long owlModel = engine.CreateModel();
            try
            {
                long owlClassSphere = engine.GetClassByName(owlModel, "Sphere");
                long owlInstanceSphere = engine.CreateInstance(owlClassSphere, "");
                engine.SetDatatypeProperty(owlInstanceSphere, engine.GetPropertyByName(owlModel, "radius"), radius);
                engine.SetDatatypeProperty(owlInstanceSphere, engine.GetPropertyByName(owlModel, "segmentationParts"), segmentationParts);

                var modelPath = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.bin");
                engine.SaveModel(owlModel, modelPath);
                base64Content = Convert.ToBase64String(System.IO.File.ReadAllBytes(modelPath));
                System.IO.File.Delete(modelPath);

                return new JsonResult(new { Geometry = base64Content });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Sphere model.");
                throw;
            }
            finally
            {
                engine.CloseModel(owlModel);
            }
        }

        public IActionResult OnGetCreateBox(double length, double width, double height)
        {
            var base64Content = string.Empty;

            long owlModel = engine.CreateModel();
            try
            {
                long owlClassBox = engine.GetClassByName(owlModel, "Box");
                long owlInstanceBox = engine.CreateInstance(owlClassBox, "");
                engine.SetDatatypeProperty(owlInstanceBox, engine.GetPropertyByName(owlModel, "length"), length);
                engine.SetDatatypeProperty(owlInstanceBox, engine.GetPropertyByName(owlModel, "width"), width);
                engine.SetDatatypeProperty(owlInstanceBox, engine.GetPropertyByName(owlModel, "height"), height);

                var modelPath = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.bin");
                engine.SaveModel(owlModel, modelPath);
                base64Content = Convert.ToBase64String(System.IO.File.ReadAllBytes(modelPath));
                System.IO.File.Delete(modelPath);

                return new JsonResult(new { Geometry = base64Content });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Box model.");
                throw;
            }
            finally
            {
                engine.CloseModel(owlModel);
            }
        }

        public IActionResult OnPostCreateTranslation([FromForm] string base64Content, [FromForm] double _41, [FromForm] double _42, [FromForm] double _43)
        {
            var modelPath = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.bin");
            System.IO.File.WriteAllBytes(modelPath, Convert.FromBase64String(base64Content));

            long owlModel = engine.CreateModel();
            try
            {
                long owlRootInstance = engine.ImportModel(owlModel, modelPath);

                //
                // Translate
                //
                long owlMatrixInstance = engine.CreateInstance(engine.GetClassByName(owlModel, "Matrix"));
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
                engine.SetDatatypeProperty(
                    owlMatrixInstance,
                    engine.GetPropertyByName(owlModel, "coordinates"),
                    vecMatrix.ToArray(),
                    vecMatrix.Count);

                long owlTransformationInstance = engine.CreateInstance(engine.GetClassByName(owlModel, "Transformation"));
                engine.SetObjectProperty(
                    owlTransformationInstance,
                    engine.GetPropertyByName(owlModel, "matrix"),
                    owlMatrixInstance);

                engine.SetObjectProperty(
                    owlTransformationInstance,
                    engine.GetPropertyByName(owlModel, "object"),
                    owlRootInstance);

                var translatedModelPath = Path.Combine(Path.GetTempPath(), $"translated_{Guid.NewGuid()}.bin");
                engine.SaveModel(owlModel, translatedModelPath);
                var translatedBase64Content = Convert.ToBase64String(System.IO.File.ReadAllBytes(translatedModelPath));
                System.IO.File.Delete(translatedModelPath);

                return new JsonResult(new { Geometry = translatedBase64Content });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Translating model.");
                throw;
            }
            finally
            {
                engine.CloseModel(owlModel);
                System.IO.File.Delete(modelPath);
            }
        }

        public IActionResult OnPostCreateRotation([FromForm] string base64Content, [FromForm] double alpha, [FromForm] double beta, [FromForm] double gamma)
        {
            var modelPath = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.bin");
            System.IO.File.WriteAllBytes(modelPath, Convert.FromBase64String(base64Content));

            long owlModel = engine.CreateModel();
            try
            {
                long owlRootInstance = engine.ImportModel(owlModel, modelPath);

                //
                // Translate
                //
                long owlMatrixInstance = engine.CreateInstance(engine.GetClassByName(owlModel, "Matrix"));
                double[] vecMatrix = MatrixRotateByEulerAngles(alpha, beta, gamma);
                engine.SetDatatypeProperty(
                    owlMatrixInstance,
                    engine.GetPropertyByName(owlModel, "coordinates"),
                    vecMatrix,
                    vecMatrix.Length);

                long owlTransformationInstance = engine.CreateInstance(engine.GetClassByName(owlModel, "Transformation"));
                engine.SetObjectProperty(
                    owlTransformationInstance,
                    engine.GetPropertyByName(owlModel, "matrix"),
                    owlMatrixInstance);

                engine.SetObjectProperty(
                    owlTransformationInstance,
                    engine.GetPropertyByName(owlModel, "object"),
                    owlRootInstance);

                var rotatedModelPath = Path.Combine(Path.GetTempPath(), $"rotated_{Guid.NewGuid()}.bin");
                engine.SaveModel(owlModel, rotatedModelPath);
                var rotatedBase64Content = Convert.ToBase64String(System.IO.File.ReadAllBytes(rotatedModelPath));
                System.IO.File.Delete(rotatedModelPath);

                return new JsonResult(new { Geometry = rotatedBase64Content });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Translating model.");
                throw;
            }
            finally
            {
                engine.CloseModel(owlModel);
                System.IO.File.Delete(modelPath);
            }
        }

        public IActionResult OnPostCreateBooleanOperation([FromForm] string base64Content1, [FromForm] string base64Content2, long type)
        {
            var modelPath1 = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.bin");
            System.IO.File.WriteAllBytes(modelPath1, Convert.FromBase64String(base64Content1));

            var modelPath2 = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.bin");
            System.IO.File.WriteAllBytes(modelPath2, Convert.FromBase64String(base64Content2));

            long owlModel = engine.CreateModel();
            try
            {
                long owlRootInstance1 = engine.ImportModel(owlModel, modelPath1);
                long owlRootInstance2 = engine.ImportModel(owlModel, modelPath2);

                //
                // Union
                //
                long owlInstanceBooleanOperation = engine.CreateInstance(engine.GetClassByName(owlModel, "BooleanOperation"));
                engine.SetObjectProperty(owlInstanceBooleanOperation, engine.GetPropertyByName(owlModel, "firstObject"), owlRootInstance1);
                engine.SetObjectProperty(owlInstanceBooleanOperation, engine.GetPropertyByName(owlModel, "secondObject"), owlRootInstance2);
                engine.SetDatatypeProperty(owlInstanceBooleanOperation, engine.GetPropertyByName(owlModel, "type"), type);
                engine.CalculateInstance(owlInstanceBooleanOperation, out long _, out long _);

                var unionModelPath = Path.Combine(Path.GetTempPath(), $"union_{Guid.NewGuid()}.bin");
                engine.SaveModel(owlModel, unionModelPath);
                var unionBase64Content = Convert.ToBase64String(System.IO.File.ReadAllBytes(unionModelPath));
                System.IO.File.Delete(unionModelPath);

                return new JsonResult(new { Geometry = unionBase64Content });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Translating model.");
                throw;
            }
            finally
            {
                engine.CloseModel(owlModel);
                System.IO.File.Delete(modelPath1);
                System.IO.File.Delete(modelPath2);
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
