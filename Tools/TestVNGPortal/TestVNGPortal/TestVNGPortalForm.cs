using System.Diagnostics;
using System.Text.Json;

namespace TestVNGPortal
{
    public partial class TestVNGPortalForm : Form
    {
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

        private async void _buttonRun_Click(object sender, EventArgs e)
        {
            if (_status != null)
            {
                await _status.DisposeAsync();
            }

            _api?.Dispose();

            _api = new VngPortalClient("http://localhost:1145");
            _status = new VngStatusClient("http://localhost:1145");

            _status.ProgressUpdateReceived += p => Debug.WriteLine($"{(p.Error ? "ERR" : "INFO")}: {p.Message}");
            _status.StatusUpdateReceived += Status_StatusUpdateReceived;
            _status.QueryUpdateReceived += q => Debug.WriteLine($"{q.Type}: {q.Query}{q.Result}");
            _status.View3DUpdateReceived += v => Debug.WriteLine($"3D view: {v.Title} ({v.Model})");

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
            }

            if (status != "Running")
            {
                //TaskFinished?.Invoke();
            }
        }
    }
}