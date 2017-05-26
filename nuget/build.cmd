@echo Off
set config=%1
if "%config%" == "" (
   set config=Release
)
set version=
if not "%PackageVersion%" == "" (
   set version=-Version %PackageVersion%
) else (
   set version=-Version 1.4.0
)
REM Determine msbuild path
set msbuild="I:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\msbuild"

REM Package restore
rem echo.
rem echo Running package restore...
rem call :ExecuteCmd nuget.exe restore ..\MassActivation.sln -NonInteractive -ConfigFile nuget.config
rem IF %ERRORLEVEL% NEQ 0 goto error
echo %msbuild%
echo Building solution...
call :ExecuteCmd %msbuild% "..\build\net35\MassActivation.net35.csproj" /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false
IF %ERRORLEVEL% NEQ 0 goto error
call :ExecuteCmd %msbuild% "..\build\net40\MassActivation.net40.csproj" /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false
IF %ERRORLEVEL% NEQ 0 goto error
call :ExecuteCmd %msbuild% "..\build\net451\MassActivation.net451.csproj" /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false
IF %ERRORLEVEL% NEQ 0 goto error
call :ExecuteCmd %msbuild% "..\netcore\MassActivation\MassActivation.netcore.csproj" /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false
IF %ERRORLEVEL% NEQ 0 goto error

echo Packaging...
set libtmp=%cd%\lib
set packagestmp="%cd%\packages"
if not exist %libtmp% mkdir %libtmp%
if not exist %packagestmp% mkdir %packagestmp%

if not exist %libtmp%\net35 mkdir %libtmp%\net35
copy ..\build\net35\bin\%config%\MassActivation.dll %libtmp%\net35 /Y
copy ..\build\net35\bin\%config%\MassActivation.xml %libtmp%\net35 /Y

if not exist %libtmp%\net40 mkdir %libtmp%\net40
copy ..\build\net40\bin\%config%\MassActivation.dll %libtmp%\net40 /Y
copy ..\build\net40\bin\%config%\MassActivation.xml %libtmp%\net40 /Y

if not exist %libtmp%\net451 mkdir %libtmp%\net451
copy ..\build\net451\bin\%config%\MassActivation.dll %libtmp%\net451 /Y
copy ..\build\net451\bin\%config%\MassActivation.xml %libtmp%\net451 /Y

if not exist %libtmp%\netstandard1.6 mkdir %libtmp%\netstandard1.6
copy ..\netcore\MassActivation\bin\%config%\netstandard1.6\MassActivation.dll %libtmp%\netstandard1.6 /Y
copy ..\netcore\MassActivation\bin\%config%\netstandard1.6\MassActivation.xml %libtmp%\netstandard1.6 /Y
copy ..\netcore\MassActivation\bin\%config%\netstandard1.6\MassActivation.deps.json %libtmp%\netstandard1.6 /Y


call :ExecuteCmd nuget.exe pack "%cd%\MassActivation.nuspec" -OutputDirectory %packagestmp% %version%
IF %ERRORLEVEL% NEQ 0 goto error

rmdir %libtmp% /S /Q

goto end

:: Execute command routine that will echo out when error
:ExecuteCmd
setlocal
set _CMD_=%*
call %_CMD_%
if "%ERRORLEVEL%" NEQ "0" echo Failed exitCode=%ERRORLEVEL%, command=%_CMD_%
exit /b %ERRORLEVEL%

:error
endlocal
echo An error has occurred during build.
call :exitSetErrorLevel
call :exitFromFunction 2>nul

:exitSetErrorLevel
exit /b 1

:exitFromFunction
()

:end
endlocal
echo Build finished successfully.