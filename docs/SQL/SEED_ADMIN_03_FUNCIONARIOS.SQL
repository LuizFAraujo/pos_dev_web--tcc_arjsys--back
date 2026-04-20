-- ============================================================================
--  ADMIN 03 — FUNCIONARIOS
-- ----------------------------------------------------------------------------
--  Tabela: Admin_Funcionarios
--  Campos NOT NULL: PessoaId, Usuario, SenhaHash, CriadoEm
--  FK (DeleteBehavior.Restrict): PessoaId -> Admin_Pessoas.Id
--  Unique: Usuario
--
--  Datas: cadastro do Funcionario logo apos a Pessoa.
--  Senha padrao para todos: "123"
--  Hash SHA256 de "123" = a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3
--
--  Dependencia: Admin_Pessoas Ids 16..25.
-- ============================================================================

INSERT INTO Admin_Funcionarios (PessoaId, Cargo, Setor, Usuario, SenhaHash, CriadoEm) VALUES
(16, 'Diretor',             'Diretoria',    'joao.silva',      'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', '2023-04-03T08:30:00'),
(17, 'Gerente Comercial',   'Comercial',    'maria.souza',     'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', '2023-04-10T08:30:00'),
(18, 'Engenheiro Mecanico', 'Engenharia',   'carlos.lima',     'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', '2023-05-02T08:30:00'),
(19, 'Analista PCP',        'PCP',          'ana.oliveira',    'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', '2023-06-19T08:30:00'),
(20, 'Comprador',           'Compras',      'roberto.santos',  'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', '2023-08-14T08:30:00'),
(21, 'Almoxarife',          'Almoxarifado', 'fernanda.costa',  'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', '2023-10-09T08:30:00'),
(22, 'Operador Producao',   'Producao',     'pedro.martins',   'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', '2024-02-12T08:30:00'),
(23, 'Auxiliar Producao',   'Producao',     'lucas.pereira',   'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', '2024-04-22T08:30:00'),
(24, 'Vendedora',           'Comercial',    'juliana.almeida', 'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', '2024-07-01T08:30:00'),
(25, 'Administrador TI',    'TI',           'marcos.rocha',    'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', '2024-09-16T08:30:00');
