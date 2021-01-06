--
-- PostgreSQL database dump
--

-- Dumped from database version 10.13
-- Dumped by pg_dump version 10.13

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

DROP DATABASE IF EXISTS rebirth_global;
--
-- Name: rebirth_global; Type: DATABASE; Schema: -; Owner: postgres
--

CREATE DATABASE rebirth_global WITH TEMPLATE = template0 ENCODING = 'UTF8' LC_COLLATE = 'English_United States.1252' LC_CTYPE = 'English_United States.1252';


ALTER DATABASE rebirth_global OWNER TO postgres;

\connect rebirth_global

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: rebirth; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA rebirth;


ALTER SCHEMA rebirth OWNER TO postgres;

--
-- Name: plpgsql; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS plpgsql WITH SCHEMA pg_catalog;


--
-- Name: EXTENSION plpgsql; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION plpgsql IS 'PL/pgSQL procedural language';


SET default_tablespace = '';

SET default_with_oids = false;

--
-- Name: accounts; Type: TABLE; Schema: rebirth; Owner: postgres
--

CREATE TABLE rebirth.accounts (
    id integer NOT NULL,
    username character varying(13) NOT NULL,
    password character varying NOT NULL,
    creation date DEFAULT CURRENT_DATE NOT NULL,
    last_login date DEFAULT CURRENT_DATE NOT NULL,
    last_ip character varying DEFAULT '127.0.0.1'::character varying NOT NULL,
    ban integer DEFAULT 0 NOT NULL,
    gender integer DEFAULT 0 NOT NULL,
    vote_points integer DEFAULT 0,
    rebirth_points integer DEFAULT 0
);


ALTER TABLE rebirth.accounts OWNER TO postgres;

--
-- Name: COLUMN accounts.password; Type: COMMENT; Schema: rebirth; Owner: postgres
--

COMMENT ON COLUMN rebirth.accounts.password IS 'unsure what the length of these encrypted passwords is gonna be';


--
-- Name: accounts_id_seq; Type: SEQUENCE; Schema: rebirth; Owner: postgres
--

CREATE SEQUENCE rebirth.accounts_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE rebirth.accounts_id_seq OWNER TO postgres;

--
-- Name: accounts_id_seq; Type: SEQUENCE OWNED BY; Schema: rebirth; Owner: postgres
--

ALTER SEQUENCE rebirth.accounts_id_seq OWNED BY rebirth.accounts.id;


--
-- Name: points_log; Type: TABLE; Schema: rebirth; Owner: postgres
--

CREATE TABLE rebirth.points_log (
    transaction_id integer NOT NULL,
    account_id integer NOT NULL,
    amount integer NOT NULL,
    datetime timestamp without time zone DEFAULT now() NOT NULL,
    source character varying DEFAULT 'system'::character varying NOT NULL,
    type smallint DEFAULT 0 NOT NULL
);


ALTER TABLE rebirth.points_log OWNER TO postgres;

--
-- Name: COLUMN points_log.source; Type: COMMENT; Schema: rebirth; Owner: postgres
--

COMMENT ON COLUMN rebirth.points_log.source IS 'who originated the transfer
usually either system or admin';


--
-- Name: COLUMN points_log.type; Type: COMMENT; Schema: rebirth; Owner: postgres
--

COMMENT ON COLUMN rebirth.points_log.type IS '0 = vote points
1 = rebirth points (donor)';


--
-- Name: points_log_transaction_id_seq; Type: SEQUENCE; Schema: rebirth; Owner: postgres
--

CREATE SEQUENCE rebirth.points_log_transaction_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE rebirth.points_log_transaction_id_seq OWNER TO postgres;

--
-- Name: points_log_transaction_id_seq; Type: SEQUENCE OWNED BY; Schema: rebirth; Owner: postgres
--

ALTER SEQUENCE rebirth.points_log_transaction_id_seq OWNED BY rebirth.points_log.transaction_id;


--
-- Name: accounts id; Type: DEFAULT; Schema: rebirth; Owner: postgres
--

ALTER TABLE ONLY rebirth.accounts ALTER COLUMN id SET DEFAULT nextval('rebirth.accounts_id_seq'::regclass);


--
-- Name: points_log transaction_id; Type: DEFAULT; Schema: rebirth; Owner: postgres
--

ALTER TABLE ONLY rebirth.points_log ALTER COLUMN transaction_id SET DEFAULT nextval('rebirth.points_log_transaction_id_seq'::regclass);


--
-- Data for Name: accounts; Type: TABLE DATA; Schema: rebirth; Owner: postgres
--

INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (67, 'yeehaw', '$2a$12$/2QRJToCjy7/H3WESzSYzu6PJzA2YpaX/FM07K2RU03eMZjBjfrYy', '2020-02-29', '2020-02-29', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (71, 'SubsDub', '$2a$12$lv.ZehEQsVCEZEFYXkJ4DuTPbjC./l9/9wzHMtdcM5/1XecdPf7NC', '2020-03-01', '2020-03-01', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (74, 'hanzsolo', '$2a$12$xlykXHXjECMNFIZ2tbb0B.YWYMU89j5x1QvGk.CxzrEz6UNSdS80W', '2020-03-01', '2020-03-01', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (95, 'crook', '$2a$12$PA9ANCa8mUKPFtHd6A.SYeXAKnuZjaDqjj6ve0Qoz8pGnhXUOd0SK', '2020-03-13', '2020-03-13', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (79, 'nofear', '$2a$12$ajbJ3NdkKX9RpKotjq4MIe62LOFmjP386cLCV3pyGndpLfemY61Di', '2020-03-04', '2020-03-04', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (90, 'bakaniinii', '$2a$12$.NlEePwZEWMXGiCG3eHtuObl3pflBZ1nMLLPJMyhVtbMD/wsV8SUC', '2020-03-07', '2020-03-07', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (92, 'f_ynsiny', '$2a$12$5lm7.IouDB/zm2JdRNTKd.hrLztqSN3dQ.tyzxd0a/KjIZ.cjDOzy', '2020-03-08', '2020-03-08', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (87, 'soshialvin', '$2a$12$RTLsBwSWhwlcXZyi.7D.suLJnU.ogv3uSPA3NnA8dn7iaLgIpuaSi', '2020-03-06', '2020-03-06', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (78, 'morimon', '$2a$12$HlawI80SYEKWxMmDmsWEtOswNBkFwrVAFMGXguuKa7MmKJr57hKpK', '2020-03-04', '2020-03-04', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (83, 'network2', '$2a$12$EbFI0Wzz0H.XinF6R/Ta4.vB2kl1V8bciQ5SPLQ8nKp9gk3pU3lye', '2020-03-05', '2020-03-05', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (113, 'snuffles', '$2a$06$86o0NYfJzLYGIW9AXy6tQOo/U/8CxVvoWa.pGLENyhARNkT7v69UC', '2020-04-21', '2020-04-21', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (103, 'detritus5', '$2a$12$1qD/X7YZUkhL0Esf0zvolOzz86gW.iioVeQDnfLh/gHozHcLnREjC', '2020-03-21', '2020-03-21', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (82, 'network', '$2a$12$UCoi2k0KwU.uZPkhMqA1W.gCcoZMEWmUL/Zpj6KYaPW3Agjmp1qpO', '2020-03-05', '2020-03-05', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (93, 'eljay', '$2a$12$3BzpYEfea6Ece4k8u8VVZenY4/5d2PnbmQnmdf1cC1EO9K3IK.j/C', '2020-03-09', '2020-03-09', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (88, 'lixy6664', '$2a$12$9Bu7wQpPCC8tnJbCeiSscuLG03u1EUkqvq6A1IzV0qbMHupru4Tta', '2020-03-06', '2020-03-06', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (73, 'blakerzito', '$2a$12$1bZL5M1BSPnsxoTZ1To6ZOBdH2CbheIhETZbZqbofqRgG/1pkBP5q', '2020-03-01', '2020-03-01', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (63, 'admin', '$2a$12$.YN8cxSCu29G.lUo/qUgje5VrJvACd9BQ6j0mO0D2hXlcDpb0bFj.', '2020-02-29', '2020-02-29', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (86, 'detritus3', '$2a$12$qtez7z4HfnFYYUFnCY9GzubjzdClTCFyuQKhNV/i7260CGMoqTGsO', '2020-03-06', '2020-03-06', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (84, 'radish', '$2a$12$odFVFoAKIfK3.G16nLZH3uwaDsrOcqwrECsRW5Iibo16NKNmuaGGy', '2020-03-05', '2020-03-05', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (65, 'tester', '$2a$12$w9c.X2INb0DcAodzjHVZWe0RkT1.XBeJIhvE.HhjcdkiqJ9Pczn22', '2020-02-29', '2020-02-29', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (104, 'admin1', '$2a$12$R3s.X7Dg9iNgkzXFLj9.ue6rr/i65lxjMtyJZZVS/gVg5YhHGfw3a', '2020-03-25', '2020-03-25', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (89, 'detritus4', '$2a$12$gDB7W41iP93f7Kgw/5aduu.qZKdi.SrR1uQYH.ZreV2qyvsnlL9/q', '2020-03-06', '2020-03-06', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (68, 'radishGM', '$2a$12$HK58XZaKW9AFHPv/RDecT.XWhwmVEoV0.dnvmkNhcluukJ.d496Q2', '2020-02-29', '2020-02-29', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (75, 'MannyMontes', '$2a$12$F7LzWs0oB/apyl9Xf7U/eeyISTOwI6E1sG/hSj5WScV36HntBaL82', '2020-03-01', '2020-03-01', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (96, 'gurimy01', '$2a$12$ZMZzb/cyIIwFTx2PBa3AGuwOZxfqCPhBQaUbCaDKQobxB/k6PB3Ym', '2020-03-14', '2020-03-14', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (85, 'Nexowned', '$2a$12$A8juNdaqAFPKKhTATGs5U.YLSPWjRskkjUc3.3lmvMGZgP4ZE91iC', '2020-03-06', '2020-03-06', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (99, 'stonerman', '$2a$12$Rl2al9QasjqFAy7O/H42F.Rb2syrShVekLkPg/CI2G3jwQzSDy58G', '2020-03-18', '2020-03-18', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (64, 'rajan', '$2a$12$d1Os1l29tnvHlcr4LE1FwueSuD5KvQd7HIbgWNR1wxh8Ev3KMYLdK', '2020-02-29', '2020-02-29', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (101, 'hazza3', '$2a$12$3oNOnYpiFChHhfkshz9Ch..c2oziM2.ZpeGFB1Nxcj8/cg.58jgpe', '2020-03-19', '2020-03-19', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (91, 'ynsiny', '$2a$12$jY0qhnAWhNFYTEG61PWkaOlCJHQQBH8/htF79FPID9r/q5X19NYkq', '2020-03-08', '2020-03-08', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (102, 'hazza33', '$2a$12$cogFYQTy./yjFVYd0uHmp.oyd8gIz8cBZeaXY91zgsCIxrm8nr9Eu', '2020-03-21', '2020-03-21', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (69, 'detritus', '$2a$12$PQrXGINYzlwt/vRspXSL7umbCRGBuFjSHFxOA3uCGdJADYD9AYllO', '2020-02-29', '2020-02-29', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (81, 'detritus2', '$2a$12$S3Dy0ooauqgLpXdXz7n5A.xNNx4rZZn6sQRn/i0crdk3nmoM18OpC', '2020-03-04', '2020-03-04', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (174, 'nigger', '$2a$06$vh.m5LtI/i/43hwT1uhjWubSBbD4PC3BP074qSDqAhuxXEXo.zIqO', '2020-08-08', '2020-08-15', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (97, 'zephyr', '$2a$12$1UVxOg3dwYU6O/zx6A/tn.WnRcIvP8FdRvCl5aG.VsHcV.m8wL46O', '2020-03-16', '2020-03-16', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (98, 'Fraysa', '$2a$12$CFBB.2g6PNLtqwaKhtNfLusVSHrFTwwsMiSjFoSryz0/rvBdW8Vji', '2020-03-18', '2020-03-18', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (80, 'detritus1', '$2a$12$YE0jhsuJyqYCrDu8XLxvlu4L/xV6waXU5vQJO49CKw1ijEL6WEgim', '2020-03-04', '2020-03-04', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (100, 'spongebob', '$2a$12$.2ToCCEtilKEOCLcUkfqOutGzZ3/FoYjO/fJ/3yZsJ7leHQ3QMYNy', '2020-03-18', '2020-03-18', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (109, 'test1', '$2a$12$OwBiryXB3jOuMtklXLMAMubj3toNiusJP1VqNx.kPMmfB88GJf4FK', '2020-04-01', '2020-04-01', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (70, 'bakaniinii2', '$2a$12$2QrE7ADGQ6NjKkjz7gMA2uB6U.QBKs7CtjXnrFVqrB12gpwdsuMpm', '2020-02-29', '2020-02-29', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (110, 'raaa', '$2a$12$T4BGIzpEEPOqtRJBz1aoeeva4K6L3tgHf7wVnMqAMDRlsAb251q0q', '2020-04-03', '2020-04-03', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (107, 'test0', '$2a$12$jsw9Iuwf7fSh45lWuWcwCeC98Y3H38qRhBXASboLuYGFzZ09Iw152', '2020-03-31', '2020-03-31', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (106, 'kimnodick', '$2a$12$QzV68B.UEWy0cQjGc/cZ5uZ19GlvlkNv61SWEVyYX938oSEwj.q66', '2020-03-30', '2020-03-30', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (94, 'snuffle', '$2a$12$J/RJPPx3H.QtSqGtxW3ZouAr6a4OXgGU5ikOiqh3iBC1wpMC7MWly', '2020-03-12', '2020-03-12', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (111, 'llirfan619ll', '$2a$12$kkgB4M/CyLJxPxe/uw4mveXEiEo3sywLNQ1nYSd26HplFWLhR7.5q', '2020-04-08', '2020-04-08', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (77, 'disparate', '$2a$12$YJcIL1uP/yD0o7agjhEPh.PpJ8gRCT1wn6jN7sbEbsdQOlh4vgPVS', '2020-03-04', '2020-03-04', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (115, 'xphanto', '$2a$06$9cKzqFx8GwruUplSTXJJJeQ/pC8U4SLJ7HrRJ.GNJFKMvgMx0M3wW', '2020-04-23', '2020-04-23', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (114, 'dyzer', '$2a$06$/5ay7iMJtor886620nCTZ.GF6DaY4C8MTr.iaZpI4QGKVzi8Lh3iK', '2020-04-23', '2020-04-23', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (105, 'bakaniinii3', '$2a$12$C.B8SkOKEEMZYyBvDrziresqhIPMoz80atUELNAsJ2pOau7QU.lSy', '2020-03-27', '2020-03-27', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (72, 'Tonblader', '$2a$12$C5Sjr71/lWUwJt0PI6LXyONfzrY0z.RaGRqvAI138plO7.NvvJXl.', '2020-03-01', '2020-03-01', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (117, 'noam1232', '$2a$06$DDsFMGUKPnGsqGY75ZgL8e.rQ6x7S3VpVVu6N3DNrzgJBNTx0v5he', '2020-04-25', '2020-04-25', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (128, 'muffinman', '$2a$06$A9IBS5DbLVw24WAq74/aquVck2UZNC61k2VKObMfqRvHnRo76yxyu', '2020-05-07', '2020-05-07', '127.0.0.1', 0, 1, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (127, 'newplayer', '$2a$06$NgfAPimT7zSTvLbAdtW3Yenpgeet0iHXW6xbmrtCQP8jFnNwZIN0S', '2020-05-06', '2020-05-06', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (108, 'bakaniinii4', '$2a$12$rLtDzTAVValcbGmMip973OgGscoArB.wI2d4yohwxS/7phXfPdV4W', '2020-03-31', '2020-06-23', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (119, 'xdisparate', '$2a$06$.ftGGfDbofb3TGmk4ZDkFe5mpmGbfTUt1NQj2mgHa0APr7nGaIj1i', '2020-04-25', '2020-04-25', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (122, 'dyzerg', '$2a$06$LF/zjG8w028fBNLRbUutcebjDOYPmR5kbQZ6Yw2l.V0sEhL6PDNkS', '2020-04-26', '2020-04-26', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (131, 'bleker', '$2a$06$R/WaOv6jFdKqtNJW/KoHcutFkU.cJq5Yp0BafnC4gkukiuYSt5/XC', '2020-05-22', '2020-05-22', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (120, 'draziliuz', '$2a$06$4rqSNIQuC.rd0M/EcA73H.05zcezQ9PtHRaIZfaM/zw8wN8IyBpRi', '2020-04-25', '2020-04-25', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (133, 'tomato213', '$2a$06$sT151g3OmJTofXfGD.RjY.huap9ExK31X/wXi8XKmVWGWhX5uzqoG', '2020-06-05', '2020-06-05', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (123, 'disparater', '$2a$06$famBmwdX2PqMuZFENY1YKef8BMB5EDqNLlYZFfF/feZbYZjVeQ3Gm', '2020-04-26', '2020-04-26', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (129, 'greveldinges', '$2a$06$L3hyNUK/8bNIsCUe23HwZuB8wS51OPOdOBlp70s.dbhJdRQTAzVT2', '2020-05-09', '2020-05-09', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (125, 'zuwur', '$2a$06$CrqJCPvTsLgQCN/xFqC1PukpbEz3TUavMQ9HmH0pTtccEpErX38Ze', '2020-04-28', '2020-04-28', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (130, 'jayss8', '$2a$06$tNNbpHu2woni7.1dNVbH1O.uqAh23/3YRJVCUpG9ClYlyYqnlUWK.', '2020-05-18', '2020-05-18', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (132, 'admin123', '$2a$06$5hxFumTMTdWAFVr3yS0lIOsseEKY0rzN0QVD7/i4c51eedSz18p7u', '2020-05-22', '2020-05-22', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (126, 'blak', '$2a$06$dSleZfY3dg1dESm6Tcv07eL0SOuclewGZPKWIz2q5ufpZwM9zxr8K', '2020-04-28', '2020-04-28', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (135, 'muffindev', '$2a$06$Ojq54uBgyydZwo97juL/RuMu.Ay5.JdTz3Te1TUwNg67lhEYG/tXq', '2020-06-06', '2020-06-06', '127.0.0.1', 0, 1, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (116, 'wolfvering', '$2a$06$51mF77fwOGEffVdRp3uylumb8njuU59EzAVNvMGD7oLD49d2zWI6W', '2020-04-23', '2020-04-23', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (124, 'vlak', '$2a$06$LBCUaogUJupXL1Wytujm5OQ/m9kbguVHxmynrYUAzJmhvSlQTnszS', '2020-04-27', '2020-04-27', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (118, 'blaker', '$2a$06$Y1mO84oP13cUS5FJ3pma0OzH.rJC1bU/CHdrBwApENNetVKXQP796', '2020-04-25', '2020-04-25', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (136, 'crounden', '$2a$06$PLKLDF.bB3KQuQ7f8EbZjOusaAhSSswY2GKPqFqsmoeKX/UMX2W3O', '2020-06-11', '2020-06-11', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (134, 'tomato212', '$2a$06$8qSpNPidqeiOdn77O964wOK2p6VYAPiyEt9QrCOotzhoY1jIuf6wW', '2020-06-05', '2020-06-05', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (121, 'draz', '$2a$06$IvKejhHqt7Ch1UEZcengbeBfpTnHfSxccNvilY4QyUKMqGO0BgNhq', '2020-04-25', '2020-04-25', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (192, 'minimumdelta', '$2a$06$IrWnKAkC62bLoB.4grrqJ.axoUKsQ.6CayDVVqOm.zlw0ncG1oVay', '2020-08-15', '2020-08-15', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (193, 'xpario', '$2a$06$pMeVxBf3XVTJF8/qtakl5.u32QHavvIAAjxwZUlpX1Z1locXmsCdG', '2020-08-15', '2020-08-15', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (141, 'adminl', '$2a$06$Z5k2.CvVHH3E.MG1pixTwevf561QY6HsFV9zyrq6roMlR/zdULdSy', '2020-08-03', '2020-08-04', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (76, 'admink', '$2a$12$MnbqOxHK.cF42kMbmNiS4.dMZayVUi./N6SyHqggDi5MLNcCWlQna', '2020-03-03', '2020-09-04', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (167, 'huyetquix3', '$2a$06$XAjknrEQ.LJgoomaqziFG.zYtFss4wBE0n4wSXD8Y8k9/yLx1ShRS', '2020-08-04', '2020-08-04', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (196, 'zygon', '$2a$06$ynMAcp1tG635Atb4KN63i.fprIt0o3zeEywKz.sBPd2ZEVlqqN1bu', '2020-08-17', '2020-09-05', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (173, 'niggers', '$2a$06$NRrNbGig9ZMR5GYjvtN57.OJJ8x/YugrfbhHumjfxw8FSdTXo3Eoa', '2020-08-08', '2020-08-08', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (175, '123123', '$2a$06$hFqiVYyR.NPcbaKVLIr1V.5qBkPlLTIAOlqyN1GpU3Za.HkhnG.8S', '2020-08-08', '2020-08-08', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (171, 'Flaw', '$2a$06$3a62HofQ6ANLFy2Fc.1NdO0fC1sgPYRmZwQCSgyWnjY3LiLCbAQhy', '2020-08-07', '2020-09-02', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (180, 'wenii', '$2a$06$bm8s3t6.S.ZdONG7aeZHmeNEd47jbz8wbcJhqJPJzLQMxUq2AKgu2', '2020-08-10', '2020-08-10', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (139, 'adminlkkd', '$2a$06$2w0V4hlNnmhxGktWITfHvOqriVOpXcX6h1b807DdVqf/DkuHDriNq', '2020-07-04', '2020-07-11', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (142, 'adminaa', '$2a$06$yBpVJPvCucISncnnQbC6ieTVgsxgx8tLQXjBs8F88fAFh3zbWdxx2', '2020-08-08', '2020-08-08', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (145, 'adminp', '$2a$06$4j3ZS75T5z608C4uRsAxQuWLts42QllngwEy2KPVTliuPjJh9lcQa', '2020-09-12', '2020-09-12', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (168, '8484', '$2a$06$kLLumkT4/W5pCYWKExvYVeDJYeZHGTq6kKPVTGxxiEXVUNRzqGkrG', '2020-08-05', '2020-08-05', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (137, 'admine', '$2a$06$ZngaAW4p7xZZSeHsSwuBKeew.QHAz79N0kHSOlBUkM822bIxNGsGy', '2020-06-27', '2020-06-28', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (66, 'admina', '$2a$12$JuceAERrn2vduO2lSiiAZuIsWiRP/5yN2dynJQNDHYBDD.cwYZzlq', '2020-02-29', '2020-10-06', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (181, 'eske', '$2a$06$KHAVrQSe76kseBvrSu40DuDuJ2sGtc22/iabzFJmw4QsZkKw0Od6S', '2020-08-10', '2020-08-10', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (194, 'moozi666', '$2a$06$Gp79Yy9HM6PQYB2y31u5TuzSHytKMI1KEIQM7QCbumDZ2DqHCov46', '2020-08-16', '2020-08-16', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (160, 'chuansheng1', '$2a$06$jQFNG16zSnOf3NqI6DazeO6ACBUFBu8I4bWg9QEawG3guNwwSTUJW', '2020-08-02', '2020-08-05', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (140, 'admini', '$2a$06$pjk6wSp/cI/rKubupeNF0.KtqQYXmx1DokmZ3dQ7HBj1WacoK0OYO', '2020-07-14', '2020-10-06', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (143, 'sdfsdf', '$2a$06$tPIFrEgUG7yAYSFRiuNXIuwBt.uTQ31lp55kDPB3EUsSrnLgF5bBC', '2020-08-10', '2020-08-10', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (182, 'kino', '$2a$06$DzH3aySkld2WioDNZ4XQXuwF1vlvbJl7.LU/QSboCOtBaLR1r3CH2', '2020-08-11', '2020-08-11', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (189, 'delta2', '$2a$06$ETZp5jctdf5NBLfX3yMA4OnqjzeDHY252uaNXZIObmuIL4d5HkByi', '2020-08-15', '2020-08-15', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (201, 'cherry1606', '$2a$06$4kOfqWqrHju7PPLzh7mybe53Ktzd21PX58i25UK0pr.s7vYbPbPJi', '2020-08-31', '2020-08-31', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (170, 'igemini', '$2a$06$NiP/L7qOP0CYcmibBagdbuUyPaVl38ogukEncMbQpozTEiC9Ji3W.', '2020-08-06', '2020-08-06', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (144, 'delta', '$2a$06$qnXtz6UQYSUqEPwKAuJUJeoQEpZxWVZqVjKRzCXBuxO/JPU6lpKu2', '2020-08-15', '2020-10-06', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (176, 'mazen', '$2a$06$FlYe7C4DH2nkA7c4F.C58O2PHM7JT.U.cvMJwndy28.SOiZahZuE.', '2020-08-09', '2020-08-09', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (190, 'lindayu', '$2a$06$aNrWUp63wAm4y4.KczNEpO20FftQMYIaRYmwwhiqnB9JKhJzCK2g.', '2020-08-15', '2020-08-15', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (172, 'yobobby123', '$2a$06$UiVNToDEbzPHGRW6AoNadOn8hkmxsoeQNkeRsuf51BFOBhf2ZyehC', '2020-08-07', '2020-08-07', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (179, 'sdfsdf', '$2a$06$CmOip3Njd9EQwrZVbgYnm.x7Cu0kVyoNUhoc/sGgBf8Gr0WJ6T27W', '2020-08-10', '2020-08-10', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (183, 'davik123456', '$2a$06$xjmZuuP6AnUWP18pBycXAe1l6eSbwnCFH821RD1KHTK3rBmcn3zzS', '2020-08-11', '2020-08-12', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (199, 'itskevin', '$2a$06$4uOevvrnRbg0MZ.p.DMUIe1dQ3KJ1klQ1oSbJY/zk7mtDInshUf/G', '2020-08-26', '2020-08-26', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (178, 'mizkif10', '$2a$06$IcWqIxBQUzn3cP1H4jjB.OKqsX28YXhfrq17.tcJOxoC55xMUqreK', '2020-08-09', '2020-08-10', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (177, 'kino0924', '$2a$06$WShzG1998LVStGXZYGQeNuWUgOody7GN998F1k9RQ4N5UHcuOfh2.', '2020-08-09', '2020-08-29', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (185, 'devmuffin', '$2a$06$37JdUIy9GZ1qMfhUZIsMm.7tLl18irNpr.yz5kUUM.jOPI6M4uuQm', '2020-08-13', '2020-08-13', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (188, 'Jacky', '$2a$06$CjvhRK/DntMBqBHQI52yMeaC6Rg9Me6vo6fA7/Y8NSZ/ahsLpq5yW', '2020-08-15', '2020-08-15', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (205, 'wii951wii', '$2a$06$p6aSWOcP34U4IIq/Jh85J.jbdac8oZFHY/VNVB7qQNHMrkAWVIM1i', '2020-09-16', '2020-09-17', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (198, 'cumwagon', '$2a$06$PcG0PeXY/ok4uh7TdSo25uivo6T3eQalWXjFsQTB9a5VenSXdL7Bm', '2020-08-24', '2020-09-17', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (187, 'Navehmill01', '$2a$06$m/O3GheQOHspGiBI.1nvQe3Z4aqnh5I4CZnjCGdhZzV9SlWNpqtLq', '2020-08-14', '2020-08-15', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (197, 'vampireknight', '$2a$06$xgzBVM8uSCDVO7spQdDiJuTvwAUs0pub9ty6K597vaCCxEMRkryti', '2020-08-18', '2020-08-18', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (184, 'Flaw2', '$2a$06$OKaGe4yQON9WZ2258AxCHOTTbOdGCNsDg8OPWfQY6/9FbpkFduSFa', '2020-08-13', '2020-09-02', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (211, 'admini', '$2a$06$r.xNee82gELopXAmSuCSc.8EFvmPvGRM42D0u0eX37CM527xrgx3q', '2020-09-29', '2020-09-29', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (203, 'iluvraji', '$2a$06$QLvaYqPXPD655/YtioXQ4.vhDs4bnV2X2BvSkjKTjLbktzgTPQEBG', '2020-09-01', '2020-09-05', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (200, 'realdudes123', '$2a$06$3UVF1PjHuyVK97xl0VWzT.sqf8B6CcyTD4BaQSXc7/AH8R2C9unhe', '2020-08-31', '2020-08-31', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (202, 'marijoh', '$2a$06$Czho133BN.UZDxStjH/9j.IITm9SoG/dYCbvktIsVanVg31XXXocy', '2020-09-01', '2020-09-13', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (210, 'Taylor', '$2a$06$x/RNu68fIVI08jquIvIsJexCpcLvcib.OdUqgdbgSO2aZoE8IIQ3q', '2020-09-24', '2020-09-28', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (206, 'hashhash', '$2a$06$r7OlWM09uJuNdTHMQ9HVh.yT7MWyE9yBBKS/x5A5ILYEMf.kMAMau', '2020-09-19', '2020-10-15', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (204, 'kamil', '$2a$06$P21PfOsd6q7aqcNJoAnE5.M0tazx8WK3LJ.oJ.gEXhG6VrJnv8mb6', '2020-09-02', '2020-09-04', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (207, 'cumsock', '$2a$06$M5qNxDXd11w61OVdOeI8x.pRoPzS45CFjaUa6BKcy6BB6WC.BYWK6', '2020-09-23', '2020-09-23', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (212, 'sexdeeza', '$2a$06$YWHdLv9jV97RSzEhYceEBeBUNMgKH93t1/TyFJySiznpNDK24JrTa', '2020-10-02', '2020-10-02', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (209, 'cumsockss', '$2a$06$sSWCPmt31pjUp69cu3g6cuNE0JHDp1nyn97Yu664usCLYwh1fQnBW', '2020-09-23', '2020-10-06', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (195, 'Artcro', '$2a$06$UsDyXFRPVGVsiVQaKwzH0OraizPCLLlfwP3SDRykXIAUs7QNW27SO', '2020-08-16', '2020-10-11', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (214, 'lain', '$2a$06$gra3AOfO/.8lUEeLzygbSO3Z21EiGMwYxlKWOJKMK5EueAsVLN3DG', '2020-10-04', '2020-10-04', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (191, 'rich', '$2a$06$vS76hbs8L9EZE6RFF560yOUgkcOX6NPGzOPPaNbkvMTz6iSsrltie', '2020-08-15', '2020-10-04', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (215, 'qwerty', '$2a$06$t7qU21fsq90439X5g9xLEOkVMoy5lgr1mqx3qSlpOxC69rgUsfyHu', '2020-10-06', '2020-10-06', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (186, 'delta', '$2a$06$Sofw7YYBBAJA066BU7QZ5e1.5FqTQLhDBF0lypSrBziyo8mZ9Opdm', '2020-08-13', '2020-10-06', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (216, 'admins', '$2a$06$vPLC5CWFoRM0cabQrdRka.NuIV8z27cLuwhBlNZROj7mi9dTaZhXO', '2020-10-06', '2020-10-06', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (218, 'moozi2345', '$2a$06$4C8sENx6/GcTilKkFx5Rn.ZXzrJabtIy65N8vmv/82DlQT5pggtXi', '2020-10-12', '2020-10-12', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (213, 'shotimoti2', '$2a$06$4fs8S4p63tZgZVPNnSxJqe2WO.9PY4Js.QtplRu.3pEqoJCqAlo2y', '2020-10-03', '2020-10-14', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (217, 'torin123', '$2a$06$RTd8urTYqheRMircSIcCpuIMc8tTsOEkGYVQ2Gri.I7JjYgRY0SJm', '2020-10-07', '2020-10-10', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (222, 'davheed', '$2a$06$8XbRxC63SYeIxy4wZUM5cuAIpIhc7SAi5a5WxSEIOHJsu1KwTJ/QO', '2020-10-15', '2020-10-15', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (219, 'fucksås', '$2a$06$A9wd1lbadmgslr4cNfuofeMZ9BhS2FPMXJwb41uywct2S05Wlg9UC', '2020-10-12', '2020-10-12', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (208, 'cumsocks', '$2a$06$lCQRu8cBJZamaULqerFefu0ZxJpgPJeHW0ObrgIaCONsMgeht.P0W', '2020-09-23', '2020-10-14', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (220, 'otkk', '$2a$06$8MbHSXw30PLCgpcU1l9S.uBkixDqt5LFQajEC2czAyi./eYtKqj86', '2020-10-12', '2020-10-15', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (221, 'otkkk', '$2a$06$wMWEhGfuO1Y0Jieb/gJxiupztrPfp2JHk6rqgPa6zPhUbZppXzWPu', '2020-10-12', '2020-10-15', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (169, 'admino', '$2a$06$tfh2xmV.ruq7GJRTOyKkLeF1ky2n7g8W1BsLR9BEH755QOrFuZEKO', '2020-08-05', '2020-12-03', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (149, 'leilei', '$2a$06$gUVjFHjhRvRg3fLEDri1Suvxbd25j1i6jX/70zvKmyQgVBQ1o0tee', '2020-07-17', '2020-07-17', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (150, 'davik1234', '$2a$06$w2pBu7oAvc9Q0Kc7IDrFgOwJW2KlLTNZ1PEToZK8GSEfRTz1UY4pm', '2020-07-17', '2020-08-12', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (151, 'davik12345', '$2a$06$eD4e1wMN7y7M76r2YR5gB.OkeEb4qpV8CLoUP1z2ZpEtcJ7lsjoJe', '2020-07-20', '2020-08-12', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (148, 'davik123', '$2a$06$rOZ5OmLXj6nsg43gUcgCB.Ge4aq2zxE1Lq6jkxMZsuHZU4CXl4qOm', '2020-07-17', '2020-08-12', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (146, 'moozi1', '$2a$06$JfSqfPjuMMG35HGU2/Nkmu.FRoLoJvMxsv/pkG9A8gNFGDf/e2COa', '2020-07-12', '2020-07-12', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (166, 'danny209', '$2a$06$7iN8fHI5BYfwubD70uI/feE6aosp4ZNqZbf5FPRbkU2930UYLJMk6', '2020-08-04', '2020-08-04', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (138, 'admino', '$2a$06$SzA0.Gmy2iFuMTety1JCa.p3xTFOZ3Nk7eEoHpz2bwOCw5aLIwm4S', '2020-06-28', '2020-10-16', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (147, 'gible', '$2a$06$SqUfp9/TozUGMW7Yyf9RouBEAOegclsfxzF2rs7rZdhJgK.CLlO0a', '2020-07-14', '2020-07-14', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (154, 'jbean209', '$2a$06$gFTuKypWXQODHTqnw/hjgOWWvmIkfPeMGgZ98Cw7fbC9HsSlaBuFG', '2020-08-01', '2020-10-11', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (158, 'wnstjq98', '$2a$06$D.M0AAkRW8DipNv.EJ75Ue4YH2G.00PnJzglkDqzQO5REvMLAuQW6', '2020-08-01', '2020-08-02', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (161, 'chuansheng2', '$2a$06$YYZIz0qw9AUgxo/VleBLQ.GVJKT8Gd4A.zRQDdif/zElwxy5Petiq', '2020-08-02', '2020-08-05', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (157, 'huyetquix1', '$2a$06$9p7BbzIHxq9tn1sAxM2bc.wnuW8EfVdRgOaj6B7plJuKr.9oRkLMm', '2020-08-01', '2020-08-12', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (156, 'Wickedry', '$2a$06$9b/JRvSxdwzhtqV674C5EOBcLc.ZFi7hh/Te648rKz0z87Djngl6m', '2020-08-01', '2020-09-29', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (152, 'Shineath', '$2a$06$R1CMV/y8w5hIifYRD35lSeBvrLJspdOWKoMN4nLhSoePrcEgkqxxa', '2020-07-21', '2020-09-23', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (163, 'moozi123', '$2a$06$TDNkAIOucw6ZkJwoSN/uLecLUeVj0L90UM5QftDbzE0vNxlK.rdVu', '2020-08-02', '2020-08-02', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (162, 'huyetquix2', '$2a$06$B1lmCXW7ahzWxwqRU3KAfO1gN9yEIgp6ECDQyqbIzn2WkDntgPuWy', '2020-08-02', '2020-08-04', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (165, 'Nitronom873', '$2a$06$vbVvTgNFEX1NPed8zN.wAOHCu38NNzrA9vIt0F8sO3YtbUiY9fqEG', '2020-08-03', '2020-08-03', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (164, 'Wickedry2', '$2a$06$H0LpnmNRTmIiTo9gZUoTQOw2mu5Kn3QK4Xz3wq/yTjmE2I4AvtfQe', '2020-08-02', '2020-08-06', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (155, 'huyetquix', '$2a$06$ibZJqEySEltQFZfle/nHMeMxz5paPmIq38yKaSvu52gBaLyE3QnZi', '2020-08-01', '2020-08-17', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (159, 'chuansheng', '$2a$06$EOinaqZQTxDxdPJwRyYMR.1mdn5g/RNkUtFc.Ecb7UGCAepOhxEpO', '2020-08-02', '2020-08-05', '127.0.0.1', 0, 0, 0, 0);
INSERT INTO rebirth.accounts (id, username, password, creation, last_login, last_ip, ban, gender, vote_points, rebirth_points) VALUES (153, 'timeclock555', '$2a$06$MYCuvHB2HNEuCTfsJmXDIOPhNFRuc6s75xg6tCGvhMPQTkKkv0HLq', '2020-07-25', '2020-08-22', '127.0.0.1', 0, 0, 0, 0);


--
-- Data for Name: points_log; Type: TABLE DATA; Schema: rebirth; Owner: postgres
--



--
-- Name: accounts_id_seq; Type: SEQUENCE SET; Schema: rebirth; Owner: postgres
--

SELECT pg_catalog.setval('rebirth.accounts_id_seq', 222, true);


--
-- Name: points_log_transaction_id_seq; Type: SEQUENCE SET; Schema: rebirth; Owner: postgres
--

SELECT pg_catalog.setval('rebirth.points_log_transaction_id_seq', 1, false);


--
-- Name: accounts accounts_pkey; Type: CONSTRAINT; Schema: rebirth; Owner: postgres
--

ALTER TABLE ONLY rebirth.accounts
    ADD CONSTRAINT accounts_pkey PRIMARY KEY (id);


--
-- Name: points_log points_log_pk; Type: CONSTRAINT; Schema: rebirth; Owner: postgres
--

ALTER TABLE ONLY rebirth.points_log
    ADD CONSTRAINT points_log_pk PRIMARY KEY (transaction_id);


--
-- Name: points_log_transaction_id_uindex; Type: INDEX; Schema: rebirth; Owner: postgres
--

CREATE UNIQUE INDEX points_log_transaction_id_uindex ON rebirth.points_log USING btree (transaction_id);


--
-- Name: SCHEMA public; Type: ACL; Schema: -; Owner: postgres
--

GRANT ALL ON SCHEMA public TO PUBLIC;


--
-- PostgreSQL database dump complete
--

