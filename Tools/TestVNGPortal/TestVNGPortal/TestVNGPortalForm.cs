using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace TestVNGPortal
{
    public partial class TestVNGPortalForm : Form
    {
        private List<string> _ifcFiles = new();
        private List<string> _workflowsFiles = new();

        private VngPortalClient? _api;
        private VngStatusClient? _status;

        private TaskCompletionSource<string?>? _taskCompletion;

        private string _dateTimeStamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        private int _currentTestIndex = 0;
        private string _currentTestName = string.Empty;
        private Stream _streamFull;
        private Stream _streamOK;
        private Stream _streamFailed;

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

            _buttonRun.Enabled = _ifcFiles.Count > 0 && !string.IsNullOrEmpty(_comboBoxWorkflows.SelectedItem?.ToString());
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
            _comboBoxWorkflows.Items.Clear();

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
                _comboBoxWorkflows.Items.Add(Path.GetFileName(file));
                _textBoxLog.AppendText($"  {file}\r\n");
            }

            _buttonRun.Enabled = _ifcFiles.Count > 0 && !string.IsNullOrEmpty(_comboBoxWorkflows.SelectedItem?.ToString());
        }

        private static bool IsTerminalStatus(string? status) =>
            status is not null &&
            (status.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
             status.Equals("Failed", StringComparison.OrdinalIgnoreCase) ||
             status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) ||
             status.Equals("Error", StringComparison.OrdinalIgnoreCase));

        private async void _buttonRun_Click(object sender, EventArgs e)
        {
            _buttonRun.Enabled = false;
            try
            {
                if (_status != null)
                {
                    await _status.DisposeAsync();
                    _status = null;
                }

                _api?.Dispose();

                _textBoxLog.Clear();
                _dateTimeStamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                _streamFull = new FileStream(Path.Combine(_textBoxIFCModelsFolder.Text, $"Test-VNGPortal-Log-Full-{_dateTimeStamp}.txt"), FileMode.Create, FileAccess.Write, FileShare.Read);
                _streamOK = new FileStream(Path.Combine(_textBoxIFCModelsFolder.Text, $"Test-VNGPortal-Log-OK-{_dateTimeStamp}.txt"), FileMode.Create, FileAccess.Write, FileShare.Read);
                _streamFailed = new FileStream(Path.Combine(_textBoxIFCModelsFolder.Text, $"Test-VNGPortal-Log-Failed-{_dateTimeStamp}.txt"), FileMode.Create, FileAccess.Write, FileShare.Read);
                _currentTestIndex = 0;
                _currentTestName = "";
                await RunWorkflowsAsync();
            }
            catch (Exception ex)
            {
                _textBoxLog.AppendText($"Error: {ex.Message}\r\n");
                _streamFull.Write(Encoding.UTF8.GetBytes($"Error: {ex.Message}\r\n"));
            }
            finally
            {
                _buttonRun.Enabled = _ifcFiles.Count > 0 && !string.IsNullOrEmpty(_comboBoxWorkflows.SelectedItem?.ToString());
            }
        }

        private async Task RunWorkflowsAsync()
        {
            if (_ifcFiles.Count == 0 || _workflowsFiles.Count == 0)
            {
                MessageBox.Show(this, "Please select both IFC models and workflows folders.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _api = new VngPortalClient("http://localhost:1145");
            _status = new VngStatusClient("http://localhost:1145");

            _status.StatusUpdateReceived += Status_StatusUpdateReceived;
            _status.ProgressUpdateReceived += p => Progess_UpdateReceived(p.Progress, p.Message, p.Error);
            _status.QueryUpdateReceived += q => Query_UpdateReceived(q.Type, q.Query, q.Result, q.Error);
            _status.View3DUpdateReceived += v => View3D_UpdateRecieved(v.Title, v.WorkflowId, v.Model, v.Error);

            await _status.StartAsync();

            var files = _ifcFiles.ToList();
            for (int index = 0; index < files.Count; index++)
            {
                var ifcFile = files[index];

                _currentTestIndex = index;
                _currentTestName = Path.GetFileName(ifcFile);
                _textBoxLog.AppendText($"Running: {ifcFile}\r\n");
                _streamFull.Write(Encoding.UTF8.GetBytes("***************************************************************************************************\r\n"));
                _streamFull.Write(Encoding.UTF8.GetBytes($"Running: {ifcFile}\r\n"));

                _taskCompletion = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);

                string? workflowId = GetWorkflowId(_workflowsFiles[_comboBoxWorkflows!.SelectedIndex]!);
                if (string.IsNullOrEmpty(workflowId))
                {
                    _textBoxLog.AppendText($"Error: Could not extract workflow ID from {_workflowsFiles[_comboBoxWorkflows.SelectedIndex]!}\r\n");
                    _streamFull.Write(Encoding.UTF8.GetBytes($"Error: Could not extract workflow ID from {_workflowsFiles[_comboBoxWorkflows.SelectedIndex]!}\r\n"));
                    return;
                }

                var result = await _api.UploadIfcAsync(ifcFile, workflowId);
                _textBoxLog.AppendText($"Upload result: TaskId={result.TaskId}, Error={result.Error ?? "NA"}\r\n");
                _streamFull.Write(Encoding.UTF8.GetBytes($"Upload result: TaskId={result.TaskId}, Error={result.Error ?? "NA\r\n"}"));

                if (!string.IsNullOrEmpty(result.Error) || string.IsNullOrEmpty(result.TaskId))
                {
                    continue;
                }

                await _status.JoinTaskAsync(result.TaskId);

                // Wait until the server reports a terminal status for this task
                var finalStatus = await _taskCompletion.Task;
                _textBoxLog.AppendText($"Task finished with status: {finalStatus}\r\n");
                _streamFull.Write(Encoding.UTF8.GetBytes($"Task finished with status: {finalStatus}\r\n\r\n"));
            } // for (int index = 0; index < files.Count; index++)

            _textBoxLog.AppendText("All tasks finished.\r\n");
            _streamFull.Write(Encoding.UTF8.GetBytes("All tasks finished.\r\n"));
        }

        private static string? GetWorkflowId(string xmlFilePath)
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            using var reader = XmlReader.Create(xmlFilePath, settings);

            // Move to the root element (<workflow>)
            if (!reader.ReadToFollowing("workflow"))
            {
                return null;
            }

            // Look only at direct children of <workflow>, so <id> tags nested deeper are ignored
            int rootDepth = reader.Depth;
            if (!reader.Read())
            {
                return null;
            }

            while (reader.Depth > rootDepth)
            {
                if (reader.NodeType == XmlNodeType.Element &&
                    reader.Depth == rootDepth + 1 &&
                    reader.Name == "id")
                {
                    return reader.ReadElementContentAsString().Trim();
                }

                reader.Skip();
            }

            return null;
        }

        private void Status_StatusUpdateReceived(string json)
        {
            using var doc = JsonDocument.Parse(json);
            Debug.WriteLine($"Status update: {doc.RootElement.GetProperty("status").GetString()}");

            if (doc.RootElement.TryGetProperty("taskDescriptorUpdate", out var update) &&
                update.TryGetProperty("Status", out var statusElement))
            {
                var status = statusElement.GetString();

                Invoke(() => _textBoxLog.AppendText($"Task status: {status}\r\n"));
                _streamFull.Write(Encoding.UTF8.GetBytes($"Task status: {status}\r\n"));

                if (IsTerminalStatus(status))
                {
                    _taskCompletion?.TrySetResult(status);
                }
            }
        }

        private void Progess_UpdateReceived(float progress, string? message, bool error)
        {
            Invoke(() =>
            {
                _textBoxLog.AppendText($"Progress: {progress * 100:F2}% - {message}\r\n");
                _streamFull.Write(Encoding.UTF8.GetBytes($"Progress: {progress * 100:F2}% - {message}\r\n"));
            });
        }

        private void Query_UpdateReceived(string? type, string? query, string? result, bool error)
        {
            Invoke(() =>
            {
                _textBoxLog.AppendText($"Query: {type} - {query} - {result}\r\n");

                bool log = false;
                bool hasError = false;
                if (type == "SHACL Validation Report")
                {
                    log = true;
                    if (result?.IndexOf("sh:Violation") != -1)
                    {                        
                        hasError = true;
                        _textBoxLog.AppendText($"SHACL Validation Report:\r\n{result}\r\n");
                        _streamFull.Write(Encoding.UTF8.GetBytes($"SHACL Validation Report:\r\n{result}\r\n"));
                        _streamFull.Flush();
                    }
                }
                else if (type == "IDS Validation Report")
                {
                    log = true;
                    if (result?.IndexOf("ERROR") != -1)
                    {                        
                        hasError = true;
                        _textBoxLog.AppendText($"IDS Validation Report:\r\n{result}\r\n");
                        _streamFull.Write(Encoding.UTF8.GetBytes($"IDS Validation Report:\r\n{result}\r\n"));
                        _streamFull.Flush();
                    }
                }

                if (log)
                {
                    if (hasError)
                    {
                        _streamFull.Write(Encoding.UTF8.GetBytes($"Model ({_currentTestIndex}/{_ifcFiles.Count}): '{_currentTestName}', Test: '{type}' FAILED\r\n"));
                        _streamFull.Flush();
                        _streamFailed.Write(Encoding.UTF8.GetBytes($"Model ({_currentTestIndex}/{_ifcFiles.Count}): '{_currentTestName}', Test: '{type}' FAILED\r\n"));
                        _streamFailed.Flush();
                    }
                    else
                    {
                        _streamFull.Write(Encoding.UTF8.GetBytes($"Model ({_currentTestIndex}/{_ifcFiles.Count}): '{_currentTestName}', Test: '{type}' OK\r\n"));
                        _streamFull.Flush();
                        _streamOK.Write(Encoding.UTF8.GetBytes($"Model ({_currentTestIndex}/{_ifcFiles.Count}): '{_currentTestName}', Test: '{type}' OK\r\n"));
                        _streamOK.Flush();
                    }
                }                
            });
        }

        private void View3D_UpdateRecieved(string? title, string? workflowId, string? model, bool error)
        {
            Invoke(() =>
            {
                _textBoxLog.AppendText($"3D View: {title} - {workflowId} - {model}\r\n");
                //_streamFull.Write(Encoding.UTF8.GetBytes($"3D View: {title} - {workflowId} - {model}\r\n"));
            });
        }

        private void _comboBoxWorkflows_SelectedIndexChanged(object sender, EventArgs e)
        {
            _buttonRun.Enabled = _ifcFiles.Count > 0 && !string.IsNullOrEmpty(_comboBoxWorkflows.SelectedItem?.ToString());
        }
    }
}