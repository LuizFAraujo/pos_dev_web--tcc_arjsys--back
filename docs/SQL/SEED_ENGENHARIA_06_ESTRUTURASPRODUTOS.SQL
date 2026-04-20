-- ============================================================================
--  ENGENHARIA 06 — ESTRUTURAS DE PRODUTOS (BOM)
-- ----------------------------------------------------------------------------
--  Tabela: Engenharia_EstruturasProdutos
--  Campos NOT NULL: ProdutoPaiId, ProdutoFilhoId, Quantidade, Posicao, CriadoEm
--  Campos opcionais: Observacao, ModificadoEm, CriadoPor, ModificadoPor
--  FKs (DeleteBehavior.Restrict): Engenharia_Produtos.Id
--
--  Datas: BOMs montados apos lancamento dos conjuntos (set/2024), progressivo
--  entre 10/2024 e 01/2025 conforme a engenharia foi documentando.
--
--  Mapa de Ids (do SEED 05):
--    40=EIX-001, 41=EIX-002, 42=EIX-003
--    43=DSC-001, 44=DSC-002, 45=DSC-003
--    46=FAC-001, 47=FAC-002, 48=FAC-003
--    55=ROT-001, 56=ROT-002, 57=ROT-003
--    66=PIC-001, 67=PIC-002, 68=PIC-003
--    31=MTR-001, 32=MTR-002, 33=MTR-003
--    24=RLM-001, 25=RLM-002, 26=RLM-003
--    16=PRF-001
-- ============================================================================


-- ---- BOM dos Rotores (primeiro documentado - out/2024) --------------------
INSERT INTO Engenharia_EstruturasProdutos
  (ProdutoPaiId, ProdutoFilhoId, Quantidade, Posicao, Observacao, CriadoEm) VALUES
(55, 40, 1,  10, 'Eixo',                     '2024-10-05T09:00:00'),  -- ROT-001 -> EIX-001
(55, 43, 1,  20, 'Disco de corte',           '2024-10-05T09:05:00'),  -- ROT-001 -> DSC-001
(55, 46, 4,  30, 'Facas de corte',           '2024-10-05T09:10:00'),  -- ROT-001 -> FAC-001 x4

(56, 41, 1,  10, 'Eixo',                     '2024-10-10T10:00:00'),  -- ROT-002 -> EIX-002
(56, 44, 1,  20, 'Disco de corte',           '2024-10-10T10:05:00'),  -- ROT-002 -> DSC-002
(56, 47, 6,  30, 'Facas de corte',           '2024-10-10T10:10:00'),  -- ROT-002 -> FAC-002 x6

(57, 42, 1,  10, 'Eixo',                     '2024-10-15T14:00:00'),  -- ROT-003 -> EIX-003
(57, 45, 1,  20, 'Disco de corte',           '2024-10-15T14:05:00'),  -- ROT-003 -> DSC-003
(57, 48, 8,  30, 'Facas de corte',           '2024-10-15T14:10:00');  -- ROT-003 -> FAC-003 x8


-- ---- BOM do PIC-500 (nov/2024) --------------------------------------------
INSERT INTO Engenharia_EstruturasProdutos
  (ProdutoPaiId, ProdutoFilhoId, Quantidade, Posicao, Observacao, CriadoEm) VALUES
(66, 55, 1,  10, 'Rotor principal',          '2024-11-08T09:00:00'),  -- PIC-001 -> ROT-001
(66, 31, 1,  20, 'Motor acionamento',        '2024-11-08T09:05:00'),  -- PIC-001 -> MTR-001
(66, 24, 2,  30, 'Rolamentos do eixo',       '2024-11-08T09:10:00'),  -- PIC-001 -> RLM-001 x2
(66, 16, 8,  40, 'Fixacao geral',            '2024-11-08T09:15:00');  -- PIC-001 -> PRF-001 x8


-- ---- BOM do PIC-700 (dez/2024) --------------------------------------------
INSERT INTO Engenharia_EstruturasProdutos
  (ProdutoPaiId, ProdutoFilhoId, Quantidade, Posicao, Observacao, CriadoEm) VALUES
(67, 56, 1,  10, 'Rotor principal',          '2024-12-02T10:00:00'),  -- PIC-002 -> ROT-002
(67, 32, 1,  20, 'Motor acionamento',        '2024-12-02T10:05:00'),  -- PIC-002 -> MTR-002
(67, 25, 2,  30, 'Rolamentos do eixo',       '2024-12-02T10:10:00'),  -- PIC-002 -> RLM-002 x2
(67, 16, 12, 40, 'Fixacao geral',            '2024-12-02T10:15:00');  -- PIC-002 -> PRF-001 x12


-- ---- BOM do PIC-900 (jan/2025) --------------------------------------------
INSERT INTO Engenharia_EstruturasProdutos
  (ProdutoPaiId, ProdutoFilhoId, Quantidade, Posicao, Observacao, CriadoEm) VALUES
(68, 57, 1,  10, 'Rotor principal',          '2025-01-10T14:00:00'),  -- PIC-003 -> ROT-003
(68, 33, 1,  20, 'Motor acionamento',        '2025-01-10T14:05:00'),  -- PIC-003 -> MTR-003
(68, 26, 2,  30, 'Rolamentos do eixo',       '2025-01-10T14:10:00'),  -- PIC-003 -> RLM-003 x2
(68, 16, 16, 40, 'Fixacao geral',            '2025-01-10T14:15:00');  -- PIC-003 -> PRF-001 x16
