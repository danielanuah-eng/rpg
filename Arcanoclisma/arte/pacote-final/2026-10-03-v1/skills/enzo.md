---
name: enzo
description: Arquivar imagens, cenários, personagens, estruturas e variações de movimento com descrições, prompts e definições reutilizáveis de agentes para projetos futuros.
---

# Enzo

Biblioteca local: `C:/Users/Anuah/OneDrive/Área de Trabalho/Biblioteca-Enzo`. Ler seu `AGENTS.md`. Guardar os arquivos originais e suas versões, prompts, descrições e procedência em formatos abertos. Indexar material do Claude somente quando houver acesso autorizado ao arquivo ou à descrição; nunca inventar a origem.

Para arquivar Arcanoclisma, executar:

```powershell
node "C:/Users/Anuah/OneDrive/Área de Trabalho/Biblioteca-Enzo/scripts/arquivar.mjs" "C:/Users/Anuah/OneDrive/Área de Trabalho/Rpg/Arcanoclisma/arte"
```

Para outro projeto com a mesma estrutura, fornecer sua pasta de arte. Para estruturas diferentes, organizar os assets e um manifesto equivalente, preservando nome, tipo, projeto, caminho, versões, descrição, prompt disponível, ferramenta, aprovação e referências. Guardar poses, movimentos e animações como variações relacionadas ao personagem; não inferir uma animação completa a partir de uma única pose.

Guardar definições de agentes e skills em `definicoes/`. Na reutilização por outra IA, explicar ferramentas e permissões necessárias. Essas definições permitem repetir o fluxo; não garantem resultado visual idêntico. Após cada meta, guardar a aprovação ou o motivo de revisão informado pelo autor. Nunca guardar credenciais nem publicar sem autorização.
