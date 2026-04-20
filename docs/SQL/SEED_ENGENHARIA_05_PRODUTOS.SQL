-- ============================================================================
--  ENGENHARIA 05 — PRODUTOS
-- ----------------------------------------------------------------------------
--  Tabela: Engenharia_Produtos
--  Campos NOT NULL: Codigo, Descricao, Unidade, Tipo, Ativo, TemPasta,
--                   TemDocumento, CriadoEm
--  Campos opcionais: DescricaoCompleta, Peso, ModificadoEm, CriadoPor, ModificadoPor
--
--  Datas: cadastro progressivo de produtos entre 06/2023 e 12/2024.
--  Produtos basicos primeiro (materias-primas e componentes comprados),
--  depois pecas fabricadas, por ultimo conjuntos completos.
--
--  Ids apos INSERT (ordem aqui = ordem real do catalogo 1..70).
-- ============================================================================


-- ---- 2023-06: Materias-primas basicas (chapas, tubos, barras) --------------
INSERT INTO Engenharia_Produtos
  (Codigo, Descricao, DescricaoCompleta, Unidade, Tipo, Peso, Ativo, TemPasta, TemDocumento, CriadoEm) VALUES
('CHP-001', 'Chapa Aco SAE 1020 3/16',         NULL, 'KG', 'MateriaPrima', NULL,   1, 0, 0, '2023-06-05T08:30:00'),
('CHP-002', 'Chapa Aco SAE 1020 1/4',          NULL, 'KG', 'MateriaPrima', NULL,   1, 0, 0, '2023-06-05T08:35:00'),
('CHP-003', 'Chapa Aco SAE 1020 3/8',          NULL, 'KG', 'MateriaPrima', NULL,   1, 0, 0, '2023-06-05T08:40:00'),
('CHP-004', 'Chapa Aco SAE 1020 1/2',          NULL, 'KG', 'MateriaPrima', NULL,   1, 0, 0, '2023-06-05T08:45:00'),
('CHP-005', 'Chapa Aco SAE 1045 3/8',          NULL, 'KG', 'MateriaPrima', NULL,   1, 0, 0, '2023-06-05T09:00:00'),
('CHP-006', 'Chapa Aco SAE 1045 1/2',          NULL, 'KG', 'MateriaPrima', NULL,   1, 0, 0, '2023-06-05T09:05:00'),
('CHP-007', 'Chapa Aco SAE 1045 5/8',          NULL, 'KG', 'MateriaPrima', NULL,   1, 0, 0, '2023-06-05T09:10:00'),
('TBO-001', 'Tubo Aco 2 SCH40',                NULL, 'MT', 'MateriaPrima', NULL,   1, 0, 0, '2023-06-08T10:15:00'),
('TBO-002', 'Tubo Aco 3 SCH40',                NULL, 'MT', 'MateriaPrima', NULL,   1, 0, 0, '2023-06-08T10:20:00'),
('TBO-003', 'Tubo Aco 4 SCH40',                NULL, 'MT', 'MateriaPrima', NULL,   1, 0, 0, '2023-06-08T10:25:00'),
('TBO-004', 'Tubo Aco 6 SCH40',                NULL, 'MT', 'MateriaPrima', NULL,   1, 0, 0, '2023-06-08T10:30:00'),
('BRR-001', 'Barra Redonda SAE 1045 D50mm',    NULL, 'MT', 'MateriaPrima', NULL,   1, 0, 0, '2023-06-12T14:00:00'),
('BRR-002', 'Barra Redonda SAE 1045 D60mm',    NULL, 'MT', 'MateriaPrima', NULL,   1, 0, 0, '2023-06-12T14:05:00'),
('BRR-003', 'Barra Redonda SAE 1045 D70mm',    NULL, 'MT', 'MateriaPrima', NULL,   1, 0, 0, '2023-06-12T14:10:00'),
('CNL-001', 'Cantoneira 2x3/16',               NULL, 'MT', 'MateriaPrima', NULL,   1, 0, 0, '2023-06-12T14:20:00');


