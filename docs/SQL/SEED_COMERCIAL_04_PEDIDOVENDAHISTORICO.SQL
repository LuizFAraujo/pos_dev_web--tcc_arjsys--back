-- =============================================
-- COMERCIAL: HISTÓRICO DE EVENTOS DOS PVs
-- Eventos: Criado, NsRecebido, RetornoSolicitado, Aprovado,
--          ProducaoIniciada, ProducaoConcluida, LiberadoEntrega, Entregue,
--          Pausado, Retomado, Cancelado, Reaberto, Devolvido
-- Cada PV tem o evento Criado + eventos refletindo como chegou ao status atual
-- =============================================

INSERT INTO Comercial_PedidoVendaHistorico (PedidoVendaId, Evento, StatusAnterior, StatusNovo, Justificativa, DataHora, CriadoEm) VALUES

-- PV 1: Normal → Entregue (2024.03.0001)
(1, 'Criado',            NULL,                 'Liberado',          NULL,                                                 '2024-03-15T09:00:00', '2024-03-15T09:00:00'),
(1, 'ProducaoIniciada',  'Liberado',           'Andamento',         NULL,                                                 '2024-03-20T08:00:00', '2024-03-20T08:00:00'),
(1, 'ProducaoConcluida', 'Andamento',          'Concluido',         NULL,                                                 '2024-05-10T17:00:00', '2024-05-10T17:00:00'),
(1, 'LiberadoEntrega',   'Concluido',          'AEntregar',         NULL,                                                 '2024-05-15T09:00:00', '2024-05-15T09:00:00'),
(1, 'Entregue',          'AEntregar',          'Entregue',          NULL,                                                 '2024-05-20T17:30:00', '2024-05-20T17:30:00'),

-- PV 2: Normal → Entregue (2024.06.0001)
(2, 'Criado',            NULL,                 'Liberado',          NULL,                                                 '2024-06-10T14:20:00', '2024-06-10T14:20:00'),
(2, 'ProducaoIniciada',  'Liberado',           'Andamento',         NULL,                                                 '2024-06-15T08:30:00', '2024-06-15T08:30:00'),
(2, 'ProducaoConcluida', 'Andamento',          'Concluido',         NULL,                                                 '2024-08-05T16:00:00', '2024-08-05T16:00:00'),
(2, 'LiberadoEntrega',   'Concluido',          'AEntregar',         NULL,                                                 '2024-08-10T10:00:00', '2024-08-10T10:00:00'),
(2, 'Entregue',          'AEntregar',          'Entregue',          NULL,                                                 '2024-08-15T16:00:00', '2024-08-15T16:00:00'),

-- PV 3: PreVenda → Entregue (2024.09.0001)
(3, 'Criado',            NULL,                 'AguardandoNS',      NULL,                                                 '2024-09-05T10:00:00', '2024-09-05T10:00:00'),
(3, 'NsRecebido',        'AguardandoNS',       'RecebidoNS',        NULL,                                                 '2024-09-18T14:00:00', '2024-09-18T14:00:00'),
(3, 'RetornoSolicitado', 'RecebidoNS',         'AguardandoRetorno', NULL,                                                 '2024-09-25T11:00:00', '2024-09-25T11:00:00'),
(3, 'Aprovado',          'AguardandoRetorno',  'Liberado',          NULL,                                                 '2024-10-15T15:30:00', '2024-10-15T15:30:00'),
(3, 'ProducaoIniciada',  'Liberado',           'Andamento',         NULL,                                                 '2024-10-22T08:00:00', '2024-10-22T08:00:00'),
(3, 'ProducaoConcluida', 'Andamento',          'Concluido',         NULL,                                                 '2024-12-02T17:00:00', '2024-12-02T17:00:00'),
(3, 'LiberadoEntrega',   'Concluido',          'AEntregar',         NULL,                                                 '2024-12-05T09:00:00', '2024-12-05T09:00:00'),
(3, 'Entregue',          'AEntregar',          'Entregue',          NULL,                                                 '2024-12-10T15:45:00', '2024-12-10T15:45:00'),

-- PV 4: Normal → Entregue → Devolvido (2025.01.0001)
(4, 'Criado',            NULL,                 'Liberado',          NULL,                                                 '2025-01-20T11:00:00', '2025-01-20T11:00:00'),
(4, 'ProducaoIniciada',  'Liberado',           'Andamento',         NULL,                                                 '2025-01-25T08:00:00', '2025-01-25T08:00:00'),
(4, 'ProducaoConcluida', 'Andamento',          'Concluido',         NULL,                                                 '2025-03-15T16:00:00', '2025-03-15T16:00:00'),
(4, 'LiberadoEntrega',   'Concluido',          'AEntregar',         NULL,                                                 '2025-03-20T10:00:00', '2025-03-20T10:00:00'),
(4, 'Entregue',          'AEntregar',          'Entregue',          NULL,                                                 '2025-03-25T14:00:00', '2025-03-25T14:00:00'),
(4, 'Devolvido',         'Entregue',           'Devolvido',         'Cliente devolveu por defeito no rotor',              '2025-04-10T10:00:00', '2025-04-10T10:00:00'),

