-- ============================================================================
--  PRODUCAO 02 — ITENS DAS ORDENS DE PRODUCAO (snapshot da BOM)
-- ----------------------------------------------------------------------------
--  Tabela: Producao_OrdensProducaoItens
--  Campos NOT NULL: OrdemProducaoId, ProdutoId, QuantidadePlanejada, QuantidadeProduzida, CriadoEm
--  Campos opcionais: Observacao, ModificadoEm, CriadoPor, ModificadoPor
--
--  FKs:
--    OrdemProducaoId -> Producao_OrdensProducao.Id  (Cascade)
--    ProdutoId       -> Engenharia_Produtos.Id      (Restrict)
--
--  Unique: (OrdemProducaoId, ProdutoId) — 1 linha por produto na OP.
--
--  Regra: itens da OP sao o SNAPSHOT da BOM EXPLODIDA CONSOLIDADA (produtos folha),
--         com QuantidadePlanejada = soma de (quantidade_no_caminho × qtde do Master).
--         Produtos intermediarios (ex: Rotor dentro do Picador) NAO entram como item
--         da Master — viram OPs Filha proprias.
--
--  Referencia rapida (Ids de OP):
--    1 = OP.2026.02.0001         Master  PIC-500 x2  (Andamento)
--    2 = OP.2026.02.0001/0001    Filha   ROT-001 x1  (Concluida)
--    3 = OP.2026.02.0001/0002    Filha   ROT-001 x1  (Andamento parcial)
--    4 = OP.2026.03.0001         Master  PIC-700 x2  (Pendente)
--    5 = OP.2026.04.0001         Master  PIC-900 x1  (Concluida, estoque)
--
--  Referencia rapida (Ids de Produto):
--    16=PRF-001  24=RLM-001  25=RLM-002  26=RLM-003
--    31=MTR-001  32=MTR-002  33=MTR-003
--    40=EIX-001  41=EIX-002  42=EIX-003
--    43=DSC-001  44=DSC-002  45=DSC-003
--    46=FAC-001  47=FAC-002  48=FAC-003
-- ============================================================================


-- ---- OP 1 (PIC-500 x2) — Andamento, itens ainda nao concluidos -----------
-- Total = BOM do PIC-500 (por unidade) x 2
-- EIX-001 x1, DSC-001 x1, FAC-001 x4, MTR-001 x1, RLM-001 x2, PRF-001 x8   -> x2 unidades
INSERT INTO Producao_OrdensProducaoItens
  (OrdemProducaoId, ProdutoId, QuantidadePlanejada, QuantidadeProduzida, Observacao, CriadoEm, ModificadoEm) VALUES
(1, 40,  2, 0, 'Eixo principal do rotor',        '2026-02-10T09:00:00', NULL),
(1, 43,  2, 0, 'Disco de corte',                 '2026-02-10T09:00:00', NULL),
(1, 46,  8, 2, 'Facas de corte (2 apontadas)',   '2026-02-10T09:00:00', '2026-04-10T15:00:00'),
(1, 31,  2, 0, 'Motor acionamento',              '2026-02-10T09:00:00', NULL),
(1, 24,  4, 0, 'Rolamentos do eixo',             '2026-02-10T09:00:00', NULL),
(1, 16, 16, 8, 'Fixacao (metade apontada)',      '2026-02-10T09:00:00', '2026-04-05T16:00:00');


-- ---- OP 2 (Filha ROT-001 x1) — Concluida ---------------------------------
-- BOM do Rotor: EIX-001 x1, DSC-001 x1, FAC-001 x4
INSERT INTO Producao_OrdensProducaoItens
  (OrdemProducaoId, ProdutoId, QuantidadePlanejada, QuantidadeProduzida, Observacao, CriadoEm, ModificadoEm) VALUES
(2, 40, 1, 1, 'Eixo',   '2026-02-10T09:05:00', '2026-04-05T16:00:00'),
(2, 43, 1, 1, 'Disco',  '2026-02-10T09:05:00', '2026-04-05T16:00:00'),
(2, 46, 4, 4, 'Facas',  '2026-02-10T09:05:00', '2026-04-05T16:00:00');


-- ---- OP 3 (Filha ROT-001 x1) — Andamento (parcial) -----------------------
-- Producao parcial: eixo 100%, disco 0%, facas 2/4
INSERT INTO Producao_OrdensProducaoItens
  (OrdemProducaoId, ProdutoId, QuantidadePlanejada, QuantidadeProduzida, Observacao, CriadoEm, ModificadoEm) VALUES
(3, 40, 1, 1, 'Eixo concluido',                   '2026-02-10T09:10:00', '2026-04-08T10:00:00'),
(3, 43, 1, 0, 'Disco aguardando usinagem',        '2026-02-10T09:10:00', NULL),
(3, 46, 4, 2, 'Facas em producao (2 de 4)',       '2026-02-10T09:10:00', '2026-04-10T14:00:00');


-- ---- OP 4 (PIC-700 x2) — Pendente, nada produzido ------------------------
-- Total = BOM do PIC-700 x 2
-- EIX-002 x1, DSC-002 x1, FAC-002 x6, MTR-002 x1, RLM-002 x2, PRF-001 x12  -> x2 unidades
INSERT INTO Producao_OrdensProducaoItens
  (OrdemProducaoId, ProdutoId, QuantidadePlanejada, QuantidadeProduzida, Observacao, CriadoEm, ModificadoEm) VALUES
(4, 41,  2, 0, 'Eixo',              '2026-03-14T10:00:00', NULL),
(4, 44,  2, 0, 'Disco',             '2026-03-14T10:00:00', NULL),
(4, 47, 12, 0, 'Facas',             '2026-03-14T10:00:00', NULL),
(4, 32,  2, 0, 'Motor',             '2026-03-14T10:00:00', NULL),
(4, 25,  4, 0, 'Rolamentos',        '2026-03-14T10:00:00', NULL),
(4, 16, 24, 0, 'Fixacao',           '2026-03-14T10:00:00', NULL);


-- ---- OP 5 (PIC-900 x1) — Concluida (estoque) -----------------------------
-- BOM do PIC-900 x 1 (Concluida: produzida == planejada)
INSERT INTO Producao_OrdensProducaoItens
  (OrdemProducaoId, ProdutoId, QuantidadePlanejada, QuantidadeProduzida, Observacao, CriadoEm, ModificadoEm) VALUES
(5, 42,  1,  1, 'Eixo',          '2026-03-28T14:00:00', '2026-04-18T17:00:00'),
(5, 45,  1,  1, 'Disco',         '2026-03-28T14:00:00', '2026-04-18T17:00:00'),
(5, 48,  8,  8, 'Facas',         '2026-03-28T14:00:00', '2026-04-18T17:00:00'),
(5, 33,  1,  1, 'Motor',         '2026-03-28T14:00:00', '2026-04-18T17:00:00'),
(5, 26,  2,  2, 'Rolamentos',    '2026-03-28T14:00:00', '2026-04-18T17:00:00'),
(5, 16, 16, 16, 'Fixacao',       '2026-03-28T14:00:00', '2026-04-18T17:00:00');
