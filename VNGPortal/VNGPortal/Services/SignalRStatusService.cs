using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Serilog.Core;
using System.Collections.Concurrent;
using System.Xml;
using System.Xml.Serialization;
using VNGPortal.Workflows;

namespace VNGPortal.Services
{
    public record TaskDescriptor(
        string TaskId,
        string GroupName,
        string Workflow,
        string Model,       
        string Status = "Pending",
        string Error = "",
        int Progress = 0, // Progress percentage (0-100)
        DateTime LastUpdated = default)
    {
        public TaskDescriptor() : this(string.Empty, string.Empty, string.Empty, string.Empty) { }
    }

    public interface ISignalRStatusService
    {
        Task AddTask(string taskId, string groupName, string workflow, string model, string status);
        Task UpdateTask(string taskId, string status);
        Task CompleteTask(string taskId, string error);
        Task RemoveTask(string taskId);
        Task<ICollection<TaskDescriptor>> GetTasks();
        Task<TaskDescriptor?> GetTask(string taskId);
        Task<IList<TaskDescriptor>> GetRunningTasks();
        Task<int> GetActiveConnectionsCount(string groupName);
        Task SendStatusUpdate(string groupName, string message);
        Task SendStatusUpdate(string groupName, string message, object data);
        Task SendProgressUpdate(string groupName, int progress, string message, bool error);
        Task SendQueryUpdate(string groupName, string type, string query, string result, bool error);
    }

    public class SignalRStatusService : ISignalRStatusService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SignalRStatusService> _logger;
        private readonly IHubContext<StatusHub> _hubContext;
        private readonly IConnectionStateService _connectionState;
        private readonly ConcurrentDictionary<string, TaskDescriptor> _tasks = new();

