using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RDF;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using VNGService.Models;

namespace VNGService.Services
{
    public class LandRegistryService : ILandRegistryService
    {
        #region Fields

        private readonly IConfiguration _configuration;
        private readonly ILogger<LandRegistryService> _logger;

        private readonly PdokKadastraleKaartService _pdokKadastraleKaartService;
        private readonly PdokBgtService _pdokBgtService;

        private readonly BagApiService _bagService;

        #endregion // Fields

        public LandRegistryService(IConfiguration configuration, ILogger<LandRegistryService> logger)
        {
            _configuration = configuration;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _pdokKadastraleKaartService = new PdokKadastraleKaartService(logger);
            _pdokBgtService = new PdokBgtService(logger);
            _bagService = new BagApiService(configuration, logger);
        }

        public async Task<List<Building>> GetBuildings(double eastings, double northings, double bboxLength)
        {
            long owlModel = Engine.CreateModel();
            Engine.SetOverrideFileIO(owlModel, Engine.flagbit0 + Engine.flagbit4, Engine.flagbit0 + Engine.flagbit4);

            //#todo: fix base64 encoding support for Unicode properties
            // Unicode support for property values
            //Engine.SetCharacterSerialization(owlModel, 0, 0, 0);
            //_ = Engine.CreateProperty(
            //    owlModel,
            //    DATATYPEPROPERTY_TYPE_WCHAR_T_ARRAY,
            //    "tag");

            try
            {
                // https://ifc43-docs.standards.buildingsmart.org/IFC/RELEASE/IFC4x3/HTML/lexical/IfcMapConversion.htm
                // RD New (EPSG:28992): Lat = northings, Lon = eastings in meters
                var point1 = new GeoPoint(Lat: northings - bboxLength / 2, Lon: eastings - bboxLength / 2);
                var point2 = new GeoPoint(Lat: northings + bboxLength / 2, Lon: eastings + bboxLength / 2);

                return await CreateBuildings(owlModel, point1, point2);
            }
            catch (Exception ex)
            { 
                _logger.LogError(ex, "Error creating building geometries");
                throw;
            }
            finally
            {
                Engine.CloseModel(owlModel);
            }
        }
        
