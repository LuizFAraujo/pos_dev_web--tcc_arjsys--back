@echo off
REM ============================================================================
REM  ARJSYS - Menu de comandos de desenvolvimento
REM ----------------------------------------------------------------------------
REM  Execute este arquivo na raiz do repositorio (onde esta o Api_ArjSys_Tcc.slnx)
REM  Chame com: arj.bat
REM  Ou simplesmente: arj
REM ============================================================================

REM ---- Configuracoes globais do script ---------------------------------------
REM chcp 65001    : troca o code page do console pra UTF-8 (evita acentos bugados)
REM setlocal      : isola variaveis desse .bat (nao polui o ambiente do terminal)
REM > nul         : suprime a mensagem "Active code page: 65001" do chcp
chcp 65001 > nul
setlocal EnableDelayedExpansion

REM ---- Variavel de controle do loop do menu ----------------------------------
REM  true  = depois de executar uma opcao, volta ao menu
REM  false = depois de executar uma opcao, fecha o script
set "VOLTAR_AO_MENU=true"

REM ---- Paths dos projetos (relativos a raiz do repo) -------------------------
REM  Centralizados aqui pra facilitar manutencao se mudar nome/local
set "PROJETO_API=app"
set "PROJETO_SEED=SeedRunner"
set "BANCO=app\Database\ArjSysDB.db"
set "PUBLISH_DIR=..\publish"


REM ============================================================================
REM  LOOP PRINCIPAL DO MENU
REM ----------------------------------------------------------------------------
REM  O rotulo :MENU e o ponto de retorno quando VOLTAR_AO_MENU=true.
REM  Cada opcao usa 'goto' pra pular pro rotulo da acao, que no fim
REM  chama :FIM_ACAO, que decide se volta ao menu ou encerra.
REM ============================================================================

:MENU
REM ---- Limpa a tela e desenha o cabecalho do menu ----------------------------
cls
echo.
echo ============================================================
echo                    ARJSYS - MENU DE DEV
echo ============================================================
echo.
echo   1 - Rodar API ^(dotnet run --project %PROJETO_API%^)
echo   2 - Rodar SeedRunner ^(popular banco com SQLs^)
echo   3 - Build ^(dotnet build da solucao^)
echo   4 - Resetar banco ^(apaga .db + aplica migrations^)
echo   5 - Adicionar migration ^(pede nome^)
echo   6 - Aplicar migrations pendentes
echo   7 - Publish release self-contained
echo.
echo   0 - Sair
echo.
echo ============================================================

REM ---- Le a opcao digitada pelo usuario --------------------------------------
REM  set /p     : pede input do usuario
REM  OPCAO=""   : zera antes de ler, evita lixo da iteracao anterior
set "OPCAO="
set /p "OPCAO=Escolha uma opcao: "

REM ---- Roteamento para o rotulo correto --------------------------------------
REM  goto LABEL  salta pra secao correspondente.
REM  Se o usuario digitar algo invalido, cai em :OPCAO_INVALIDA.
if "%OPCAO%"=="1" goto ACAO_API
if "%OPCAO%"=="2" goto ACAO_SEED
if "%OPCAO%"=="3" goto ACAO_BUILD
if "%OPCAO%"=="4" goto ACAO_RESET_DB
if "%OPCAO%"=="5" goto ACAO_MIGRATION_ADD
if "%OPCAO%"=="6" goto ACAO_MIGRATION_UPDATE
if "%OPCAO%"=="7" goto ACAO_PUBLISH
if "%OPCAO%"=="0" goto SAIR

goto OPCAO_INVALIDA


REM ============================================================================
REM  ACAO 1 - Rodar API
REM ----------------------------------------------------------------------------
REM  dotnet run             : compila (se preciso) e executa o projeto
REM  --project %PROJETO_API%: aponta pra pasta do .csproj da API
REM  Kestrel escuta em http://localhost:7000 (vindo do appsettings.json)
REM  Ctrl+C encerra a API e volta pro menu (se VOLTAR_AO_MENU=true)
REM ============================================================================
:ACAO_API
cls
echo.
echo [ACAO 1] Rodando API em %PROJETO_API%...
echo ------------------------------------------------------------
dotnet run --project %PROJETO_API%
goto FIM_ACAO


REM ============================================================================
REM  ACAO 2 - Rodar SeedRunner
REM ----------------------------------------------------------------------------
REM  dotnet run             : compila/executa o projeto SeedRunner
REM  --project %PROJETO_SEED%: aponta pra pasta do SeedRunner.csproj
REM  O SeedRunner le seed-config.ini e seed-order.txt da sua propria pasta
REM  e executa os .sql listados contra o banco configurado em DbPath.
REM ============================================================================
:ACAO_SEED
cls
echo.
echo [ACAO 2] Rodando SeedRunner...
echo ------------------------------------------------------------
dotnet run --project %PROJETO_SEED%
goto FIM_ACAO


REM ============================================================================
REM  ACAO 3 - Build da solucao
REM ----------------------------------------------------------------------------
REM  dotnet build  : compila todos os projetos da solucao (.slnx).
REM                  Verifica erros de compilacao sem rodar nada.
REM  Sem --project, ele procura .sln/.slnx na pasta atual.
REM ============================================================================
:ACAO_BUILD
cls
echo.
echo [ACAO 3] Fazendo build da solucao...
echo ------------------------------------------------------------
dotnet build
goto FIM_ACAO


