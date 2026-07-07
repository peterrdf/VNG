using System.Text;
using VNGPortal.Services;

namespace VNGPortal.Workflows
{
    public abstract class _Workflow
    {
        #region Fields
        protected readonly ILogger _logger;
        protected readonly IConfiguration _configuration;
        protected readonly ISignalRStatusService _signalRStatus;
        protected readonly string _groupName;
        protected readonly Dictionary<string, string>? _options;
        #endregion // Fields

        #region Methods
        public _Workflow(IConfiguration configuration, ILogger logger, ISignalRStatusService signalRStatus, string groupName, Dictionary<string, string>? options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _signalRStatus = signalRStatus ?? throw new ArgumentNullException(nameof(signalRStatus));
            _groupName = groupName ?? throw new ArgumentNullException(nameof(groupName));
            _options = options;
        }

        public abstract Task<bool> ExecuteAsync(TaskDescriptor taskDescriptor);

        protected string GetOption(string key, string defaultValue = "")
        {
            if (_options != null && _options.TryGetValue(key, out var value))
            {
                return value;
            }
            return defaultValue;
        }

        protected async Task<int> ExecuteProcess(string exePath, string args, string workingDirectory = "", int timeoutHours = 1, IEnumerable<string>? extraPathEntries = null)
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                }
            };

            // IIS worker processes do not inherit the interactive user PATH
            if (extraPathEntries != null)
            {
                var currentPath = process.StartInfo.EnvironmentVariables["PATH"] ?? string.Empty;
                var injected = string.Join(Path.PathSeparator.ToString(), extraPathEntries);
                process.StartInfo.EnvironmentVariables["PATH"] = $"{injected}{Path.PathSeparator}{currentPath}";
            }

            var outputBuilder = new StringBuilder();
            var outputLock = new object();
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    _logger.LogInformation(e.Data);
                    lock (outputLock)
                    {
                        outputBuilder.AppendLine(e.Data);
                    }
                }
            };

            var errorBuilder = new StringBuilder();
            var errorLock = new object();
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    _logger.LogError(e.Data);
                    lock (errorLock)
                    {
                        errorBuilder.AppendLine(e.Data);
                    }
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            int timeoutMilliseconds = timeoutHours * 60 * 60 * 1000;
            var timeoutTask = Task.Delay(timeoutMilliseconds);
            var processTask = process.WaitForExitAsync();

            var completedTask = await Task.WhenAny(processTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                _logger.LogError($"Process timed out after {timeoutHours} hours. Killing process...");
                process.Kill(entireProcessTree: true);
                return -1;
            }

            return process.ExitCode;
        }
        #endregion // Methods

        #region Properties
        public abstract string Name { get; }
        public abstract string Description { get; }
        #endregion // Properties
    }
}
