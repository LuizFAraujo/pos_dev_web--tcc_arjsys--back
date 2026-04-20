-- ============================================================================
--  COMERCIAL 01 — PEDIDOS DE VENDA
-- ----------------------------------------------------------------------------
--  Tabela: Comercial_PedidosVenda
--  Campos NOT NULL: Codigo, ClienteId, Tipo, Status, Data, CriadoEm
--  FK (DeleteBehavior.Restrict): ClienteId -> Admin_Clientes.Id
--
--  Cobertura de cenarios (15 PVs, out/2024 a abr/2026):
--    - 3 Aguardando (VendaFutura, pre-pedidos recentes)
--    - 3 EmAndamento (Normal, em producao)
--    - 1 Pausado
--    - 2 Concluido
--    - 2 AguardandoEntrega
--    - 2 Entregue (ciclo completo)
--    - 2 Cancelado
--
--  Data = data da venda (negocio). CriadoEm normalmente = Data, mas pode
--  divergir se o PV foi cadastrado depois (ex: venda boca-a-boca).
-- ============================================================================

INSERT INTO Comercial_PedidosVenda
  (Codigo, ClienteId, Tipo, Status, Data, DataEntrega, Observacoes, CriadoEm) VALUES

-- Entregue (ciclo mais antigo - completo)
('PV.2024.10.0001',  1, 'Normal',      'Entregue',          '2024-10-18T09:30:00', '2025-01-25', 'Primeiro pedido - entregue conforme planejado',                       '2024-10-18T09:30:00'),
('PV.2024.11.0001',  3, 'Normal',      'Entregue',          '2024-11-08T14:15:00', '2025-02-20', NULL,                                                                  '2024-11-08T14:15:00'),

-- AguardandoEntrega (pronto, logistica em andamento)
('PV.2025.08.0001',  2, 'Normal',      'AguardandoEntrega', '2025-08-22T10:00:00', '2026-04-30', 'Aguardando agendamento da transportadora',                            '2025-08-22T10:00:00'),
('PV.2025.10.0001',  5, 'Normal',      'AguardandoEntrega', '2025-10-14T15:40:00', '2026-05-10', NULL,                                                                  '2025-10-14T15:40:00'),

-- Concluido (fabricado, aguardando conferencia para expedicao)
('PV.2025.09.0001',  4, 'Normal',      'Concluido',         '2025-09-05T11:20:00', '2026-05-05', 'Producao finalizada em 10/04/2026',                                   '2025-09-05T11:20:00'),
('PV.2025.11.0001',  7, 'Normal',      'Concluido',         '2025-11-28T08:50:00', '2026-05-15', NULL,                                                                  '2025-11-28T08:50:00'),

-- Cancelado (Normal - cancelado apos iniciar)
('PV.2025.12.0001',  6, 'Normal',      'Cancelado',         '2025-12-10T13:45:00', '2026-04-20', 'Cliente desistiu apos analise de credito',                            '2025-12-10T13:45:00'),

-- Pausado (producao parada aguardando material)
('PV.2026.01.0001', 10, 'Normal',      'Pausado',           '2026-01-20T09:00:00', '2026-06-10', 'Pausado - aguardando chegada de rolamentos importados',               '2026-01-20T09:00:00'),

-- EmAndamento (producao ativa)
('PV.2026.02.0001',  8, 'Normal',      'EmAndamento',       '2026-02-12T10:30:00', '2026-06-20', 'Urgente - safra iniciando',                                           '2026-02-12T10:30:00'),
('PV.2026.03.0001', 11, 'Normal',      'EmAndamento',       '2026-03-05T14:00:00', '2026-07-15', NULL,                                                                  '2026-03-05T14:00:00'),
('PV.2026.04.0001', 12, 'Normal',      'EmAndamento',       '2026-04-02T11:10:00', '2026-08-01', 'Cliente solicitou 2 picadores PIC-700',                               '2026-04-02T11:10:00'),

-- Cancelado (VendaFutura - cancelado ainda em pre-pedido)
('PV.2025.09.0002', 13, 'VendaFutura', 'Cancelado',         '2025-09-18T16:20:00', NULL,         'Pre-pedido cancelado - mudanca de projeto do cliente',                '2025-09-18T16:20:00'),

-- Aguardando (VendaFutura - pre-pedidos recentes)
('PV.2026.03.0002',  9, 'VendaFutura', 'Aguardando',        '2026-03-18T10:00:00', '2026-10-30', 'Cliente avaliando financiamento',                                     '2026-03-18T10:00:00'),
('PV.2026.04.0002', 14, 'VendaFutura', 'Aguardando',        '2026-04-08T11:45:00', '2026-11-15', 'Aguardando aprovacao do conselho',                                    '2026-04-08T11:45:00'),
('PV.2026.04.0003', 15, 'VendaFutura', 'Aguardando',        '2026-04-15T09:20:00', NULL,         'Primeiro contato - orcamento enviado',                                '2026-04-15T09:20:00');
