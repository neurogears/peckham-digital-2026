[CmdletBinding()] param (
    [string[]]$LibrarySources,
    [bool]$UseGalleryForWorkflowsDirectory=$false,
    [bool]$UseGalleryForExamplesDirectory=$true,
    [string]$OutputFolder=$null
)
Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

if ($OutputFolder) {
    $OutputFolder = Join-Path (Get-Location) $OutputFolder
}

function Process-Workflow-Collection([bool]$useGallery, [string]$workflowPath, [string]$environmentPath) {
    $libPath = $LibrarySources

    if ($useGallery) {
        $libPath = @()
        $galleryPath = Join-Path $environmentPath 'Gallery'
        $null = New-Item -ItemType Directory -Path $galleryPath -Force
        foreach ($librarySource in $LibrarySources) {
            Get-ChildItem -Path $librarySource -Filter *.nupkg | Copy-Item -Destination $galleryPath
        }
    }

    $bootstrapperPath = (Join-Path $environmentPath 'Bonsai.exe')
    .\bonsai-docfx\modules\Export-Image.ps1 -libPath $libPath -workflowPath $workflowPath -bootstrapperPath $bootstrapperPath -outputFolder $OutputFolder -documentationRoot $PSScriptRoot
}

Push-Location $PSScriptRoot
try {
    if (Test-Path -Path 'workflows/') {
        Process-Workflow-Collection $UseGalleryForWorkflowsDirectory './workflows' '../.bonsai/'
    }

    if (Test-Path -Path 'examples/') {
        # Each example folder renders with its own .bonsai environment when it has one,
        # and with the workshop environment at the top of the repository otherwise.
        Write-Host "Processing examples in the 'examples/' directory..."
        foreach ($example in (Get-ChildItem -Path 'examples/' -Directory)) {
            $environment = Join-Path $example.FullName '.bonsai'
            if (Test-Path -Path $environment) {
                Write-Host "Processing environment: $environment"
                Process-Workflow-Collection $UseGalleryForExamplesDirectory $example.FullName $environment
            } else {
                Write-Host "Processing $($example.FullName) with the workshop environment"
                Process-Workflow-Collection $UseGalleryForWorkflowsDirectory $example.FullName '../.bonsai/'
            }
        }
    }
} finally {
    Pop-Location
}
