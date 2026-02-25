-- =============================================
-- COMERCIAL: PEDIDOS DE VENDA
-- =============================================
INSERT INTO Comercial_PedidosVenda (Codigo, ClienteId, Status, Data, Observacoes, CriadoEm) VALUES
('PV.2025.06.0001', 1, 'Entregue', '2025-06-10T10:00:00', NULL, '2025-06-10T10:00:00'),
('PV.2025.06.0002', 2, 'Entregue', '2025-06-15T14:00:00', 'Urgente - safra iniciando', '2025-06-15T14:00:00'),
('PV.2025.07.0001', 3, 'Entregue', '2025-07-02T09:00:00', NULL, '2025-07-02T09:00:00'),
('PV.2025.08.0001', 4, 'Concluido', '2025-08-10T11:00:00', 'Entrega parcial autorizada', '2025-08-10T11:00:00'),
('PV.2025.09.0001', 5, 'Concluido', '2025-09-05T08:00:00', NULL, '2025-09-05T08:00:00'),
('PV.2025.10.0001', 6, 'EmProducao', '2025-10-12T10:00:00', 'Cliente pediu reforço na estrutura', '2025-10-12T10:00:00'),
('PV.2025.10.0002', 7, 'EmProducao', '2025-10-20T15:00:00', NULL, '2025-10-20T15:00:00'),
('PV.2025.11.0001', 8, 'Aprovado', '2025-11-05T09:00:00', 'Aguardando liberação financeira', '2025-11-05T09:00:00'),
('PV.2025.11.0002', 9, 'Aprovado', '2025-11-18T14:00:00', NULL, '2025-11-18T14:00:00'),
('PV.2025.12.0001', 10, 'Aprovado', '2025-12-03T10:00:00', 'Previsão entrega março/2026', '2025-12-03T10:00:00'),
('PV.2026.01.0001', 11, 'Aprovado', '2026-01-08T08:00:00', NULL, '2026-01-08T08:00:00'),
('PV.2026.01.0002', 12, 'Orcamento', '2026-01-15T11:00:00', 'Aguardando aprovação diretoria', '2026-01-15T11:00:00'),
('PV.2026.01.0003', 13, 'Orcamento', '2026-01-22T16:00:00', NULL, '2026-01-22T16:00:00'),
('PV.2026.02.0001', 14, 'Orcamento', '2026-02-01T09:00:00', 'Primeira compra do cliente', '2026-02-01T09:00:00'),
('PV.2026.02.0002', 15, 'Orcamento', '2026-02-10T10:00:00', NULL, '2026-02-10T10:00:00'),
('PV.2026.02.0003', 1, 'Orcamento', '2026-02-15T14:00:00', 'Segundo pedido Metalúrgica São José', '2026-02-15T14:00:00'),
('PV.2026.02.0004', 2, 'Cancelado', '2026-02-18T08:00:00', 'Cliente desistiu', '2026-02-18T08:00:00'),
('PV.2025.07.0002', 1, 'Entregue', '2025-07-20T10:00:00', NULL, '2025-07-20T10:00:00'),
('PV.2025.08.0002', 5, 'Concluido', '2025-08-25T09:00:00', NULL, '2025-08-25T09:00:00'),
('PV.2025.09.0002', 3, 'EmProducao', '2025-09-18T13:00:00', 'Reforço extra solicitado', '2025-09-18T13:00:00');

-- =============================================
-- COMERCIAL: ITENS DOS PEDIDOS
-- =============================================
-- PV.2025.06.0001 (id=1) - Metalúrgica São José
INSERT INTO Comercial_PedidosVendaItens (PedidoVendaId, ProdutoId, Quantidade, PrecoUnitario, CriadoEm) VALUES
(1, 1, 1, 185000.00, '2025-06-10T10:00:00'),
(1, 61, 1, 45000.00, '2025-06-10T10:00:00');

-- PV.2025.06.0002 (id=2) - Usina Santa Maria
INSERT INTO Comercial_PedidosVendaItens (PedidoVendaId, ProdutoId, Quantidade, PrecoUnitario, CriadoEm) VALUES
(2, 2, 2, 220000.00, '2025-06-15T14:00:00'),
(2, 62, 2, 65000.00, '2025-06-15T14:00:00'),
(2, 16, 2, 8500.00, '2025-06-15T14:00:00');

