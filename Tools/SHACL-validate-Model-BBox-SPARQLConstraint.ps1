<# PowerShell 7.*
Console Output
.\SHACL-validate-All-BBOX-SPARQLConstraint.ps1 -DatasetName "test1"

FilePath Output
.\SHACL-validate-All-BBOX-SPARQLConstraint.ps1 -DatasetName "test1" > validation-results.txt

Console and FilePath Output
.\SHACL-validate-All-BBOX-SPARQLConstraint.ps1 -DatasetName "test1" | Tee-Object -FilePath validation-results.txt
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$DatasetName
)
$fusekiBase = "http://localhost:3030"
$graphUri = "https://vng.nl/geometries/"

$shapesBody = @"
@prefix sh:   <http://www.w3.org/ns/shacl#> .
@prefix geom: <https://vng.nl/geometry/> .
@prefix rdf:  <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
@prefix xsd:  <http://www.w3.org/2001/XMLSchema#> .

geom:DatasetBBoxShape
    a sh:NodeShape ;
    sh:targetNode <$graphUri> ;

    sh:sparql [
        sh:prefixes [
            sh:declare [ sh:prefix "geom" ; sh:namespace "https://vng.nl/geometry/"^^xsd:anyURI ]
        ] ;
        sh:select """
            SELECT ?this ?globalMinX ?globalMaxX ?globalMinY ?globalMaxY ?globalMinZ ?globalMaxZ
            WHERE {

                {
                    SELECT
                        (MIN(?bboxMinX - ?eastings)  AS ?globalMinX)
                        (MAX(?bboxMaxX - ?eastings)  AS ?globalMaxX)
                        (MIN(?bboxMinY - ?northings) AS ?globalMinY)
                        (MAX(?bboxMaxY - ?northings) AS ?globalMaxY)
                        (MIN(?bboxMinZ)              AS ?globalMinZ)
                        (MAX(?bboxMaxZ)              AS ?globalMaxZ)
                    WHERE {
                        ?geom a              geom:Geometry ;
                              geom:eastings  ?eastings ;
                              geom:northings ?northings ;
                              geom:bboxMinX  ?bboxMinX ;
                              geom:bboxMaxX  ?bboxMaxX ;
                              geom:bboxMinY  ?bboxMinY ;
                              geom:bboxMaxY  ?bboxMaxY ;
                              geom:bboxMinZ  ?bboxMinZ ;
                              geom:bboxMaxZ  ?bboxMaxZ .
                    }
                }

                FILTER(
                    ?globalMaxX - ?globalMinX < 1 || ?globalMaxX - ?globalMinX > 100 ||
                    ?globalMaxY - ?globalMinY < 1 || ?globalMaxY - ?globalMinY > 100 ||
                    ?globalMaxZ - ?globalMinZ < 1 || ?globalMaxZ - ?globalMinZ > 100
                )
            }
        """ ;
        sh:message "Dataset BBox violation: globalMinX={?globalMinX}, globalMaxX={?globalMaxX}, globalMinY={?globalMinY}, globalMaxY={?globalMaxY}, globalMinZ={?globalMinZ}, globalMaxZ={?globalMaxZ}. All values must be in [1, 100]." ;
    ] ;
    sh:severity sh:Violation .
"@

$uri = "$fusekiBase/$DatasetName/shacl?graph=$graphUri"

$response = Invoke-RestMethod -Uri $uri `
    -Method Post `
    -ContentType "text/turtle" `
    -Body $shapesBody

$response