        public async Task<List<Parcel>> GetParcels(double eastings, double northings, double bboxLength)
        {
            long owlModel = Engine.CreateModel();
            Engine.SetOverrideFileIO(owlModel, Engine.flagbit0 + Engine.flagbit4, Engine.flagbit0 + Engine.flagbit4);

            //#todo: fix base64 encoding support for Unicode properties
            // Unicode support for property values
            //Engine.SetCharacterSerialization(owlModel, 0, 0, 0);
            //_ = Engine.CreateProperty(
            //    owlModel,
            //    DATATYPEPROPERTY_TYPE_WCHAR_T_ARRAY,
            //    "tag");

            try
            {
                // https://ifc43-docs.standards.buildingsmart.org/IFC/RELEASE/IFC4x3/HTML/lexical/IfcMapConversion.htm
                // RD New (EPSG:28992): Lat = northings, Lon = eastings in meters
                var point1 = new GeoPoint(Lat: northings - bboxLength / 2, Lon: eastings - bboxLength / 2);
                var point2 = new GeoPoint(Lat: northings + bboxLength / 2, Lon: eastings + bboxLength / 2);                

                return await CreateParcels(owlModel, point1, point2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating parcel geometries");
                throw;
            }
            finally
            {
                Engine.CloseModel(owlModel);
            }
        }

        private async Task<List<Building>> CreateBuildings(long owlModel, GeoPoint point1, GeoPoint point2)
        {
            var buildings = new List<Building>();

            var isLinuxPlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            var FileStorage = isLinuxPlatform ? "FileStorageLinux" : "FileStorage";

            var modelsDir = _configuration[$"{FileStorage}:ModelsDir"];
            if (string.IsNullOrEmpty(modelsDir))
            {
                _logger.LogError("Models path is not configured.");
                return buildings;
            }

            var featuresCollection = await _pdokKadastraleKaartService.GetFeaturesByBoundingBoxAsync(
                    KadastraleKaartLayer.Bebouwing,
                    point1,
                    point2);
            if (featuresCollection != null)
            {
                //#todo: fix base64 encoding support for Unicode properties
                // Number Identifier Properties
                //long huisnummerProperty = Engine.CreateProperty(
                //   owlModel,
                //   DATATYPEPROPERTY_TYPE_WCHAR_T_ARRAY,
                //   "huisnummer");
                //long huisletterProperty = Engine.CreateProperty(
                //    owlModel,
                //    DATATYPEPROPERTY_TYPE_WCHAR_T_ARRAY,
                //    "huisletter");
                //long postcodeProperty = Engine.CreateProperty(
                //    owlModel,
                //    DATATYPEPROPERTY_TYPE_WCHAR_T_ARRAY,
                //    "postcode");

                //// Address Properties
                //long CityProperty = Engine.CreateProperty(
                //    owlModel,
                //    DATATYPEPROPERTY_TYPE_WCHAR_T_ARRAY,
                //    "City");
                //long StreetNameProperty = Engine.CreateProperty(
                //    owlModel,
                //    DATATYPEPROPERTY_TYPE_WCHAR_T_ARRAY,
                //    "StreetName");

                //// Building Properties
                //long bronhouderProperty = Engine.CreateProperty(
                //    owlModel,
                //    DATATYPEPROPERTY_TYPE_WCHAR_T_ARRAY,
                //    "bronhouder");
                //long identificatieBAGPNDProperty = Engine.CreateProperty(
                //    owlModel,
                //    DATATYPEPROPERTY_TYPE_WCHAR_T_ARRAY,
                //    "identificatieBAGPND");

                long owlDefaultMaterialInstance = Material.GetBuildingDefaultMaterial(owlModel);

                foreach (var feature in featuresCollection.Features)
                {
                    if (feature.Geometry != null)
                    {
                        long owlBuildingInstance = RetrievePDOKGeometry(
                            owlModel,
                            $"Bebouwing {feature.Id}",
                            feature.Geometry,
                            owlDefaultMaterialInstance,
                            0.0);
                        if (owlBuildingInstance != 0)
                        {
                            //#todo: fix base64 encoding support for Unicode properties
                            // Properties
                            //string value = feature.Properties.TryGetProperty("bronhouder", out var bronhouderProp) ? bronhouderProp.GetString() ?? "$" : "$";
                            //SetStringPropertyW(owlBuildingInstance, bronhouderProperty, value);
                            string premiseId = feature.Properties.TryGetProperty("identificatieBAGPND", out var identificatieBAGPNDProp) ? identificatieBAGPNDProp.GetString() ?? "$" : "$";
                            //SetStringPropertyW(owlBuildingInstance, identificatieBAGPNDProperty, premiseId);

                            var buildingName = $"Bebouwing {feature.Id}";

                            // BAG REST API
                            if (premiseId != "$")
                            {
                                // Number Identifier (Nummeraanduiding)
                                //var numberIdentifierResponse = await _bagService.GetNumberIdentifierByPremiseIdAsync(premiseId);
                                //if (numberIdentifierResponse?.Embedded?.Nummeraanduidingen != null)
                                //{
                                //    foreach (var numberIdentifier in numberIdentifierResponse.Embedded.Nummeraanduidingen)
                                //    {
                                //        value = numberIdentifier?.Nummeraanduiding?.Huisnummer.ToString() ?? "$";
                                //        SetStringPropertyW(owlBuildingInstance, huisnummerProperty, value);
                                //        value = numberIdentifier?.Nummeraanduiding?.Huisletter ?? "$";
                                //        SetStringPropertyW(owlBuildingInstance, huisletterProperty, value);
                                //        value = numberIdentifier?.Nummeraanduiding?.Postcode ?? "$";
                                //        SetStringPropertyW(owlBuildingInstance, postcodeProperty, value);
                                //    }
                                //}

                                // Address
                                var addressResponse = await _bagService.GetAddressByPandIdAsync(premiseId);
                                if (addressResponse?.Embedded?.Adressen != null)
                                {
                                    var address = addressResponse.Embedded.Adressen[0];

                                    //value = address?.City ?? "$";
                                    //SetStringPropertyW(owlBuildingInstance, CityProperty, value);
                                    //value = address?.StreetName ?? "$";
                                    //SetStringPropertyW(owlBuildingInstance, StreetNameProperty, value);

                                    buildingName = $"{address?.City}, {address?.StreetName}, {address?.HouseNumber}, {address?.Postcode}";
                                }
                            }

                            using var memoryStream = new MemoryStream();
                            Engine.SaveInstanceTreeS(owlBuildingInstance, (value, size) =>
                            {
                                byte[] buffer = new byte[size];
                                Marshal.Copy(value, buffer, 0, (int)size);

                                memoryStream.Write(buffer, 0, (int)size);
                            }, 20000);

                            buildings.Add(new Building
                            {
                                Id = premiseId,
                                Name = buildingName,
                                Geometry = System.Text.Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length)
                            });
                        }
                    }
                }
            }

            return buildings;
        }

