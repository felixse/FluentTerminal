reg add "HKCU\Software\Classes\Directory\shell\Open Fluent Terminal here\command" /d "\"%LOCALAPPDATA%\Microsoft\WindowsApps\flute.exe\" new \"%%1\"" /f
reg add "HKCU\Software\Classes\Directory\Background\shell\Open Fluent Terminal here\command" /d "\"%LOCALAPPDATA%\Microsoft\WindowsApps\flute.exe\" new \"%%V\"" /f
reg add "HKCU\Software\Classes\LibraryFolder\Background\shell\Open Fluent Terminal here\command" /d "\"%LOCALAPPDATA%\Microsoft\WindowsApps\flute.exe\" new \"%%V\"" /f
