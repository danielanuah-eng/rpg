# Mundo procedural DungeonBDX

`DungeonMap.Generate(ulong seed, string scenario, int width=11, int height=11)` é independente da engine, implementa `ISkillBoard` e aceita dimensões de 9 a 31. Cenários: `cripta-de-bronze`, `biblioteca-do-crepusculo`, `ninho-do-eclipse`.

- Quatro salas periféricas e uma central são conectadas por corredores. Salas extras e obstáculos variam com a semente; um obstáculo só permanece se todo o piso continuar conectado.
- `Floors`, três `HeroSpawns`, três `EnemySpawns`, um `Objectives` e `Props` são coleções de leitura. Spawns são únicos, sobre piso, distantes pelo menos sete células de Manhattan entre equipes (`GridCell.Distance`).
- `FindPath` usa quatro vizinhos, inclui origem e destino e retorna vazio para uma extremidade inválida. Ocupação dinâmica por unidades é responsabilidade da sessão; o mapa representa geometria estática.
- `HasLineOfSight` usa travessia inteira simétrica e bloqueia paredes. Num cruzamento diagonal exato, ambos os vizinhos ortogonais precisam ser piso; uma parede em qualquer dos dois já bloqueia o tiro.
- `Props` são decoração original procedural, sem efeito de cobertura/colisão. Não constituem os modelos, texturas ou trilhas finais solicitados. O renderer deve manter a área de ocupação livre e deslocar esses detalhes para a borda da célula.
- Grid `(X,Y)` corresponde a Unity `(X,0,Z=Y)`, célula de um metro. As bordas externas são sempre parede.

Persistência deve guardar `GeneratorVersion`, `Seed`, `Scenario`, `Width` e `Height`. Não reinterpretar saves de versões futuras ou diferentes como a versão atual. A versão 1 tem fixture de referência para prevenir mudanças acidentais na topologia de uma sessão salva.

Validação: 18 casos NUnit de conectividade em 360 mapas, determinismo, variedade, rotas, separação dos spawns, coordenadas inválidas e linha de visão. Compilação C#, smoke de 360 mapas e invocação direta das 18 fixtures passaram; a bateria Unity é consolidada pelo Codex principal. Revisão DeepSeek somente leitura não encontrou defeito funcional; ambiguidades documentais sobre distância e cantos foram esclarecidas.