-- PV.2025.07.0001 (id=3) - Fazenda Boa Esperança
INSERT INTO Comercial_PedidosVendaItens (PedidoVendaId, ProdutoId, Quantidade, PrecoUnitario, CriadoEm) VALUES
(3, 1, 1, 185000.00, '2025-07-02T09:00:00');

-- PV.2025.08.0001 (id=4) - Coop. Agrícola do Norte
INSERT INTO Comercial_PedidosVendaItens (PedidoVendaId, ProdutoId, Quantidade, PrecoUnitario, CriadoEm) VALUES
(4, 3, 1, 295000.00, '2025-08-10T11:00:00'),
(4, 62, 1, 65000.00, '2025-08-10T11:00:00');

-- PV.2025.09.0001 (id=5) - Agropecuária Três Irmãos
INSERT INTO Comercial_PedidosVendaItens (PedidoVendaId, ProdutoId, Quantidade, PrecoUnitario, CriadoEm) VALUES
(5, 2, 1, 220000.00, '2025-09-05T08:00:00'),
(5, 61, 2, 45000.00, '2025-09-05T08:00:00');

-- PV.2025.10.0001 (id=6) - Biomassa Sul
INSERT INTO Comercial_PedidosVendaItens (PedidoVendaId, ProdutoId, Quantidade, PrecoUnitario, CriadoEm) VALUES
(6, 3, 2, 295000.00, '2025-10-12T10:00:00'),
(6, 62, 2, 65000.00, '2025-10-12T10:00:00'),
(6, 17, 4, 12000.00, '2025-10-12T10:00:00');

-- PV.2025.10.0002 (id=7) - Madeireira Progresso
INSERT INTO Comercial_PedidosVendaItens (PedidoVendaId, ProdutoId, Quantidade, PrecoUnitario, CriadoEm) VALUES
(7, 1, 1, 185000.00, '2025-10-20T15:00:00'),
(7, 61, 1, 45000.00, '2025-10-20T15:00:00');

-- PV.2025.11.0001 (id=8) - Engenho Central
INSERT INTO Comercial_PedidosVendaItens (PedidoVendaId, ProdutoId, Quantidade, PrecoUnitario, CriadoEm) VALUES
(8, 2, 1, 220000.00, '2025-11-05T09:00:00');

-- PV.2025.11.0002 (id=9) - Celulose Planalto
INSERT INTO Comercial_PedidosVendaItens (PedidoVendaId, ProdutoId, Quantidade, PrecoUnitario, CriadoEm) VALUES
(9, 3, 1, 295000.00, '2025-11-18T14:00:00'),
(9, 62, 3, 65000.00, '2025-11-18T14:00:00');

-- PV.2025.12.0001 (id=10) - Campo Verde
INSERT INTO Comercial_PedidosVendaItens (PedidoVendaId, ProdutoId, Quantidade, PrecoUnitario, CriadoEm) VALUES
(10, 2, 2, 220000.00, '2025-12-03T10:00:00'),
(10, 61, 2, 45000.00, '2025-12-03T10:00:00');

-- PV.2026.01.0001 (id=11) - Agroindústria Cerrado
INSERT INTO Comercial_PedidosVendaItens (PedidoVendaId, ProdutoId, Quantidade, PrecoUnitario, CriadoEm) VALUES
(11, 1, 1, 190000.00, '2026-01-08T08:00:00'),
(11, 16, 1, 8500.00, '2026-01-08T08:00:00');

-- PV.2026.01.0002 (id=12) - Florestal Amazônia
INSERT INTO Comercial_PedidosVendaItens (PedidoVendaId, ProdutoId, Quantidade, PrecoUnitario, CriadoEm) VALUES
(12, 3, 1, 300000.00, '2026-01-15T11:00:00'),
(12, 62, 1, 68000.00, '2026-01-15T11:00:00');

-- PV.2026.01.0003 (id=13) - Sítio São Pedro
INSERT INTO Comercial_PedidosVendaItens (PedidoVendaId, ProdutoId, Quantidade, PrecoUnitario, CriadoEm) VALUES
(13, 1, 1, 190000.00, '2026-01-22T16:00:00');

