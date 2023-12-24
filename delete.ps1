Remove-Item .\SchattenclownBot\bin\Debug\net8.0 -Exclude appsettings.json -Force -Confirm:$false
Remove-Item .\SchattenclownBot\bin\Release\net8.0 -Exclude appsettings.json -Force -Confirm:$false
Remove-Item .\SchattenclownBot\obj\ -Recurse