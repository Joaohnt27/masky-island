param([string]$UnityPath)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$validationRoot = Join-Path $projectRoot ('Builds/GV-' + [guid]::NewGuid().ToString('N').Substring(0, 8))

if (-not $UnityPath) {
    $versionText = Get-Content -LiteralPath (Join-Path $projectRoot 'ProjectSettings/ProjectVersion.txt') -Raw
    $version = [regex]::Match($versionText, 'm_EditorVersion: (\S+)').Groups[1].Value
    $UnityPath = "C:/Program Files/Unity/Hub/Editor/$version/Editor/Unity.exe"
}
if (-not (Test-Path -LiteralPath $UnityPath)) {
    throw 'Unity não encontrado. Informe -UnityPath com o caminho para Unity.exe.'
}

New-Item -ItemType Directory -Force -Path $validationRoot | Out-Null
foreach ($folder in @('Assets', 'Packages', 'ProjectSettings')) {
    Copy-Item -LiteralPath (Join-Path $projectRoot $folder) -Destination $validationRoot -Recurse -Force
}
# A cópia tem preferências próprias; os testes não alteram o nickname/volumes do jogador.
$testSettingsPath = Join-Path $validationRoot 'ProjectSettings/ProjectSettings.asset'
$testSettingsText = Get-Content -LiteralPath $testSettingsPath -Raw
$testSettingsText = $testSettingsText -replace '(?m)^  productName:.*$', '  productName: Mask Island Validation'
Set-Content -LiteralPath $testSettingsPath -Value $testSettingsText -Encoding UTF8
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'GameplayValidation.cs') -Destination (Join-Path $validationRoot 'Assets/Editor/GameplayValidation.cs') -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'GameplayValidationRunner.cs') -Destination (Join-Path $validationRoot 'Assets/Scripts/GameplayValidationRunner.cs') -Force

$reportPath = Join-Path $validationRoot 'validation-results.txt'
if (Test-Path -LiteralPath $reportPath) {
    Remove-Item -LiteralPath $reportPath
}
$logPath = Join-Path $projectRoot 'Temp/GameplayWork/validation.log'
$arguments = @('-batchmode', '-nographics', '-projectPath', ('"' + $validationRoot + '"'),
    '-executeMethod', 'GameplayValidation.Run', '-logFile', ('"' + $logPath + '"'))
$validationProcess = Start-Process -FilePath $UnityPath -ArgumentList $arguments -WindowStyle Hidden -PassThru -Wait
if (Test-Path -LiteralPath $reportPath) {
    Get-Content -LiteralPath $reportPath
} else {
    Get-Content -LiteralPath $logPath -Tail 60
    throw 'Unity encerrou sem relatório. Verifique compilação/licença no log.'
}
if ($validationProcess.ExitCode -ne 0) {
    throw "Validação falhou: código $($validationProcess.ExitCode)."
}
