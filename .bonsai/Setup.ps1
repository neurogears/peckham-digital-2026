Push-Location $PSScriptRoot
# Windows PowerShell redraws a progress bar for every chunk, which makes big downloads slow.
$ProgressPreference = 'SilentlyContinue'
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

# The MoveNet package behind the pose demos needs the native TensorFlow 2.8 runtime, which
# is not on NuGet. Bonsai.config lists the shared lib folder at the top of the workshop
# folder as a library folder, so put tensorflow.dll there. A machine with an NVIDIA card
# gets the GPU build; it falls back to the CPU by itself when CUDA is not installed.

function Extract-Folder-in-Archive {
    param (
        [string]$archivePath,
        [string]$folderEntry,
        [string]$destinationPath
    )
    # .NET resolves relative paths against the process directory, not the PowerShell location
    $archivePath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($archivePath)
    $destinationPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($destinationPath)
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

$gpuAvailable = $false
if (Get-CimInstance -ClassName Win32_VideoController -ErrorAction SilentlyContinue | Where-Object { $_.Name -like '*NVIDIA*' }) {
    $gpuAvailable = $true
}
elseif (Get-Command "nvidia-smi" -ErrorAction SilentlyContinue) {
    $gpuAvailable = $true
}

if (!(Test-Path "../lib/tensorflow.dll")) {
    if ($gpuAvailable) {
        $tensorflowUrl = "https://storage.googleapis.com/tensorflow/libtensorflow/libtensorflow-gpu-windows-x86_64-2.8.0.zip"
        Write-Host "NVIDIA card found. Downloading the TensorFlow 2.8.0 GPU runtime (about 243 MB)."
    }
    else {
        $tensorflowUrl = "https://storage.googleapis.com/tensorflow/libtensorflow/libtensorflow-cpu-windows-x86_64-2.8.0.zip"
        Write-Host "Downloading the TensorFlow 2.8.0 CPU runtime (about 63 MB)."
    }
    New-Item -ItemType Directory -Path "../lib" -Force | Out-Null
    Invoke-WebRequest $tensorflowUrl -OutFile "temp_tensorflow.zip"
    Extract-Folder-in-Archive -archivePath "temp_tensorflow.zip" -folderEntry "lib/" -destinationPath "../lib"
    Remove-Item -Path "temp_tensorflow.zip"
}
Pop-Location
