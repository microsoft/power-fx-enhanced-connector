@echo off

set nugetRoot=C:\Users\lucgen\.nuget\packages
set dvVer=1.3.0
set dvRoot=C:\Data\Power-Fx-Dataverse
set dvConfig=Debug

set dvSource=%dvRoot%\src

set nugetDest1=%nugetRoot%\Microsoft.PowerFx.Dataverse\%dvVer%\lib\netstandard2.0
set nugetDest2=%nugetRoot%\Microsoft.PowerFx.Dataverse.Eval\%dvVer%\lib\netstandard2.0
set nugetDest3=%nugetRoot%\Microsoft.PowerFx.Dataverse.Sql\%dvVer%\lib\netstandard2.0

set source1=%dvSource%\PowerFx.Dataverse\bin\%dvConfig%\netstandard2.0
set source2=%dvSource%\PowerFx.Dataverse.Eval\bin\%dvConfig%\netstandard2.0
set source3=%dvSource%\Microsoft.PowerFx.Dataverse.Sql\bin\%dvConfig%\netstandard2.0

set options=/e /s /np /nfl /ndl 

echo "%source1% --> %nugetDest1%"
robocopy "%source1%" "%nugetDest1%" %options% > NUL
echo "%source2% --> %nugetDest2%"
robocopy "%source2%" "%nugetDest2%" %options% > NUL
echo "%source3% --> %nugetDest3%"
robocopy "%source3%" "%nugetDest3%" %options% > NUL

echo.
echo *** DO NOT FORGET TO REBUILD ALL YOUR PROJECTS ***
echo on