-- PV 5: PreVenda → Cancelado (2025.04.0001)
(5, 'Criado',            NULL,                 'AguardandoNS',      NULL,                                                 '2025-04-12T08:30:00', '2025-04-12T08:30:00'),
(5, 'NsRecebido',        'AguardandoNS',       'RecebidoNS',        NULL,                                                 '2025-04-22T15:00:00', '2025-04-22T15:00:00'),
(5, 'RetornoSolicitado', 'RecebidoNS',         'AguardandoRetorno', NULL,                                                 '2025-04-28T10:00:00', '2025-04-28T10:00:00'),
(5, 'Cancelado',         'AguardandoRetorno',  'Cancelado',         'Cliente cancelou — desistiu do financiamento',       '2025-05-02T14:00:00', '2025-05-02T14:00:00'),

-- PV 6: Normal → Cancelado → Reaberto (2025.07.0001)
(6, 'Criado',            NULL,                 'Liberado',          NULL,                                                 '2025-07-18T16:00:00', '2025-07-18T16:00:00'),
(6, 'Cancelado',         'Liberado',           'Cancelado',         'Cliente pediu cancelamento temporário',              '2025-07-30T11:00:00', '2025-07-30T11:00:00'),
(6, 'Reaberto',          'Cancelado',          'Reaberto',          'Cliente reativou — reavaliação comercial',           '2025-08-15T09:30:00', '2025-08-15T09:30:00'),

-- PV 7: Normal → AEntregar (2025.10.0001)
(7, 'Criado',            NULL,                 'Liberado',          NULL,                                                 '2025-10-22T09:15:00', '2025-10-22T09:15:00'),
(7, 'ProducaoIniciada',  'Liberado',           'Andamento',         NULL,                                                 '2025-10-28T08:00:00', '2025-10-28T08:00:00'),
(7, 'ProducaoConcluida', 'Andamento',          'Concluido',         NULL,                                                 '2026-03-20T17:00:00', '2026-03-20T17:00:00'),
(7, 'LiberadoEntrega',   'Concluido',          'AEntregar',         NULL,                                                 '2026-04-01T10:00:00', '2026-04-01T10:00:00'),

-- PV 8: Normal → Concluido (2025.11.0001)
(8, 'Criado',            NULL,                 'Liberado',          NULL,                                                 '2025-11-05T13:40:00', '2025-11-05T13:40:00'),
(8, 'ProducaoIniciada',  'Liberado',           'Andamento',         NULL,                                                 '2025-11-12T08:00:00', '2025-11-12T08:00:00'),
(8, 'ProducaoConcluida', 'Andamento',          'Concluido',         NULL,                                                 '2026-03-20T17:00:00', '2026-03-20T17:00:00'),

-- PV 9: Normal → Pausado (2026.01.0001)
(9, 'Criado',            NULL,                 'Liberado',          NULL,                                                 '2026-01-14T10:20:00', '2026-01-14T10:20:00'),
(9, 'ProducaoIniciada',  'Liberado',           'Andamento',         NULL,                                                 '2026-01-20T08:00:00', '2026-01-20T08:00:00'),
(9, 'Pausado',           'Andamento',          'Pausado',           'Cliente solicitou pausa de 30 dias',                 '2026-02-28T11:00:00', '2026-02-28T11:00:00'),

-- PV 10: Normal → Andamento (2026.02.0001)
(10, 'Criado',           NULL,                 'Liberado',          NULL,                                                 '2026-02-08T14:00:00', '2026-02-08T14:00:00'),
(10, 'ProducaoIniciada', 'Liberado',           'Andamento',         NULL,                                                 '2026-03-15T08:30:00', '2026-03-15T08:30:00'),

-- PV 11: Normal → Andamento (2026.03.0001)
(11, 'Criado',           NULL,                 'Liberado',          NULL,                                                 '2026-03-12T09:45:00', '2026-03-12T09:45:00'),
(11, 'ProducaoIniciada', 'Liberado',           'Andamento',         NULL,                                                 '2026-04-05T10:15:00', '2026-04-05T10:15:00'),

-- PV 12: Normal → Liberado (2026.04.0001) — apenas criado
(12, 'Criado',           NULL,                 'Liberado',          NULL,                                                 '2026-04-02T15:30:00', '2026-04-02T15:30:00'),

-- PV 13: PreVenda → AguardandoRetorno (2026.04.0002)
(13, 'Criado',           NULL,                 'AguardandoNS',      NULL,                                                 '2026-04-08T11:00:00', '2026-04-08T11:00:00'),
(13, 'NsRecebido',       'AguardandoNS',       'RecebidoNS',        NULL,                                                 '2026-04-12T16:00:00', '2026-04-12T16:00:00'),
(13, 'RetornoSolicitado','RecebidoNS',         'AguardandoRetorno', NULL,                                                 '2026-04-15T14:00:00', '2026-04-15T14:00:00'),

-- PV 14: PreVenda → RecebidoNS (2026.04.0003)
(14, 'Criado',           NULL,                 'AguardandoNS',      NULL,                                                 '2026-04-14T09:00:00', '2026-04-14T09:00:00'),
(14, 'NsRecebido',       'AguardandoNS',       'RecebidoNS',        NULL,                                                 '2026-04-18T16:30:00', '2026-04-18T16:30:00'),

-- PV 15: PreVenda → AguardandoNS (2026.04.0004) — apenas criado
(15, 'Criado',           NULL,                 'AguardandoNS',      NULL,                                                 '2026-04-19T16:20:00', '2026-04-19T16:20:00');