-- ---- 2023-08: Componentes comprados (parafusos, porcas, rolamentos) -------
INSERT INTO Engenharia_Produtos
  (Codigo, Descricao, DescricaoCompleta, Unidade, Tipo, Peso, Ativo, TemPasta, TemDocumento, CriadoEm) VALUES
('PRF-001', 'Parafuso Sextavado M10x30 8.8',   NULL, 'PC', 'Comprado',     0.030,  1, 0, 0, '2023-08-10T09:00:00'),
('PRF-002', 'Parafuso Sextavado M12x40 8.8',   NULL, 'PC', 'Comprado',     0.048,  1, 0, 0, '2023-08-10T09:05:00'),
('PRF-003', 'Parafuso Sextavado M16x50 8.8',   NULL, 'PC', 'Comprado',     0.115,  1, 0, 0, '2023-08-10T09:10:00'),
('PRF-004', 'Parafuso Sextavado M20x60 8.8',   NULL, 'PC', 'Comprado',     0.210,  1, 0, 0, '2023-08-10T09:15:00'),
('PRC-001', 'Porca Sextavada M10',             NULL, 'PC', 'Comprado',     0.015,  1, 0, 0, '2023-08-10T09:25:00'),
('PRC-002', 'Porca Sextavada M12',             NULL, 'PC', 'Comprado',     0.022,  1, 0, 0, '2023-08-10T09:30:00'),
('PRC-003', 'Porca Sextavada M16',             NULL, 'PC', 'Comprado',     0.052,  1, 0, 0, '2023-08-10T09:35:00'),
('PRC-004', 'Porca Sextavada M20',             NULL, 'PC', 'Comprado',     0.095,  1, 0, 0, '2023-08-10T09:40:00'),
('RLM-001', 'Rolamento 6310 ZZ',               NULL, 'UN', 'Comprado',     0.95,   1, 0, 0, '2023-08-15T10:00:00'),
('RLM-002', 'Rolamento 6312 2RS',              NULL, 'UN', 'Comprado',     1.35,   1, 0, 0, '2023-08-15T10:05:00'),
('RLM-003', 'Rolamento 6314 ZZ',               NULL, 'UN', 'Comprado',     2.10,   1, 0, 0, '2023-08-15T10:10:00'),
('RLM-004', 'Rolamento 22220 E',               NULL, 'UN', 'Comprado',     3.80,   1, 0, 0, '2023-08-15T10:15:00'),
('RET-001', 'Retentor 50x72x10',               NULL, 'UN', 'Comprado',     0.035,  1, 0, 0, '2023-08-15T10:25:00'),
('RET-002', 'Retentor 60x85x12',               NULL, 'UN', 'Comprado',     0.048,  1, 0, 0, '2023-08-15T10:30:00'),
('RET-003', 'Retentor 70x100x12',              NULL, 'UN', 'Comprado',     0.065,  1, 0, 0, '2023-08-15T10:35:00');


-- ---- 2023-10: Motores e transmissao ---------------------------------------
INSERT INTO Engenharia_Produtos
  (Codigo, Descricao, DescricaoCompleta, Unidade, Tipo, Peso, Ativo, TemPasta, TemDocumento, CriadoEm) VALUES
