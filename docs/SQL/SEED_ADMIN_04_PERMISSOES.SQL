-- ============================================================================
--  ADMIN 04 — PERMISSOES
-- ----------------------------------------------------------------------------
--  Tabela: Admin_Permissoes
--  Campos NOT NULL: FuncionarioId, Modulo, Nivel, CriadoEm
--  FK (DeleteBehavior.Cascade): FuncionarioId -> Admin_Funcionarios.Id
--  Unique: (FuncionarioId, Modulo)
--
--  Datas: permissoes configuradas no mesmo dia da contratacao, poucos minutos
--  apos o cadastro do funcionario.
-- ============================================================================


-- ---- joao.silva (Id=1) - Diretor = Admin em tudo (2023-04-03) --------------
INSERT INTO Admin_Permissoes (FuncionarioId, Modulo, Nivel, CriadoEm) VALUES
(1, 'Engenharia',    'Admin', '2023-04-03T09:00:00'),
(1, 'Comercial',     'Admin', '2023-04-03T09:01:00'),
(1, 'Producao',      'Admin', '2023-04-03T09:02:00'),
(1, 'Compras',       'Admin', '2023-04-03T09:03:00'),
(1, 'Almoxarifado',  'Admin', '2023-04-03T09:04:00'),
(1, 'Admin',         'Admin', '2023-04-03T09:05:00');


-- ---- maria.souza (Id=2) - Gerente Comercial (2023-04-10) -------------------
INSERT INTO Admin_Permissoes (FuncionarioId, Modulo, Nivel, CriadoEm) VALUES
(2, 'Comercial',     'LeituraEscrita', '2023-04-10T09:00:00'),
(2, 'Engenharia',    'Leitura',        '2023-04-10T09:01:00'),
(2, 'Producao',      'Leitura',        '2023-04-10T09:02:00'),
(2, 'Almoxarifado',  'Leitura',        '2023-04-10T09:03:00');


-- ---- carlos.lima (Id=3) - Engenheiro (2023-05-02) --------------------------
INSERT INTO Admin_Permissoes (FuncionarioId, Modulo, Nivel, CriadoEm) VALUES
(3, 'Engenharia',    'LeituraEscrita', '2023-05-02T09:00:00'),
(3, 'Producao',      'Leitura',        '2023-05-02T09:01:00');


-- ---- ana.oliveira (Id=4) - Analista de Producao (2023-06-19) ---------------
INSERT INTO Admin_Permissoes (FuncionarioId, Modulo, Nivel, CriadoEm) VALUES
(4, 'Producao',      'LeituraEscrita', '2023-06-19T09:00:00'),
(4, 'Engenharia',    'Leitura',        '2023-06-19T09:01:00'),
(4, 'Compras',       'Leitura',        '2023-06-19T09:02:00'),
(4, 'Almoxarifado',  'Leitura',        '2023-06-19T09:03:00');


-- ---- roberto.santos (Id=5) - Comprador (2023-08-14) ------------------------
INSERT INTO Admin_Permissoes (FuncionarioId, Modulo, Nivel, CriadoEm) VALUES
(5, 'Compras',       'LeituraEscrita', '2023-08-14T09:00:00'),
(5, 'Almoxarifado',  'Leitura',        '2023-08-14T09:01:00'),
(5, 'Producao',      'Leitura',        '2023-08-14T09:02:00');


-- ---- fernanda.costa (Id=6) - Almoxarife (2023-10-09) -----------------------
INSERT INTO Admin_Permissoes (FuncionarioId, Modulo, Nivel, CriadoEm) VALUES
(6, 'Almoxarifado',  'LeituraEscrita', '2023-10-09T09:00:00');


-- ---- pedro.martins (Id=7) - Operador Producao (2024-02-12) -----------------
INSERT INTO Admin_Permissoes (FuncionarioId, Modulo, Nivel, CriadoEm) VALUES
(7, 'Producao',      'Leitura', '2024-02-12T09:00:00'),
(7, 'Engenharia',    'Leitura', '2024-02-12T09:01:00');


-- ---- lucas.pereira (Id=8) - Auxiliar Producao (2024-04-22) -----------------
INSERT INTO Admin_Permissoes (FuncionarioId, Modulo, Nivel, CriadoEm) VALUES
(8, 'Producao',      'Leitura', '2024-04-22T09:00:00');


-- ---- juliana.almeida (Id=9) - Vendedora (2024-07-01) -----------------------
INSERT INTO Admin_Permissoes (FuncionarioId, Modulo, Nivel, CriadoEm) VALUES
(9, 'Comercial',     'LeituraEscrita', '2024-07-01T09:00:00'),
(9, 'Almoxarifado',  'Leitura',        '2024-07-01T09:01:00');


-- ---- marcos.rocha (Id=10) - TI = Admin em tudo (2024-09-16) ----------------
INSERT INTO Admin_Permissoes (FuncionarioId, Modulo, Nivel, CriadoEm) VALUES
(10, 'Engenharia',    'Admin', '2024-09-16T09:00:00'),
(10, 'Comercial',     'Admin', '2024-09-16T09:01:00'),
(10, 'Producao',      'Admin', '2024-09-16T09:02:00'),
(10, 'Compras',       'Admin', '2024-09-16T09:03:00'),
(10, 'Almoxarifado',  'Admin', '2024-09-16T09:04:00'),
(10, 'Admin',         'Admin', '2024-09-16T09:05:00');