REM ============================================================================
REM  ACAO 4 - Resetar banco de dados
REM ----------------------------------------------------------------------------
REM  ATENCAO: acao destrutiva. Apaga o .db completamente e recria do zero
REM  aplicando todas as migrations. Dados sao perdidos.
REM
REM  Pede confirmacao antes de executar.
REM
REM  Passos:
REM    1. Pergunta se o usuario quer mesmo resetar
REM    2. Se o arquivo do banco existe, apaga (del /q = sem prompt)
REM    3. dotnet ef database update : roda todas as migrations pendentes,
REM       o que nesse caso cria o banco inteiro do zero
REM    --project %PROJETO_API% : necessario porque o DbContext esta na API
REM ============================================================================
:ACAO_RESET_DB
cls
echo.
echo [ACAO 4] RESETAR BANCO DE DADOS
echo ------------------------------------------------------------
echo ATENCAO: esta operacao APAGA o banco %BANCO% completamente.
echo Todos os dados serao perdidos.
echo.
set "CONFIRMA="
set /p "CONFIRMA=Tem certeza? (S/N): "

REM  /I faz comparacao case-insensitive (aceita 's' ou 'S')
if /I not "%CONFIRMA%"=="S" (
    echo.
    echo Operacao cancelada.
    goto FIM_ACAO
)

echo.
echo Apagando banco...
if exist "%BANCO%" (
    del /q "%BANCO%"
    echo Banco apagado.
) else (
    echo Banco nao existia, prosseguindo.
)

echo.
echo Aplicando migrations...
dotnet ef database update --project %PROJETO_API%

echo.
echo Banco recriado.
goto FIM_ACAO


REM ============================================================================
REM  ACAO 5 - Adicionar nova migration
REM ----------------------------------------------------------------------------
REM  dotnet ef migrations add <nome> : compara o DbContext atual com o ultimo
REM  snapshot e gera os arquivos da migration nova em app/Migrations/.
REM
REM  Pede o nome da migration ao usuario.
REM  Valida que o nome nao e vazio antes de executar.
REM  --project %PROJETO_API% : necessario porque as migrations vivem na API
REM ============================================================================
:ACAO_MIGRATION_ADD
cls
echo.
echo [ACAO 5] ADICIONAR MIGRATION
echo ------------------------------------------------------------
set "NOME_MIG="
set /p "NOME_MIG=Nome da migration (ex: AddCampoXYZ): "

REM  Se o nome estiver vazio, cancela
if "%NOME_MIG%"=="" (
    echo.
    echo Nome vazio. Operacao cancelada.
    goto FIM_ACAO
)

echo.
echo Gerando migration "%NOME_MIG%"...
echo ------------------------------------------------------------
dotnet ef migrations add %NOME_MIG% --project %PROJETO_API%
goto FIM_ACAO


REM ============================================================================
REM  ACAO 6 - Aplicar migrations pendentes
REM ----------------------------------------------------------------------------
REM  dotnet ef database update : aplica todas as migrations que ainda nao
REM  estao no banco. Nao apaga dados, so roda as que faltam.
REM
REM  Util depois de fazer git pull e alguem ter adicionado migrations novas,
REM  ou depois de rodar a acao 5 pra criar a migration.
REM ============================================================================
:ACAO_MIGRATION_UPDATE
cls
echo.
echo [ACAO 6] Aplicando migrations pendentes...
echo ------------------------------------------------------------
dotnet ef database update --project %PROJETO_API%
goto FIM_ACAO


REM ============================================================================
REM  ACAO 7 - Publish release self-contained
REM ----------------------------------------------------------------------------
REM  dotnet publish                 : compila para distribuicao
REM  -c Release                     : modo release (otimizado, sem debug)
REM  -o %PUBLISH_DIR%                : pasta de saida (um nivel acima do repo)
REM  --self-contained true          : inclui o runtime .NET no pacote
REM                                   (nao precisa ter .NET instalado no alvo)
REM  Sem -r especificado, usa RID padrao do SO atual (win-x64 no Windows).
REM
REM  OBS: Publica so a API. SeedRunner NAO entra no publish (nao esta no .slnx).
REM ============================================================================
:ACAO_PUBLISH
cls
echo.
echo [ACAO 7] Publicando API em modo Release self-contained...
echo Destino: %PUBLISH_DIR%
echo ------------------------------------------------------------
dotnet publish %PROJETO_API% -c Release -o %PUBLISH_DIR% --self-contained true
goto FIM_ACAO


REM ============================================================================
REM  ROTULO: OPCAO INVALIDA
REM ----------------------------------------------------------------------------
REM  Mostra mensagem e cai no :FIM_ACAO pra decidir se volta ao menu ou sai
REM ============================================================================
:OPCAO_INVALIDA
echo.
echo [AVISO] Opcao "%OPCAO%" invalida.
goto FIM_ACAO


REM ============================================================================
REM  ROTULO: FIM DE ACAO
REM ----------------------------------------------------------------------------
REM  Chamado depois que qualquer acao termina.
REM  Decide o proximo passo com base em VOLTAR_AO_MENU:
REM    - true  : pausa, mostra "pressione tecla" e volta pro :MENU
REM    - false : cai direto em :SAIR
REM
REM  /I     : comparacao case-insensitive (aceita 'true' ou 'TRUE')
REM  pause  : exibe "Pressione qualquer tecla..." e espera input
REM ============================================================================
:FIM_ACAO
echo.
if /I "%VOLTAR_AO_MENU%"=="true" (
    pause
    goto MENU
) else (
    goto SAIR
)


REM ============================================================================
REM  ROTULO: SAIR
REM ----------------------------------------------------------------------------
REM  Finaliza o script de forma limpa.
REM  endlocal : libera as variaveis isoladas pelo setlocal
REM  exit /b  : encerra o .bat sem fechar a janela do terminal
REM ============================================================================
:SAIR
echo.
echo Encerrando ARJSYS menu.
endlocal
exit /b 0
