-- ============================================================================
--  ARJSYS - TRUNCATE GERAL
-- ----------------------------------------------------------------------------
--  Apaga os DADOS de todas as tabelas de negócio.
--  Mantém a estrutura (schema) e a tabela interna do EF (__EFMigrationsHistory).
--
--  Uso recomendado: listar este arquivo como PRIMEIRA linha do seed-order.txt
--  para limpar o banco antes de repopular com os SEEDs.
--
--  Estratégia:
--    1. Desliga o enforcement de FK temporariamente (PRAGMA foreign_keys = OFF).
--       Isso permite deletar em qualquer ordem sem erro de "FK constraint failed".
--    2. Executa DELETE em cada tabela.
--    3. Zera a sequência de autoincrement das tabelas (IDs voltam a começar em 1).
--    4. Religa o enforcement de FK (PRAGMA foreign_keys = ON).
--
--  Observações:
--    - DELETE FROM <tabela>           apaga todas as linhas mas mantém a tabela
--    - DELETE FROM sqlite_sequence    zera o contador de AUTOINCREMENT do SQLite
--    - PRAGMA foreign_keys = OFF      é sessão-scoped, afeta só esta conexão
--    - A tabela __EFMigrationsHistory NÃO é tocada (preserva histórico do EF)
-- ============================================================================


-- ---- 1) Desliga FKs temporariamente -----------------------------------------
-- Evita erro ao deletar tabelas "pai" antes das "filhas".
-- Com FK off, podemos deletar em qualquer ordem sem violação de constraint.
PRAGMA foreign_keys = OFF;


-- ---- 2) Limpa tabelas do módulo PRODUÇÃO ------------------------------------
-- Filhas primeiro (Historico, Itens) e depois Master (OrdensProducao),
-- mas como FK está desligada a ordem é livre.
DELETE FROM Producao_OrdemProducaoHistorico;
DELETE FROM Producao_OrdensProducaoItens;
DELETE FROM Producao_OrdensProducao;


-- ---- 3) Limpa tabelas do módulo COMERCIAL -----------------------------------
-- Ordem lógica (filhas primeiro), mas como FK está desligada, a ordem é livre.
DELETE FROM Comercial_PedidoVendaHistorico;
DELETE FROM Comercial_NumerosSerie;
DELETE FROM Comercial_PedidosVendaItens;
DELETE FROM Comercial_PedidosVenda;


-- ---- 4) Limpa tabelas do módulo ADMIN ---------------------------------------
DELETE FROM Admin_Notificacoes;
DELETE FROM Admin_Permissoes;
DELETE FROM Admin_Funcionarios;
DELETE FROM Admin_Clientes;
DELETE FROM Admin_Pessoas;


-- ---- 5) Limpa tabelas do módulo ENGENHARIA ----------------------------------
DELETE FROM Engenharia_EstruturasProdutos;
DELETE FROM Engenharia_Produtos;
DELETE FROM Engenharia_GruposVinculos;
DELETE FROM Engenharia_GruposProdutos;
DELETE FROM Engenharia_PathDocumentos;
DELETE FROM Engenharia_Configuracoes;


-- ---- 6) Zera os contadores de AUTOINCREMENT --------------------------------
-- Sem isso, mesmo depois do DELETE os novos INSERTs continuam com ID a partir
-- do último valor usado. Zerando, a próxima linha inserida recebe Id = 1.
-- A tabela sqlite_sequence é interna do SQLite e guarda o último Id de cada
-- tabela com AUTOINCREMENT.
DELETE FROM sqlite_sequence;


-- ---- 7) Religa FKs ----------------------------------------------------------
-- Volta ao comportamento normal: o banco passa a validar FKs em qualquer
-- INSERT/UPDATE/DELETE subsequente.
PRAGMA foreign_keys = ON;
