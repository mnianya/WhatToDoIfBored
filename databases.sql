--
-- PostgreSQL database dump
--

-- Dumped from database version 17.2
-- Dumped by pg_dump version 17.2

-- Started on 2025-12-01 13:46:49

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 220 (class 1259 OID 16445)
-- Name: completed_tasks; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.completed_tasks (
    task_id integer NOT NULL,
    user_id integer NOT NULL,
    task_name text NOT NULL
);


ALTER TABLE public.completed_tasks OWNER TO postgres;

--
-- TOC entry 219 (class 1259 OID 16444)
-- Name: completed_tasks_task_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.completed_tasks_task_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.completed_tasks_task_id_seq OWNER TO postgres;

--
-- TOC entry 4870 (class 0 OID 0)
-- Dependencies: 219
-- Name: completed_tasks_task_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.completed_tasks_task_id_seq OWNED BY public.completed_tasks.task_id;


--
-- TOC entry 222 (class 1259 OID 16460)
-- Name: liked_tasks; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.liked_tasks (
    like_id integer NOT NULL,
    user_id integer NOT NULL,
    task_name text NOT NULL
);


ALTER TABLE public.liked_tasks OWNER TO postgres;

--
-- TOC entry 221 (class 1259 OID 16459)
-- Name: liked_tasks_like_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.liked_tasks_like_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.liked_tasks_like_id_seq OWNER TO postgres;

--
-- TOC entry 4871 (class 0 OID 0)
-- Dependencies: 221
-- Name: liked_tasks_like_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.liked_tasks_like_id_seq OWNED BY public.liked_tasks.like_id;


--
-- TOC entry 218 (class 1259 OID 16434)
-- Name: users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.users (
    user_id integer NOT NULL,
    username character varying(50) NOT NULL,
    email character varying(100) NOT NULL,
    password character varying(100) NOT NULL
);


ALTER TABLE public.users OWNER TO postgres;

--
-- TOC entry 217 (class 1259 OID 16433)
-- Name: users_user_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.users_user_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.users_user_id_seq OWNER TO postgres;

--
-- TOC entry 4872 (class 0 OID 0)
-- Dependencies: 217
-- Name: users_user_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.users_user_id_seq OWNED BY public.users.user_id;


--
-- TOC entry 4706 (class 2604 OID 16448)
-- Name: completed_tasks task_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.completed_tasks ALTER COLUMN task_id SET DEFAULT nextval('public.completed_tasks_task_id_seq'::regclass);


--
-- TOC entry 4707 (class 2604 OID 16463)
-- Name: liked_tasks like_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.liked_tasks ALTER COLUMN like_id SET DEFAULT nextval('public.liked_tasks_like_id_seq'::regclass);


--
-- TOC entry 4705 (class 2604 OID 16437)
-- Name: users user_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users ALTER COLUMN user_id SET DEFAULT nextval('public.users_user_id_seq'::regclass);


--
-- TOC entry 4715 (class 2606 OID 16453)
-- Name: completed_tasks completed_tasks_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.completed_tasks
    ADD CONSTRAINT completed_tasks_pkey PRIMARY KEY (task_id);


--
-- TOC entry 4717 (class 2606 OID 16468)
-- Name: liked_tasks liked_tasks_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.liked_tasks
    ADD CONSTRAINT liked_tasks_pkey PRIMARY KEY (like_id);


--
-- TOC entry 4709 (class 2606 OID 16443)
-- Name: users users_email_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_email_key UNIQUE (email);


--
-- TOC entry 4711 (class 2606 OID 16439)
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (user_id);


--
-- TOC entry 4713 (class 2606 OID 16441)
-- Name: users users_username_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_username_key UNIQUE (username);


--
-- TOC entry 4718 (class 2606 OID 16454)
-- Name: completed_tasks completed_tasks_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.completed_tasks
    ADD CONSTRAINT completed_tasks_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(user_id) ON DELETE CASCADE;


--
-- TOC entry 4719 (class 2606 OID 16469)
-- Name: liked_tasks liked_tasks_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.liked_tasks
    ADD CONSTRAINT liked_tasks_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(user_id) ON DELETE CASCADE;


-- Completed on 2025-12-01 13:46:50

--
-- PostgreSQL database dump complete
--

