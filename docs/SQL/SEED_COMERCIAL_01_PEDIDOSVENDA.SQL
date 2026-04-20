-- =============================================
-- COMERCIAL: PEDIDOS DE VENDA
-- 15 PVs distribuídos entre os status do fluxo v3
-- Status: AguardandoNS, RecebidoNS, AguardandoRetorno, Liberado,
--         Andamento, Concluido, AEntregar, Entregue,
--         Pausado, Cancelado, Reaberto, Devolvido
-- Tipo: Normal, PreVenda
-- Cronologia: PVs mais antigos em 2024 → mais recentes em 2026
-- =============================================

INSERT INTO Comercial_PedidosVenda (Codigo, ClienteId, Tipo, Status, Data, DataEntrega, Observacoes, CriadoEm, ModificadoEm) VALUES
-- Ciclo completo finalizado (Entregue) — 2024
('PV.2024.03.0001',  1, 'Normal',   'Entregue',          '2024-03-15T09:00:00', '2024-05-20T00:00:00', 'Picador PIC-500 entregue conforme contrato',           '2024-03-15T09:00:00', '2024-05-20T17:30:00'),
('PV.2024.06.0001',  3, 'Normal',   'Entregue',          '2024-06-10T14:20:00', '2024-08-15T00:00:00', 'Trituradora industrial — entrega realizada',           '2024-06-10T14:20:00', '2024-08-15T16:00:00'),
('PV.2024.09.0001',  5, 'PreVenda', 'Entregue',          '2024-09-05T10:00:00', '2024-12-10T00:00:00', 'Pré-venda aprovada após financiamento',                '2024-09-05T10:00:00', '2024-12-10T15:45:00'),

-- Devolvido (só pode vir de Entregue) — 2025
('PV.2025.01.0001',  2, 'Normal',   'Devolvido',         '2025-01-20T11:00:00', '2025-03-25T00:00:00', 'Cliente devolveu por defeito no rotor',                '2025-01-20T11:00:00', '2025-04-10T10:00:00'),

-- Cancelado e Reaberto — 2025
('PV.2025.04.0001',  7, 'PreVenda', 'Cancelado',         '2025-04-12T08:30:00', NULL,                  'Cliente cancelou — desistiu do financiamento',          '2025-04-12T08:30:00', '2025-05-02T14:00:00'),
('PV.2025.07.0001',  4, 'Normal',   'Reaberto',          '2025-07-18T16:00:00', '2025-10-20T00:00:00', 'Reaberto após reavaliação comercial',                   '2025-07-18T16:00:00', '2025-08-15T09:30:00'),

-- Ciclo em andamento — 2025/2026
('PV.2025.10.0001',  6, 'Normal',   'AEntregar',         '2025-10-22T09:15:00', '2026-05-15T00:00:00', 'Aguardando expedição',                                  '2025-10-22T09:15:00', '2026-04-01T10:00:00'),
('PV.2025.11.0001',  8, 'Normal',   'Concluido',         '2025-11-05T13:40:00', '2026-05-30T00:00:00', 'Produção concluída',                                    '2025-11-05T13:40:00', '2026-03-20T17:00:00'),

-- Pausado — 2026
('PV.2026.01.0001',  9, 'Normal',   'Pausado',           '2026-01-14T10:20:00', '2026-07-20T00:00:00', 'Cliente solicitou pausa de 30 dias',                    '2026-01-14T10:20:00', '2026-02-28T11:00:00'),

-- Em Andamento — 2026
('PV.2026.02.0001', 10, 'Normal',   'Andamento',         '2026-02-08T14:00:00', '2026-08-10T00:00:00', 'Produção em andamento',                                 '2026-02-08T14:00:00', '2026-03-15T08:30:00'),
('PV.2026.03.0001', 11, 'Normal',   'Andamento',         '2026-03-12T09:45:00', '2026-09-20T00:00:00', 'OP iniciada semana passada',                            '2026-03-12T09:45:00', '2026-04-05T10:15:00'),

-- Liberado — 2026 (Normal pronto pra Produção)
('PV.2026.04.0001', 12, 'Normal',   'Liberado',          '2026-04-02T15:30:00', '2026-10-15T00:00:00', 'Liberado para produção',                                '2026-04-02T15:30:00', NULL),

-- Fluxo PreVenda em andamento — 2026
('PV.2026.04.0002', 13, 'PreVenda', 'AguardandoRetorno', '2026-04-08T11:00:00', '2026-11-30T00:00:00', 'Proposta enviada ao cliente',                           '2026-04-08T11:00:00', '2026-04-15T14:00:00'),
('PV.2026.04.0003', 14, 'PreVenda', 'RecebidoNS',        '2026-04-14T09:00:00', NULL,                  'NS gerado pela Engenharia, avaliando com cliente',      '2026-04-14T09:00:00', '2026-04-18T16:30:00'),
('PV.2026.04.0004', 15, 'PreVenda', 'AguardandoNS',      '2026-04-19T16:20:00', NULL,                  'PV criado, aguardando NS da Engenharia',                '2026-04-19T16:20:00', NULL);
