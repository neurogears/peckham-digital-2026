Push-Location $PSScriptRoot
$release = "https://github.com/bonsai-rx/bonsai/releases/latest/download/Bonsai.zip"
$version = $null
$configPath = "./Bonsai.config"
if (Test-Path $configPath) {
    [xml]$config = Get-Content $configPath
    $bootstrapper = $config.PackageConfiguration.Packages.Package.where{$_.id -eq 'Bonsai'}
    if ($bootstrapper) {
        $version = $bootstrapper.version
        $release = "https://github.com/bonsai-rx/bonsai/releases/download/$version/Bonsai.zip"
    }
}
# Replace a Bonsai.exe left over from an earlier pin instead of reusing it.
if ((Test-Path "./Bonsai.exe") -and $version) {
    $installed = (Get-Item "./Bonsai.exe").VersionInfo.ProductVersion
    if (-not $installed.StartsWith($version)) {
        Write-Host "Bonsai.exe is $installed but Bonsai.config pins $version. Replacing it."
        Remove-Item -Path "./Bonsai.exe" -Force
        Remove-Item -Path "./Packages" -Recurse -Force -ErrorAction SilentlyContinue
    }
}
if (!(Test-Path "./Bonsai.exe")) {
    Invoke-WebRequest $release -OutFile "temp.zip"
    Move-Item -Path "NuGet.config" "temp.config" -ErrorAction SilentlyContinue
    Expand-Archive "temp.zip" -DestinationPath "." -Force
    Move-Item -Path "temp.config" "NuGet.config" -Force -ErrorAction SilentlyContinue
    Remove-Item -Path "temp.zip"
    Remove-Item -Path "Bonsai32.exe" -ErrorAction SilentlyContinue
}
& .\Bonsai.exe --no-editor
Pop-Location
