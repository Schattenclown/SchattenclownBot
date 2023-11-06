Remove-Item .\bin\Debug\net7.0 -Exclude appsettings.json -Force -Confirm:$false
Remove-Item .\bin\Release\net7.0 -Exclude appsettings.json -Force -Confirm:$false
Remove-Item .\obj\ -Recurse