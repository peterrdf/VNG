using System.Globalization;

namespace GenerateTestData
{
    public partial class GenerateTestData : Form
    {
        #region Fields
        private List<Location> _locations = new List<Location>();
        #endregion // Fields

        #region Methods
        public GenerateTestData()
        {
            InitializeComponent();
        }

        private void _buttonRun_Click(object sender, EventArgs e)
        {
            try
            {
                _locations = LoadLocations(_textBoxLocationsFile.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        #endregion // Methods
    }
}
