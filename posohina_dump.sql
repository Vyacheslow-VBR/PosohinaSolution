--
-- PostgreSQL database dump
--

-- Dumped from database version 16.3
-- Dumped by pg_dump version 16.3

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

DROP DATABASE IF EXISTS posohina;
--
-- Name: posohina; Type: DATABASE; Schema: -; Owner: app
--

CREATE DATABASE posohina WITH TEMPLATE = template0 ENCODING = 'UTF8' LOCALE_PROVIDER = libc LOCALE = 'ru_RU.UTF-8';


ALTER DATABASE posohina OWNER TO app;

\connect posohina

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
-- Name: app; Type: SCHEMA; Schema: -; Owner: app
--

CREATE SCHEMA app;


ALTER SCHEMA app OWNER TO app;

--
-- Name: calculate_discount_posohina(integer); Type: FUNCTION; Schema: app; Owner: app
--

CREATE FUNCTION app.calculate_discount_posohina(p_partner_id integer) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    total_quantity INTEGER;
    result_discount INTEGER;
BEGIN
    SELECT COALESCE(SUM(quantity), 0) INTO total_quantity
    FROM sales_posohina
    WHERE partner_id = p_partner_id;
    
    IF total_quantity < 10000 THEN
        result_discount := 0;
    ELSIF total_quantity < 50000 THEN
        result_discount := 5;
    ELSIF total_quantity < 300000 THEN
        result_discount := 10;
    ELSE
        result_discount := 15;
    END IF;
    
    RETURN result_discount;
END;
$$;


ALTER FUNCTION app.calculate_discount_posohina(p_partner_id integer) OWNER TO app;

--
-- Name: calculate_total_sum_posohina(); Type: FUNCTION; Schema: app; Owner: app
--

CREATE FUNCTION app.calculate_total_sum_posohina() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
    product_cost DECIMAL(10, 2);
BEGIN
    SELECT cost INTO product_cost FROM products_posohina WHERE id = NEW.product_id;
    NEW.total_sum = NEW.quantity * product_cost;
    RETURN NEW;
END;
$$;


ALTER FUNCTION app.calculate_total_sum_posohina() OWNER TO app;

--
-- Name: refresh_partner_discount_posohina(integer); Type: FUNCTION; Schema: app; Owner: app
--

CREATE FUNCTION app.refresh_partner_discount_posohina(p_partner_id integer) RETURNS void
    LANGUAGE plpgsql
    AS $$
BEGIN
    UPDATE partners_posohina
    SET discount = calculate_discount_posohina(p_partner_id)
    WHERE id = p_partner_id;
END;
$$;


ALTER FUNCTION app.refresh_partner_discount_posohina(p_partner_id integer) OWNER TO app;

--
-- Name: trigger_discount_update_posohina(); Type: FUNCTION; Schema: app; Owner: app
--

CREATE FUNCTION app.trigger_discount_update_posohina() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
    target_partner_id INTEGER;
BEGIN
    IF TG_OP = 'DELETE' THEN
        target_partner_id := OLD.partner_id;
    ELSE
        target_partner_id := NEW.partner_id;
    END IF;
    
    PERFORM refresh_partner_discount_posohina(target_partner_id);
    RETURN NULL;
END;
$$;


ALTER FUNCTION app.trigger_discount_update_posohina() OWNER TO app;

--
-- Name: update_timestamp_posohina(); Type: FUNCTION; Schema: app; Owner: app
--

CREATE FUNCTION app.update_timestamp_posohina() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$;


ALTER FUNCTION app.update_timestamp_posohina() OWNER TO app;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: partner_types_posohina; Type: TABLE; Schema: app; Owner: app
--

CREATE TABLE app.partner_types_posohina (
    id integer NOT NULL,
    name character varying(100) NOT NULL
);


ALTER TABLE app.partner_types_posohina OWNER TO app;

--
-- Name: partner_types_posohina_id_seq; Type: SEQUENCE; Schema: app; Owner: app
--

CREATE SEQUENCE app.partner_types_posohina_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE app.partner_types_posohina_id_seq OWNER TO app;

