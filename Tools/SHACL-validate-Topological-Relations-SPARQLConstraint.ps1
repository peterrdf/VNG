<# PowerShell 7.*
Console Output
.\SHACL-validate-IfcWalls-SPARQLConstraint.ps1 -DatasetName "test1"

FilePath Output
.\SHACL-validate-IfcWalls-SPARQLConstraint.ps1 -DatasetName "test1" > validation-results.txt

Console and FilePath Output
.\SHACL-validate-IfcWalls-SPARQLConstraint.ps1 -DatasetName "test1" | Tee-Object -FilePath validation-results.txt
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$DatasetName
)
$fusekiBase = "http://localhost:3030"
$graphUri = "https://vng.nl/geometries/"

# SHACL shapes in Turtle format
$shapesBody = @"
@prefix sh:   <http://www.w3.org/ns/shacl#> .
@prefix geom: <https://vng.nl/geometry/> .
@prefix rdf:  <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
@prefix xsd:  <http://www.w3.org/2001/XMLSchema#> .

geom:GeometryShape
    a sh:NodeShape ;
    sh:targetClass geom:Geometry ;

    sh:sparql [
        sh:prefixes [
            sh:declare [
                sh:prefix "geom" ;
                sh:namespace "https://vng.nl/geometry/"^^xsd:anyURI
            ] ;
            sh:declare [
                sh:prefix "rdf" ;
                sh:namespace "http://www.w3.org/1999/02/22-rdf-syntax-ns#"^^xsd:anyURI
            ]
        ] ;
        sh:select """
            SELECT `$this ?value ?globalId
            WHERE {
                `$this geom:globalId ?globalId ;
                       geom:topologicalRelation ?value .
                FILTER(?value != "CONTAINED BY")
            }
        """ ;
        sh:message "geom:topologicalRelation is NOT 'CONTAINED BY'. GlobalId: {?globalId}" ;
    ] ;
    sh:severity sh:Violation .
"@

# Validate using Fuseki's SHACL endpoint
$uri = "$fusekiBase/$DatasetName/shacl?graph=$graphUri"

$response = Invoke-RestMethod -Uri $uri `
    -Method Post `
    -ContentType "text/turtle" `
    -Body $shapesBody

$response