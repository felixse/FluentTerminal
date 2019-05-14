param(
    [string]$packageName = $null,
    [switch]$InstallCert,
    [switch]$EnableSideLoad
)

# Load assemblies
[System.Reflection.Assembly]::LoadWithPartialName("System.Windows.Forms")

function Is-Developer-Mode-Enabled {
    $enabled = (test-path HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock) -and ((get-item HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock).Property.Count -gt 1)

    if($enabled){
        $enabled = ((get-itemproperty -Path HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock -Name AllowAllTrustedApps).AllowAllTrustedApps -eq 1) -and ((get-itemproperty -Path HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock -Name AllowDevelopmentWithoutDevLicense).AllowDevelopmentWithoutDevLicense -eq 1)
    }
    return $enabled
}

if($InstallCert -or $EnableSideLoad){
    if($InstallCert){
        certutil.exe -addstore TrustedPeople "$PSScriptRoot\$packageName.cer"
    }

    if($EnableSideLoad){
        set-itemproperty -Path Registry::HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock -Name AllowAllTrustedApps -Value 1 -Verbose
        set-itemproperty -Path Registry::HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock -Name AllowDevelopmentWithoutDevLicense -Value 1 -Verbose
    }
    exit 0
}
else{
    $PackageSignature = Get-AuthenticodeSignature "$PSScriptRoot\$packageName.appxbundle"
    $PackageCertificate = $PackageSignature.SignerCertificate

    if (!$PackageCertificate)
    {
    	throw "Usigned package"
    	exit -1
    }

    while (-Not (Is-Developer-Mode-Enabled))
    {
       $userChoice = [System.Windows.Forms.MessageBox]::Show("Please enable Developer mode on your device to proceed further. Do you want to update the settings manually?", "Fluent Terminal Installer", [System.Windows.Forms.MessageBoxButtons]::YesNo)
       if ($userChoice -eq [System.Windows.Forms.DialogResult]::Yes) {
           [System.Diagnostics.Process]::Start("ms-settings:developers")
           [System.Windows.Forms.Messagebox]::Show("Please press OK once you enabled Developer mode")
           continue
       }
       if ($userChoice -eq [System.Windows.Forms.DialogResult]::No) {
           [System.Windows.Forms.Messagebox]::Show("Installer will update HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock automatically to turn on Developer mode")
           break
       }
    }

    $enableSideLoad = -Not (Is-Developer-Mode-Enabled)

    $trustCert = $PackageSignature.Status -ne "Valid"

    if ($enableSideLoad -or $trustCert)
    {
        $RelaunchArgs = '-ExecutionPolicy Bypass -file "' + "$PSScriptRoot\Install-UntrustedAppx.ps1" + '"' + " $packageName"
    
        if($trustCert){
            $RelaunchArgs += " -InstallCert"
        }

        if($enableSideLoad){
            $RelaunchArgs += " -EnableSideLoad"
        }

        $AdminProcess = Start-Process "powershell.exe" -Verb RunAs -WorkingDirectory $PSScriptRoot -ArgumentList $RelaunchArgs -Wait
    }

    $DependencyPackages = Get-ChildItem (Join-Path (Join-Path $PSScriptRoot "Dependencies") "*.appx")
    
    if ($DependencyPackages.Count -gt 0)
    {
    	Add-AppxPackage -Path "$PSScriptRoot\$packageName.appxbundle" -DependencyPath $DependencyPackages.FullName -ForceApplicationShutdown -Verbose
    }
    else
    {
    	Add-AppxPackage -Path "$PSScriptRoot\$packageName.appxbundle" -ForceApplicationShutdown -Verbose
    }
}