('MTR-001', 'Motor Eletrico 50CV',             NULL, 'UN', 'Comprado',   180.0,    1, 0, 0, '2023-10-02T08:00:00'),
('MTR-002', 'Motor Eletrico 75CV',             NULL, 'UN', 'Comprado',   250.0,    1, 0, 0, '2023-10-02T08:05:00'),
('MTR-003', 'Motor Eletrico 100CV',            NULL, 'UN', 'Comprado',   320.0,    1, 0, 0, '2023-10-02T08:10:00'),
('MTR-004', 'Motor Eletrico 25CV',             NULL, 'UN', 'Comprado',    95.0,    1, 0, 0, '2023-10-02T08:15:00'),
('PLI-001', 'Polia D200mm 4 canais',           NULL, 'UN', 'Comprado',     8.5,    1, 0, 0, '2023-10-10T14:00:00'),
('PLI-002', 'Polia D300mm 6 canais',           NULL, 'UN', 'Comprado',    14.0,    1, 0, 0, '2023-10-10T14:05:00'),
('COR-001', 'Correia B-85',                    NULL, 'UN', 'Comprado',     0.45,   1, 0, 0, '2023-10-10T14:15:00'),
('COR-002', 'Correia B-100',                   NULL, 'UN', 'Comprado',     0.55,   1, 0, 0, '2023-10-10T14:20:00'),
('COR-003', 'Correia C-120',                   NULL, 'UN', 'Comprado',     0.80,   1, 0, 0, '2023-10-10T14:25:00');


-- ---- 2024-01: Pecas fabricadas (eixos, discos, facas) ---------------------
INSERT INTO Engenharia_Produtos
  (Codigo, Descricao, DescricaoCompleta, Unidade, Tipo, Peso, Ativo, TemPasta, TemDocumento, CriadoEm) VALUES
('EIX-001', 'Eixo do Rotor D50x800mm',         NULL, 'UN', 'Fabricado',   12.5,    1, 0, 0, '2024-01-15T08:30:00'),
('EIX-002', 'Eixo do Rotor D60x1000mm',        NULL, 'UN', 'Fabricado',   18.3,    1, 0, 0, '2024-01-15T08:35:00'),
('EIX-003', 'Eixo do Rotor D70x1200mm',        NULL, 'UN', 'Fabricado',   25.0,    1, 0, 0, '2024-01-15T08:40:00'),
('DSC-001', 'Disco de Corte D600mm',           NULL, 'UN', 'Fabricado',   45.0,    1, 0, 0, '2024-01-18T10:00:00'),
('DSC-002', 'Disco de Corte D800mm',           NULL, 'UN', 'Fabricado',   68.0,    1, 0, 0, '2024-01-18T10:05:00'),
('DSC-003', 'Disco de Corte D1000mm',          NULL, 'UN', 'Fabricado',   95.0,    1, 0, 0, '2024-01-18T10:10:00'),
('FAC-001', 'Faca de Corte 200x80x15mm',       NULL, 'PC', 'Fabricado',    1.8,    1, 0, 0, '2024-01-25T14:30:00'),
('FAC-002', 'Faca de Corte 250x100x20mm',      NULL, 'PC', 'Fabricado',    3.2,    1, 0, 0, '2024-01-25T14:35:00'),
('FAC-003', 'Faca de Corte 300x120x25mm',      NULL, 'PC', 'Fabricado',    5.5,    1, 0, 0, '2024-01-25T14:40:00');


-- ---- 2024-03: Caixas de mancal, peneiras, calcos --------------------------
INSERT INTO Engenharia_Produtos
  (Codigo, Descricao, DescricaoCompleta, Unidade, Tipo, Peso, Ativo, TemPasta, TemDocumento, CriadoEm) VALUES
('CXM-001', 'Caixa de Mancal Completa D50',    NULL, 'UN', 'Fabricado',    8.5,    1, 0, 0, '2024-03-05T09:00:00'),
('CXM-002', 'Caixa de Mancal Completa D60',    NULL, 'UN', 'Fabricado',   12.0,    1, 0, 0, '2024-03-05T09:05:00'),
('PEN-001', 'Peneira Classificadora D800mm',   NULL, 'UN', 'Fabricado',   15.0,    1, 0, 0, '2024-03-12T10:30:00'),
('PEN-002', 'Peneira Classificadora D1000mm',  NULL, 'UN', 'Fabricado',   22.0,    1, 0, 0, '2024-03-12T10:35:00'),
('CLC-001', 'Calco de Regulagem 2mm',          NULL, 'PC', 'Fabricado',    0.12,   1, 0, 0, '2024-03-20T15:00:00'),
('CLC-002', 'Calco de Regulagem 3mm',          NULL, 'PC', 'Fabricado',    0.18,   1, 0, 0, '2024-03-20T15:05:00');


