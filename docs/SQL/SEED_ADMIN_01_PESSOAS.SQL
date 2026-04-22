-- ============================================================================
--  ADMIN 01 — PESSOAS (base)
-- ----------------------------------------------------------------------------
--  Tabela: Admin_Pessoas
--  Campos NOT NULL: Codigo, Nome, Tipo, Ativo, CriadoEm
--  Codigo: gerado por tipo (CLI-NNNN para Cliente, FUN-NNNN para Funcionario)
--
--  Datas: cadastros progressivos de mar/2023 a mar/2025.
--    - Clientes: cadastrados conforme iam fechando primeira venda
--    - Funcionarios: sequencia de contratacoes
--
--  Ordem dos Ids (apos TRUNCATE):
--    Ids 1..15  = Clientes  (CLI-0001 .. CLI-0015)
--    Ids 16..25 = Funcionarios (FUN-0001 .. FUN-0010)
-- ============================================================================


-- ---- Clientes (Ids 1..15) - 2023-2025 --------------------------------------
INSERT INTO Admin_Pessoas (Codigo, Nome, CpfCnpj, Telefone, Email, Endereco, Cidade, Estado, Cep, Tipo, Ativo, CriadoEm) VALUES
('CLI-0001', 'Metalurgica Sao Jose Ltda',      '12.345.678/0001-90', '(44) 3222-1100', 'contato@saojose.com.br',         'Rod. BR-376 Km 15',        'Maringa',             'PR', '87050-000', 'Cliente', 1, '2023-03-08T10:00:00'),
('CLI-0002', 'Usina Santa Maria S/A',          '23.456.789/0001-01', '(16) 3333-2200', 'compras@santamaria.com.br',      'Av. Industrial 500',       'Ribeirao Preto',      'SP', '14090-000', 'Cliente', 1, '2023-05-22T14:30:00'),
('CLI-0003', 'Fazenda Boa Esperanca',          '34.567.890/0001-12', '(67) 3344-5566', 'fazenda@boaesperanca.com.br',    'Estrada Rural s/n',        'Dourados',            'MS', '79800-000', 'Cliente', 1, '2023-07-14T09:15:00'),
('CLI-0004', 'Coop. Agricola do Norte',        '45.678.901/0001-23', '(44) 3255-7788', 'compras@coopnorte.com.br',       'Rua do Comercio 120',      'Londrina',            'PR', '86010-000', 'Cliente', 1, '2023-09-05T11:20:00'),
('CLI-0005', 'Agropecuaria Tres Irmaos',       '56.789.012/0001-34', '(17) 3266-8899', 'contato@tresirmaos.agr.br',      'Rod. SP-425 Km 80',        'Sao Jose do Rio Preto','SP', '15090-000', 'Cliente', 1, '2023-11-18T08:40:00'),
('CLI-0006', 'Biomassa Sul Ind. Com.',         '67.890.123/0001-45', '(41) 3277-9900', 'vendas@biomassasul.com.br',      'Rua das Industrias 45',    'Curitiba',            'PR', '81200-000', 'Cliente', 1, '2024-01-23T15:10:00'),
('CLI-0007', 'Madeireira Progresso',           '78.901.234/0001-56', '(46) 3288-1122', 'compras@progresso.com.br',       'Av. Brasil 890',           'Pato Branco',         'PR', '85500-000', 'Cliente', 1, '2024-03-12T10:30:00'),
('CLI-0008', 'Engenho Central Ltda',           '89.012.345/0001-67', '(19) 3299-2233', 'producao@engenhocentral.com.br', 'Rua da Usina 300',         'Piracicaba',          'SP', '13400-000', 'Cliente', 1, '2024-05-08T13:45:00'),
('CLI-0009', 'Celulose Planalto S/A',          '90.123.456/0001-78', '(42) 3300-3344', 'suprimentos@planalto.com.br',    'Rod. PR-151 Km 25',        'Ponta Grossa',        'PR', '84050-000', 'Cliente', 1, '2024-07-19T09:50:00'),
('CLI-0010', 'Ind. Maquinas Campo Verde',      '01.234.567/0001-89', '(65) 3311-4455', 'comercial@campoverde.ind.br',    'Distrito Industrial L.8',  'Rondonopolis',        'MT', '78700-000', 'Cliente', 1, '2024-09-04T14:00:00'),
('CLI-0011', 'Agroindustria Cerrado',          '11.222.333/0001-44', '(62) 3322-5566', 'compras@agrocerrado.com.br',     'Av. Goias 1500',           'Goiania',             'GO', '74000-000', 'Cliente', 1, '2024-10-27T11:15:00'),
('CLI-0012', 'Florestal Amazonia Ltda',        '22.333.444/0001-55', '(91) 3333-6677', 'contato@florestalamazonia.com.br','Rod. Augusto M. Km 12',   'Belem',               'PA', '66000-000', 'Cliente', 1, '2024-12-11T16:20:00'),
('CLI-0013', 'Sitio Sao Pedro',                 NULL,                '(44) 99901-1234', 'joao@email.com',                'Estrada Municipal s/n',   'Mandaguari',          'PR', '86975-000', 'Cliente', 1, '2025-01-15T08:30:00'),
('CLI-0014', 'Fazenda Agua Branca',            '33.444.555/0001-66', '(43) 3344-7788', 'admin@aguabranca.agr.br',        'Rod. PR-445 Km 30',        'Cambe',               'PR', '86180-000', 'Cliente', 1, '2025-02-26T10:45:00'),
('CLI-0015', 'Reciclagem Verde Ltda',          '44.555.666/0001-77', '(44) 3355-8899', 'orcamento@reciclagemverde.com.br','Rua Recicla 200',         'Maringa',             'PR', '87080-000', 'Cliente', 1, '2025-03-18T14:25:00');


