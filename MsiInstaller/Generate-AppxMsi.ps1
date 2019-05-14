$WixRoot = "$PSScriptRoot\wix"
$InstallFileswsx = "..\Template.wxs"
$InstallFilesWixobj = "..\FluentTerminalInstaller.wixobj"

if(!(Test-Path "$WixRoot\candle.exe"))
{
    
    Write-Host Downloading Wixtools..
    New-Item $WixRoot -type directory -force | Out-Null
    # Download Wix version 3.11.1 - https://github.com/wixtoolset/wix3/releases/tag/wix3111rtm
    Invoke-WebRequest -Uri https://github.com/wixtoolset/wix3/releases/download/wix3111rtm/wix311-binaries.zip -Method Get -OutFile $WixRoot\WixTools.zip

    Write-Host Extracting Wixtools..
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::ExtractToDirectory("$WixRoot\WixTools.zip", $WixRoot)
}

pushd "$WixRoot"
.\candle.exe $InstallFileswsx -ext WixUtilExtension -o "$PSScriptRoot\FluentTerminalInstaller.wixobj" 
.\light.exe $InstallFilesWixobj -ext WixUtilExtension -b "$PSScriptRoot" -o "$PSScriptRoot\FluentTerminalInstaller.msi" 
popd