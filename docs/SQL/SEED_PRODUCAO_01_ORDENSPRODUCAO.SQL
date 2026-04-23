-- ============================================================================
--  PRODUCAO 01 — ORDENS DE PRODUCAO
-- ----------------------------------------------------------------------------
--  Tabela: Producao_OrdensProducao
--  Campos NOT NULL: Codigo, ProdutoId, Status, CriadoEm
--  Campos opcionais: PedidoVendaId (null = OP de estoque), OrdemPaiId (null = Master),
--                    DataInicio, DataFim, Observacoes, ModificadoEm, CriadoPor, ModificadoPor
--
--  FKs:
--    PedidoVendaId -> Comercial_PedidosVenda.Id     (Restrict, opcional)
--    ProdutoId     -> Engenharia_Produtos.Id        (Restrict)
--    OrdemPaiId    -> Producao_OrdensProducao.Id    (Cascade, opcional)
--
--  Codigo: OP.AAAA.MM.NNNN para Master, OP.AAAA.MM.NNNN/NNNN para Filha.
--  Unique index em Codigo.
--
--  Cenarios do seed (3 Masters + 2 Filhas = 5 OPs totais):
--
--  Id 1  OP.2026.02.0001        Master  PIC-500  PV 10    Andamento
--                               (PV 10 Andamento; 1 filha Concluida, 1 Andamento)
--  Id 2  OP.2026.02.0001/0001   Filha   ROT-001  (sub)    Concluida
--  Id 3  OP.2026.02.0001/0002   Filha   ROT-001  (sub)    Andamento
--
--  Id 4  OP.2026.03.0001        Master  PIC-700  PV 11    Pendente
--                               (PV 11 Andamento; OP ainda nao iniciada)
--
--  Id 5  OP.2026.04.0001        Master  PIC-900  NULL     Concluida
--                               (OP de estoque, ciclo completo)
--
--  Mapa de Produtos (Engenharia_Produtos.Id):
--    55 = ROT-001    66 = PIC-001 (PIC-500)
--    56 = ROT-002    67 = PIC-002 (PIC-700)
--    57 = ROT-003    68 = PIC-003 (PIC-900)
-- ============================================================================

INSERT INTO Producao_OrdensProducao
  (Codigo, PedidoVendaId, ProdutoId, OrdemPaiId, Status, DataInicio, DataFim, Observacoes, CriadoEm, ModificadoEm) VALUES

-- ---- Master 1 (Id 1): PIC-500 para PV 10, em Andamento -------------------
('OP.2026.02.0001',       10, 66, NULL, 'Andamento', '2026-03-15T08:30:00', NULL,                  'OP master para atender PV 10 (2 x PIC-500)', '2026-02-10T09:00:00', '2026-03-15T08:30:00'),

-- ---- Filhas do Master 1 (Ids 2 e 3): produzem 2 rotores ROT-001 ---------
('OP.2026.02.0001/0001',  10, 55, 1,    'Concluida', '2026-03-15T08:30:00', '2026-04-05T16:00:00', 'Rotor 1 concluido',                          '2026-02-10T09:05:00', '2026-04-05T16:00:00'),
('OP.2026.02.0001/0002',  10, 55, 1,    'Andamento', '2026-04-06T08:00:00', NULL,                  'Rotor 2 em producao',                        '2026-02-10T09:10:00', '2026-04-06T08:00:00'),

-- ---- Master 2 (Id 4): PIC-700 para PV 11, Pendente -----------------------
('OP.2026.03.0001',       11, 67, NULL, 'Pendente',  NULL,                  NULL,                  'OP master para atender PV 11 (2 x PIC-700)', '2026-03-14T10:00:00', NULL),

-- ---- Master 3 (Id 5): PIC-900 para estoque, Concluida --------------------
('OP.2026.04.0001',       NULL, 68, NULL, 'Concluida','2026-04-01T08:00:00', '2026-04-18T17:00:00', 'OP de estoque (sem PV) — producao concluida','2026-03-28T14:00:00', '2026-04-18T17:00:00');
