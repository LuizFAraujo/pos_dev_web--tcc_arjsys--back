-- ============================================================================
--  COMERCIAL 04 — HISTORICO DOS PEDIDOS DE VENDA
-- ----------------------------------------------------------------------------
--  Tabela: Comercial_PedidoVendaHistorico
--  Campos NOT NULL: PedidoVendaId, Evento, DataHora, CriadoEm
--  Campos opcionais: StatusAnterior, StatusNovo, Justificativa, ModificadoEm, ...
--  FK (DeleteBehavior.Cascade): PedidoVendaId -> Comercial_PedidosVenda.Id
--
--  Cada PV tem pelo menos o evento Criado. Quem passou por transicoes de
--  status tem a trilha completa. Justificativa presente em:
--    Pausado, Cancelado, Retroceder (nao ha nos seeds), Reaberto (nao ha).
--
--  Cronologia coerente: DataHora <= CriadoEm do proximo evento.
--  Status iniciais:
--    Normal       -> Criado com StatusNovo=EmAndamento
--    VendaFutura  -> Criado com StatusNovo=Aguardando
-- ============================================================================


-- ---- PV 1 (Normal, Entregue) - ciclo completo ------------------------------
INSERT INTO Comercial_PedidoVendaHistorico
  (PedidoVendaId, Evento, StatusAnterior, StatusNovo, Justificativa, DataHora, CriadoEm) VALUES
(1, 'Criado',             NULL,                'EmAndamento',       NULL, '2024-10-18T09:30:00', '2024-10-18T09:30:00'),
(1, 'Concluido',           'EmAndamento',       'Concluido',         NULL, '2024-12-20T16:00:00', '2024-12-20T16:00:00'),
(1, 'AguardandoEntrega',   'Concluido',         'AguardandoEntrega', NULL, '2025-01-10T10:00:00', '2025-01-10T10:00:00'),
(1, 'Entregue',            'AguardandoEntrega', 'Entregue',          NULL, '2025-01-25T14:30:00', '2025-01-25T14:30:00');


-- ---- PV 2 (Normal, Entregue) - ciclo completo ------------------------------
INSERT INTO Comercial_PedidoVendaHistorico
  (PedidoVendaId, Evento, StatusAnterior, StatusNovo, Justificativa, DataHora, CriadoEm) VALUES
(2, 'Criado',             NULL,                'EmAndamento',       NULL, '2024-11-08T14:15:00', '2024-11-08T14:15:00'),
(2, 'Concluido',           'EmAndamento',       'Concluido',         NULL, '2025-01-22T11:30:00', '2025-01-22T11:30:00'),
(2, 'AguardandoEntrega',   'Concluido',         'AguardandoEntrega', NULL, '2025-02-05T09:45:00', '2025-02-05T09:45:00'),
(2, 'Entregue',            'AguardandoEntrega', 'Entregue',          NULL, '2025-02-20T15:00:00', '2025-02-20T15:00:00');


-- ---- PV 3 (Normal, AguardandoEntrega) --------------------------------------
INSERT INTO Comercial_PedidoVendaHistorico
  (PedidoVendaId, Evento, StatusAnterior, StatusNovo, Justificativa, DataHora, CriadoEm) VALUES
(3, 'Criado',             NULL,                'EmAndamento',       NULL, '2025-08-22T10:00:00', '2025-08-22T10:00:00'),
(3, 'Concluido',           'EmAndamento',       'Concluido',         NULL, '2026-03-15T17:00:00', '2026-03-15T17:00:00'),
(3, 'AguardandoEntrega',   'Concluido',         'AguardandoEntrega', NULL, '2026-04-10T10:20:00', '2026-04-10T10:20:00');


-- ---- PV 4 (Normal, AguardandoEntrega) --------------------------------------
INSERT INTO Comercial_PedidoVendaHistorico
  (PedidoVendaId, Evento, StatusAnterior, StatusNovo, Justificativa, DataHora, CriadoEm) VALUES
(4, 'Criado',             NULL,                'EmAndamento',       NULL, '2025-10-14T15:40:00', '2025-10-14T15:40:00'),
(4, 'Concluido',           'EmAndamento',       'Concluido',         NULL, '2026-03-28T16:00:00', '2026-03-28T16:00:00'),
(4, 'AguardandoEntrega',   'Concluido',         'AguardandoEntrega', NULL, '2026-04-12T11:15:00', '2026-04-12T11:15:00');


-- ---- PV 5 (Normal, Concluido) ----------------------------------------------
INSERT INTO Comercial_PedidoVendaHistorico
  (PedidoVendaId, Evento, StatusAnterior, StatusNovo, Justificativa, DataHora, CriadoEm) VALUES
(5, 'Criado',             NULL,                'EmAndamento',       NULL, '2025-09-05T11:20:00', '2025-09-05T11:20:00'),
(5, 'Concluido',           'EmAndamento',       'Concluido',         NULL, '2026-04-10T17:00:00', '2026-04-10T17:00:00');


