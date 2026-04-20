-- ============================================================================
--  ENGENHARIA 04 — PATH DE DOCUMENTOS
-- ----------------------------------------------------------------------------
--  Tabela: Engenharia_PathDocumentos
--  Campos NOT NULL: GrupoProdutoId, Path, ControlarPorPrefixo, Ativo, CriadoEm
--  FK: GrupoProdutoId -> Engenharia_GruposProdutos.Id (Restrict)
--  Unique: GrupoProdutoId (1 path por prefixo)
--
--  Vazio por padrao — paths sao cadastrados manualmente conforme necessidade.
--  Exemplo (comentado) para cadastrar um path alternativo para o prefixo 10:
--
--  INSERT INTO Engenharia_PathDocumentos
--    (GrupoProdutoId, Path, ControlarPorPrefixo, Ativo, CriadoEm) VALUES
--    (1, 'D:\Codigos\ProdutoFinal', 0, 1, '2023-03-01T10:00:00');
-- ============================================================================

-- Placeholder para manter arquivo valido mesmo vazio
SELECT 1 WHERE 0 = 1;
