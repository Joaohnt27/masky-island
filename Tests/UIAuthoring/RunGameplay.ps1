$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$copyRoot = Join-Path $projectRoot 'Temp/GameplayWork/ValidationProject'
New-Item -ItemType Directory -Force -Path $copyRoot | Out-Null
foreach ($folder in @('Assets', 'Packages', 'ProjectSettings')) {
    Copy-Item -LiteralPath (Join-Path $projectRoot $folder) -Destination $copyRoot -Recurse -Force
}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'AuthorGameplayUI.cs') -Destination (Join-Path $copyRoot 'Assets/Editor/AuthorGameplayUI.cs') -Force
$versionText = Get-Content -LiteralPath (Join-Path $projectRoot 'ProjectSettings/ProjectVersion.txt') -Raw
$version = [regex]::Match($versionText, 'm_EditorVersion: (\S+)').Groups[1].Value
$unityPath = "C:/Program Files/Unity/Hub/Editor/$version/Editor/Unity.exe"
$logPath = Join-Path $projectRoot 'Temp/PauseWork/authoring.log'
$arguments = @('-batchmode', '-nographics', '-quit', '-projectPath', ('"' + $copyRoot + '"'), '-executeMethod', 'AuthorGameplayUI.Run', '-logFile', ('"' + $logPath + '"'))
$process = Start-Process -FilePath $unityPath -ArgumentList $arguments -WindowStyle Hidden -PassThru -Wait
Get-Content -LiteralPath $logPath -Tail 20
if ($process.ExitCode -ne 0) { throw 'Falha na criação da UI; consulte o log.' }
