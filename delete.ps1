Remove-Item .\SchattenclownBot\bin\Debug\net7.0 -Exclude appsettings.json -Force -Confirm:$false
Remove-Item .\SchattenclownBot\bin\Release\net7.0 -Exclude appsettings.json -Force -Confirm:$false
Remove-Item .\SchattenclownBot\obj\ -Recurse