        private async Task<List<Parcel>> CreateParcels(long owlModel, GeoPoint point1, GeoPoint point2)
        {
            var parcels = new List<Parcel>();

            var isLinuxPlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            var FileStorage = isLinuxPlatform ? "FileStorageLinux" : "FileStorage";

            var modelsDir = _configuration[$"{FileStorage}:ModelsDir"];
            if (string.IsNullOrEmpty(modelsDir))
            {
                _logger.LogError("Models path is not configured.");
                return parcels;
            }

            var featuresCollection = await _pdokKadastraleKaartService.GetParcelsByBoundingBoxAsync(point1, point2);
            if (featuresCollection != null)
            {
                //#todo: fix base64 encoding support for Unicode properties
                // Parcel Identifier Properties
                //long perceelnummerProperty = Engine.CreateProperty(
                //    owlModel,
                //    DATATYPEPROPERTY_TYPE_WCHAR_T_ARRAY,
                //    "perceelnummer");
                //long kadastraleGemeenteWaardeProperty = Engine.CreateProperty(
                //    owlModel,
                //    DATATYPEPROPERTY_TYPE_WCHAR_T_ARRAY,
                //    "kadastraleGemeenteWaarde");
                //long AKRKadastraleGemeenteCodeWaardeProperty = Engine.CreateProperty(
                //    owlModel,
                //    DATATYPEPROPERTY_TYPE_WCHAR_T_ARRAY,
                //    "AKRKadastraleGemeenteCodeWaarde");

                long owlDefaultMaterialInstance = Material.GetParcelDefaultMaterial(owlModel);

                for (int i = 0; i < featuresCollection.Count; i++)
                {
                    var parcel = featuresCollection[i];
                    if ((parcel.Feature?.Geometry != null) && !string.IsNullOrEmpty(parcel.Perceel?.IdentificatieLokaalID))
                    {
                        string name = "Parcel ";
                        name += parcel.Perceel.KadastraleGemeenteWaarde ?? "$";
                        name += " (";
                        name += parcel.Perceel.AKRKadastraleGemeenteCodeWaarde ?? "$";
                        name += ") ";
                        name += parcel.Perceel?.Sectie ?? "$";
                        name += " ";
                        name += parcel.Perceel?.Perceelnummer.ToString() ?? "$";

                        long owlParcelInstance = RetrievePDOKGeometry(
                            owlModel,
                            name,
                            parcel.Feature.Geometry,
                            owlDefaultMaterialInstance,
                            0.0);
                        if (owlParcelInstance != 0)
                        {
                            //#todo: fix base64 encoding support for Unicode properties
                            // Properties
                            //string value = parcel.Perceel?.Perceelnummer.ToString() ?? "$";
                            //SetStringPropertyW(owlParcelInstance, perceelnummerProperty, value);
                            //value = parcel.Perceel?.KadastraleGemeenteWaarde ?? "$";
                            //SetStringPropertyW(owlParcelInstance, kadastraleGemeenteWaardeProperty, value);
                            //value = parcel.Perceel?.AKRKadastraleGemeenteCodeWaarde ?? "$";
                            //SetStringPropertyW(owlParcelInstance, AKRKadastraleGemeenteCodeWaardeProperty, value);

                            using var memoryStream = new MemoryStream();
                            Engine.SaveInstanceTreeS(owlParcelInstance, (value, size) =>
                            {
                                byte[] buffer = new byte[size];
                                Marshal.Copy(value, buffer, 0, (int)size);

                                memoryStream.Write(buffer, 0, (int)size);
                            }, 20000);

                            Engine.CloseModel(owlModel);

                            parcels.Add(new Parcel
                            {
                                Id = parcel.Perceel.IdentificatieLokaalID,
                                Name = parcel.Perceel?.KadastraleGemeenteWaarde ?? "$",
                                Geometry = System.Text.Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length)
                            });
                        }
                    }
                }                
            }

