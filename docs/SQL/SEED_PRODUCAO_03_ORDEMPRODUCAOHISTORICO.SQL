-- ============================================================================
--  PRODUCAO 03 — HISTORICO DE EVENTOS DAS ORDENS DE PRODUCAO
-- ----------------------------------------------------------------------------
--  Tabela: Producao_OrdemProducaoHistorico
--  Campos NOT NULL: OrdemProducaoId, Evento, DataHora, CriadoEm
--  Campos opcionais: StatusAnterior, StatusNovo, Justificativa, Detalhe, ModificadoEm
--
--  FK: OrdemProducaoId -> Producao_OrdensProducao.Id  (Cascade)
--
--  Eventos registrados (enum EventoOrdemProducao):
--    Criada       | Iniciada    | Pausada     | Retomada
--    Concluida    | Cancelada   | Apontamento
--
--  Regras aplicadas:
--    - Criada: StatusAnterior = NULL, StatusNovo = Pendente
--    - Iniciada: Pendente -> Andamento
--    - Concluida: Andamento -> Concluida
--    - Apontamento: StatusAnterior/StatusNovo = NULL, Detalhe livre
--    - Justificativa: obrigatoria em Pausada/Cancelada/retrocessos (nenhum caso aqui)
--
--  Ids das OPs no seed:
--    1 = OP.2026.02.0001         Master  (Andamento)
--    2 = OP.2026.02.0001/0001    Filha   (Concluida)
--    3 = OP.2026.02.0001/0002    Filha   (Andamento parcial)
--    4 = OP.2026.03.0001         Master  (Pendente)
--    5 = OP.2026.04.0001         Master  (Concluida estoque)
-- ============================================================================

INSERT INTO Producao_OrdemProducaoHistorico
  (OrdemProducaoId, Evento, StatusAnterior, StatusNovo, Justificativa, Detalhe, DataHora, CriadoEm) VALUES

-- ---- OP 1: Master PIC-500 (Pendente -> Andamento) ------------------------
(1, 'Criada',       NULL,        'Pendente',  NULL, NULL,                                    '2026-02-10T09:00:00', '2026-02-10T09:00:00'),
(1, 'Iniciada',     'Pendente',  'Andamento', NULL, NULL,                                    '2026-03-15T08:30:00', '2026-03-15T08:30:00'),
(1, 'Apontamento',  NULL,        NULL,        NULL, 'PRF-001 +8 un (fixacao lote 1)',        '2026-04-05T16:00:00', '2026-04-05T16:00:00'),
(1, 'Apontamento',  NULL,        NULL,        NULL, 'FAC-001 +2 un (facas lote inicial)',    '2026-04-10T15:00:00', '2026-04-10T15:00:00'),

-- ---- OP 2: Filha ROT-001 #1 (ciclo completo Concluida) -------------------
(2, 'Criada',       NULL,        'Pendente',  NULL, NULL,                                    '2026-02-10T09:05:00', '2026-02-10T09:05:00'),
(2, 'Iniciada',     'Pendente',  'Andamento', NULL, NULL,                                    '2026-03-15T08:30:00', '2026-03-15T08:30:00'),
(2, 'Apontamento',  NULL,        NULL,        NULL, 'EIX-001 +1 un',                         '2026-03-22T11:00:00', '2026-03-22T11:00:00'),
(2, 'Apontamento',  NULL,        NULL,        NULL, 'DSC-001 +1 un',                         '2026-03-28T14:30:00', '2026-03-28T14:30:00'),
(2, 'Apontamento',  NULL,        NULL,        NULL, 'FAC-001 +4 un (jogo completo)',         '2026-04-05T10:00:00', '2026-04-05T10:00:00'),
(2, 'Concluida',    'Andamento', 'Concluida', NULL, NULL,                                    '2026-04-05T16:00:00', '2026-04-05T16:00:00'),

-- ---- OP 3: Filha ROT-001 #2 (Andamento parcial) --------------------------
(3, 'Criada',       NULL,        'Pendente',  NULL, NULL,                                    '2026-02-10T09:10:00', '2026-02-10T09:10:00'),
(3, 'Iniciada',     'Pendente',  'Andamento', NULL, NULL,                                    '2026-04-06T08:00:00', '2026-04-06T08:00:00'),
(3, 'Apontamento',  NULL,        NULL,        NULL, 'EIX-001 +1 un',                         '2026-04-08T10:00:00', '2026-04-08T10:00:00'),
(3, 'Apontamento',  NULL,        NULL,        NULL, 'FAC-001 +2 un (parcial, faltam 2)',     '2026-04-10T14:00:00', '2026-04-10T14:00:00'),

-- ---- OP 4: Master PIC-700 (apenas Criada) --------------------------------
(4, 'Criada',       NULL,        'Pendente',  NULL, NULL,                                    '2026-03-14T10:00:00', '2026-03-14T10:00:00'),

-- ---- OP 5: Master PIC-900 estoque (ciclo completo) -----------------------
(5, 'Criada',       NULL,        'Pendente',  NULL, NULL,                                    '2026-03-28T14:00:00', '2026-03-28T14:00:00'),
(5, 'Iniciada',     'Pendente',  'Andamento', NULL, NULL,                                    '2026-04-01T08:00:00', '2026-04-01T08:00:00'),
(5, 'Apontamento',  NULL,        NULL,        NULL, 'EIX-003 +1, DSC-003 +1',                '2026-04-05T11:00:00', '2026-04-05T11:00:00'),
(5, 'Apontamento',  NULL,        NULL,        NULL, 'FAC-003 +8 un (jogo completo)',         '2026-04-10T15:00:00', '2026-04-10T15:00:00'),
(5, 'Apontamento',  NULL,        NULL,        NULL, 'MTR-003 +1, RLM-003 +2',                '2026-04-14T09:00:00', '2026-04-14T09:00:00'),
(5, 'Apontamento',  NULL,        NULL,        NULL, 'PRF-001 +16 un (fixacao final)',        '2026-04-17T16:00:00', '2026-04-17T16:00:00'),
(5, 'Concluida',    'Andamento', 'Concluida', NULL, NULL,                                    '2026-04-18T17:00:00', '2026-04-18T17:00:00');
