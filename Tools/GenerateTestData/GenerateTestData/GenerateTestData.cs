using Lucene.Net.Search;
using RDF;
using Serilog;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms.Design;

namespace GenerateTestData
{
    public partial class GenerateTestData : Form
    {
        #region Constants
        private readonly string InsertTestDataGraphQuery =
            """
            PREFIX ifc:     <https://standards.buildingsmart.org/IFC/DEV/IFC4/ADD2/OWL#>
            PREFIX express: <https://w3id.org/express#>
            PREFIX vng:     <http://vng.nl/geometry-ext.ttl#>
            PREFIX dso:     <http://vng.nl/areas/>

            DROP SILENT GRAPH <test-data-living-areas.ttl> ;

            INSERT {
                GRAPH <test-data-living-areas.ttl> {
                    ?areaUri	a	dso:area ;
            			            dso:id       ?id ;
            			            dso:name     ?name ;
            			            dso:group    ?group ;
            			            dso:type     ?type ;
            			            dso:geometry ?geometry .
                }
            }
            WHERE {    
                (###X### ###Y### ###BBOX-SIZE### 'wonen') vng:dsoArea (?id ?name ?group ?type ?geometry) .
                BIND(IRI(CONCAT(STR(dso:), ?id)) AS ?areaUri)
            }
            """;

        private readonly string SelectGeometryQuery =
            """
            PREFIX dso:    <http://vng.nl/areas/>

            SELECT ?id ?group ?geometry
            WHERE {
              GRAPH <test-data-living-areas.ttl> {
                ?area	a       dso:area ;
                              	dso:id    		?id ;
                              	dso:group  	    ?group ;
                              	dso:geometry  	?geometry .
              }
            }
            ORDER BY ?id
            """;
        #endregion // Constants

        #region Fields
        private readonly ILogger _logger = Log.ForContext<GenerateTestData>();
        private List<Location> _locations = new List<Location>();
        #endregion // Fields

        #region Methods
        public GenerateTestData()
        {
            InitializeComponent();
        }

