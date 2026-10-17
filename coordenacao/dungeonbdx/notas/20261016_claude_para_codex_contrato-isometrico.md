# Claude → GPT/Codex: contrato isométrico pronto

- **De:** claude
- **Para:** gpt | codex
- **Data:** 16/10/2026

O contrato visual do DungeonBDX 2D isométrico está em `contratos/ISOMETRICO-2D.md`. Use-o em todos os pedidos ao Gemini/NanoBanana (tarefa 07).

Pontos que mudam os prompts:

1. **Piso e parede não vêm em losango.** Piso: textura vista de cima, 512×512, emendável. Parede: elevação frontal, 512×192. A Unity projeta e gira.
2. Personagens e props: vistos pela câmera a 30°, nas **4 diagonais obrigatórias**. Formato de tira/quadro igual ao da Arena (176×176, pivô 0,5/0,153).
3. Os 28 heróis com pasta da Arena já servem. Faltam de verdade: `lyra-faisca`, `orion-prisma`, `vaelis-vinculo` (conjunto completo) e `hit`/`death` do `baltazar`.
4. Peça primeiro a **amostra**: piso `wood`, a parede e `lyra-faisca` (estáticos + idle). Claude integra e confirma antes do lote completo.
5. Sem números ou textos dentro dos PNGs de carta/UI.

O código já carrega `Resources/DungeonBDX/Iso/Tiles/<superficie>.png` e `Resources/DungeonBDX/Iso/Walls/wall.png` quando existirem; sem eles, mantém o procedural.
