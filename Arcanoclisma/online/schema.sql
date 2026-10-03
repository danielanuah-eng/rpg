-- Banco D1 do Arcanoclisma online. Rodar: npm run db:local (teste) ou npm run db:remoto (produção).
CREATE TABLE IF NOT EXISTS usuarios (
  id TEXT PRIMARY KEY, email TEXT NOT NULL UNIQUE, nome TEXT NOT NULL,
  senha TEXT, sal TEXT, google TEXT, verificado INTEGER NOT NULL DEFAULT 0, criado INTEGER NOT NULL
);
CREATE TABLE IF NOT EXISTS sessoes (token TEXT PRIMARY KEY, usuario TEXT NOT NULL, expira INTEGER NOT NULL);
CREATE TABLE IF NOT EXISTS baralhos (
  id TEXT PRIMARY KEY, usuario TEXT NOT NULL, nome TEXT NOT NULL, conjurador TEXT NOT NULL,
  lista TEXT NOT NULL, itens TEXT NOT NULL, publico INTEGER NOT NULL DEFAULT 0,
  criado INTEGER NOT NULL, atualizado INTEGER NOT NULL
);
CREATE INDEX IF NOT EXISTS baralhos_usuario ON baralhos(usuario);
CREATE INDEX IF NOT EXISTS baralhos_publico ON baralhos(publico, atualizado);
CREATE TABLE IF NOT EXISTS partidas (
  id TEXT PRIMARY KEY, modo TEXT NOT NULL, j0 TEXT, j1 TEXT, conj0 TEXT, conj1 TEXT,
  vencedor INTEGER, turnos INTEGER, motivo TEXT, inicio INTEGER, fim INTEGER, versao TEXT
);
CREATE INDEX IF NOT EXISTS partidas_fim ON partidas(fim);
CREATE TABLE IF NOT EXISTS opinioes (
  id TEXT PRIMARY KEY, usuario TEXT NOT NULL, tipo TEXT NOT NULL, nota INTEGER, texto TEXT NOT NULL,
  contexto TEXT, estado TEXT NOT NULL DEFAULT 'novo', resposta TEXT, pai TEXT, criado INTEGER NOT NULL
);
CREATE INDEX IF NOT EXISTS opinioes_criado ON opinioes(criado);
CREATE INDEX IF NOT EXISTS opinioes_pai ON opinioes(pai);
CREATE TABLE IF NOT EXISTS ideias (
  id TEXT PRIMARY KEY, usuario TEXT NOT NULL, categoria TEXT NOT NULL, titulo TEXT NOT NULL, texto TEXT NOT NULL,
  estado TEXT NOT NULL DEFAULT 'nova', resposta TEXT, votos INTEGER NOT NULL DEFAULT 0, criado INTEGER NOT NULL
);
CREATE TABLE IF NOT EXISTS votos (ideia TEXT NOT NULL, usuario TEXT NOT NULL, PRIMARY KEY (ideia, usuario));
-- Edições do autor (tela Editor): uma linha por salvamento; a mais nova vale. A sala online guarda a versão do início da partida.
CREATE TABLE IF NOT EXISTS edicoes (
  id INTEGER PRIMARY KEY AUTOINCREMENT, dados TEXT NOT NULL, nota TEXT, usuario TEXT NOT NULL, criado INTEGER NOT NULL
);
