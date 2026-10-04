-- 04/10/2026: marcação Yes/No do autor nas opiniões e ideias (o que o Claude lê no diagnóstico).
-- Rodar uma vez em bancos criados antes desta data:
--   node node_modules/wrangler/bin/wrangler.js d1 execute arcanoclisma --remote --file=migracoes/0002-claude-yes-no.sql
ALTER TABLE opinioes ADD COLUMN claude INTEGER;
ALTER TABLE ideias ADD COLUMN claude INTEGER;
