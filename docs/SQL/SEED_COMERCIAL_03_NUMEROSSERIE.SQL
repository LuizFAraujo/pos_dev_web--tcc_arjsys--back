-- ============================================================================
--  COMERCIAL 03 — NUMEROS DE SERIE
-- ----------------------------------------------------------------------------
--  Tabela: Comercial_NumerosSerie
--  Campos NOT NULL: Codigo, PedidoVendaId, CriadoEm
--  FK (DeleteBehavior.Restrict): PedidoVendaId -> Comercial_PedidosVenda.Id
--  Unique: PedidoVendaId (1 PV tem no maximo 1 NS)
--
--  NS nao tem tipo nem status proprios (refatoracao v2) — herda do PV.
--  Codigo no formato II.MM.AA.NNNNN (idade empresa.mes.ano.sequencial).
--  Empresa fundada em 1966:
--    2024 -> idade 58, 2025 -> 59, 2026 -> 60
--
--  CriadoEm: alguns dias apos o PV (engenharia pega quando esta pronta).
--
--  Quais PVs tem NS:
--    1 (Entregue) SIM       2 (Entregue) SIM
--    3 (AgEntrega) SIM      4 (AgEntrega) SIM
--    5 (Concluido) SIM      6 (Concluido) SIM
--    7 (Cancelado) NAO (cancelado antes engenharia pegar)
--    8 (Pausado) SIM        9 (EmAndamento) SIM
--    10 (EmAndamento) SIM   11 (EmAndamento) SIM
--    12 (Cancelado VF) NAO
--    13 (Aguardando) SIM (engenharia ja preparou)
--    14 (Aguardando) NAO    15 (Aguardando) NAO
-- ============================================================================

INSERT INTO Comercial_NumerosSerie
  (Codigo, PedidoVendaId, CodigoProjeto, CriadoEm) VALUES

-- Sequenciais por mes/ano (formato II.MM.AA.NNNNN)
('58.10.24.00001',  1, 'PRJ-2024-0001', '2024-10-22T08:00:00'),  -- PV 1, PIC-500
('58.11.24.00001',  2, 'PRJ-2024-0002', '2024-11-12T09:15:00'),  -- PV 2, PIC-700
('59.08.25.00001',  3, 'PRJ-2025-0018', '2025-08-26T10:30:00'),  -- PV 3, PIC-900
('59.10.25.00001',  4, 'PRJ-2025-0025', '2025-10-17T14:00:00'),  -- PV 4, Faca+Calco
('59.09.25.00001',  5, 'PRJ-2025-0021', '2025-09-09T11:00:00'),  -- PV 5, PIC-700 verde
('59.11.25.00001',  6, 'PRJ-2025-0028', '2025-12-02T09:00:00'),  -- PV 6, PIC-500
('60.01.26.00001',  8, 'PRJ-2026-0003', '2026-01-23T10:00:00'),  -- PV 8, PIC-900 pausado
('60.02.26.00001',  9, 'PRJ-2026-0005', '2026-02-16T08:30:00'),  -- PV 9, PIC-700
('60.03.26.00001', 10, 'PRJ-2026-0008', '2026-03-09T14:15:00'),  -- PV 10, 2x PIC-500
('60.04.26.00001', 11, 'PRJ-2026-0012', '2026-04-05T09:00:00'),  -- PV 11, 2x PIC-700
('60.03.26.00002', 13, 'PRJ-2026-0010', '2026-03-22T11:30:00');  -- PV 13, Aguardando mas ja preparado
