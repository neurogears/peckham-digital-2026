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

$gpuAvailable = $false
$torchUrl = $null
$tensorflowUrl = $null

function Extract-Folder-in-Archive {
    param (
        [string]$archivePath,
        [string]$folderEntry,
        [string]$destinationPath
    )
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [System.IO.Compression.ZipFile]::OpenRead($archivePath)
    foreach ($entry in $zip.Entries) {
        if ($entry.FullName.StartsWith($folderEntry)) {
            $destination = Join-Path $destinationPath ($entry.FullName.Substring($folderEntry.Length))
            if ($entry.Name) {
                [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $destination, $true)
            }
            else {
                New-Item -ItemType Directory -Path $destination -Force | Out-Null
            }
        }
    }
    $zip.Dispose()
}

if ($IsWindows) {
    if (Get-WmiObject -ClassName Win32_VideoController | Where-Object { $_.Name.contains('NVIDIA') }) {
        $gpuAvailable = $true
    }
}
else {
    if (Get-Command "nvidia-smi" -ErrorAction SilentlyContinue) {
        $gpuAvailable = $true
    }
}
if ($gpuAvailable) {
    $torchUrl = "https://download.pytorch.org/libtorch/cu121/libtorch-win-shared-with-deps-2.5.1%2Bcu121.zip"
    $tensorflowUrl = "https://storage.googleapis.com/tensorflow/libtensorflow/libtensorflow-gpu-windows-x86_64-2.8.0.zip"
}
else {
    $torchUrl = "https://download.pytorch.org/libtorch/cpu/libtorch-win-shared-with-deps-2.5.1%2Bcpu.zip"
    $tensorflowUrl = "https://storage.googleapis.com/tensorflow/libtensorflow/libtensorflow-cpu-windows-x86_64-2.8.0.zip"
}
if (!(Test-Path "../lib/torch.dll")) {
    Invoke-WebRequest $torchUrl -OutFile "temp_torch.zip"
    Extract-Folder-in-Archive -archivePath "temp_torch.zip" -folderEntry "libtorch/lib/" -destinationPath "../lib"
    Remove-Item -Path "temp_torch.zip"
}
if (!(Test-Path "../lib/tensorflow.dll")) {
    Invoke-WebRequest $tensorflowUrl -OutFile "temp_tensorflow.zip"
    Extract-Folder-in-Archive -archivePath "temp_tensorflow.zip" -folderEntry "lib/" -destinationPath "../lib"
    Remove-Item -Path "temp_tensorflow.zip"
}


# & .\Bonsai.exe --no-editor
Pop-Location
