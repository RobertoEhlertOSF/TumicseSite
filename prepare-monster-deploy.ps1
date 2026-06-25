param(
    [string]$DeployRepoPath = "..\TumicseSite-Deploy",
    [string]$CommitMessage = "Publish TumicseSite",
    [switch]$SkipPush
)

$ErrorActionPreference = "Stop"

function Invoke-Checked {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,

        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    Write-Host "> $FilePath $($Arguments -join ' ')"
    & $FilePath @Arguments

    if ($LASTEXITCODE -ne 0) {
        throw "Command failed with exit code ${LASTEXITCODE}: $FilePath $($Arguments -join ' ')"
    }
}

function Assert-RequiredFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Required publish file not found: $Path"
    }
}

function Assert-SafeProductionConfig {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Production config not found in publish output: $Path"
    }

    $content = Get-Content -LiteralPath $Path -Raw

    if ($content -match '(?i)Command\s+Timeout\s*=') {
        throw "Do not put Command Timeout in appsettings.Production.json. Configure command timeout in UseSqlServer instead."
    }

    $allowedPlaceholders = @(
        "",
        "__SET_IN_SERVER__",
        "SENHA_REAL",
        "SENHA_REAL_AQUI",
        "CHANGE_ME",
        "YOUR_PASSWORD"
    )

    $matches = [regex]::Matches($content, '(?i)\b(?:Password|Pwd)\s*=\s*([^;"}]+)')
    foreach ($match in $matches) {
        $value = $match.Groups[1].Value.Trim()

        if ($allowedPlaceholders -notcontains $value) {
            throw "appsettings.Production.json appears to contain a real database password. Use the MonsterASP environment variable ConnectionStrings__DefaultConnection instead."
        }
    }
}

function Remove-DirectorySafely {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [string]$AllowedRoot
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        return
    }

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    $fullRoot = [System.IO.Path]::GetFullPath($AllowedRoot)

    if (-not $fullPath.StartsWith($fullRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to remove a path outside the expected root: $fullPath"
    }

    Remove-Item -LiteralPath $fullPath -Recurse -Force
}

function Clear-DeployRepository {
    param(
        [Parameter(Mandatory = $true)]
        [string]$DeployRepoRoot
    )

    Get-ChildItem -LiteralPath $DeployRepoRoot -Force |
        Where-Object { $_.Name -ne ".git" } |
        Remove-Item -Recurse -Force
}

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = [System.IO.Path]::GetFullPath($repoRoot)
$projectPath = Join-Path $repoRoot "TumicseSite\TumicseSite.csproj"
$publishDir = Join-Path $repoRoot "publish-monster"

if ([System.IO.Path]::IsPathRooted($DeployRepoPath)) {
    $deployRepoRoot = [System.IO.Path]::GetFullPath($DeployRepoPath)
}
else {
    $deployRepoRoot = [System.IO.Path]::GetFullPath((Join-Path $repoRoot $DeployRepoPath))
}

if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
    throw "Project file not found: $projectPath"
}

if (-not (Test-Path -LiteralPath $deployRepoRoot -PathType Container)) {
    throw "Deploy repository folder not found: $deployRepoRoot. Clone RobertoEhlertOSF/TumicseSite-Deploy there or pass -DeployRepoPath."
}

if (-not (Test-Path -LiteralPath (Join-Path $deployRepoRoot ".git"))) {
    throw "Deploy repository is not a Git repository: $deployRepoRoot"
}

if ($deployRepoRoot.Equals($repoRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Deploy repository path cannot be the source repository root."
}

Invoke-Checked "git" @("-C", $repoRoot, "rev-parse", "--is-inside-work-tree")
Invoke-Checked "git" @("-C", $deployRepoRoot, "rev-parse", "--is-inside-work-tree")

$currentDeployBranch = (& git -C $deployRepoRoot branch --show-current).Trim()
if ($LASTEXITCODE -ne 0) {
    throw "Could not determine deploy repository branch."
}

if ($currentDeployBranch -ne "main") {
    Invoke-Checked "git" @("-C", $deployRepoRoot, "switch", "main")
}

Invoke-Checked "dotnet" @("clean", $projectPath, "-c", "Release")
Invoke-Checked "dotnet" @("restore", $projectPath)
Invoke-Checked "dotnet" @("build", $projectPath, "-c", "Release", "--no-restore")

Remove-DirectorySafely -Path $publishDir -AllowedRoot $repoRoot
Invoke-Checked "dotnet" @("publish", $projectPath, "-c", "Release", "-o", $publishDir, "--no-restore")

Assert-RequiredFile (Join-Path $publishDir "web.config")
Assert-RequiredFile (Join-Path $publishDir "TumicseSite.dll")
Assert-RequiredFile (Join-Path $publishDir "TumicseSite.deps.json")
Assert-RequiredFile (Join-Path $publishDir "TumicseSite.runtimeconfig.json")
Assert-RequiredFile (Join-Path $publishDir "Microsoft.Data.SqlClient.dll")
Assert-SafeProductionConfig (Join-Path $publishDir "appsettings.Production.json")

Clear-DeployRepository -DeployRepoRoot $deployRepoRoot

Get-ChildItem -LiteralPath $publishDir -Force |
    ForEach-Object {
        Copy-Item -LiteralPath $_.FullName -Destination $deployRepoRoot -Recurse -Force
    }

Assert-RequiredFile (Join-Path $deployRepoRoot "web.config")
Assert-RequiredFile (Join-Path $deployRepoRoot "TumicseSite.dll")
Assert-RequiredFile (Join-Path $deployRepoRoot "TumicseSite.deps.json")
Assert-RequiredFile (Join-Path $deployRepoRoot "TumicseSite.runtimeconfig.json")
Assert-RequiredFile (Join-Path $deployRepoRoot "Microsoft.Data.SqlClient.dll")
Assert-SafeProductionConfig (Join-Path $deployRepoRoot "appsettings.Production.json")

if (Test-Path -LiteralPath (Join-Path $deployRepoRoot "publish-monster")) {
    throw "Invalid deploy repository layout: publish-monster was copied as a subfolder."
}

Invoke-Checked "git" @("-C", $deployRepoRoot, "add", ".")
Invoke-Checked "git" @("-C", $deployRepoRoot, "commit", "--allow-empty", "-m", $CommitMessage)

if ($SkipPush) {
    Write-Host "SkipPush was specified. Deploy repository was prepared locally at $deployRepoRoot."
}
else {
    Invoke-Checked "git" @("-C", $deployRepoRoot, "push", "origin", "main")
}

Write-Host "MonsterASP deploy repository is ready: $deployRepoRoot"
