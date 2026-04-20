-- ============================================================================
--  ADMIN 02 — CLIENTES
-- ----------------------------------------------------------------------------
--  Tabela: Admin_Clientes
--  Campos NOT NULL: PessoaId, CriadoEm
--  FK (DeleteBehavior.Restrict): PessoaId -> Admin_Pessoas.Id
--
--  Datas: cadastro do Cliente logo apos a Pessoa (poucos minutos depois).
--  Dependencia: Admin_Pessoas Ids 1..15.
-- ============================================================================

INSERT INTO Admin_Clientes (PessoaId, RazaoSocial, InscricaoEstadual, ContatoComercial, CriadoEm) VALUES
(1,  'Metalurgica Sao Jose Ltda',         '901.23456-78', 'Paulo - (44) 99801-0001',    '2023-03-08T10:15:00'),
(2,  'Usina Santa Maria S/A',              '902.34567-89', 'Ricardo - (16) 99802-0002', '2023-05-22T14:40:00'),
(3,  'Fazenda Boa Esperanca Ltda',         '903.45678-90', 'Antonio - (67) 99803-0003', '2023-07-14T09:25:00'),
(4,  'Cooperativa Agricola do Norte',      '904.56789-01', 'Sandra - (44) 99804-0004',  '2023-09-05T11:30:00'),
(5,  'Agropecuaria Tres Irmaos S/A',       '905.67890-12', 'Marcelo - (17) 99805-0005', '2023-11-18T08:50:00'),
(6,  'Biomassa Sul Ind. e Com. Ltda',      '906.78901-23', 'Claudia - (41) 99806-0006', '2024-01-23T15:20:00'),
(7,  'Madeireira Progresso Ltda',          '907.89012-34', 'Jorge - (46) 99807-0007',   '2024-03-12T10:40:00'),
(8,  'Engenho Central Ltda',               '908.90123-45', 'Fabio - (19) 99808-0008',   '2024-05-08T13:55:00'),
(9,  'Celulose Planalto S/A',              '909.01234-56', 'Helena - (42) 99809-0009',  '2024-07-19T10:00:00'),
(10, 'Ind. de Maquinas Campo Verde Ltda',  '910.12345-67', 'Rodrigo - (65) 99810-0010', '2024-09-04T14:10:00'),
(11, 'Agroindustria Cerrado Ltda',         '911.23456-78', 'Patricia - (62) 99811-0011','2024-10-27T11:25:00'),
(12, 'Florestal Amazonia Ltda',            '912.34567-89', 'Leandro - (91) 99812-0012', '2024-12-11T16:30:00'),
(13, 'Joao Batista da Silva - ME',         NULL,           'Joao - (44) 99901-1234',    '2025-01-15T08:40:00'),
(14, 'Fazenda Agua Branca Ltda',           '914.56789-01', 'Daniela - (43) 99814-0014', '2025-02-26T10:55:00'),
(15, 'Reciclagem Verde Ltda',              '915.67890-12', 'Thiago - (44) 99815-0015',  '2025-03-18T14:35:00');