        private async void _buttonRun_Click(object sender, EventArgs e)
        {
            long owlModel = Engine.CreateModel();
            Engine.SetOverrideFileIO(owlModel, Engine.flagbit0 + Engine.flagbit4, Engine.flagbit0 + Engine.flagbit4);

            try
            {
                if (string.IsNullOrWhiteSpace(_textBoxIFCFile.Text) || !File.Exists(_textBoxIFCFile.Text))
                {
                    throw new Exception("Please select a valid IFC file.");
                }

                if (string.IsNullOrWhiteSpace(_textBoxLocationsFile.Text) || !File.Exists(_textBoxLocationsFile.Text))
                {
                    throw new Exception("Please select a valid locations file.");
                }

                if (!int.TryParse(_textBoxBBoxSize.Text, out int bboxSize) || (bboxSize <= 0))
                {
                    throw new Exception("Please enter a valid number of BBox Size.");
                }

                if (!int.TryParse(_textBoxRandomLocations.Text, out int randomLocations) || (randomLocations <= 0))
                {
                    throw new Exception("Please enter a valid number of test data to generate.");
                }

                _locations = LoadLocations(_textBoxLocationsFile.Text);
                if (_locations.Count == 0)
                {
                    throw new Exception("No locations found in the specified file.");
                }

                SPARQLServer = new SPARQL.Server(_logger);
                DatasetName = "test1"; //#todo

                foreach (var location in _locations)
                {
                    _logger.Information("Generating test data for location: {LocationName} (X: {X}, Y: {Y})", location.Name, location.X, location.Y);
                    string query = InsertTestDataGraphQuery
                        .Replace("###X###", location.X.ToString(CultureInfo.InvariantCulture))
                        .Replace("###Y###", location.Y.ToString(CultureInfo.InvariantCulture))
                        .Replace("###BBOX-SIZE###", bboxSize.ToString());
                    if (await SPARQLServer.ExecuteInsertAsync(DatasetName, query))
                    {
                        _logger.Information("Successfully inserted test data for location: {LocationName}", location.Name);

                        var queryResult = await SPARQLServer.ExecuteQueryAsync(DatasetName, SelectGeometryQuery);
                        if (queryResult.Count == 0)
                        {
                            MessageBox.Show($"No geometry data found for location: {location.Name}");
                            continue;
                        }

                        var geomtries = SPARQLServer.RetrieveGeometries(queryResult, "geometry");
                        for (int i = 0; i < geomtries.Count; i++)
                        {
                            var geometry = geomtries[i];
                            _logger.Information("Geometry for location {LocationName} (ID: {ID}, Group: {Group}): {Geometry}", location.Name, queryResult[i]["id"], queryResult[i]["group"], geometry);

                            long owlInstanceInput = ImportBase64Model(owlModel, Path.GetTempPath(), geometry);

                            double[]? transformationMatrix = null;
                            double[] startVector = new double[3];
                            double[] endVector = new double[3];
#pragma warning disable CS8604 // Possible null reference argument.
                            Engine.GetBoundingBox(owlInstanceInput, transformationMatrix, startVector, endVector);
#pragma warning restore CS8604 // Possible null reference argument.

                            // Center of the BBox (Z is always 0)
                            double centerX = (startVector[0] + endVector[0]) / 2.0;
                            double centerY = (startVector[1] + endVector[1]) / 2.0;
                            string? address = await GetAddressByPointAsync(centerX, centerY);
                            if (string.IsNullOrEmpty(address))
                            {
                                MessageBox.Show($"No address found for location: {location.Name} (X: {centerX}, Y: {centerY})");
                                continue;
                            }
                            _logger.Information("Address for location {LocationName}: {Address}", location.Name, address);
                        }
                    }
                    else
                    {
                        throw new Exception($"Failed to insert test data for location: {location.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while generating test data.");
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Engine.CloseModel(owlModel);
            }
        }

        private void _buttonSelectIFCFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "IFC Files (*.ifc)|*.ifc|All Files (*.*)|*.*||";
            openFileDialog.Title = "Open";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _textBoxIFCFile.Text = openFileDialog.FileName;
            }
        }

        private void _buttonSelectLocationsFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*||";
            openFileDialog.Title = "Open";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _textBoxLocationsFile.Text = openFileDialog.FileName;
            }
        }

        private static List<Location> LoadLocations(string path)
        {
            var result = new List<Location>();

            foreach (string line in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] parts = line.Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3)
                {
                    throw new FormatException("Invalid line: " + line);
                }

                result.Add(new Location
                {
                    Name = parts[0].Trim(),
                    X = double.Parse(parts[1], CultureInfo.InvariantCulture),
                    Y = double.Parse(parts[2], CultureInfo.InvariantCulture)
                });
            }

            return result;
        }

        public async Task<string?> GetAddressByPointAsync(double pointX, double pointY)
        {
            using var client = new HttpClient();
            var url = FormattableString.Invariant(
                $"https://api.pdok.nl/bzk/locatieserver/search/v3_1/reverse?X={pointX}&Y={pointY}&type=adres&rows=1&fl=weergavenaam,nummeraanduiding_id,adresseerbaarobject_id");

            using var doc = JsonDocument.Parse(await client.GetStringAsync(url));
            var docs = doc.RootElement.GetProperty("response").GetProperty("docs");
            return docs.GetArrayLength() > 0
                ? docs[0].GetProperty("weergavenaam").GetString()
                : null;
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

        private static string GenerateIFCFileName(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                return "_.ifc";
            }

            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(address.Trim().Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());

            // Windows doesn't allow trailing dots or spaces in file names
            sanitized = sanitized.TrimEnd('.', ' ');
            if (sanitized.Length == 0)
            {
                sanitized = "_";
            }

            return sanitized + ".ifc";
        }
        #endregion // Methods

        #region Properties
        private SPARQL.Server? SPARQLServer { get; set; } = null;
        private string DatasetName { get; set; } = string.Empty;
        #endregion // Properties
    }
}
