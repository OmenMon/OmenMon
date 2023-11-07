@echo off
rem
rem  //\\   OmenMon: Hardware Monitoring & Control Utility
rem //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
rem     //  https://omenmon.github.io/
rem
set DOTNET_CLI_TELEMETRY_OPTOUT=1
setlocal
rem For Visual Studio 2022 Build Tools only (no IDE):
set msbuild="%ProgramFiles%\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\amd64\MSBuild.exe"
rem For Visual Studio 2022 Community Edition (IDE):
set msbuild="%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\amd64\MSBuild.exe"
set msbuild_flags=/p:AssemblyVersion=0.0.0.0 /p:AssemblyVersionWord=Manual /p:Configuration=Release
set nuget=%~dps0nuget.exe
set nuget_url=https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
set op_scope=build clean kill prepare test usage
set op=%~1
set taskkill=%SystemRoot%\System32\taskkill.exe
set result_bin=OmenMon.exe
set test_param=-Ec -Ec HPCM RPM0^^(2^^) RPM2^^(2^^) -Bios -Bios Backlight=Off Color=0080FF:00FF00:00FF00:FFFFFF Backlight=On -Prog -Task -Usage
pushd %~dps0
for %%p in (%op_scope%) do if "%op%"=="%%p" echo BEGIN %~n0 (%op%) & goto %op%
echo BEGIN %~n0 & goto usage

:build
call :TaskKill %result_bin%
%msbuild% /t:Clean,Build %msbuild_flags%
goto end

:clean
call :TaskKill %result_bin%
%msbuild% /t:Clean
rem Handled within .csproj now:
rem call :RecursivelyRemoveDir Bin
rem call :RecursivelyRemoveDir Obj
goto end

:kill
call :TaskKill %result_bin%
goto end

:prepare
if not exist %nuget% powershell Invoke-WebRequest -OutFile %nuget% -Uri %nuget_url%
%nuget% restore
goto end

:test
if exist Bin\%result_bin% ( Bin\%result_bin% %test_param% ) ^
else if exist %result_bin% %result_bin% %test_param%
goto end

:usage
echo Usage: %~n0 ^<build^|clean^|kill^|prepare^|test^|usage^>
goto end

:RecursivelyRemoveDir
if not "%1"=="" for /d /r %%d in (*) do if exist %%~dpsd%1 rd /q /s %%~dpsd%1
exit /b

:TaskKill
%taskkill% /f /im "%1"
exit /b

:end
echo END %~n0
popd
