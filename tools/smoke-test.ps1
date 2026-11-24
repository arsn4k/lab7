# Smoke test of the ItOpsMonitor API deployed on Render (Windows PowerShell 5.1 / PowerShell 7)
# Usage: .\tools\smoke-test.ps1 -BaseUrl https://itops-api.onrender.com -Origin https://itops-web.onrender.com
param(
    [Parameter(Mandatory = $true)] [string] $BaseUrl,
    [string] $Origin = ""
)

$ErrorActionPreference = 'Stop'
$BaseUrl = $BaseUrl.TrimEnd('/')
$script:failed = 0

function Step([string] $name, [scriptblock] $action) {
    try {
        $result = & $action
        Write-Host "[OK]   $name" -ForegroundColor Green
        return $result
    }
    catch {
        Write-Host "[FAIL] $name -> $($_.Exception.Message)" -ForegroundColor Red
        $script:failed++
    }
}

function Expect-Status([int] $expected, [scriptblock] $request) {
    try { & $request | Out-Null; $code = 200 }
    catch { $code = [int]$_.Exception.Response.StatusCode }
    if ($code -ne $expected) { throw "expected HTTP $expected, got $code" }
}

$json = 'application/json'
$name = 'smoke-' + (Get-Date -Format 'HHmmss')

Step 'GET /health -> Healthy' {
    $h = Invoke-RestMethod "$BaseUrl/health"
    if ($h.status -ne 'Healthy') { throw "status = $($h.status)" }
} | Out-Null

Step 'GET /swagger/v1/swagger.json' { Invoke-RestMethod "$BaseUrl/swagger/v1/swagger.json" } | Out-Null

$body = @{ hostname = $name; ip_address = '10.9.0.1'; environment = 'Staging'; cpu_alert_threshold = 80 } | ConvertTo-Json
$created = Step "POST /api/servers ($name) -> 201" {
    Invoke-RestMethod "$BaseUrl/api/servers" -Method Post -Body $body -ContentType $json
}

Step 'GET /api/servers/{id}' {
    $s = Invoke-RestMethod "$BaseUrl/api/servers/$($created.id)"
    if ($s.hostname -ne $name) { throw 'hostname mismatch' }
} | Out-Null

Step 'POST duplicate hostname (upper case) -> 409' {
    $dup = @{ hostname = $name.ToUpper(); ip_address = '10.9.0.2'; environment = 'Staging' } | ConvertTo-Json
    Expect-Status 409 { Invoke-RestMethod "$BaseUrl/api/servers" -Method Post -Body $dup -ContentType $json }
} | Out-Null

Step 'POST invalid data (threshold 150) -> 400' {
    $bad = @{ hostname = 'bad-host'; ip_address = '10.9.0.3'; environment = 'Test'; cpu_alert_threshold = 150 } | ConvertTo-Json
    Expect-Status 400 { Invoke-RestMethod "$BaseUrl/api/servers" -Method Post -Body $bad -ContentType $json }
} | Out-Null

Step 'POST /api/servers/{id}/metrics -> 201' {
    $m = @{ cpu_usage = 42.5; memory_usage = 61.0; temperature = 48.25 } | ConvertTo-Json
    Invoke-RestMethod "$BaseUrl/api/servers/$($created.id)/metrics" -Method Post -Body $m -ContentType $json
} | Out-Null

Step 'GET /api/servers/{id}/metrics -> 1 record' {
    $list = @(Invoke-RestMethod "$BaseUrl/api/servers/$($created.id)/metrics")
    if ($list.Count -lt 1) { throw 'no metrics' }
} | Out-Null

Step 'DELETE /api/servers/{id} -> 204' {
    Invoke-RestMethod "$BaseUrl/api/servers/$($created.id)" -Method Delete
} | Out-Null

Step 'GET deleted server -> 404' {
    Expect-Status 404 { Invoke-RestMethod "$BaseUrl/api/servers/$($created.id)" }
} | Out-Null

if ($Origin) {
    Step "CORS preflight from $Origin" {
        $r = Invoke-WebRequest "$BaseUrl/api/servers" -Method Options -UseBasicParsing -Headers @{
            Origin = $Origin; 'Access-Control-Request-Method' = 'POST' }
        if ($r.Headers['Access-Control-Allow-Origin'] -ne $Origin) { throw 'Access-Control-Allow-Origin missing' }
    } | Out-Null
}

if ($script:failed -gt 0) { Write-Host "Failed checks: $script:failed" -ForegroundColor Red; exit 1 }
Write-Host 'All checks passed' -ForegroundColor Green
