-- ============================================================================
--  ENGENHARIA 01 — CONFIGURACOES
-- ----------------------------------------------------------------------------
--  Tabela: Engenharia_Configuracoes
--  Campos NOT NULL: Chave, Valor, CriadoEm
--  Campos opcionais: Descricao, ModificadoEm, CriadoPor, ModificadoPor
--
--  Datas: setup inicial do sistema em janeiro/2023.
-- ============================================================================

INSERT INTO Engenharia_Configuracoes (Chave, Valor, Descricao, CriadoEm) VALUES
('PathRaizDocumentos',       'D:\Codigos', 'Caminho raiz padrao para documentos/desenhos de produtos', '2023-01-15T08:00:00'),
('ControlarPorPrefixoRaiz',  'false',      'Se true, o path raiz organiza por subpasta de prefixo',   '2023-01-15T08:05:00');
