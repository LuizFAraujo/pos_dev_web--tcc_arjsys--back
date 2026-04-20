-- ============================================================================
--  COMERCIAL 02 — ITENS DOS PEDIDOS DE VENDA
-- ----------------------------------------------------------------------------
--  Tabela: Comercial_PedidosVendaItens
--  Campos NOT NULL: PedidoVendaId, Quantidade, Descricao, CriadoEm
--  FK (DeleteBehavior.Cascade): PedidoVendaId -> Comercial_PedidosVenda.Id
--
--  Itens sao DESCRICAO LIVRE (refatoracao v2) — sem vinculo com Produto.
--  CriadoEm igual ao CriadoEm do PV (itens cadastrados junto do pedido).
--
--  Dependencia: Comercial_PedidosVenda Ids 1..15.
--    1=PV.2024.10.0001 Entregue            2=PV.2024.11.0001 Entregue
--    3=PV.2025.08.0001 AguardandoEntrega   4=PV.2025.10.0001 AguardandoEntrega
--    5=PV.2025.09.0001 Concluido           6=PV.2025.11.0001 Concluido
--    7=PV.2025.12.0001 Cancelado           8=PV.2026.01.0001 Pausado
--    9=PV.2026.02.0001 EmAndamento         10=PV.2026.03.0001 EmAndamento
--    11=PV.2026.04.0001 EmAndamento        12=PV.2025.09.0002 Cancelado
--    13=PV.2026.03.0002 Aguardando         14=PV.2026.04.0002 Aguardando
--    15=PV.2026.04.0003 Aguardando
-- ============================================================================

INSERT INTO Comercial_PedidosVendaItens
  (PedidoVendaId, Quantidade, Descricao, Observacao, CriadoEm) VALUES

-- PV 1 (Entregue)
(1, 1, 'Picador de Madeira PIC-500',                 NULL,                          '2024-10-18T09:30:00'),

-- PV 2 (Entregue)
(2, 1, 'Picador de Madeira PIC-700',                 NULL,                          '2024-11-08T14:15:00'),
(2, 2, 'Disco de Corte D800mm',                      'Reposicao preventiva',        '2024-11-08T14:15:00'),

-- PV 3 (AguardandoEntrega)
(3, 1, 'Picador de Madeira PIC-900',                 'Linha industrial',            '2025-08-22T10:00:00'),
(3, 1, 'Caixa de Mancal Completa D60 reserva',       NULL,                          '2025-08-22T10:00:00'),

-- PV 4 (AguardandoEntrega)
(4, 3, 'Conjunto Faca + Calco de regulagem',         'Reposicao programada',        '2025-10-14T15:40:00'),

-- PV 5 (Concluido)
(5, 1, 'Picador de Madeira PIC-700 customizado',     'Cor verde, pintura especial', '2025-09-05T11:20:00'),
(5, 1, 'Esteira Transportadora 5m',                  NULL,                          '2025-09-05T11:20:00'),

-- PV 6 (Concluido)
(6, 1, 'Picador de Madeira PIC-500',                 NULL,                          '2025-11-28T08:50:00'),

-- PV 7 (Cancelado - Normal)
(7, 1, 'Picador de Madeira PIC-900',                 'Cancelado antes de iniciar',  '2025-12-10T13:45:00'),

-- PV 8 (Pausado)
(8, 1, 'Picador de Madeira PIC-900',                 'Aguardando chegada do eixo',  '2026-01-20T09:00:00'),

-- PV 9 (EmAndamento)
(9, 1, 'Picador de Madeira PIC-700',                 'Motor 75CV',                  '2026-02-12T10:30:00'),

-- PV 10 (EmAndamento)
(10, 2, 'Picador de Madeira PIC-500',                NULL,                          '2026-03-05T14:00:00'),
(10, 1, 'Esteira Transportadora 3m',                 NULL,                          '2026-03-05T14:00:00'),
(10, 4, 'Jogo de facas reserva modelo 500',          'Entrega junto',               '2026-03-05T14:00:00'),

-- PV 11 (EmAndamento)
(11, 2, 'Picador de Madeira PIC-700',                'Dois picadores identicos',    '2026-04-02T11:10:00'),

-- PV 12 (Cancelado - VendaFutura)
(12, 2, 'Picador de Madeira PIC-700',                'Pre-pedido cancelado',        '2025-09-18T16:20:00'),

-- PV 13 (Aguardando)
(13, 1, 'Picador de Madeira PIC-900 industrial',     'Linha pesada',                '2026-03-18T10:00:00'),

-- PV 14 (Aguardando)
(14, 2, 'Picador de Madeira PIC-500',                'Modelo compacto',             '2026-04-08T11:45:00'),
(14, 1, 'Kit manutencao anual',                      'Facas + rolamentos reserva',  '2026-04-08T11:45:00'),

-- PV 15 (Aguardando)
(15, 1, 'Picador de Madeira PIC-700 completo',       'Modelo base, sem esteira',    '2026-04-15T09:20:00'),
(15, 1, 'Esteira Transportadora 5m',                 'Integrada ao picador',        '2026-04-15T09:20:00');
