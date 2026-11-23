param([string]$UnityPath)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
# Fora de Temp: fechar o Editor do projeto principal pode limpar essa pasta.
$validationRoot = Join-Path $projectRoot ('Builds/WM-' + [guid]::NewGuid().ToString('N').Substring(0, 8))
if (-not $UnityPath) {
    $versionText = Get-Content -LiteralPath (Join-Path $projectRoot 'ProjectSettings/ProjectVersion.txt') -Raw
    $version = [regex]::Match($versionText, 'm_EditorVersion: (\S+)').Groups[1].Value
    $UnityPath = "C:/Program Files/Unity/Hub/Editor/$version/Editor/Unity.exe"
}
if (-not (Test-Path -LiteralPath $UnityPath)) { throw 'Unity.exe not found; supply -UnityPath.' }
New-Item -ItemType Directory -Force -Path $validationRoot | Out-Null
foreach ($folder in @('Assets', 'Packages', 'ProjectSettings')) {
    Copy-Item -LiteralPath (Join-Path $projectRoot $folder) -Destination $validationRoot -Recurse -Force
}
$settingsPath = Join-Path $validationRoot 'ProjectSettings/ProjectSettings.asset'
$settings = Get-Content -LiteralPath $settingsPath -Raw
$settings = $settings -replace '(?m)^  productName:.*$', '  productName: Mask Island Puzzle Validation'
Set-Content -LiteralPath $settingsPath -Value $settings -Encoding UTF8
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'WorldMiningValidation.cs') -Destination (Join-Path $validationRoot 'Assets/Scripts/WorldMiningValidation.cs')
$logPath = Join-Path $validationRoot 'world-mining.log'
$arguments = @('-batchmode', '-burst-disable-compilation', '-screen-width', '1280', '-screen-height', '720',
    '-projectPath', ('"' + $validationRoot + '"'), '-executeMethod', 'WorldMiningValidation.Run', '-logFile', ('"' + $logPath + '"'))
$process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -WindowStyle Hidden -PassThru
$process.WaitForExit()
Write-Output "Validation project: $validationRoot"
$report = Join-Path $validationRoot 'world-mining-results.txt'
if (Test-Path -LiteralPath $report) { Get-Content -LiteralPath $report }
else { Get-Content -LiteralPath $logPath -Tail 30; throw 'Unity exited without a test report.' }
if ($process.ExitCode -ne 0) { throw "Validation failed: $($process.ExitCode)" }

