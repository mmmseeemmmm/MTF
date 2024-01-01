@echo off

if (%1)==() (
  set projectPath=%cd%\
)else (
  set projectPath=%1
)

echo Project path is %projectPath%



echo --- Copy Client ---
xcopy /Y /s "%projectPath%bin\Release\Client\*.*" "%projectPath%bin\Deploy\Client\"
xcopy /Y /s "%projectPath%bin\Release\Client\*.pdb" "%projectPath%bin\Deploy\DebugInfo\Client\"
del "%projectPath%bin\Deploy\Client\*.vshost.exe.*"
del "%projectPath%bin\Deploy\Client\*.pdb"
del "%projectPath%bin\Deploy\Client\*.log"
del "%projectPath%bin\Deploy\Client\settings.xml"

echo --- Copy Server ---
xcopy /Y /s "%projectPath%bin\Release\Server\*.*" "%projectPath%bin\Deploy\Server\"
xcopy /Y /s "%projectPath%bin\Release\Server\*.pdb" "%projectPath%bin\Deploy\DebugInfo\Server\"
del "%projectPath%bin\Deploy\Server\*.vshost.exe.*"
del /s "%projectPath%bin\Deploy\Server\*.pdb"
del /s "%projectPath%bin\Deploy\Server\*.log"
del /s "%projectPath%bin\Deploy\Server\settings.xml"
rmdir /S /Q "%projectPath%bin\Deploy\Server\data\sequences\"
mkdir "%projectPath%bin\Deploy\Server\data\sequences"
rmdir /S /Q "%projectPath%bin\Deploy\Server\data\functionalityLibrary\"
mkdir "%projectPath%bin\Deploy\Server\data\functionalityLibrary"

echo --- Copy UsbAccessRequest ---
xcopy /Y /s "%projectPath%bin\Release\UsbAccessRequest\*.*" "%projectPath%bin\Deploy\UsbAccessRequest\"

echo --- Copy source codes ---
xcopy /Y /s "%projectPath%FunctionalityLibrary\*.*" "%projectPath%bin\Deploy\src\FunctionalityLibrary\"
xcopy /Y /s "%projectPath%MTFAccessControl\*.*" "%projectPath%bin\Deploy\src\MTFAccessControl\"
xcopy /Y /s "%projectPath%MTFClient\*.*" "%projectPath%bin\Deploy\src\MTFClient\"
xcopy /Y /s "%projectPath%MTFClientServerCommon\*.*" "%projectPath%bin\Deploy\src\MTFClientServerCommon\"
xcopy /Y /s "%projectPath%MTFCommon\*.*" "%projectPath%bin\Deploy\src\MTFCommon\"
xcopy /Y /s "%projectPath%MTFComponentHost\*.*" "%projectPath%bin\Deploy\src\MTFComponentHost\"
xcopy /Y /s "%projectPath%MTFServer\*.*" "%projectPath%bin\Deploy\src\MTFServer\"
xcopy /Y /s "%projectPath%packages\*.*" "%projectPath%bin\Deploy\src\packages\"
xcopy /Y /s "%projectPath%RFIDTestingApp\*.*" "%projectPath%bin\Deploy\src\RFIDTestingApp\"
xcopy /Y /s "%projectPath%UnitTests\*.*" "%projectPath%bin\Deploy\src\UnitTests\"
xcopy /Y "%projectPath%MTF.sln" "%projectPath%bin\Deploy\src\"

