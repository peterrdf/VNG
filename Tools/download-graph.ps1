<# PowerShell 7.*
# Download the default graph
.\download-graph.ps1 -EndpointUrl "http://localhost:3030/test1" -OutputFile "default.ttl"

# Download a named graph
.\download-graph.ps1 -EndpointUrl "http://localhost:3030/test1" -GraphUri "https://vng.nl/geometries/" -OutputFile "geometries.ttl"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$EndpointUrl,

    [Parameter()]
    [string]$GraphUri,

    [Parameter()]
    [string]$OutputFile = "output.ttl"
)

$ProgressPreference = 'SilentlyContinue'
Add-Type -AssemblyName System.Net.Http

if (-not [System.IO.Path]::IsPathRooted($OutputFile)) {
    $OutputFile = Join-Path (Get-Location) $OutputFile
}

if ($GraphUri) {
    $requestUrl = "$EndpointUrl`?graph=$([Uri]::EscapeDataString($GraphUri))"
    Write-Host "Downloading named graph: $GraphUri"
} else {
    $requestUrl = "$EndpointUrl`?default"
    Write-Host "Downloading default graph"
}

try {
    $handler = New-Object System.Net.Http.HttpClientHandler
    $client = New-Object System.Net.Http.HttpClient($handler)
    $client.Timeout = [TimeSpan]::FromMinutes(30)
    $client.DefaultRequestHeaders.Add("Accept", "text/turtle")

    Write-Host "Requesting: $requestUrl"

    $response = $client.GetAsync($requestUrl, [System.Net.Http.HttpCompletionOption]::ResponseHeadersRead).GetAwaiter().GetResult()

    Write-Host "Status: $($response.StatusCode) ($([int]$response.StatusCode))"
    Write-Host "Content-Type: $($response.Content.Headers.ContentType)"
    Write-Host "Content-Length: $($response.Content.Headers.ContentLength)"

    if (-not $response.IsSuccessStatusCode) {
        $errorBody = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        Write-Error "Server returned $([int]$response.StatusCode): $errorBody"
        return
    }

    $stream = $response.Content.ReadAsStreamAsync().GetAwaiter().GetResult()
    $fileStream = [System.IO.FileStream]::new($OutputFile, [System.IO.FileMode]::Create, [System.IO.FileAccess]::Write)

    try {
        $buffer = New-Object byte[] 81920
        $totalBytes = 0
        while (($bytesRead = $stream.Read($buffer, 0, $buffer.Length)) -gt 0) {
            $fileStream.Write($buffer, 0, $bytesRead)
            $totalBytes += $bytesRead
            Write-Host "`rDownloaded: $([math]::Round($totalBytes / 1KB, 2)) KB" -NoNewline
        }
        Write-Host ""

        if ($totalBytes -eq 0) {
            Write-Warning "Response was empty — check the endpoint URL. Common Graph Store Protocol endpoints:`n  Fuseki: http://host:3030/<dataset>/data`n  GraphDB: http://host:7200/repositories/<repo>/rdf-graphs/service"
        } else {
            Write-Host "Saved to $OutputFile ($([math]::Round($totalBytes / 1KB, 2)) KB)"
        }
    }
    finally {
        $fileStream.Flush()
        $fileStream.Close()
        $stream.Close()
    }
}
catch {
    Write-Error "Failed to download graph: $_"
}
finally {
    if ($client) { $client.Dispose() }
}