-- PV.2026.02.0001 (id=14) - Fazenda Água Branca
INSERT INTO Comercial_PedidosVendaItens (PedidoVendaId, ProdutoId, Quantidade, PrecoUnitario, CriadoEm) VALUES
(14, 2, 1, 225000.00, '2026-02-01T09:00:00'),
(14, 61, 1, 48000.00, '2026-02-01T09:00:00');

-- PV.2026.02.0002 (id=15) - Reciclagem Verde
INSERT INTO Comercial_PedidosVendaItens (PedidoVendaId, ProdutoId, Quantidade, PrecoUnitario, CriadoEm) VALUES
(15, 1, 2, 190000.00, '2026-02-10T10:00:00'),
(15, 62, 2, 68000.00, '2026-02-10T10:00:00');

-- PV.2026.02.0003 (id=16) - Metalúrgica São José (2º pedido)
INSERT INTO Comercial_PedidosVendaItens (PedidoVendaId, ProdutoId, Quantidade, PrecoUnitario, CriadoEm) VALUES
(16, 3, 1, 300000.00, '2026-02-15T14:00:00');

-- PV.2026.02.0004 (id=17) - Cancelado, mas tinha itens
INSERT INTO Comercial_PedidosVendaItens (PedidoVendaId, ProdutoId, Quantidade, PrecoUnitario, CriadoEm) VALUES
(17, 1, 1, 190000.00, '2026-02-18T08:00:00');

-- PV.2025.07.0002 (id=18) - Metalúrgica São José (outro)
INSERT INTO Comercial_PedidosVendaItens (PedidoVendaId, ProdutoId, Quantidade, PrecoUnitario, CriadoEm) VALUES
(18, 2, 1, 220000.00, '2025-07-20T10:00:00'),
(18, 17, 2, 12000.00, '2025-07-20T10:00:00');

-- PV.2025.08.0002 (id=19) - Agropecuária Três Irmãos
INSERT INTO Comercial_PedidosVendaItens (PedidoVendaId, ProdutoId, Quantidade, PrecoUnitario, CriadoEm) VALUES
(19, 1, 1, 185000.00, '2025-08-25T09:00:00');

-- PV.2025.09.0002 (id=20) - Fazenda Boa Esperança
INSERT INTO Comercial_PedidosVendaItens (PedidoVendaId, ProdutoId, Quantidade, PrecoUnitario, CriadoEm) VALUES
(20, 2, 1, 225000.00, '2025-09-18T13:00:00'),
(20, 61, 1, 45000.00, '2025-09-18T13:00:00');

-- =============================================
-- COMERCIAL: NÚMEROS DE SÉRIE
-- (somente para pedidos aprovados ou acima)
-- Idade empresa 2025: 59, 2026: 60
-- =============================================
INSERT INTO Comercial_NumerosSerie (Codigo, PedidoVendaId, Status, CriadoEm) VALUES
('59.06.25.00001', 1, 'Entregue', '2025-06-10T12:00:00'),
('59.06.25.00002', 2, 'Entregue', '2025-06-15T16:00:00'),
('59.07.25.00001', 3, 'Entregue', '2025-07-02T11:00:00'),
('59.08.25.00001', 4, 'Concluido', '2025-08-10T13:00:00'),
('59.09.25.00001', 5, 'Concluido', '2025-09-05T10:00:00'),
('59.10.25.00001', 6, 'EmFabricacao', '2025-10-12T12:00:00'),
('59.10.25.00002', 7, 'EmFabricacao', '2025-10-20T17:00:00'),
('59.11.25.00001', 8, 'Aberto', '2025-11-05T11:00:00'),
('59.11.25.00002', 9, 'Aberto', '2025-11-18T16:00:00'),
('59.12.25.00001', 10, 'Aberto', '2025-12-03T12:00:00'),
('60.01.26.00001', 11, 'Aberto', '2026-01-08T10:00:00'),
('59.07.25.00002', 18, 'Entregue', '2025-07-20T12:00:00'),
('59.08.25.00002', 19, 'Concluido', '2025-08-25T11:00:00'),
('59.09.25.00002', 20, 'EmFabricacao', '2025-09-18T15:00:00');