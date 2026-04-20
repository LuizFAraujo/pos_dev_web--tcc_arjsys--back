-- ============================================================================
--  COMERCIAL 03 — NUMEROS DE SERIE
-- ----------------------------------------------------------------------------
--  Tabela: Comercial_NumerosSerie
--  Campos NOT NULL: Codigo, PedidoVendaId, CriadoEm
--  FK (DeleteBehavior.Restrict): PedidoVendaId -> Comercial_PedidosVenda.Id
--  FK (DeleteBehavior.Restrict): ProdutoId     -> Engenharia_Produtos.Id  (opcional)
--  Unique: PedidoVendaId (1 PV tem no maximo 1 NS)
--
--  NS nao tem tipo nem status proprios — herda do PV.
--  Codigo no formato II.MM.AA.NNNNN (idade empresa.mes.ano.sequencial).
--  Empresa fundada em 1966:
--    2024 -> idade 58, 2025 -> 59, 2026 -> 60
--
--  ProdutoId: FK para o Produto BOM que atende o PV (projeto).
--  Produtos BOM no seed (com estrutura):
--    66 = PIC-001 (Picador PIC-500)
--    67 = PIC-002 (Picador PIC-700)
--    68 = PIC-003 (Picador PIC-900)
--    69 = EST-001 (Esteira 3m)
--    70 = EST-002 (Esteira 5m)
--
--  Quais PVs tem NS (coerencia com seed 01 v3):
--    PV  1 Normal   Entregue           -> SIM (ciclo completo)
--    PV  2 Normal   Entregue           -> SIM (ciclo completo)
--    PV  3 PreVenda Entregue           -> SIM (passou por AguardandoNS)
--    PV  4 Normal   Devolvido          -> SIM (foi entregue antes)
--    PV  5 PreVenda Cancelado          -> SIM (passou por RecebidoNS antes de cancelar)
--    PV  6 Normal   Reaberto           -> NAO (cancelou antes de producao)
--    PV  7 Normal   AEntregar          -> SIM
--    PV  8 Normal   Concluido          -> SIM
--    PV  9 Normal   Pausado            -> SIM
--    PV 10 Normal   Andamento          -> SIM
--    PV 11 Normal   Andamento          -> SIM
--    PV 12 Normal   Liberado           -> NAO (NS sera criado via OP na Fase 3b)
--    PV 13 PreVenda AguardandoRetorno  -> SIM (passou por RecebidoNS)
--    PV 14 PreVenda RecebidoNS         -> SIM (engenharia ja gerou)
--    PV 15 PreVenda AguardandoNS       -> NAO (engenharia ainda nao gerou)
-- ============================================================================

INSERT INTO Comercial_NumerosSerie
  (Codigo, PedidoVendaId, ProdutoId, CriadoEm, ModificadoEm) VALUES

-- 2024
('58.03.24.00001',  1, 66, '2024-03-18T10:00:00', NULL),  -- PV 1, PIC-500
('58.06.24.00001',  2, 67, '2024-06-14T09:30:00', NULL),  -- PV 2, PIC-700
('58.09.24.00001',  3, 68, '2024-09-10T11:00:00', NULL),  -- PV 3 (PreVenda), PIC-900

-- 2025
('59.01.25.00001',  4, 66, '2025-01-22T08:45:00', NULL),  -- PV 4, PIC-500 (depois devolvido)
('59.04.25.00001',  5, 67, '2025-04-18T14:20:00', NULL),  -- PV 5 (PreVenda cancelado), PIC-700
('59.10.25.00001',  7, 66, '2025-10-24T09:00:00', NULL),  -- PV 7, PIC-500
('59.11.25.00001',  8, 67, '2025-11-10T10:30:00', NULL),  -- PV 8, PIC-700

-- 2026
('60.01.26.00001',  9, 67, '2026-01-16T11:00:00', NULL),  -- PV 9 (Pausado), PIC-700
('60.02.26.00001', 10, 66, '2026-02-11T08:30:00', NULL),  -- PV 10, PIC-500
('60.03.26.00001', 11, 67, '2026-03-15T14:00:00', NULL),  -- PV 11, PIC-700
('60.04.26.00001', 13, 66, '2026-04-10T09:30:00', NULL),  -- PV 13 (PreVenda AguardandoRetorno), PIC-500
('60.04.26.00002', 14, 68, '2026-04-16T15:00:00', NULL);  -- PV 14 (PreVenda RecebidoNS), PIC-900