--
-- Name: partner_types_posohina_id_seq; Type: SEQUENCE OWNED BY; Schema: app; Owner: app
--

ALTER SEQUENCE app.partner_types_posohina_id_seq OWNED BY app.partner_types_posohina.id;


--
-- Name: partners_posohina; Type: TABLE; Schema: app; Owner: app
--

CREATE TABLE app.partners_posohina (
    id integer NOT NULL,
    type_id integer NOT NULL,
    name character varying(200) NOT NULL,
    director_fullname character varying(200),
    phone character varying(20),
    email character varying(100),
    rating integer,
    discount integer DEFAULT 0,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    address text,
    CONSTRAINT partners_posohina_rating_check CHECK ((rating >= 0))
);


ALTER TABLE app.partners_posohina OWNER TO app;

--
-- Name: partners_posohina_id_seq; Type: SEQUENCE; Schema: app; Owner: app
--

CREATE SEQUENCE app.partners_posohina_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE app.partners_posohina_id_seq OWNER TO app;

--
-- Name: partners_posohina_id_seq; Type: SEQUENCE OWNED BY; Schema: app; Owner: app
--

ALTER SEQUENCE app.partners_posohina_id_seq OWNED BY app.partners_posohina.id;


--
-- Name: products_posohina; Type: TABLE; Schema: app; Owner: app
--

CREATE TABLE app.products_posohina (
    id integer NOT NULL,
    name character varying(200) NOT NULL,
    code character varying(50),
    cost numeric(10,2),
    CONSTRAINT products_posohina_cost_check CHECK ((cost > (0)::numeric))
);


ALTER TABLE app.products_posohina OWNER TO app;

--
-- Name: products_posohina_id_seq; Type: SEQUENCE; Schema: app; Owner: app
--

CREATE SEQUENCE app.products_posohina_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE app.products_posohina_id_seq OWNER TO app;

--
-- Name: products_posohina_id_seq; Type: SEQUENCE OWNED BY; Schema: app; Owner: app
--

ALTER SEQUENCE app.products_posohina_id_seq OWNED BY app.products_posohina.id;


--
-- Name: sales_posohina; Type: TABLE; Schema: app; Owner: app
--

CREATE TABLE app.sales_posohina (
    id integer NOT NULL,
    partner_id integer NOT NULL,
    product_id integer NOT NULL,
    quantity integer NOT NULL,
    sale_date date DEFAULT CURRENT_DATE NOT NULL,
    total_sum numeric(10,2),
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT sales_posohina_quantity_check CHECK ((quantity > 0))
);


ALTER TABLE app.sales_posohina OWNER TO app;

--
-- Name: sales_posohina_id_seq; Type: SEQUENCE; Schema: app; Owner: app
--

CREATE SEQUENCE app.sales_posohina_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE app.sales_posohina_id_seq OWNER TO app;

--
-- Name: sales_posohina_id_seq; Type: SEQUENCE OWNED BY; Schema: app; Owner: app
--

ALTER SEQUENCE app.sales_posohina_id_seq OWNED BY app.sales_posohina.id;


--
-- Name: partner_types_posohina id; Type: DEFAULT; Schema: app; Owner: app
--

ALTER TABLE ONLY app.partner_types_posohina ALTER COLUMN id SET DEFAULT nextval('app.partner_types_posohina_id_seq'::regclass);


--
-- Name: partners_posohina id; Type: DEFAULT; Schema: app; Owner: app
--

ALTER TABLE ONLY app.partners_posohina ALTER COLUMN id SET DEFAULT nextval('app.partners_posohina_id_seq'::regclass);


--
-- Name: products_posohina id; Type: DEFAULT; Schema: app; Owner: app
--

ALTER TABLE ONLY app.products_posohina ALTER COLUMN id SET DEFAULT nextval('app.products_posohina_id_seq'::regclass);


--
-- Name: sales_posohina id; Type: DEFAULT; Schema: app; Owner: app
--

ALTER TABLE ONLY app.sales_posohina ALTER COLUMN id SET DEFAULT nextval('app.sales_posohina_id_seq'::regclass);


