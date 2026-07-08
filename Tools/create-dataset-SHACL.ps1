<# PowerShell 7.*
.\create-dataset-SHACL.ps1 -DatasetName "test1"
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$DatasetName
)

$FusekiAdminUrl = "http://localhost:3030/$/datasets"
$FusekiUser = "admin"
$FusekiPassword = "admin123"

$credentials = [Convert]::ToBase64String(
    [System.Text.Encoding]::ASCII.GetBytes("${FusekiUser}:${FusekiPassword}"))

$assemblerConfig = @"
@prefix :        <#> .
@prefix fuseki:  <http://jena.apache.org/fuseki#> .
@prefix rdf:     <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
@prefix rdfs:    <http://www.w3.org/2000/01/rdf-schema#> .
@prefix tdb2:    <http://jena.apache.org/2016/tdb#> .
@prefix ja:      <http://jena.hpl.hp.com/2005/11/Assembler#> .
@prefix shacl:   <http://www.w3.org/ns/shacl#> .

:service rdf:type fuseki:Service ;
    fuseki:name "$DatasetName" ;
    fuseki:endpoint [ fuseki:operation fuseki:query ] ;
    fuseki:endpoint [ fuseki:operation fuseki:update ] ;
    fuseki:endpoint [ fuseki:operation fuseki:gsp-rw ] ;
    fuseki:endpoint [ fuseki:operation fuseki:shacl ;
                     fuseki:name "shacl" ] ;
    fuseki:dataset :dataset .

:dataset rdf:type tdb2:DatasetTDB ;
    tdb2:location "$DatasetName" .
"@

$boundary = [System.Guid]::NewGuid().ToString()
$LF = "`r`n"
$body = (
    "--$boundary",
    "Content-Disposition: form-data; name=`"file`"; filename=`"$DatasetName-assembler.ttl`"",
    "Content-Type: application/octet-stream",
    "",
    $assemblerConfig,
    "--$boundary--"
) -join $LF

try {
    $response = Invoke-RestMethod -Uri $FusekiAdminUrl -Method Post `
        -Headers @{ Authorization = "Basic $credentials" } `
        -ContentType "multipart/form-data; boundary=$boundary" `
        -Body $body

    Write-Host "Dataset '$DatasetName' created successfully with SHACL support."
}
catch {
    Write-Error "Error creating dataset: $_"
}