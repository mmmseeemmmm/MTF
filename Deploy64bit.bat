@echo off

if (%1)==() (
  set projectPath=%cd%\
)else (
  set projectPath=%1
)

echo Project path is %projectPath%



echo --- Copy Client ---
xcopy /Y /s "%projectPath%bin64\Release\Client\*.*" "%projectPath%bin64\Deploy\Client\"
xcopy /Y /s "%projectPath%bin64\Release\Client\*.pdb" "%projectPath%bin64\Deploy\DebugInfo\Client\"
del "%projectPath%bin64\Deploy\Client\*.vshost.exe.*"
del "%projectPath%bin64\Deploy\Client\*.pdb"
del "%projectPath%bin64\Deploy\Client\*.log"
del "%projectPath%bin64\Deploy\Client\settings.xml"

echo --- Copy Server ---
xcopy /Y /s "%projectPath%bin64\Release\Server\*.*" "%projectPath%bin64\Deploy\Server\"
xcopy /Y /s "%projectPath%bin64\Release\Server\*.pdb" "%projectPath%bin64\Deploy\DebugInfo\Server\"
del "%projectPath%bin64\Deploy\Server\*.vshost.exe.*"
del /s "%projectPath%bin64\Deploy\Server\*.pdb"
del /s "%projectPath%bin64\Deploy\Server\*.log"
del /s "%projectPath%bin64\Deploy\Server\settings.xml"
rmdir /S /Q "%projectPath%bin64\Deploy\Server\data\sequences\"
mkdir "%projectPath%bin64\Deploy\Server\data\sequences"
rmdir /S /Q "%projectPath%bin64\Deploy\Server\data\functionalityLibrary\"
mkdir "%projectPath%bin64\Deploy\Server\data\functionalityLibrary"

echo --- Copy UsbAccessRequest ---
xcopy /Y /s "%projectPath%bin64\Release\UsbAccessRequest\*.*" "%projectPath%bin64\Deploy\UsbAccessRequest\"

echo --- Copy source codes ---
xcopy /Y /s "%projectPath%FunctionalityLibrary\*.*" "%projectPath%bin64\Deploy\src\FunctionalityLibrary\"
xcopy /Y /s "%projectPath%MTFAccessControl\*.*" "%projectPath%bin64\Deploy\src\MTFAccessControl\"
xcopy /Y /s "%projectPath%MTFClient\*.*" "%projectPath%bin64\Deploy\src\MTFClient\"
xcopy /Y /s "%projectPath%MTFClientServerCommon\*.*" "%projectPath%bin64\Deploy\src\MTFClientServerCommon\"
xcopy /Y /s "%projectPath%MTFCommon\*.*" "%projectPath%bin64\Deploy\src\MTFCommon\"
xcopy /Y /s "%projectPath%MTFComponentHost\*.*" "%projectPath%bin64\Deploy\src\MTFComponentHost\"
xcopy /Y /s "%projectPath%MTFServer\*.*" "%projectPath%bin64\Deploy\src\MTFServer\"
xcopy /Y /s "%projectPath%packages\*.*" "%projectPath%bin64\Deploy\src\packages\"
xcopy /Y /s "%projectPath%RFIDTestingApp\*.*" "%projectPath%bin64\Deploy\src\RFIDTestingApp\"
xcopy /Y /s "%projectPath%UnitTests\*.*" "%projectPath%bin64\Deploy\src\UnitTests\"
xcopy /Y "%projectPath%MTF.sln" "%projectPath%bin64\Deploy\src\"

