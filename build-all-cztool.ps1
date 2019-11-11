$genver = ( Select-Xml -Path "$PSScriptRoot\CzTool\CzTool.csproj" -XPath //Project/PropertyGroup/AssemblyVersion ).Node.InnerText
$targets_os = "osx-x64"
$targets_conf = "Debug", "Release"
$targets_sc = [ordered]@{
    Standalone = "true";
    Normal = "false";
}
New-Item "$PSScriptRoot\bin" -ItemType "directory" -Force
New-Item "$PSScriptRoot\build" -ItemType "directory" -Force
foreach($os in $targets_os){
    foreach($conf in $targets_conf){
        foreach($sc in $targets_sc.Keys){
            Remove-Item "$PSScriptRoot\build\*" -Recurse -Force
            $key = $os + "_" + $conf + "_" + $sc
            Write-Output "--- $key | Building target... ---"
            dotnet publish -f netcoreapp2.2 "$PSScriptRoot\CzTool" -c $conf -r $os --self-contained $targets_sc[$sc] -o "$PSScriptRoot\build" -v m
            Write-Output "--- $key | Compressing... ---"
            Compress-Archive -Path "$PSScriptRoot\build\*" -DestinationPath ("$PSScriptRoot\bin\CzTool_" + $genver + "_$key.zip") -Force
            Write-Output "--- $key | Done with target ---"
        }
    }
}
Remove-Item "$PSScriptRoot\build\*" -Recurse -Force
Write-Output("--- Done with targets ---")
