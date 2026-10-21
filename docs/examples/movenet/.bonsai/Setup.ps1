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

# The MoveNet package needs the native TensorFlow 2.8 runtime, which is not on NuGet.
# Bonsai.config lists the shared lib folder at the top of the workshop folder as a
# library folder, so put tensorflow.dll there.
if (!(Test-Path "../../../../lib/tensorflow.dll")) {
    $tensorflow = "https://storage.googleapis.com/tensorflow/libtensorflow/libtensorflow-cpu-windows-x86_64-2.8.0.zip"
    Write-Host "Downloading the TensorFlow 2.8.0 CPU runtime (about 63 MB)."
    Invoke-WebRequest $tensorflow -OutFile "tensorflow.zip"
    Expand-Archive "tensorflow.zip" -DestinationPath "tensorflow-temp" -Force
    New-Item -ItemType Directory -Path "../../../../lib" -Force | Out-Null
    Copy-Item "tensorflow-temp/lib/tensorflow.dll" "../../../../lib/tensorflow.dll" -Force
    Remove-Item -Path "tensorflow-temp" -Recurse -Force
    Remove-Item -Path "tensorflow.zip"
}
Pop-Location
