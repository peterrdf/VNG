using System.Diagnostics;
using System.Text.Json;

namespace TestVNGPortal
{
    public partial class TestVNGPortalForm : Form
    {
        private List<string> _ifcFiles = new();
        private List<string> _workflowsFiles = new();

        private VngPortalClient? _api;
        private VngStatusClient? _status;

        public TestVNGPortalForm()
        {
            InitializeComponent();
            FormClosing += async (_, _) =>
            {
                if (_status != null)
                {
                    await _status.DisposeAsync();
                    _status = null;
                }

                _api?.Dispose();
                _api = null;
            };
        }

        private void _buttonSelectIFCModelsFolder_Click(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select the folder containing IFC models",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            _textBoxIFCModelsFolder.Text = dialog.SelectedPath;

            try
            {
                _ifcFiles = Directory
                    .EnumerateFiles(dialog.SelectedPath, "*.ifc", SearchOption.AllDirectories)
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _textBoxLog.AppendText($"Found {_ifcFiles.Count} IFC file(s) in '{dialog.SelectedPath}'\r\n");

            foreach (var file in _ifcFiles)
            {
                _textBoxLog.AppendText($"  {file}\r\n");
            }

            _buttonRun.Enabled = _ifcFiles.Count > 0 && _workflowsFiles.Count > 0;
        }

        private void _buttonSelectWorkflowsFolder_Click(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select the folder containing XML-s (workflows)",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            _textBoxWorkflowsFolder.Text = dialog.SelectedPath;

            try
            {
                _workflowsFiles = Directory
                    .EnumerateFiles(dialog.SelectedPath, "*.xml", SearchOption.AllDirectories)
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _textBoxLog.AppendText($"Found {_workflowsFiles.Count} XML file(s) in '{dialog.SelectedPath}'\r\n");

            foreach (var file in _workflowsFiles)
            {
                _textBoxLog.AppendText($"  {file}\r\n");
            }

            _buttonRun.Enabled = _ifcFiles.Count > 0 && _workflowsFiles.Count > 0;
        }

        private async void _buttonRun_Click(object sender, EventArgs e)
        {
            if (_status != null)
            {
                await _status.DisposeAsync();
            }

            _api?.Dispose();

            _textBoxLog.Clear();

            _api = new VngPortalClient("http://localhost:1145");
            _status = new VngStatusClient("http://localhost:1145");

            _status.StatusUpdateReceived += Status_StatusUpdateReceived;
            _status.ProgressUpdateReceived += p => Progess_UpdateReceived(p.Progress, p.Message, p.Error);
            _status.QueryUpdateReceived += q => Query_UpdateReceived(q.Type, q.Query, q.Result, q.Error);
            _status.View3DUpdateReceived += v => View3D_UpdateRecieved(v.Title, v.WorkflowId, v.Model, v.Error);

            await _status.StartAsync();

            var result = await _api.UploadIfcAsync(@"D:\Temp\TestData\IFC\VNG\IFC Data\Kievitsweg_R23_ILS Spaces update.ifc", "1");
            if (result.Error == null && result.TaskId != null)
            {
                await _status.JoinTaskAsync(result.TaskId);
            }
        }

        private void Status_StatusUpdateReceived(string json)
        {
            var doc = JsonDocument.Parse(json);
            Debug.WriteLine($"Status update: {doc.RootElement.GetProperty("status").GetString()}");

            string? status = null;
            if (doc.RootElement.TryGetProperty("taskDescriptorUpdate", out var update) &&
                update.TryGetProperty("Status", out var statusElement))
            {
                status = statusElement.GetString();
                Invoke(() =>
                {
                    _textBoxLog.AppendText($"Task status: {status}\r\n");
                });
            }

            Invoke(() =>
            {
                _textBoxLog.AppendText($"Task finished with status: {status}\r\n");
            });
        }

        private void Progess_UpdateReceived(float progress, string? message, bool error)
        {
            Invoke(() =>
            {
                _textBoxLog.AppendText($"Progress: {progress * 100:F2}% - {message}\r\n");
            });
        }

        private void Query_UpdateReceived(string? type, string? query, string? result, bool error)
        {
            Invoke(() =>
            {
                _textBoxLog.AppendText($"Query: {type} - {query} - {result}\r\n");
            });
        }

        private void View3D_UpdateRecieved(string? title, string? workflowId, string? model, bool error)
        {
            Invoke(() =>
            {
                _textBoxLog.AppendText($"3D View: {title} - {workflowId} - {model}\r\n");
            });
        }
    }
}