--
-- Data for Name: partner_types_posohina; Type: TABLE DATA; Schema: app; Owner: app
--

COPY app.partner_types_posohina (id, name) FROM stdin;
1	ООО
2	ЗАО
3	ИП
4	АО
5	ПАО
\.


--
-- Data for Name: partners_posohina; Type: TABLE DATA; Schema: app; Owner: app
--

COPY app.partners_posohina (id, type_id, name, director_fullname, phone, email, rating, discount, created_at, updated_at, address) FROM stdin;
1	1	Вектор	Смирнов Алексей Петрович	+7(911)222-33-44	vector@mail.ru	5	5	2026-03-15 01:35:50.766364	2026-03-15 01:35:50.818216	\N
2	2	Альянс	Козлова Елена Викторовна	+7(922)333-44-55	alyans@yandex.ru	4	5	2026-03-15 01:35:50.766364	2026-03-15 01:35:50.818216	\N
3	3	Соколов ИП	Соколов Дмитрий Игоревич	+7(933)444-55-66	sokolov@bk.ru	3	0	2026-03-15 01:35:50.766364	2026-03-15 02:07:00.003469	\N
5	1	123	123	123	123	123	5	2026-03-14 21:07:43.207831	2026-03-15 16:48:13.460764	указан
\.


--
-- Data for Name: products_posohina; Type: TABLE DATA; Schema: app; Owner: app
--

COPY app.products_posohina (id, name, code, cost) FROM stdin;
1	Товар А-1	COD001	1200.00
2	Товар Б-2	COD002	2800.00
3	Товар В-3	COD003	650.00
4	Товар Г-4	COD004	3500.00
5	Товар Д-5	COD005	1800.00
\.


--
-- Data for Name: sales_posohina; Type: TABLE DATA; Schema: app; Owner: app
--

COPY app.sales_posohina (id, partner_id, product_id, quantity, sale_date, total_sum, created_at) FROM stdin;
1	1	1	6000	2024-02-10	7200000.00	2026-03-15 01:35:50.793027
2	1	2	4000	2024-03-15	11200000.00	2026-03-15 01:35:50.793027
3	1	3	7000	2024-04-20	4550000.00	2026-03-15 01:35:50.793027
4	2	4	25000	2024-02-05	87500000.00	2026-03-15 01:35:50.793027
5	2	5	18000	2024-03-18	32400000.00	2026-03-15 01:35:50.793027
7	3	3	3000	2024-05-25	1950000.00	2026-03-15 01:35:50.793027
16	5	2	11111	2026-03-01	31110800.00	2026-03-14 21:08:23.789364
17	5	4	123	2026-02-24	430500.00	2026-03-14 21:15:30.763031
\.


--
-- Name: partner_types_posohina_id_seq; Type: SEQUENCE SET; Schema: app; Owner: app
--

SELECT pg_catalog.setval('app.partner_types_posohina_id_seq', 5, true);


--
-- Name: partners_posohina_id_seq; Type: SEQUENCE SET; Schema: app; Owner: app
--

SELECT pg_catalog.setval('app.partners_posohina_id_seq', 5, true);


--
-- Name: products_posohina_id_seq; Type: SEQUENCE SET; Schema: app; Owner: app
--

SELECT pg_catalog.setval('app.products_posohina_id_seq', 5, true);


--
-- Name: sales_posohina_id_seq; Type: SEQUENCE SET; Schema: app; Owner: app
--

SELECT pg_catalog.setval('app.sales_posohina_id_seq', 17, true);


--
-- Name: partner_types_posohina partner_types_posohina_name_key; Type: CONSTRAINT; Schema: app; Owner: app
--

ALTER TABLE ONLY app.partner_types_posohina
    ADD CONSTRAINT partner_types_posohina_name_key UNIQUE (name);


--
-- Name: partner_types_posohina partner_types_posohina_pkey; Type: CONSTRAINT; Schema: app; Owner: app
--

ALTER TABLE ONLY app.partner_types_posohina
    ADD CONSTRAINT partner_types_posohina_pkey PRIMARY KEY (id);


