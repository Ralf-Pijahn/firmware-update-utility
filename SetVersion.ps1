#***************************************************************************************************************************
# Set Version Script for FirmwareUpdater project                                                                                    
#                                                                                                                          
# Usage:                                                                                                                   
# SerVersion [buildoption]       
#
#  build option can be one of: Debug | Release | BinRelease
#   Debug       :-  A complete debug build from source is performed
#   Release     :-  A complete release build from source is performed
#
#                                                                                          
#                                                                                                                          
# Environment Variables to set before Executing
#  
# xver_FirmwareUpdater              : The version number to set on the  Software Product specific assemblies
#
#***************************************************************************************************************************

##check we've got the parameter
param([string]$build=$(Throw  "Parameter missing -build [Debug|Release|BinRelease]"))

#Function to return all the assembly info file objects below the start path
function GetAssemblyInfoFiles ( $startPath )
{
    get-childitem $startPath -recurse |? {$_.Name -eq "AssemblyInfo.cs"} ;
}

#
# function to substitute the exising version info string in collection of source files
# with a new version and file version string
# and update the Assembly Configuration
#
function Update-SourceVersion([string]$Version , [string]$Config='')
{
  $NewVersion = 'AssemblyVersion("' + $Version + '")';
  $NewFileVersion = 'AssemblyFileVersion("' + $Version + '")';
  $NewConfiguration = 'AssemblyConfiguration("' + $Config + '")';
 

  foreach ($o in $input) 
  {
    Write-output $o.FullName
    $TmpFile = $o.FullName + ".tmp"

     get-content $o.FullName | 
        %{$_ -replace 'AssemblyConfiguration\(".?"\)', $NewConfiguration } |
        %{$_ -replace 'AssemblyVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)', $NewVersion } |
        %{$_ -replace 'AssemblyFileVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)', $NewFileVersion }  > $TmpFile

     move-item $TmpFile $o.FullName -force
  }
}

#
# Function to check that a supplied version number is valid
#
function CheckVersionValid($Version)
{
    if(! [System.Text.RegularExpressions.Regex]::Match($Version, "^[0-9]+(\.[0-9]+){1,3}$").Success )
    {
        throw new-object FormatException "Invalid version number format"
    }
}

#Start of processing

$BuildType = "Release";
$BinLibs = $false;

#Check the parameter


switch($build.ToLower())
{
 "debug" 
  { 
     $BuildType = "Debug"
  }
 "release"
 {
 
 }
 default
 {
    throw "Invalid build option"
 }

}


#clean up the dependencies folder or create it
foreach($fldr in "dep","bin")
{
    if( Test-Path ".\$fldr\")
    {
        #trash out any existing dependencies
        Remove-Item -recurse -force ".\$fldr\*";
    }
    else
    {
        mkdir(".\$fldr\");
    }
}

if(! (Test-Path env:xver_FirmwareUpdater))
    { $ProductVersion = read-host Product Version Number; }
else
    { $ProductVersion = $env:xver_FirmwareUpdater; }
CheckVersionValid($ProductVersion) 



    #delete all the .svn folders - there's probably a better way of doing the copy to exclude them but this works 
    $toDelete = dir .\dep\ -recurse | where { $_.PsIsContainer -and ($_.Name -eq ".svn") }
    foreach($f in $toDelete)
    {
     remove-item $f.FullName -force -recurse
    }
    
    
    #At this point, these copied files will be the only files in .\dep
    #change their attributes to make them readonly so we can't accidentally build new versions over the top of them
    get-childitem ".\dep\" -recurse | where-object{ !($_.PsIsContainer) } | foreach-object{ $_.IsReadOnly = $true};


#Change the version numbers on all the Product specific source.
GetAssemblyInfoFiles ".\FirmwareUpdater\Properties" | Update-SourceVersion $ProductVersion $BuildType



