-- ============================================================================
--  ADMIN 05 — NOTIFICACOES
-- ----------------------------------------------------------------------------
--  Tabela: Admin_Notificacoes
--  Campos NOT NULL: ModuloDestino, Tipo, Titulo, Mensagem, Lida, CriadoEm
--  OrigemTabela e OrigemId sao opcionais (rastreabilidade)
--
--  Mix de notificacoes lidas e nao lidas, atravessando todos os modulos.
-- ============================================================================


INSERT INTO Admin_Notificacoes (ModuloDestino, Tipo, Titulo, Mensagem, Lida, DataLeitura, OrigemTabela, OrigemId, CriadoEm, ModificadoEm) VALUES

-- Comercial (3) — mix
('Comercial', 'Info',    'PV 13 aguardando retorno',          'Proposta enviada ao cliente. Acompanhe o retorno.',                 0, NULL,                  'Comercial_PedidosVenda', 13, '2026-04-15T14:00:00', NULL),
('Comercial', 'Sucesso', 'PV 1 entregue',                     'Entrega do PV.2024.03.0001 confirmada.',                            1, '2024-05-21T09:15:00', 'Comercial_PedidosVenda', 1,  '2024-05-20T17:30:00', '2024-05-21T09:15:00'),
('Comercial', 'Aviso',   'PV 4 devolvido',                    'Cliente devolveu PV.2025.01.0001 por defeito no rotor.',            0, NULL,                  'Comercial_PedidosVenda', 4,  '2025-04-10T10:00:00', NULL),

-- Producao (3)
('Producao',  'Aviso',   'PV 12 liberado para produção',      'Novo PV liberado: PV.2026.04.0001. Programar OP.',                  0, NULL,                  'Comercial_PedidosVenda', 12, '2026-04-02T15:30:00', NULL),
('Producao',  'Info',    'OP em andamento',                   'PV 10 em produção desde 15/03.',                                    1, '2026-03-16T08:00:00', 'Comercial_PedidosVenda', 10, '2026-03-15T08:30:00', '2026-03-16T08:00:00'),
('Producao',  'Erro',    'Atraso detectado',                  'PV 9 pausado há mais de 45 dias.',                                  0, NULL,                  'Comercial_PedidosVenda', 9,  '2026-04-14T10:00:00', NULL),

-- Engenharia (2)
('Engenharia','Aviso',   'PV 15 aguarda geração de NS',       'PV.2026.04.0004 aguardando N.Série ha mais de 24h.',                0, NULL,                  'Comercial_PedidosVenda', 15, '2026-04-20T09:00:00', NULL),
('Engenharia','Info',    'Novo PV PreVenda',                  'PV.2026.04.0003 aguarda análise técnica.',                          1, '2026-04-18T17:00:00', 'Comercial_PedidosVenda', 14, '2026-04-14T09:00:00', '2026-04-18T17:00:00'),

-- Compras (1)
('Compras',   'Info',    'Lista de compras para PV 11',       'Itens da OP do PV.2026.03.0001 disponíveis para cotação.',          0, NULL,                  'Comercial_PedidosVenda', 11, '2026-04-05T10:15:00', NULL),

-- Almoxarifado (1)
('Almoxarifado','Aviso', 'Material reservado',                'Material da OP do PV 8 separado e aguardando expedição.',           0, NULL,                  'Comercial_PedidosVenda', 8,  '2026-03-20T17:00:00', NULL),

-- Admin (2) — sem origem (sistema)
('Admin',     'Info',    'Backup diário concluído',           'Backup do banco realizado com sucesso.',                            1, '2026-04-20T06:05:00', NULL,                     NULL,'2026-04-20T06:00:00', '2026-04-20T06:05:00'),
('Admin',     'Erro',    'Falha no agendamento',              'Tarefa programada "limpeza-logs" falhou na última execução.',       0, NULL,                  NULL,                     NULL,'2026-04-19T23:55:00', NULL);