-- ---- Funcionarios (Ids 16..25) - 2023-2024 ---------------------------------
INSERT INTO Admin_Pessoas (Codigo, Nome, CpfCnpj, Telefone, Email, Endereco, Cidade, Estado, Cep, Tipo, Ativo, CriadoEm) VALUES
('FUN-0001', 'Joao Carlos Silva',      '123.456.789-00', '(44) 99902-0001', 'joao.silva@email.com',      NULL, 'Maringa', 'PR', NULL, 'Funcionario', 1, '2023-04-03T08:00:00'),
('FUN-0002', 'Maria Fernanda Souza',   '234.567.890-11', '(44) 99902-0002', 'maria.souza@email.com',     NULL, 'Maringa', 'PR', NULL, 'Funcionario', 1, '2023-04-10T08:00:00'),
('FUN-0003', 'Carlos Eduardo Lima',    '345.678.901-22', '(44) 99902-0003', 'carlos.lima@email.com',     NULL, 'Maringa', 'PR', NULL, 'Funcionario', 1, '2023-05-02T08:00:00'),
('FUN-0004', 'Ana Paula Oliveira',     '456.789.012-33', '(44) 99902-0004', 'ana.oliveira@email.com',    NULL, 'Maringa', 'PR', NULL, 'Funcionario', 1, '2023-06-19T08:00:00'),
('FUN-0005', 'Roberto Santos Filho',   '567.890.123-44', '(44) 99902-0005', 'roberto.santos@email.com',  NULL, 'Maringa', 'PR', NULL, 'Funcionario', 1, '2023-08-14T08:00:00'),
('FUN-0006', 'Fernanda Costa',         '678.901.234-55', '(44) 99902-0006', 'fernanda.costa@email.com',  NULL, 'Maringa', 'PR', NULL, 'Funcionario', 1, '2023-10-09T08:00:00'),
('FUN-0007', 'Pedro Henrique Martins', '789.012.345-66', '(44) 99902-0007', 'pedro.martins@email.com',   NULL, 'Maringa', 'PR', NULL, 'Funcionario', 1, '2024-02-12T08:00:00'),
('FUN-0008', 'Lucas Gabriel Pereira',  '890.123.456-77', '(44) 99902-0008', 'lucas.pereira@email.com',   NULL, 'Maringa', 'PR', NULL, 'Funcionario', 1, '2024-04-22T08:00:00'),
('FUN-0009', 'Juliana Almeida',        '901.234.567-88', '(44) 99902-0009', 'juliana.almeida@email.com', NULL, 'Maringa', 'PR', NULL, 'Funcionario', 1, '2024-07-01T08:00:00'),
('FUN-0010', 'Marcos Vinicius Rocha',  '012.345.678-99', '(44) 99902-0010', 'marcos.rocha@email.com',    NULL, 'Maringa', 'PR', NULL, 'Funcionario', 1, '2024-09-16T08:00:00');
