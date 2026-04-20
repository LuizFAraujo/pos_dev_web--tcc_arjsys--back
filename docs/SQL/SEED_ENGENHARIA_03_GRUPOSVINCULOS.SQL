-- ============================================================================
--  ENGENHARIA 03 — VINCULOS DE GRUPOS
-- ----------------------------------------------------------------------------
--  Tabela: Engenharia_GruposVinculos
--  Campos NOT NULL: GrupoPaiId, GrupoFilhoId, CriadoEm
--  FKs (DeleteBehavior.Restrict): Engenharia_GruposProdutos.Id
--
--  Datas: configuracao dos vinculos em 01/02/2023 (apos cadastro dos grupos).
--
--  Dependencia de Ids (do SEED 02):
--    Coluna1: 1..10   Coluna2: 11..35   Coluna3: 36..50
-- ============================================================================


-- ---- Coluna1 -> Coluna2 (01/02/2023 manha) ---------------------------------
INSERT INTO Engenharia_GruposVinculos (GrupoPaiId, GrupoFilhoId, CriadoEm) VALUES
(1, 11, '2023-02-01T09:00:00'),  -- ProdFinal  -> PCP
(1, 12, '2023-02-01T09:02:00'),  -- ProdFinal  -> EST
(2, 13, '2023-02-01T09:05:00'),  -- Conj       -> SGE
(2, 14, '2023-02-01T09:07:00'),  -- Conj       -> MEC
(2, 15, '2023-02-01T09:09:00'),  -- Conj       -> ELT
(2, 16, '2023-02-01T09:11:00'),  -- Conj       -> HID
(3, 13, '2023-02-01T09:15:00'),  -- PecaFab    -> SGE
(3, 14, '2023-02-01T09:17:00'),  -- PecaFab    -> MEC
(3, 15, '2023-02-01T09:19:00'),  -- PecaFab    -> ELT
(4, 14, '2023-02-01T09:25:00'),  -- PecaUsin   -> MEC
(4, 13, '2023-02-01T09:27:00'),  -- PecaUsin   -> SGE
(5, 18, '2023-02-01T09:35:00'),  -- Comprado   -> FIX
(5, 19, '2023-02-01T09:37:00'),  -- Comprado   -> VED
(5, 20, '2023-02-01T09:39:00'),  -- Comprado   -> ROL
(5, 21, '2023-02-01T09:41:00'),  -- Comprado   -> MOT
(5, 23, '2023-02-01T09:43:00'),  -- Comprado   -> TRN
(7, 24, '2023-02-01T10:00:00'),  -- MatPrima   -> CHP
(7, 25, '2023-02-01T10:02:00'),  -- MatPrima   -> TUB
(7, 26, '2023-02-01T10:04:00'),  -- MatPrima   -> BRR
(7, 27, '2023-02-01T10:06:00'),  -- MatPrima   -> CNL
(7, 28, '2023-02-01T10:08:00'),  -- MatPrima   -> PFL
(8, 29, '2023-02-01T10:15:00'),  -- Consum     -> SOL
(8, 30, '2023-02-01T10:17:00'),  -- Consum     -> PIN
(8, 31, '2023-02-01T10:19:00'),  -- Consum     -> ABR
(8, 32, '2023-02-01T10:21:00'),  -- Consum     -> FER
(8, 33, '2023-02-01T10:23:00'),  -- Consum     -> EPI
(9, 34, '2023-02-01T10:30:00'),  -- Embal      -> CXP
(9, 35, '2023-02-01T10:32:00');  -- Embal      -> PLT


-- ---- Coluna2 -> Coluna3 (01/02/2023 tarde) ---------------------------------
INSERT INTO Engenharia_GruposVinculos (GrupoPaiId, GrupoFilhoId, CriadoEm) VALUES
(11, 36, '2023-02-01T14:00:00'),  -- PCP -> GW10
(11, 37, '2023-02-01T14:02:00'),  -- PCP -> GW20
(12, 38, '2023-02-01T14:05:00'),  -- EST -> BM20
(12, 39, '2023-02-01T14:07:00'),  -- EST -> BM30
(24, 40, '2023-02-01T14:15:00'),  -- CHP -> 1020
(24, 41, '2023-02-01T14:17:00'),  -- CHP -> 1045
(24, 42, '2023-02-01T14:19:00'),  -- CHP -> 4140
(24, 43, '2023-02-01T14:21:00'),  -- CHP -> INOX
(25, 44, '2023-02-01T14:25:00'),  -- TUB -> SC40
(25, 45, '2023-02-01T14:27:00'),  -- TUB -> SC80
(26, 41, '2023-02-01T14:30:00'),  -- BRR -> 1045
(26, 42, '2023-02-01T14:32:00'),  -- BRR -> 4140
(20, 47, '2023-02-01T14:40:00'),  -- ROL -> SKF
(20, 48, '2023-02-01T14:42:00'),  -- ROL -> NSK
(21, 46, '2023-02-01T14:45:00');  -- MOT -> WEG
