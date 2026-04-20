-- ============================================================================
--  ENGENHARIA 02 — GRUPOS DE PRODUTOS
-- ----------------------------------------------------------------------------
--  Tabela: Engenharia_GruposProdutos
--  Campos NOT NULL: Codigo, Descricao, Nivel, QtdCaracteres, Ativo, CriadoEm
--  Campos opcionais: PathDocumentos, ModificadoEm, CriadoPor, ModificadoPor
--
--  Datas: cadastro inicial dos grupos em jan/2023 (logo apos as Configuracoes).
--  Variacao de minutos entre as 3 colunas para simular sequencia de trabalho.
--
--  Ids gerados (apos TRUNCATE):
--    Coluna1: Ids 1..10   (10 itens)
--    Coluna2: Ids 11..35  (25 itens)
--    Coluna3: Ids 36..50  (15 itens)
-- ============================================================================


-- ---- Coluna 1 (Ids 1..10) - cadastrados em 16/01/2023 pela manha ----------
INSERT INTO Engenharia_GruposProdutos (Codigo, Descricao, Nivel, QtdCaracteres, PathDocumentos, Ativo, CriadoEm) VALUES
('10', 'Produto Final',    'Coluna1', 2, NULL, 1, '2023-01-16T09:00:00'),
('20', 'Conjunto Montado', 'Coluna1', 2, NULL, 1, '2023-01-16T09:05:00'),
('30', 'Peca Fabricada',   'Coluna1', 2, NULL, 1, '2023-01-16T09:10:00'),
('40', 'Peca Usinada',     'Coluna1', 2, NULL, 1, '2023-01-16T09:15:00'),
('50', 'Comprado',         'Coluna1', 2, NULL, 1, '2023-01-16T09:20:00'),
('60', 'Servico',          'Coluna1', 2, NULL, 1, '2023-01-16T09:25:00'),
('70', 'Materia Prima',    'Coluna1', 2, NULL, 1, '2023-01-16T09:30:00'),
('80', 'Consumivel',       'Coluna1', 2, NULL, 1, '2023-01-16T09:35:00'),
('90', 'Embalagem',        'Coluna1', 2, NULL, 1, '2023-01-16T09:40:00'),
('99', 'Diversos',         'Coluna1', 2, NULL, 1, '2023-01-16T09:45:00');


-- ---- Coluna 2 (Ids 11..35) - cadastrados em 16/01/2023 a tarde ------------
INSERT INTO Engenharia_GruposProdutos (Codigo, Descricao, Nivel, QtdCaracteres, PathDocumentos, Ativo, CriadoEm) VALUES
('PCP', 'Picador Completo',   'Coluna2', 3, NULL, 1, '2023-01-16T14:00:00'),
('EST', 'Esteira',             'Coluna2', 3, NULL, 1, '2023-01-16T14:03:00'),
('SGE', 'Subconjunto Geral',   'Coluna2', 3, NULL, 1, '2023-01-16T14:06:00'),
('MEC', 'Mecanica',            'Coluna2', 3, NULL, 1, '2023-01-16T14:09:00'),
('ELT', 'Eletrica',            'Coluna2', 3, NULL, 1, '2023-01-16T14:12:00'),
('HID', 'Hidraulica',          'Coluna2', 3, NULL, 1, '2023-01-16T14:15:00'),
('PNE', 'Pneumatica',          'Coluna2', 3, NULL, 1, '2023-01-16T14:18:00'),
('FIX', 'Fixacao',             'Coluna2', 3, NULL, 1, '2023-01-16T14:21:00'),
('VED', 'Vedacao',             'Coluna2', 3, NULL, 1, '2023-01-16T14:24:00'),
('ROL', 'Rolamentos',          'Coluna2', 3, NULL, 1, '2023-01-16T14:27:00'),
('MOT', 'Motores',             'Coluna2', 3, NULL, 1, '2023-01-16T14:30:00'),
('RED', 'Redutores',           'Coluna2', 3, NULL, 1, '2023-01-16T14:33:00'),
('TRN', 'Transmissao',         'Coluna2', 3, NULL, 1, '2023-01-16T14:36:00'),
('CHP', 'Chapas',              'Coluna2', 3, NULL, 1, '2023-01-16T14:39:00'),
('TUB', 'Tubos',               'Coluna2', 3, NULL, 1, '2023-01-16T14:42:00'),
('BRR', 'Barras',              'Coluna2', 3, NULL, 1, '2023-01-16T14:45:00'),
('CNL', 'Cantoneiras',         'Coluna2', 3, NULL, 1, '2023-01-16T14:48:00'),
('PFL', 'Perfis Laminados',    'Coluna2', 3, NULL, 1, '2023-01-16T14:51:00'),
('SOL', 'Soldagem',            'Coluna2', 3, NULL, 1, '2023-01-16T14:54:00'),
('PIN', 'Pintura',             'Coluna2', 3, NULL, 1, '2023-01-16T14:57:00'),
('ABR', 'Abrasivos',           'Coluna2', 3, NULL, 1, '2023-01-16T15:00:00'),
('FER', 'Ferramentas',         'Coluna2', 3, NULL, 1, '2023-01-16T15:03:00'),
('EPI', 'EPIs',                'Coluna2', 3, NULL, 1, '2023-01-16T15:06:00'),
('CXP', 'Caixa Papelao',       'Coluna2', 3, NULL, 1, '2023-01-16T15:09:00'),
('PLT', 'Pallets',             'Coluna2', 3, NULL, 1, '2023-01-16T15:12:00');


-- ---- Coluna 3 (Ids 36..50) - cadastrados em 17/01/2023 -------------------
INSERT INTO Engenharia_GruposProdutos (Codigo, Descricao, Nivel, QtdCaracteres, PathDocumentos, Ativo, CriadoEm) VALUES
('GW10', 'Modelo GW10', 'Coluna3', 4, NULL, 1, '2023-01-17T08:30:00'),
('GW20', 'Modelo GW20', 'Coluna3', 4, NULL, 1, '2023-01-17T08:33:00'),
('BM20', 'Modelo BM20', 'Coluna3', 4, NULL, 1, '2023-01-17T08:36:00'),
('BM30', 'Modelo BM30', 'Coluna3', 4, NULL, 1, '2023-01-17T08:39:00'),
('1020', 'SAE 1020',    'Coluna3', 4, NULL, 1, '2023-01-17T08:42:00'),
('1045', 'SAE 1045',    'Coluna3', 4, NULL, 1, '2023-01-17T08:45:00'),
('4140', 'SAE 4140',    'Coluna3', 4, NULL, 1, '2023-01-17T08:48:00'),
('INOX', 'Inox 304',    'Coluna3', 4, NULL, 1, '2023-01-17T08:51:00'),
('SC40', 'SCH 40',      'Coluna3', 4, NULL, 1, '2023-01-17T08:54:00'),
('SC80', 'SCH 80',      'Coluna3', 4, NULL, 1, '2023-01-17T08:57:00'),
('WEG0', 'WEG',         'Coluna3', 4, NULL, 1, '2023-01-17T09:00:00'),
('SKF0', 'SKF',         'Coluna3', 4, NULL, 1, '2023-01-17T09:03:00'),
('NSK0', 'NSK',         'Coluna3', 4, NULL, 1, '2023-01-17T09:06:00'),
('3M00', '3M',          'Coluna3', 4, NULL, 1, '2023-01-17T09:09:00'),
('GNRC', 'Generico',    'Coluna3', 4, NULL, 1, '2023-01-17T09:12:00');
