@echo off

set nugetRoot=C:\Users\lucgen\.nuget\packages
set pfxVer=1.4.0-build.20250429-1002

set pfxRoot=C:\Data\Power-Fx
set pfxConfig=Debug

set pfxSource=%pfxRoot%\src\libraries
set pfxTest=%pfxRoot%\src\tests\.Net7.0

set nugetDest1=%nugetRoot%\microsoft.powerfx.core\%pfxVer%\lib\netstandard2.0
set nugetDest2=%nugetRoot%\microsoft.powerfx.connectors\%pfxVer%\lib\netstandard2.0
set nugetDest3=%nugetRoot%\microsoft.powerfx.json\%pfxVer%\lib\netstandard2.0
set nugetDest4=%nugetRoot%\microsoft.powerfx.core.tests\%pfxVer%\lib\net7.0
set nugetDest5=%nugetRoot%\microsoft.powerfx.interpreter\%pfxVer%\lib\netstandard2.0
set nugetDest6=%nugetRoot%\microsoft.powerfx.languageserverprotocol\%pfxVer%\lib\netstandard2.0
set nugetDest7=%nugetRoot%\microsoft.powerfx.transport.attributes\%pfxVer%\lib\netstandard2.0

set source1=%pfxSource%\Microsoft.PowerFx.Core\bin\%pfxConfig%\netstandard2.0
set source2=%pfxSource%\Microsoft.PowerFx.Connectors\bin\%pfxConfig%\netstandard2.0
set source3=%pfxSource%\Microsoft.PowerFx.Json\bin\%pfxConfig%\netstandard2.0
set source4=%pfxTest%\Microsoft.PowerFx.Core.Tests\bin\%pfxConfig%\net7.0
set source5=%pfxSource%\Microsoft.PowerFx.Interpreter\bin\%pfxConfig%\netstandard2.0
set source6=%pfxSource%\Microsoft.PowerFx.LanguageServerProtocol\bin\%pfxConfig%\netstandard2.0
set source7=%pfxSource%\Microsoft.PowerFx.Transport.Attributes\bin\%pfxConfig%\netstandard2.0

set options=/e /s /np /nfl /ndl 

echo "%source1% --> %nugetDest1%"
robocopy "%source1%" "%nugetDest1%" %options% > NUL
echo "%source2% --> %nugetDest2%"
robocopy "%source2%" "%nugetDest2%" %options% > NUL
echo "%source3% --> %nugetDest3%"
robocopy "%source3%" "%nugetDest3%" %options% > NUL
echo "%source4% --> %nugetDest4%"
robocopy "%source4%" "%nugetDest4%" %options% > NUL
echo "%source5% --> %nugetDest5%"
robocopy "%source5%" "%nugetDest5%" %options% > NUL
echo "%source6% --> %nugetDest6%"
robocopy "%source6%" "%nugetDest6%" %options% > NUL
echo "%source7% --> %nugetDest7%"
robocopy "%source7%" "%nugetDest7%" %options% > NUL

echo.
echo *** DO NOT FORGET TO REBUILD ALL YOUR PROJECTS ***
echo on
