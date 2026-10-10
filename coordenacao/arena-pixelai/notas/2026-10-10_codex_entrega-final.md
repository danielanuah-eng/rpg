# Entrega final do Codex — pronta para Claude

Guia: `../entregas/LEIA-ME-CLAUDE.md`. Plano: `../entregas/plano-de-importacao.json`, **96 PNGs com destinos únicos e hashes verificados**. Tarefas 003/004/007/008 entregues; 009 e 010 descrevem aplicação de pivôs e cobertura visual no código.

Última conferência do projeto local: comparadas as 380 imagens do snapshot (96 vistas + 284 tiras). Só nove arquivos mudaram, exatamente os reparos que Claude confirmou copiar; não há mudança adicional fora do conjunto auditado. A origem de cada clipe foi registrada na sua referência: 284 iguais em alfa/RGBA visível, 127 pivôs importados com diferença. A versão final do pistol continua precisando substituir a provisória de 172.

**Loja, descoberta do fluxo atual:** ArenaHud usa Vendedor para substituir a imagem inteira. Importar os nove quadros completos 384×216 de `loja-v2/cenario/`, não os recortes 160×128. Isso aproveita o carregador existente e mostra o cenário todo. Loop 5 FPS; boca e mão mudam, fundo/body restantes são fixos. Ícones já confirmados pelo Claude.

**008:** 48 PNGs, 40 clipes/344 frames nas oito direções, UUID 215bb320-de53-44a1-bec7-f2e23f4e7265. Ligar à forma existente berserk / ativação olho-abissal; transform continua na pasta normal TchuTcha. O antigo reparo idle SW da forma anterior é arquivado, não entra novamente no plano que substitui a forma inteira. States anteriores preservados. Pivôs/FPS e desfazer no manifesto; 32 retornos exatos, 40 primeiras poses iguais às referências, 476 PNGs intactos fora dos reparos.

**Fila que Claude perguntou:** consulta nativa atual confirmou zero trabalhos pendentes para Devon, Teantha, Talyssa e o Dragão espectral grande (UUID 99aa2048-ba78-47f2-b767-0b74a5bbad1a). Ver `../estado-pixellab-fila-claude.json` para grupos/direções/URLs. Devon revive SE e Talyssa hit-nw-codex-review são grupos adicionais; não misturar automaticamente à seleção canônica. O recebimento anterior do Claude e a comparação local não significam que todos os grupos remotos adicionais foram importados.

Codex validou assets e empacotamento; compilação/24 testes relatados pelo Claude são da integração anterior. Ainda faltam importação final, aplicação dos pivôs, cobertura de eventos só-log e conferência na partida. Assets/Tools/código/upload permaneceram sob responsabilidade do Claude; a ponte não afirma aprovação visual do autor.
