# Using default localhost URL
# .\set-api-key.ps1 -ServiceName "bag" -APIKey "your-api-key-here"

# Specifying a custom URL
# .\set-api-key.ps1 -ServiceName "dso" -APIKey "your-api-key-here" -BaseUrl "https://yourserver.com"

param(
    [Parameter(Mandatory=$true)]
    [string]$ServiceName,
    
    [Parameter(Mandatory=$true)]
    [string]$APIKey,
    
    [Parameter(Mandatory=$false)]
    [string]$BaseUrl = "http://localhost:1140"
)

$endpoint = "$BaseUrl/Settings?handler=APIKey"

$body = @{
    service = $ServiceName
    key = $APIKey
}

try {
    $response = Invoke-WebRequest -Uri $endpoint -Method POST -Body $body -ContentType "application/x-www-form-urlencoded" -UseBasicParsing
    
    $result = $response.Content | ConvertFrom-Json
    
    if ($result.Success) {
        Write-Host "Success: $($result.Result)" -ForegroundColor Green
    } else {
        Write-Host "Failed: $($result.Result)" -ForegroundColor Red
    }
}
catch {
    Write-Host "Error calling API: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Response content: $($_.Exception.Response)" -ForegroundColor Yellow
    
    # Try to get the actual response body for debugging
    if ($null -ne $response) {
        Write-Host "Response body: $($response.Content)" -ForegroundColor Yellow
    }
    
    exit 1
}