        private static readonly HashSet<string> TerminalStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Completed", "Failed", "Cancelled", "Error"
        };

        private static readonly HashSet<string> RunningStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Running", "Processing", "Uploading"
        };

        public SignalRStatusService(            
            IConfiguration configuration,
            ILogger<SignalRStatusService> logger,
            IHubContext<StatusHub> hubContext, 
            IConnectionStateService connectionState)
        {
            _logger = logger;
            _configuration = configuration;
            _hubContext = hubContext;
            _connectionState = connectionState;

            //#todo: Load tasks from XML files on startup
            //LoadTasks();

            _ = Task.Run(async () => await ExecuteTask());
        }

        public async Task AddTask(string taskId, string groupName, string workflow, string model, string status)
        {
            if (string.IsNullOrEmpty(taskId))
            {
                throw new ArgumentException("TaskId cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(groupName))
            {
                throw new ArgumentException("groupName cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(model))
            {
                throw new ArgumentException("Model cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentException("Status cannot be null or empty.");
            }

            var newTask = new TaskDescriptor(taskId, groupName, workflow, model,  status, LastUpdated: DateTime.UtcNow);
            if (!_tasks.TryAdd(taskId, newTask))
            {
                throw new InvalidOperationException($"Task with ID '{taskId}' already exists.");
            }

            await SendTaskUpdate(newTask);
        }

        public async Task UpdateTask(string taskId, string status)
        {
            if (string.IsNullOrEmpty(taskId))
            {
                throw new ArgumentException("TaskId cannot be null or empty.");
            }

            TaskDescriptor? taskDescriptorUpdated = null;

            try
            {
                _tasks.AddOrUpdate(
                    taskId,
                    // Add factory
                    key => throw new InvalidOperationException($"Task with ID '{taskId}' not found."),
                    // Update factory
                    (key, existingTask) =>
                    {
                        taskDescriptorUpdated = existingTask with
                        {
                            Status = status,
                            LastUpdated = DateTime.UtcNow
                        };
                        return taskDescriptorUpdated;
                    });

                if (taskDescriptorUpdated != null)
                {
                    await SendTaskUpdate(taskDescriptorUpdated);
                }
            }
            catch (InvalidOperationException)
            {
                // Task not found
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to complete task '{taskId}': {ex.Message}", ex);
            }
        }

        public async Task CompleteTask(string taskId, string error)
        {
            if (string.IsNullOrEmpty(taskId))
            {
                throw new ArgumentException("TaskId cannot be null or empty.");
            }

            TaskDescriptor? taskDescriptorUpdated = null;

            try
            {
                _tasks.AddOrUpdate(
                    taskId,
                    // Add factory
                    key => throw new InvalidOperationException($"Task with ID '{taskId}' not found."),
                    // Update factory
                    (key, existingTask) =>
                    {
                        // Already in terminal state
                        if (TerminalStatuses.Contains(existingTask.Status))
                        {
                            return existingTask;
                        }

                        taskDescriptorUpdated = existingTask with
                        {
                            Status = string.IsNullOrEmpty(error) ? "Completed" : "Error",
                            Error = error ?? string.Empty,
                            Progress = string.IsNullOrEmpty(error) ? 100 : existingTask.Progress,
                            LastUpdated = DateTime.UtcNow
                        };
                        return taskDescriptorUpdated;
                    });

                if (taskDescriptorUpdated != null)
                {
                    //#todo: Save task status to XML file
                    //var isLinuxPlatform = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);
                    //var fileStorage = isLinuxPlatform ? "FileStorageLinux" : "FileStorage";

                    //var tasksDir = _configuration[$"{fileStorage}:TasksDir"]!;
                    //var taskXMLPath = Path.Combine(tasksDir, $"{taskId}.xml");
                    //var serializer = new XmlSerializer(typeof(TaskDescriptor));
                    //using var stream = System.IO.File.Create(taskXMLPath);
                    //serializer.Serialize(stream, taskDescriptorUpdated);

                    await SendTaskUpdate(taskDescriptorUpdated);
                }
            }
            catch (InvalidOperationException)
            {
                // Task not found
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to complete task '{taskId}': {ex.Message}", ex);
            }
        }

        public async Task RemoveTask(string taskId)
        {
            if (string.IsNullOrEmpty(taskId))
            {
                throw new ArgumentException("TaskId cannot be null or empty.");
            }

            bool taskRemoved = _tasks.TryRemove(taskId, out var taskDescriptor);
            if (taskRemoved && (taskDescriptor != null))
            {
                string statusJSON = JsonConvert.SerializeObject(new
                {
                    status = _tasks.IsEmpty ? "idle" : "busy",
                    tasks = _tasks
                }, Newtonsoft.Json.Formatting.Indented);

                await SendStatusUpdate(taskDescriptor.GroupName, statusJSON);
            }
        }

        public Task<ICollection<TaskDescriptor>> GetTasks()
        {
            return Task.FromResult(_tasks.Values);
        }

        public Task<TaskDescriptor?> GetTask(string taskId)
        {
            if (string.IsNullOrEmpty(taskId))
            {
                throw new ArgumentException("TaskId cannot be null or empty.");
            }

            _tasks.TryGetValue(taskId, out var taskDescriptor);
            return Task.FromResult(taskDescriptor);
        }

        public Task<IList<TaskDescriptor>> GetRunningTasks()
        {
            var runningTasks = _tasks.Values
                .Where(t => !string.IsNullOrEmpty(t.Status) && RunningStatuses.Contains(t.Status))
                .ToList();

            return Task.FromResult<IList<TaskDescriptor>>(runningTasks);
        }

        public async Task<int> GetActiveConnectionsCount(string groupName)
        {
            var connections = await _connectionState.GetConnectionsInGroup(groupName);
            return connections.Count();
        }

        public async Task SendStatusUpdate(string groupName, string message)
        {
            var connections = await _connectionState.GetConnectionsInGroup(groupName);
            if (connections.Any())
            {
                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveStatusUpdate", message);
            }
        }

        public async Task SendStatusUpdate(string groupName, string message, object data)
        {
            var connections = await _connectionState.GetConnectionsInGroup(groupName);
            if (connections.Any())
            {
                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveStatusUpdate", message, data);
            }
        }

        public async Task SendProgressUpdate(string groupName, int progress, string message, bool error)
        {
            var connections = await _connectionState.GetConnectionsInGroup(groupName);
            if (connections.Any())
            {
                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveProgressUpdate", new { progress, message, error });
            }
        }

        public async Task SendQueryUpdate(string groupName, string type, string query, string result, bool error)
        {
            var connections = await _connectionState.GetConnectionsInGroup(groupName);
            if (connections.Any())
            {
                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveQueryUpdate", new { type, query, result, error });
            }
        }

        private async Task SendTaskUpdate(TaskDescriptor taskDescriptor)
        {
            var runningTasks = _tasks.Values
                .Where(t => !string.IsNullOrEmpty(t.Status) && RunningStatuses.Contains(t.Status))
                .ToList();

            string statusJSON = JsonConvert.SerializeObject(new
            {
                status = runningTasks.Count() > 0 ? "busy" : "idle",
                runningTasks,
                taskDescriptorUpdate = taskDescriptor,
            }, Newtonsoft.Json.Formatting.Indented);

            await SendStatusUpdate(taskDescriptor.GroupName, statusJSON);
        }

        private void LoadTasks()
        {
            var isLinuxPlatform = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);
            var fileStorage = isLinuxPlatform ? "FileStorageLinux" : "FileStorage";

            var provider = new PhysicalFileProvider(_configuration[$"{fileStorage}:TasksDir"]!);
            var taskXMLs = provider.GetDirectoryContents("/").Where((fileInfo) =>
            {
                if (fileInfo.IsDirectory)
                    return false;

                if (!fileInfo.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    return false;

                return true;
            });

            List<TaskDescriptor> xmlTasks = new();
            foreach (var taskXML in taskXMLs)
            {
                if (taskXML?.PhysicalPath != null)
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(taskXML.PhysicalPath);

                    var id = Path.GetFileNameWithoutExtension(taskXML.Name);
                    if (string.IsNullOrEmpty(id))
                    {
                        continue;
                    }

                    _tasks.TryAdd(id, new TaskDescriptor
                    {
                        TaskId = id,
                        GroupName = xmlDoc.SelectSingleNode("//TaskDescriptor/GroupName")?.InnerText ?? "Unknown",
                        Model = xmlDoc.SelectSingleNode("//TaskDescriptor/Model")?.InnerText ?? "Unknown",
                        Status = xmlDoc.SelectSingleNode("//TaskDescriptor/Status")?.InnerText ?? "Pending",
                        Error = xmlDoc.SelectSingleNode("//TaskDescriptor/Error")?.InnerText ?? "",
                        Progress = int.TryParse(xmlDoc.SelectSingleNode("//TaskDescriptor/Progress")?.InnerText, out var progress) ? progress : 0,
                        LastUpdated = xmlDoc.SelectSingleNode("//TaskDescriptor/LastUpdated")?.InnerText != null
                            ? DateTime.Parse(xmlDoc.SelectSingleNode("//TaskDescriptor/LastUpdated")!.InnerText)
                            : DateTime.UtcNow
                    });
                }
            }
        }

        public async Task ExecuteTask()
        {
            while (true)
            {
                var runningTasks = _tasks.Values
                .Where(t => !string.IsNullOrEmpty(t.Status) && RunningStatuses.Contains(t.Status))
                .ToList();
                if (runningTasks.Count == 0)
                {
                    var pendingTasks = _tasks.Values.Where(t => string.Equals(t.Status, "Pending", StringComparison.OrdinalIgnoreCase)).ToList();
                    if (pendingTasks.Count > 0)
                    {
                        var taskDescriptor = pendingTasks.First();

                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await RunTask(taskDescriptor);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Error executing task {taskDescriptor.TaskId}: {ex.Message}");
                            }
                        });
                    }
                }

                // Check for tasks every 10 seconds
                await Task.Delay(10 * 1000); 
            }
        }

        public async Task RunTask(TaskDescriptor taskDescriptor)
        {
            try
            {
                await UpdateTask(taskDescriptor.TaskId, "Running");
                if (taskDescriptor.Workflow == "VNG")
                {
                    var workflow = new VNGWorkflow(_configuration, _logger, this, taskDescriptor.TaskId, null);
                    if (await workflow.ExecuteAsync(taskDescriptor))
                    {
                        await CompleteTask(taskDescriptor.TaskId, "");
                        return;
                    }
                }
                else
                {
                    _logger.LogError("Invalid workflow specified.");
                    await CompleteTask(taskDescriptor.TaskId, "Invalid workflow specified.");
                    return;
                }

                //#todo
                //_Workflow.SaveModelXml(openMVG_openMVS.NAME, taskDescriptor.TaskId, taskDescriptor.Model, modelsDir);
                _logger.LogError("Internal error.");
                await CompleteTask(taskDescriptor.TaskId, "Internal error.");
            }
            catch (Exception ex)
            {
                //#todo
                //_Workflow.SaveModelXml(openMVG_openMVS.NAME, taskDescriptor.TaskId, taskDescriptor.Model, modelsDir);
                _logger.LogError(ex, "Running task failed.");
                await CompleteTask(taskDescriptor.TaskId, ex.Message);
                return;
            }
        }
    }
}