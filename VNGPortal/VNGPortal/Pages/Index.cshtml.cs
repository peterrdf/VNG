using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RDF;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VNGPortal.IFC2RDF;
using VNGPortal.Services;
using VNGPortal.Workflows;

namespace VNGPortal.Pages;

public class IndexModel : PageModel
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<IndexModel> _logger;
    private readonly ISignalRStatusService _signalRStatus;

    [BindProperty]
    public IFormFile? IfcFile { get; set; }

    [BindProperty]
    public string? SelectedWorkflowId { get; set; }

    public string? Message { get; set; }

    public SortedDictionary<int, Workflow> Workflows { get; private set; } = new SortedDictionary<int, Workflow>();

    public IndexModel(IConfiguration configuration, ILogger<IndexModel> logger, ISignalRStatusService signalRStatus)
    {
        _configuration = configuration;
        _logger = logger;
        _signalRStatus = signalRStatus;
    }

    public void OnGet()
    {
        Workflows = GetWorkflows();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (IfcFile == null || IfcFile.Length == 0)
        {
            Message = "Please select a valid IFC file.";
            return new JsonResult(new { taskId = (string?)null, error = Message });
        }

        // Validate file extension
        var extension = Path.GetExtension(IfcFile.FileName).ToLowerInvariant();
        if (extension != ".ifc")
        {
            Message = "Only .ifc files are allowed.";
            return new JsonResult(new { taskId = (string?)null, error = Message });
        }

        var isLinuxPlatform = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);
        var fileStorage = isLinuxPlatform ? "FileStorageLinux" : "FileStorage";

        var modelsDir = _configuration[$"{fileStorage}:ModelsDir"]!;
        Directory.CreateDirectory(modelsDir);

        var modelId = Guid.NewGuid().ToString();
        var modelDir = Path.Combine(_configuration[$"{fileStorage}:ModelsDir"]!, modelId);
        Directory.CreateDirectory(modelDir);

        var filePath = Path.Combine(modelDir, IfcFile.FileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await IfcFile.CopyToAsync(stream);
        }

        Message = $"File '{IfcFile.FileName}' uploaded successfully!";
        _logger.LogInformation("IFC file uploaded: {FileName} saved at {FilePath}", IfcFile.FileName, filePath);


        //#todo: create a task status file in the tasks directory
        //var taskDescriptor = new TaskDescriptor(
        //        modelId,
        //        modelId,
        //        "VNG",
        //        Path.GetFileName(IfcFile.FileName),
        //        "Pending",
        //        LastUpdated: DateTime.UtcNow);
        //var taskXMLPath = Path.Combine(tasksDir, $"{taskId}.xml");
        //var serializer = new XmlSerializer(typeof(TaskDescriptor));
        //using var stream = System.IO.File.Create(taskXMLPath);
        //serializer.Serialize(stream, taskDescriptor);

        if (string.IsNullOrEmpty(SelectedWorkflowId))
        {
            Message = "Please select a workflow.";
            return new JsonResult(new { taskId = (string?)null, error = Message });
        }

        await _signalRStatus.AddTask(modelId, modelId, SelectedWorkflowId, Path.GetFileName(IfcFile.FileName), "Pending");

        return new JsonResult(new { taskId = (string?)modelId, error = (string?)null });
    }

    public Task<bool> IsGeoReferenced(string filePath)
    {
        // Add null terminator to byte arrays for P/Invoke marshaling
        var filePathBytes = Encoding.UTF8.GetBytes(filePath + "\0");
        var schemaBytes = Encoding.UTF8.GetBytes("\0");
        var sdaiModel = IFCEngine.sdaiOpenModelBN(0, filePathBytes, schemaBytes);
        if (sdaiModel == 0)
        {
            _logger.LogError("Failed to open IFC model: {FilePath}", filePath);
            return Task.FromResult(false);
        }

        double dEastings = 0.0;
        double dNorthings = 0.0;

        long owlModel = 0;
        IFCEngine.owlGetModel(sdaiModel, out owlModel);

        long sdaiAggr = IFCEngine.sdaiGetEntityExtentBN(sdaiModel, "IFCMAPCONVERSION");
        if (sdaiAggr == 0)
        {
            _logger.LogWarning("No IFCMAPCONVERSION entity found in the model.");
            return Task.FromResult(false);
        }

        IFCEngine.engiGetAggrElement(sdaiAggr, 0, IFCEngine.sdaiINSTANCE, out long sdaiIfcMapConversionInstance);
        if (sdaiIfcMapConversionInstance == 0)
        {
            _logger.LogWarning("No instance of IFCMAPCONVERSION found in the model.");
            return Task.FromResult(false);
        }

        long sdaiIfcMapConversionEntity = IFCEngine.sdaiGetInstanceType(sdaiIfcMapConversionInstance);
        if (sdaiIfcMapConversionEntity == 0)
        {
            _logger.LogWarning("Failed to get the entity type of the IFCMAPCONVERSION instance.");
            return Task.FromResult(false);
        }

        long iIndex = 0;
        long sdaiAttr = IFCEngine.engiGetEntityAttributeByIndex(
            sdaiIfcMapConversionEntity,
            iIndex++,
            false,
            true);

        while (sdaiAttr != 0)
        {
            IFCEngine.engiGetEntityArgumentName(sdaiIfcMapConversionEntity,
                iIndex,
                IFCEngine.sdaiSTRING,
                out IntPtr attributeName);
            if (attributeName != IntPtr.Zero)
            {
                string? strAttributeName = Marshal.PtrToStringAnsi(attributeName);
                if (strAttributeName == "Eastings")
                {
                    if (IFCEngine.sdaiGetAttrBN(sdaiIfcMapConversionInstance, strAttributeName, IFCEngine.sdaiREAL, out dEastings) == 0)
                    {
                        _logger.LogWarning("Failed to get Eastings attribute.");
                        return Task.FromResult(false);
                    }
                }
                else if (strAttributeName == "Northings")
                {
                    if (IFCEngine.sdaiGetAttrBN(sdaiIfcMapConversionInstance, strAttributeName, IFCEngine.sdaiREAL, out dNorthings) == 0)
                    {
                        _logger.LogWarning("Failed to get Northings attribute.");
                        return Task.FromResult(false);
                    }
                }
            }

            sdaiAttr = IFCEngine.engiGetEntityAttributeByIndex(
                sdaiIfcMapConversionEntity,
                iIndex++,
                false,
                true);
        } // while (sdaiAttr != 0)

        return Task.FromResult(true);
    }

    public SortedDictionary<int, Workflow> GetWorkflows()
    {
        SortedDictionary<int, Workflow> sortedWorflows = new SortedDictionary<int, Workflow>();

        WorkflowStorage workflowStorage = new WorkflowStorage(_configuration, _logger);
        var worfklows = workflowStorage.LoadWorkflows();

        int fakeWorkflowId = worfklows.Count;
        foreach (var workflow in worfklows.Values)
        {
            if (!int.TryParse(workflow.Id, out int workflowId))
            {
                workflowId = fakeWorkflowId++;
            }

            sortedWorflows.Add(workflowId, workflow);
        }

        return sortedWorflows;
    }
}