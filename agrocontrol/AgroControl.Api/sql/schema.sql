-- =============================================================
-- AgroControl - Script Relacional (Fase 1)
-- SGBD alvo de execucao: SQLite (portavel / sem setup pesado)
-- Aplica: PRIMARY KEY, FOREIGN KEY, NOT NULL, UNIQUE e CHECK
-- =============================================================

PRAGMA foreign_keys = ON;

-- -------------------------------------------------------------
-- T_AGC_USUARIO : Operadores do sistema
-- -------------------------------------------------------------
CREATE TABLE IF NOT EXISTS T_AGC_USUARIO (
    ID_USUARIO     INTEGER      NOT NULL PRIMARY KEY AUTOINCREMENT,
    NM_USUARIO     TEXT         NOT NULL,
    DS_EMAIL       TEXT         NOT NULL UNIQUE,
    DS_SENHA_HASH  TEXT         NOT NULL,
    DT_CRIACAO     TEXT         NOT NULL DEFAULT (datetime('now'))
);

-- -------------------------------------------------------------
-- T_AGC_ESTUFA : Estufas monitoradas
-- -------------------------------------------------------------
CREATE TABLE IF NOT EXISTS T_AGC_ESTUFA (
    ID_ESTUFA      INTEGER      NOT NULL PRIMARY KEY AUTOINCREMENT,
    NM_ESTUFA      TEXT         NOT NULL,
    DS_LOCALIZACAO TEXT         NOT NULL,
    TP_CULTIVO     TEXT         NOT NULL
);

-- -------------------------------------------------------------
-- T_AGC_DISPOSITIVO : Dispositivos (sensores) por estufa
-- -------------------------------------------------------------
CREATE TABLE IF NOT EXISTS T_AGC_DISPOSITIVO (
    ID_DISPOSITIVO INTEGER      NOT NULL PRIMARY KEY AUTOINCREMENT,
    CD_UUID        TEXT         NOT NULL UNIQUE,        -- Ex: 'ESTUFA-MARTE-01'
    ID_ESTUFA      INTEGER      NOT NULL,
    CONSTRAINT FK_DISPOSITIVO_ESTUFA
        FOREIGN KEY (ID_ESTUFA) REFERENCES T_AGC_ESTUFA (ID_ESTUFA)
);

-- -------------------------------------------------------------
-- T_AGC_TELEMETRIA_LOG : Leituras dos sensores
-- -------------------------------------------------------------
CREATE TABLE IF NOT EXISTS T_AGC_TELEMETRIA_LOG (
    ID_TELEMETRIA   INTEGER     NOT NULL PRIMARY KEY AUTOINCREMENT,
    ID_DISPOSITIVO  INTEGER     NOT NULL,
    VL_TEMPERATURA  REAL        NOT NULL,
    VL_UMIDADE      REAL        NOT NULL,
    VL_AGUA         REAL        NOT NULL,
    VL_LUMINOSIDADE REAL        NOT NULL,
    DT_TIMESTAMP    TEXT        NOT NULL DEFAULT (datetime('now')),
    CONSTRAINT FK_TELEMETRIA_DISPOSITIVO
        FOREIGN KEY (ID_DISPOSITIVO) REFERENCES T_AGC_DISPOSITIVO (ID_DISPOSITIVO)
);

-- -------------------------------------------------------------
-- T_AGC_ALERTA_EVENTO : Eventos / panes detectados
-- -------------------------------------------------------------
CREATE TABLE IF NOT EXISTS T_AGC_ALERTA_EVENTO (
    ID_ALERTA       INTEGER     NOT NULL PRIMARY KEY AUTOINCREMENT,
    ID_DISPOSITIVO  INTEGER     NOT NULL,
    TP_ALERTA       TEXT        NOT NULL,
    DS_MENSAGEM     TEXT        NOT NULL,
    DS_GRAVIDADE    TEXT        NOT NULL CHECK (DS_GRAVIDADE IN ('Alerta', 'Critico')),
    DT_TIMESTAMP    TEXT        NOT NULL DEFAULT (datetime('now')),
    ST_RESOLVIDO    TEXT        NOT NULL DEFAULT 'N' CHECK (ST_RESOLVIDO IN ('S', 'N')),
    CONSTRAINT FK_ALERTA_DISPOSITIVO
        FOREIGN KEY (ID_DISPOSITIVO) REFERENCES T_AGC_DISPOSITIVO (ID_DISPOSITIVO)
);

-- Indices de apoio para consultas frequentes
CREATE INDEX IF NOT EXISTS IX_TELEMETRIA_DISP_DT  ON T_AGC_TELEMETRIA_LOG (ID_DISPOSITIVO, DT_TIMESTAMP);
CREATE INDEX IF NOT EXISTS IX_ALERTA_RESOLVIDO    ON T_AGC_ALERTA_EVENTO (ST_RESOLVIDO, DS_GRAVIDADE);

-- =============================================================
-- SEED minimo para permitir testes ponta-a-ponta
-- (uma estufa + um dispositivo 'ESTUFA-MARTE-01')
-- =============================================================
INSERT INTO T_AGC_ESTUFA (NM_ESTUFA, DS_LOCALIZACAO, TP_CULTIVO)
SELECT 'Estufa Marte', 'Setor A - Base Ares', 'Tomate Hidroponico'
WHERE NOT EXISTS (SELECT 1 FROM T_AGC_ESTUFA);

INSERT INTO T_AGC_DISPOSITIVO (CD_UUID, ID_ESTUFA)
SELECT 'ESTUFA-MARTE-01', (SELECT ID_ESTUFA FROM T_AGC_ESTUFA ORDER BY ID_ESTUFA LIMIT 1)
WHERE NOT EXISTS (SELECT 1 FROM T_AGC_DISPOSITIVO WHERE CD_UUID = 'ESTUFA-MARTE-01');
