copy /y FluentTerminal.ico "%LOCALAPPDATA%\Microsoft\WindowsApps"
reg add "HKCU\Software\Classes\Directory\shell\Open Fluent Terminal here\command" /d "\"%LOCALAPPDATA%\Microsoft\WindowsApps\flute.exe\" new \"%%V\"" /f
reg add "HKCU\Software\Classes\Directory\shell\Open Fluent Terminal here" /v icon /t REG_SZ /d "%LOCALAPPDATA%\Microsoft\WindowsApps\FluentTerminal.ico" /f
reg add "HKCU\Software\Classes\Directory\Background\shell\Open Fluent Terminal here\command" /d "\"%LOCALAPPDATA%\Microsoft\WindowsApps\flute.exe\" new \"%%V\"" /f
reg add "HKCU\Software\Classes\Directory\Background\shell\Open Fluent Terminal here" /v icon /t REG_SZ /d "%LOCALAPPDATA%\Microsoft\WindowsApps\FluentTerminal.ico" /f
reg add "HKCU\Software\Classes\Drive\shell\Open Fluent Terminal here\command" /d "\"%LOCALAPPDATA%\Microsoft\WindowsApps\flute.exe\" new \"%%V\"" /f
reg add "HKCU\Software\Classes\Drive\shell\Open Fluent Terminal here" /v icon /t REG_SZ /d "%LOCALAPPDATA%\Microsoft\WindowsApps\FluentTerminal.ico" /f
reg add "HKCU\Software\Classes\LibraryFolder\Background\shell\Open Fluent Terminal here\command" /d "\"%LOCALAPPDATA%\Microsoft\WindowsApps\flute.exe\" new \"%%V\"" /f
reg add "HKCU\Software\Classes\LibraryFolder\Background\shell\Open Fluent Terminal here" /v icon /t REG_SZ /d "%LOCALAPPDATA%\Microsoft\WindowsApps\FluentTerminal.ico" /f