-- ---- PV 6 (Normal, Concluido) ----------------------------------------------
INSERT INTO Comercial_PedidoVendaHistorico
  (PedidoVendaId, Evento, StatusAnterior, StatusNovo, Justificativa, DataHora, CriadoEm) VALUES
(6, 'Criado',             NULL,                'EmAndamento',       NULL, '2025-11-28T08:50:00', '2025-11-28T08:50:00'),
(6, 'Concluido',           'EmAndamento',       'Concluido',         NULL, '2026-04-15T16:30:00', '2026-04-15T16:30:00');


-- ---- PV 7 (Normal, Cancelado) - cancelado com justificativa ----------------
INSERT INTO Comercial_PedidoVendaHistorico
  (PedidoVendaId, Evento, StatusAnterior, StatusNovo, Justificativa, DataHora, CriadoEm) VALUES
(7, 'Criado',             NULL,                'EmAndamento',       NULL,                                          '2025-12-10T13:45:00', '2025-12-10T13:45:00'),
(7, 'Cancelado',           'EmAndamento',       'Cancelado',         'Cliente desistiu apos analise de credito',   '2025-12-28T10:00:00', '2025-12-28T10:00:00');


-- ---- PV 8 (Normal, Pausado) - pausado com justificativa --------------------
INSERT INTO Comercial_PedidoVendaHistorico
  (PedidoVendaId, Evento, StatusAnterior, StatusNovo, Justificativa, DataHora, CriadoEm) VALUES
(8, 'Criado',             NULL,                'EmAndamento',       NULL,                                                        '2026-01-20T09:00:00', '2026-01-20T09:00:00'),
(8, 'Pausado',             'EmAndamento',       'Pausado',           'Pausado aguardando chegada de rolamentos importados',       '2026-03-05T14:00:00', '2026-03-05T14:00:00');


-- ---- PV 9 (Normal, EmAndamento) - so criacao -------------------------------
INSERT INTO Comercial_PedidoVendaHistorico
  (PedidoVendaId, Evento, StatusAnterior, StatusNovo, Justificativa, DataHora, CriadoEm) VALUES
(9, 'Criado', NULL, 'EmAndamento', NULL, '2026-02-12T10:30:00', '2026-02-12T10:30:00');


-- ---- PV 10 (Normal, EmAndamento) - so criacao ------------------------------
INSERT INTO Comercial_PedidoVendaHistorico
  (PedidoVendaId, Evento, StatusAnterior, StatusNovo, Justificativa, DataHora, CriadoEm) VALUES
(10, 'Criado', NULL, 'EmAndamento', NULL, '2026-03-05T14:00:00', '2026-03-05T14:00:00');


-- ---- PV 11 (Normal, EmAndamento) - so criacao ------------------------------
INSERT INTO Comercial_PedidoVendaHistorico
  (PedidoVendaId, Evento, StatusAnterior, StatusNovo, Justificativa, DataHora, CriadoEm) VALUES
(11, 'Criado', NULL, 'EmAndamento', NULL, '2026-04-02T11:10:00', '2026-04-02T11:10:00');


-- ---- PV 12 (VendaFutura, Cancelado) - cancelado em pre-pedido --------------
INSERT INTO Comercial_PedidoVendaHistorico
  (PedidoVendaId, Evento, StatusAnterior, StatusNovo, Justificativa, DataHora, CriadoEm) VALUES
(12, 'Criado',             NULL,                'Aguardando',        NULL,                                                     '2025-09-18T16:20:00', '2025-09-18T16:20:00'),
(12, 'Cancelado',           'Aguardando',        'Cancelado',         'Pre-pedido cancelado - mudanca de projeto do cliente',  '2025-10-25T11:00:00', '2025-10-25T11:00:00');


-- ---- PV 13 (VendaFutura, Aguardando) - so criacao --------------------------
INSERT INTO Comercial_PedidoVendaHistorico
  (PedidoVendaId, Evento, StatusAnterior, StatusNovo, Justificativa, DataHora, CriadoEm) VALUES
(13, 'Criado', NULL, 'Aguardando', NULL, '2026-03-18T10:00:00', '2026-03-18T10:00:00');


-- ---- PV 14 (VendaFutura, Aguardando) - so criacao --------------------------
INSERT INTO Comercial_PedidoVendaHistorico
  (PedidoVendaId, Evento, StatusAnterior, StatusNovo, Justificativa, DataHora, CriadoEm) VALUES
(14, 'Criado', NULL, 'Aguardando', NULL, '2026-04-08T11:45:00', '2026-04-08T11:45:00');


-- ---- PV 15 (VendaFutura, Aguardando) - so criacao --------------------------
INSERT INTO Comercial_PedidoVendaHistorico
  (PedidoVendaId, Evento, StatusAnterior, StatusNovo, Justificativa, DataHora, CriadoEm) VALUES
(15, 'Criado', NULL, 'Aguardando', NULL, '2026-04-15T09:20:00', '2026-04-15T09:20:00');