            return parcels;
        }

        private long RetrievePDOKGeometry(long owlModel, string name, PdokGeometry geometry, long owlMaterialInstance = 0, double z = 0.0)
        {
            switch (geometry.Type)
            {
                case "Polygon":
                    if (geometry != null)
                    {
                        var polygon = geometry.AsPolygon();

                        long owlBoundaryRepresentationInstance = Engine.CreateInstance(Engine.GetClassByName(owlModel, "BoundaryRepresentation"));
                        // #todo: fix base64 encoding support for Unicode properties
                        //SetStringPropertyW(
                        //    owlBoundaryRepresentationInstance,
                        //    Engine.GetPropertyByName(owlModel, "tag"), 
                        //    name);
                        int index = 0;
                        List<long> indices = new();
                        List<double> vertices = new();
                        for (int j = 0; j < polygon?.Length; j++)
                        {
                            var ring = polygon[j];
                            for (int k = 0; k < ring.Length; k++)
                            {
                                var point = ring[k];
                                vertices.Add(point[0]);
                                vertices.Add(point[1]);
                                vertices.Add(z); // Z coordinate
                                indices.Add(index++);
                            }

                            if (j == 0)
                            {
                                indices.Add(-1); // Mark end of exterior ring
                            }
                            else
                            {
                                indices.Add(-2); // Mark end of interior ring
                            }
                        }

                        Engine.SetDatatypeProperty(
                            owlBoundaryRepresentationInstance,
                            Engine.GetPropertyByName(owlModel, "indices"),
                            indices.ToArray(),
                            indices.Count);

                        Engine.SetDatatypeProperty(
                            owlBoundaryRepresentationInstance,
                            Engine.GetPropertyByName(owlModel, "vertices"),
                            vertices.ToArray(),
                            vertices.Count);

                        if (owlMaterialInstance != 0)
                        {
                            Engine.SetObjectProperty(
                                owlBoundaryRepresentationInstance,
                                Engine.GetPropertyByName(owlModel, "material"),
                                owlMaterialInstance);
                        }

                        return owlBoundaryRepresentationInstance;
                    }
                    break;

                case "LineString":
                    if (geometry != null)
                    {
                        List<double> vertices = new();
                        var lineString = geometry.AsLineString();
                        if (lineString != null)
                        {
                            for (int i = 0; i < lineString.Length; i++)
                            {
                                var point = lineString[i];
                                vertices.Add(point[0]);
                                vertices.Add(point[1]);
                                vertices.Add(z); // Z coordinate
                            }
                        }

                        long owlPolyLineInstance = Engine.CreateInstance(Engine.GetClassByName(owlModel, "PolyLine3D"));
                        //#todo: fix base64 encoding support for Unicode properties
                        //SetStringPropertyW(
                        //    owlPolyLineInstance, 
                        //    Engine.GetPropertyByName(owlModel, "tag"), 
                        //    name);
                        Engine.SetDatatypeProperty(
                            owlPolyLineInstance,
                            Engine.GetPropertyByName(owlModel, "coordinates"),
                            vertices.ToArray(),
                            vertices.Count);

                        return owlPolyLineInstance;
                    }
                    break;

                default:
                    Debug.Assert(false, $"Unexpected geometry type: {geometry.Type}");
                    break;
            }

            return 0;
        }

        #region OWL API Interop
        const long DATATYPEPROPERTY_TYPE_WCHAR_T_ARRAY = 8;

        [DllImport(Engine.enginedll, EntryPoint = "SetDatatypeProperty")]
        public static extern Int64 SetDatatypeProperty(Int64 owlInstance, Int64 owlDatatypeProperty, IntPtr values, Int64 card);

        private static void SetStringPropertyW(long owlInstance, long property, string value)
        {
            IntPtr wcharPtr = Marshal.StringToHGlobalUni(value);

            try
            {
                IntPtr ptrToPtr = Marshal.AllocHGlobal(IntPtr.Size);

                try
                {
                    Marshal.WriteIntPtr(ptrToPtr, wcharPtr);

                    SetDatatypeProperty(owlInstance, property, ptrToPtr, 1);
                }
                finally
                {
                    Marshal.FreeHGlobal(ptrToPtr);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(wcharPtr);
            }
        }
        #endregion // OWL API Interop
    }
}
