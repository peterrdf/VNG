<# PowerShell 7.*
Console Output
.\SHACL-validate-IfcWalls-BBOX-SPARQLConstraint.ps1 -DatasetName "test1"

FilePath Output
.\SHACL-validate-IfcWalls-BBOX-SPARQLConstraint.ps1 -DatasetName "test1" > validation-results.txt

Console and FilePath Output
.\SHACL-validate-IfcWalls-BBOX-SPARQLConstraint.ps1 -DatasetName "test1" | Tee-Object -FilePath validation-results.txt
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
            ]
        ] ;
        sh:select """
            SELECT `$this ?globalId ?deltaX ?deltaY ?deltaZ
            WHERE {
                `$this geom:globalId ?globalId ;
                       geom:bboxMinX ?minX ;
                       geom:bboxMaxX ?maxX ;
                       geom:bboxMinY ?minY ;
                       geom:bboxMaxY ?maxY ;
                       geom:bboxMinZ ?minZ ;
                       geom:bboxMaxZ ?maxZ .

                BIND((?maxX - ?minX) AS ?deltaX)
                BIND((?maxY - ?minY) AS ?deltaY)
                BIND((?maxZ - ?minZ) AS ?deltaZ)

                FILTER(
                    ?deltaX < 1 || ?deltaX > 100 ||
                    ?deltaY < 1 || ?deltaY > 100 ||
                    ?deltaZ < 1 || ?deltaZ > 100
                )
            }
        """ ;
        sh:message "BBox violation for GlobalId {?globalId}: deltaX={?deltaX}, deltaY={?deltaY}, deltaZ={?deltaZ}. All axis deltas must be in [1, 100]." ;
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