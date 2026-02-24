-- Picador 500 (id=1) → 8 filhos
INSERT INTO Engenharia_EstruturasProdutos (ProdutoPaiId, ProdutoFilhoId, Quantidade, Posicao, Observacao, CriadoEm) VALUES
(1, 4, 1, 10, NULL, '2026-02-22T00:00:00'),
(1, 16, 1, 20, NULL, '2026-02-22T00:00:00'),
(1, 51, 1, 30, NULL, '2026-02-22T00:00:00'),
(1, 55, 1, 40, NULL, '2026-02-22T00:00:00'),
(1, 39, 8, 50, NULL, '2026-02-22T00:00:00'),
(1, 42, 8, 60, NULL, '2026-02-22T00:00:00'),
(1, 63, 2, 70, NULL, '2026-02-22T00:00:00'),
(1, 58, 5, 80, 'Primer + acabamento', '2026-02-22T00:00:00');

-- Picador 700 (id=2) → 8 filhos
INSERT INTO Engenharia_EstruturasProdutos (ProdutoPaiId, ProdutoFilhoId, Quantidade, Posicao, Observacao, CriadoEm) VALUES
(2, 5, 1, 10, NULL, '2026-02-22T00:00:00'),
(2, 17, 1, 20, NULL, '2026-02-22T00:00:00'),
(2, 52, 1, 30, NULL, '2026-02-22T00:00:00'),
(2, 56, 1, 40, NULL, '2026-02-22T00:00:00'),
(2, 40, 12, 50, NULL, '2026-02-22T00:00:00'),
(2, 43, 12, 60, NULL, '2026-02-22T00:00:00'),
(2, 64, 3, 70, NULL, '2026-02-22T00:00:00'),
(2, 58, 8, 80, 'Primer + acabamento', '2026-02-22T00:00:00');

-- Picador 900 (id=3) → 8 filhos
INSERT INTO Engenharia_EstruturasProdutos (ProdutoPaiId, ProdutoFilhoId, Quantidade, Posicao, Observacao, CriadoEm) VALUES
(3, 6, 1, 10, NULL, '2026-02-22T00:00:00'),
(3, 18, 1, 20, NULL, '2026-02-22T00:00:00'),
(3, 52, 1, 30, NULL, '2026-02-22T00:00:00'),
(3, 57, 1, 40, NULL, '2026-02-22T00:00:00'),
(3, 41, 16, 50, NULL, '2026-02-22T00:00:00'),
(3, 44, 16, 60, NULL, '2026-02-22T00:00:00'),
(3, 65, 4, 70, NULL, '2026-02-22T00:00:00'),
(3, 59, 10, 80, NULL, '2026-02-22T00:00:00');

-- Rotor 500 (id=4) → 5 filhos
INSERT INTO Engenharia_EstruturasProdutos (ProdutoPaiId, ProdutoFilhoId, Quantidade, Posicao, Observacao, CriadoEm) VALUES
(4, 7, 1, 10, NULL, '2026-02-22T00:00:00'),
(4, 10, 1, 20, NULL, '2026-02-22T00:00:00'),
(4, 13, 6, 30, NULL, '2026-02-22T00:00:00'),
(4, 20, 2, 40, NULL, '2026-02-22T00:00:00'),
(4, 46, 2, 50, NULL, '2026-02-22T00:00:00');

-- Rotor 700 (id=5) → 5 filhos
INSERT INTO Engenharia_EstruturasProdutos (ProdutoPaiId, ProdutoFilhoId, Quantidade, Posicao, Observacao, CriadoEm) VALUES
(5, 8, 1, 10, NULL, '2026-02-22T00:00:00'),
(5, 11, 1, 20, NULL, '2026-02-22T00:00:00'),
(5, 14, 8, 30, NULL, '2026-02-22T00:00:00'),
(5, 21, 2, 40, NULL, '2026-02-22T00:00:00'),
(5, 47, 2, 50, NULL, '2026-02-22T00:00:00');

-- Rotor 900 (id=6) → 5 filhos
INSERT INTO Engenharia_EstruturasProdutos (ProdutoPaiId, ProdutoFilhoId, Quantidade, Posicao, Observacao, CriadoEm) VALUES
(6, 9, 1, 10, NULL, '2026-02-22T00:00:00'),
(6, 12, 1, 20, NULL, '2026-02-22T00:00:00'),
(6, 15, 10, 30, NULL, '2026-02-22T00:00:00'),
(6, 22, 2, 40, NULL, '2026-02-22T00:00:00'),
(6, 48, 2, 50, NULL, '2026-02-22T00:00:00');

-- Caixa Mancal D50 (id=49) → 3 filhos
INSERT INTO Engenharia_EstruturasProdutos (ProdutoPaiId, ProdutoFilhoId, Quantidade, Posicao, Observacao, CriadoEm) VALUES
(49, 26, 15.5, 10, NULL, '2026-02-22T00:00:00'),
(49, 20, 2, 20, NULL, '2026-02-22T00:00:00'),
(49, 46, 2, 30, NULL, '2026-02-22T00:00:00');

-- Caixa Mancal D60 (id=50) → 3 filhos
INSERT INTO Engenharia_EstruturasProdutos (ProdutoPaiId, ProdutoFilhoId, Quantidade, Posicao, Observacao, CriadoEm) VALUES
(50, 27, 20.0, 10, NULL, '2026-02-22T00:00:00'),
(50, 21, 2, 20, NULL, '2026-02-22T00:00:00'),
(50, 47, 2, 30, NULL, '2026-02-22T00:00:00');

-- Peneira D800 (id=51) → 2 filhos
INSERT INTO Engenharia_EstruturasProdutos (ProdutoPaiId, ProdutoFilhoId, Quantidade, Posicao, Observacao, CriadoEm) VALUES
(51, 26, 8.0, 10, NULL, '2026-02-22T00:00:00'),
(51, 33, 3.2, 20, NULL, '2026-02-22T00:00:00');

-- Peneira D1000 (id=52) → 2 filhos
INSERT INTO Engenharia_EstruturasProdutos (ProdutoPaiId, ProdutoFilhoId, Quantidade, Posicao, Observacao, CriadoEm) VALUES
(52, 27, 12.0, 10, NULL, '2026-02-22T00:00:00'),
(52, 34, 4.5, 20, NULL, '2026-02-22T00:00:00');

-- Esteira 3m (id=61) → 4 filhos
INSERT INTO Engenharia_EstruturasProdutos (ProdutoPaiId, ProdutoFilhoId, Quantidade, Posicao, Observacao, CriadoEm) VALUES
(61, 33, 6.0, 10, NULL, '2026-02-22T00:00:00'),
(61, 26, 25.0, 20, NULL, '2026-02-22T00:00:00'),
(61, 70, 8.0, 30, NULL, '2026-02-22T00:00:00'),
(61, 19, 1, 40, 'Motor da esteira', '2026-02-22T00:00:00');