param (
    [Parameter(Mandatory = $true)]
    [string]$Version,          # e.g. "v4.0.1"

    [Parameter(Mandatory = $true)]
    [string]$Description,      # e.g. "Improved performance, bug fixes"

    [string]$ProjectPath = "./StickFightExtendedPlayers.csproj",   # Change this to your actual .csproj
    [string]$DllName = "StickFightExtendedPlayers.dll",             # Name of the built DLL to attach
	[string]$Repo = "Bknibb/StickFightExtendedPlayers"
)

# Ensure we're using a clean version string
if ($Version -notmatch '^v\d+\.\d+\.\d+$') {
    Write-Error "Version must follow the format vX.Y.Z (e.g. v1.2.3)"
    exit 1
}

# Check version in Plugin.cs
$pluginCsPath = "./Plugin.cs"
if (-not (Test-Path $pluginCsPath)) {
    Write-Error "❌ Plugin.cs file not found at path $pluginCsPath"
    exit 1
}

$csContent = Get-Content $pluginCsPath -Raw
if ($csContent -match 'public\s+const\s+string\s+PLUGIN_VERSION\s*=\s*"(?<csVersion>\d+\.\d+\.\d+)"') {
    $csVersion = $matches['csVersion']
    $cleanInputVersion = $Version.TrimStart("v")

    if ($csVersion -ne $cleanInputVersion) {
        Write-Error "❌ Version mismatch: script version is '$cleanInputVersion' but Plugin.cs has '$csVersion'"
        exit 1
    }

    Write-Host "✅ Version match confirmed: $csVersion"
} else {
    Write-Error "❌ Could not find a version declaration in Plugin.cs"
    exit 1
}

Write-Host "🔧 Building project..."
dotnet build $ProjectPath -property:SolutionDir=$PWD\
if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Build failed."
    exit 1
}

# Determine output directory
$outputDir = Join-Path -Path (Split-Path $ProjectPath -Parent) -ChildPath "bin/Debug/net35"
$dllPath = Get-ChildItem -Recurse -Path $outputDir -Filter $DllName | Select-Object -First 1

if (-not $dllPath) {
    Write-Error "❌ DLL '$DllName' not found in build output."
    exit 1
}

# Commit and tag
Write-Host "🏷️ Creating git tag '$Version'..."
git tag $Version
git push origin $Version

$Description = $Description -replace "\\n", "`n"



# Create GitHub release with asset
Write-Host "🚀 Creating GitHub release..."
gh release create $Version "$dllPath" `
    --title "Release $Version" `
    --notes "$Description" `
	--repo "$Repo"

Write-Host "✅ Release $Version created successfully with attached DLL."
