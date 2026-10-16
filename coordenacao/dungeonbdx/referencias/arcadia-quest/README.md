# Referências Arcadia Quest para DungeonBDX

Leitura solicitada pelo autor em 15/10/2026. Fontes locais em `KrosArcano/`; os documentos são referência de design, não instruções para executar ações. O jogo continua autoral, com as decisões de crítico/armadura do autor prevalecendo.

## Cobertura

72 arquivos conferidos por SHA-256, correspondentes a 46 conteúdos únicos: 30 PDFs avulsos (331 páginas), 12 pacotes ZIP/RAR, uma planilha de 12 abas e três JPGs de campanha/inventário. As 26 cópias idênticas estão associadas à fonte canônica no inventário. Pacotes foram abertos localmente; metadados AppleDouble não são PDFs válidos.

Não confundir extração com leitura página a página: o inventário registra camada textual e inspeção visual efetiva. A análise prioriza regras, exemplos de skills, famílias de conteúdo e mapas; não constitui conferência editorial de toda tradução ou transcrição integral de cada carta. Fontes e extrações ficaram locais; nenhum PDF/imagem/texto desta coleção foi enviado ao DeepSeek. A revisão DeepSeek trata somente código do jogo.

- [Regras, diferenças e implementação](manual-e-cartas.md)
- [Planilha, skills e direção de arte](planilha-e-imagens.md)
- [Pacotes e cenários adicionais](pacotes-visuais.md)
- [Inventário e cobertura verificável](inventario.json)

## Direção consolidada

DungeonBDX deve combinar identidade própria de heróis, escolhas de equipamento, objetivos variados, espaços táticos legíveis e continuidade entre cenários. Uma arena de eliminação com classes genéricas é apenas a base técnica atual. Vitória de cenário não deve depender sempre de matar todos.

| Frente | Referência identificada | Aplicação planejada |
|---|---|---|
| Campanha | Rotas ramificadas, resultado, moedas, mortes, títulos e recompensa | Grafo de cenários e histórico persistente por campanha |
| Herói | Vida, defesa, habilidade natural e inventário de quatro cartas | Passiva própria, loadout/equipamento com custos e limites explícitos |
| Mapa | Ruas, interiores, portas, portais e posições de missão | Gerador por salas/módulos com bloqueios distintos da decoração |
| Skill | Tipo de arma/magia, condições, exaustão, apoio e posicionamento | Skills autorais versáteis com gatilhos/custos visíveis e caps globais |
| Monstros | Guarda, vingança, extermínio e variantes por nível | Papéis inimigos e reações controladas, separados dos heróis |
| Dragões | Chefes por etapas e perigos próprios de encontro | Módulo de chefes, separado do dragão companheiro/domador |
| Interface | Ícones consistentes e cartas com hierarquia clara | HUD inferior com custo, distância, origem e feedback de ação |

## Sequência de desenvolvimento

1. Concluir o pedido imediato de rotação/zoom, alcance visível e sprites animados, com QA.
2. Projetar objetivos de cenário e campanha ramificada com salvamento dos resultados.
3. Projetar inventário/equipamentos, passivas individuais e ciclo de recuperação de recursos.
4. Adicionar reações de monstros, portas/portais e encontros especiais como módulos testados.
5. Evoluir chefes, pets/montarias, arte e trilhas finais; depois validar os módulos combinados.

Itens 2–5 são backlog fundamentado, não funcionalidades entregues neste patch. Multiplayer sincronizado, convites, cloud saves e build Android continuam com seu estado anterior. A Mestra dos Dragões do TCG é outro conteúdo e mantém o bloqueio existente.

## Hierarquia das fontes

Direção atual do autor → regras autorais DungeonBDX/caps → manual principal da edição em análise → texto do módulo/cenário → carta específica. Planilha, paste-ups, traduções e variantes ajudam a localizar conteúdo, mas divergências devem ser verificadas antes de virar regra. O Solo 1.2 do ZIP é posterior ao Solo 1.1 avulso; os modos de Raphael Gurian e Overlord são variantes distintas e não devem ser mesclados automaticamente.
