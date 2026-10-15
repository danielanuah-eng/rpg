# Revisão mecânica 01

DeepSeek/OpenRouter, leitura exclusiva de cinco arquivos novos. Sem falha confirmada de raycast, EventSystem, destruição de filhos, snapshots, paginação ou preservação de cena/settings. Ajustada inconsistência `HasMechanic(None)`: retorna false; teste incluído.

Pendências `?`: Input System já é dependência instalada no projeto (compilação anterior passou); fontes TMP ausentes causaram a falha real 9/10, importação oficial não interativa adicionada ao Prepare. Nomes/skills de origem recebem asserções no teste de importação. Build e execução visual ficam com Codex, não foram feitos pelo revisor. Fontes e recursos de TMP devem ficar integralmente no módulo novo.

Relatório isolado usou 2 passos. O levantamento inicial teve leitura excessiva do catálogo de arquivos pelo agente; futuras revisões devem ficar na pasta estrita do módulo e em arquivos explícitos.