-- ---- 2024-05: Rotores ------------------------------------------------------
INSERT INTO Engenharia_Produtos
  (Codigo, Descricao, DescricaoCompleta, Unidade, Tipo, Peso, Ativo, TemPasta, TemDocumento, CriadoEm) VALUES
('ROT-001', 'Rotor do Picador 500',            NULL, 'UN', 'Fabricado',    NULL,   1, 0, 0, '2024-05-10T09:00:00'),
('ROT-002', 'Rotor do Picador 700',            NULL, 'UN', 'Fabricado',    NULL,   1, 0, 0, '2024-05-10T09:05:00'),
('ROT-003', 'Rotor do Picador 900',            NULL, 'UN', 'Fabricado',    NULL,   1, 0, 0, '2024-05-10T09:10:00');


-- ---- 2024-07: Suportes e tintas -------------------------------------------
INSERT INTO Engenharia_Produtos
  (Codigo, Descricao, DescricaoCompleta, Unidade, Tipo, Peso, Ativo, TemPasta, TemDocumento, CriadoEm) VALUES
('SUP-001', 'Suporte do Motor 50CV',           NULL, 'UN', 'Fabricado',    6.8,    1, 0, 0, '2024-07-02T10:00:00'),
('SUP-002', 'Suporte do Motor 75CV',           NULL, 'UN', 'Fabricado',    9.2,    1, 0, 0, '2024-07-02T10:05:00'),
('SUP-003', 'Suporte do Motor 100CV',          NULL, 'UN', 'Fabricado',   12.5,    1, 0, 0, '2024-07-02T10:10:00'),
('PTR-001', 'Tinta Primer Anticorrosivo',      NULL, 'LT', 'Comprado',     NULL,   1, 0, 0, '2024-07-15T14:00:00'),
('PTR-002', 'Tinta Esmalte Azul',              NULL, 'LT', 'Comprado',     NULL,   1, 0, 0, '2024-07-15T14:05:00'),
('PTR-003', 'Tinta Esmalte Verde',             NULL, 'LT', 'Comprado',     NULL,   1, 0, 0, '2024-07-15T14:10:00'),
('SOL-001', 'Arame de Solda MIG 1.0mm',        NULL, 'KG', 'Comprado',     NULL,   1, 0, 0, '2024-07-20T09:30:00'),
('SOL-002', 'Eletrodo 7018 3.25mm',            NULL, 'KG', 'Comprado',     NULL,   1, 0, 0, '2024-07-20T09:35:00');


-- ---- 2024-09: Conjuntos completos (Picador e Esteira) ---------------------
INSERT INTO Engenharia_Produtos
  (Codigo, Descricao, DescricaoCompleta, Unidade, Tipo, Peso, Ativo, TemPasta, TemDocumento, CriadoEm) VALUES
('PIC-001', 'Picador de Madeira PIC-500',      NULL, 'UN', 'Fabricado',    NULL,   1, 0, 0, '2024-09-10T08:00:00'),
('PIC-002', 'Picador de Madeira PIC-700',      NULL, 'UN', 'Fabricado',    NULL,   1, 0, 0, '2024-09-10T08:05:00'),
('PIC-003', 'Picador de Madeira PIC-900',      NULL, 'UN', 'Fabricado',    NULL,   1, 0, 0, '2024-09-10T08:10:00'),
('EST-001', 'Esteira Transportadora 3m',       NULL, 'UN', 'Fabricado',   85.0,    1, 0, 0, '2024-09-20T11:00:00'),
('EST-002', 'Esteira Transportadora 5m',       NULL, 'UN', 'Fabricado',  130.0,    1, 0, 0, '2024-09-20T11:05:00');