--
-- Name: partners_posohina partners_posohina_pkey; Type: CONSTRAINT; Schema: app; Owner: app
--

ALTER TABLE ONLY app.partners_posohina
    ADD CONSTRAINT partners_posohina_pkey PRIMARY KEY (id);


--
-- Name: products_posohina products_posohina_code_key; Type: CONSTRAINT; Schema: app; Owner: app
--

ALTER TABLE ONLY app.products_posohina
    ADD CONSTRAINT products_posohina_code_key UNIQUE (code);


--
-- Name: products_posohina products_posohina_pkey; Type: CONSTRAINT; Schema: app; Owner: app
--

ALTER TABLE ONLY app.products_posohina
    ADD CONSTRAINT products_posohina_pkey PRIMARY KEY (id);


--
-- Name: sales_posohina sales_posohina_pkey; Type: CONSTRAINT; Schema: app; Owner: app
--

ALTER TABLE ONLY app.sales_posohina
    ADD CONSTRAINT sales_posohina_pkey PRIMARY KEY (id);


--
-- Name: idx_partners_type_posohina; Type: INDEX; Schema: app; Owner: app
--

CREATE INDEX idx_partners_type_posohina ON app.partners_posohina USING btree (type_id);


--
-- Name: idx_sales_date_posohina; Type: INDEX; Schema: app; Owner: app
--

CREATE INDEX idx_sales_date_posohina ON app.sales_posohina USING btree (sale_date);


--
-- Name: idx_sales_partner_posohina; Type: INDEX; Schema: app; Owner: app
--

CREATE INDEX idx_sales_partner_posohina ON app.sales_posohina USING btree (partner_id);


--
-- Name: idx_sales_product_posohina; Type: INDEX; Schema: app; Owner: app
--

CREATE INDEX idx_sales_product_posohina ON app.sales_posohina USING btree (product_id);


--
-- Name: sales_posohina calculate_total_before_insert_posohina; Type: TRIGGER; Schema: app; Owner: app
--

CREATE TRIGGER calculate_total_before_insert_posohina BEFORE INSERT ON app.sales_posohina FOR EACH ROW EXECUTE FUNCTION app.calculate_total_sum_posohina();


--
-- Name: sales_posohina trigger_discount_calculation_posohina; Type: TRIGGER; Schema: app; Owner: app
--

CREATE TRIGGER trigger_discount_calculation_posohina AFTER INSERT OR DELETE OR UPDATE ON app.sales_posohina FOR EACH ROW EXECUTE FUNCTION app.trigger_discount_update_posohina();


--
-- Name: partners_posohina update_partners_timestamp_posohina; Type: TRIGGER; Schema: app; Owner: app
--

CREATE TRIGGER update_partners_timestamp_posohina BEFORE UPDATE ON app.partners_posohina FOR EACH ROW EXECUTE FUNCTION app.update_timestamp_posohina();


--
-- Name: partners_posohina partners_posohina_type_id_fkey; Type: FK CONSTRAINT; Schema: app; Owner: app
--

ALTER TABLE ONLY app.partners_posohina
    ADD CONSTRAINT partners_posohina_type_id_fkey FOREIGN KEY (type_id) REFERENCES app.partner_types_posohina(id) ON DELETE RESTRICT;


--
-- Name: sales_posohina sales_posohina_partner_id_fkey; Type: FK CONSTRAINT; Schema: app; Owner: app
--

ALTER TABLE ONLY app.sales_posohina
    ADD CONSTRAINT sales_posohina_partner_id_fkey FOREIGN KEY (partner_id) REFERENCES app.partners_posohina(id) ON DELETE CASCADE;


--
-- Name: sales_posohina sales_posohina_product_id_fkey; Type: FK CONSTRAINT; Schema: app; Owner: app
--

ALTER TABLE ONLY app.sales_posohina
    ADD CONSTRAINT sales_posohina_product_id_fkey FOREIGN KEY (product_id) REFERENCES app.products_posohina(id) ON DELETE RESTRICT;


--
-- PostgreSQL database dump complete
--

