@echo off
setlocal enabledelayedexpansion

REM ============================================================================
REM  ProtheusImporter - BUILD
REM ============================================================================


REM ----------------------------------------------------------------------------
REM  AMBIENTE - EDITE AQUI MANUALMENTE:
REM    TRABALHO = usa dotnet.CMD (REVO01WKS104, sem admin)
REM    CASA     = usa dotnet     (MAIN)
REM ----------------------------------------------------------------------------
set AMBIENTE=TRABALHO


REM ----------------------------------------------------------------------------
REM  Resolve o comando do .NET a partir da variavel AMBIENTE.
REM ----------------------------------------------------------------------------
if /i "%AMBIENTE%"=="TRABALHO" (
    set DOTNET=dotnet.CMD
) else (
    set DOTNET=dotnet
)


REM ============================================================================
REM  LOOP DO MENU
REM ============================================================================
:MENU
cls
echo.
echo  ==========================================================================
echo  ==                                                                      ==
echo  ==                   ProtheusImporter  ^|  Build                         ==
echo  ==                                                                      ==
echo  ==========================================================================
echo.
echo     Ambiente : %AMBIENTE%
echo     Comando  : %DOTNET%
echo.
echo  --------------------------------------------------------------------------
echo.
echo     [1]  Publish com .NET embutido   (self-contained, ~70MB)
echo     [2]  Publish sem .NET embutido   (framework-dependent, ~1MB)
echo     [3]  Build Debug                 (desenvolvimento)
echo     [4]  Sair
echo.
echo  --------------------------------------------------------------------------
echo.
set /p OPCAO="   Escolha [1-4]: "

REM Redireciona pra cada bloco de acordo com a opcao digitada.
if "%OPCAO%"=="1" goto OPCAO1
if "%OPCAO%"=="2" goto OPCAO2
if "%OPCAO%"=="3" goto OPCAO3
if "%OPCAO%"=="4" goto FIM

echo.
echo     ^>^> Opcao invalida. Tente novamente.
timeout /t 2 >nul
goto MENU


REM ============================================================================
REM  OPCAO 1: Self-contained single file (.exe ~70MB, nao precisa .NET instalado)
REM  Flags:
REM    -c Release                -> build otimizado (nao Debug)
REM    -r win-x64                -> runtime alvo (Windows 64 bits)
REM    --self-contained true     -> empacota o .NET junto do exe
REM    -p:PublishSingleFile=true -> junta tudo em 1 arquivo .exe
REM  Saida: bin\Release\net10.0\win-x64\publish\ProtheusImporter.exe
REM ============================================================================
:OPCAO1
cls
echo.
echo  ==========================================================================
echo    Publish self-contained  (win-x64, single file)
echo  ==========================================================================
echo.
%DOTNET% publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
goto RESULTADO


REM ============================================================================
REM  OPCAO 2: Framework-dependent (.exe ~1MB, exige .NET 10 instalado)
REM  Flags:
REM    -c Release -> build otimizado
REM  Saida: bin\Release\net10.0\publish\
REM ============================================================================
:OPCAO2
cls
echo.
echo  ==========================================================================
echo    Publish framework-dependent
echo  ==========================================================================
echo.
%DOTNET% publish -c Release
goto RESULTADO


REM ============================================================================
REM  OPCAO 3: Build Debug (desenvolvimento local, sem publicar)
REM  Saida: bin\Debug\net10.0\
REM ============================================================================
:OPCAO3
cls
echo.
echo  ==========================================================================
echo    Build Debug
echo  ==========================================================================
echo.
%DOTNET% build
goto RESULTADO


REM ============================================================================
REM  RESULTADO - exibido apos qualquer build.
REM ============================================================================
:RESULTADO
echo.
if errorlevel 1 (
    echo  ==========================================================================
    echo    BUILD FALHOU
    echo  ==========================================================================
) else (
    echo  ==========================================================================
    echo    BUILD CONCLUIDO
    echo  ==========================================================================
)
echo.
pause
goto MENU


REM ============================================================================
REM  FIM
REM ============================================================================
:FIM
endlocal
exit /b 0