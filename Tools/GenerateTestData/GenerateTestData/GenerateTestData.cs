using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace GenerateTestData
{
    public partial class GenerateTestDataForm : Form
    {
        #region Fields
        private string _ifcModelPath;
        private List<Location> _locations = new List<Location>();
        #endregion // Fields

        #region Methods
        public GenerateTestDataForm()
        {
            InitializeComponent();
        }

        private void _buttonSelectIFCModel_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "IFC Files (*.ifc)|*.ifc|All Files (*.*)|*.*||";
            openFileDialog.Title = "Open";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _textBoxIFCModel.Text = openFileDialog.FileName;
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

        private void _buttonSelectLocationFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*||";
            openFileDialog.Title = "Open";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _textBoxLocationsFile.Text = openFileDialog.FileName;
            }
        }
    }
}