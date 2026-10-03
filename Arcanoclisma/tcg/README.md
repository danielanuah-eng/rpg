# Arcanoclisma TCG

Motor de regras em TypeScript puro (sem dependências, Node 24+), pensado para depois ter interface HTML e ser portado para C# no Unity.

- `docs/LIVRO-DE-REGRAS.md` — regras e decisões pendentes (seção 12).
- `src/tipos.ts` — dados e comandos; `src/motor.ts` — regras; `src/cartas-teste.ts` — cartas PLACEHOLDER.
- Todo o jogo é feito por comandos (`executar(estado, comando)`), com sementes para repetir partidas.

```bash
npm test
```

Arte e cartas visuais: responsabilidade do Codex (ver `../../COLABORACAO-IAS.md`).
