<# PowerShell 7.*
Console Output
.\SHACL-validate-Height-SPARQLConstraint.ps1 -DatasetName "test1"

FilePath Output
.\SHACL-validate-Height-SPARQLConstraint.ps1 -DatasetName "test1" > validation-results.txt

Console and FilePath Output
.\SHACL-validate-Height-SPARQLConstraint.ps1 -DatasetName "test1" | Tee-Object -FilePath validation-results.txt
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
@prefix ifc:  <https://standards.buildingsmart.org/IFC/DEV/IFC4/ADD2/OWL#> .
@prefix rdf:  <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
@prefix xsd:  <http://www.w3.org/2001/XMLSchema#> .

geom:IfcElementHeightShape
    a sh:NodeShape ;
    sh:targetSubjectsOf geom:globalId ;
    sh:targetSubjectsOf geom:bboxMaxZ ;

    sh:sparql [
        sh:prefixes [
            sh:declare [ sh:prefix "geom" ; sh:namespace "https://vng.nl/geometry/"^^xsd:anyURI ] ;
            sh:declare [ sh:prefix "ifc"  ; sh:namespace "https://standards.buildingsmart.org/IFC/DEV/IFC4/ADD2/OWL#"^^xsd:anyURI ] ;
            sh:declare [ sh:prefix "rdf"  ; sh:namespace "http://www.w3.org/1999/02/22-rdf-syntax-ns#"^^xsd:anyURI ]
        ] ;
        sh:select """
            SELECT ?this ?globalId ?bboxMaxZ ?globalMaxZ
            WHERE {
                # Per-node values (any type)
                ?this geom:bboxMaxZ ?bboxMaxZ .
                OPTIONAL { ?this geom:globalId ?globalId . }

                # globalMaxZ computed from IfcWall nodes ONLY
                {
                    SELECT (MAX(?z) AS ?globalMaxZ)
                    WHERE {
                        ?elem rdf:type ?ifcType .
                        VALUES ?ifcType {
                            ifc:IfcWall
                        }
                        ?elem geom:bboxMaxZ ?z .
                    }
                }

                FILTER(?bboxMaxZ - ?globalMaxZ > 1)
            }
        """ ;
        sh:message "Height violation for GlobalId {?globalId}: bboxMaxZ={?bboxMaxZ} minus globalMaxZ={?globalMaxZ} exceeds 1." ;
    ] ;

    sh:severity sh:Violation .
"@

$uri = "$fusekiBase/$DatasetName/shacl?graph=$graphUri"

$response = Invoke-RestMethod -Uri $uri `
    -Method Post `
    -ContentType "text/turtle" `
    -Body $shapesBody

$response