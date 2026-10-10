# Fase 9 — VFX, áudio, rede e exportação

Implementada na cena principal, Godot 4.6.1. Não é necessário colar código.

- `scripts/VisualFXManager.gd`: efeitos de ataque, sopro, morte e dano flutuante; Host transmite coordenadas por RPC confiável, clientes somente apresentam. IDs descartam repetição. Efeitos transitórios não são reproduzidos no snapshot tardio.
- `scripts/AudioManager.gd`: pool de 12 vozes, quatro WAV locais, liberação por interação no navegador, aviso de recurso ausente. Não transmite áudio pela rede.
- `audio/sfx/*.wav`: sons básicos sintetizados; procedimento reproduzível em `tools/generate_sfx.mjs`.
- `NetworkManager.gd`: snapshots aguardam ação atual, removem entidades derrotadas antes de aplicar posições/HP/PA; desconexão remove P2 e libera movimento sem encerrar Host. Reconexão não ressuscita P2.
- Monstros usam ataque comum por padrão. O sopro está implementado e validado; um monstro com metadado `vfx = "breath"` o utiliza. A cena existente contém peças geométricas, sem modelo de dragão; nenhum modelo foi substituído.

## Verificação

`node tools/test_phase9.mjs` executa testes locais e dois processos Godot via WebSocket loopback. Precisa de permissão para conexão local.

Resultado: **1.005 verificações, zero falhas**. Inclui grade, movimento, turnos, combate, mortes, entrada durante ação, HP/PA/posição tardios, recusa de efeitos forjados, sequência dos efeitos, desconexão durante movimento e reconexão.

Pacote de recursos exportado: `build/phase9.pck`. Inclui scripts compilados, cenas e os quatro sons importados; não é um executável independente.

Teste gráfico `tests/test_phase9_visual.gd`: partículas, números de dano, reprodução de SFX e liberação de efeitos passaram. Captura em `.artifacts/phase9/vfx.png`; logs na mesma pasta. O ambiente restrito impediu criar o cache de shaders em `user://`; a renderização e o teste completaram mesmo assim.

O teste legado de turnos foi atualizado para aguardar o sinal de término da IA e usar uma casa que não esteja ocupada pelos monstros atuais. Nenhuma regra de turno foi alterada.

## Checklist de exportação

- Presets Windows Desktop, Web e Android em `export_presets.cfg`, com todos os recursos e exclusão de testes/ferramentas/entregas. Instalar templates **4.6.1** para produzir executáveis; não encontrados nesta instalação.
- Release exige `wss://`. WS está liberado apenas por padrão em builds de depuração; uma página HTTPS sempre exige WSS. Não há fallback silencioso para WS nem desativação da validação TLS.
- Host nativo: TCP 9080. Produção: proxy TLS com certificado válido e suporte a WebSocket Upgrade, publicando WSS na porta 443 e encaminhando para WS no Host. O domínio e certificado do serviço de produção ainda não estão configurados.
- Web: Compatibility, HTTPS, `.wasm` com `application/wasm`, `.pck` com `application/octet-stream`, arquivos do mesmo export publicados juntos; gesto do usuário libera áudio. Preset sem threads. Se habilitar threads, servir `Cross-Origin-Opener-Policy: same-origin` e `Cross-Origin-Embedder-Policy: require-corp`, com recursos externos compatíveis com isolamento.
- Android: `permissions/internet=true`, ARM64, pacote `org.arcanoclisma.arenatatica`, versão `0.9.0`, código `9`. Configurar SDK/JDK, ícones finais e assinatura release. Para loja, habilitar Gradle/AAB e usar keystore próprio; nenhum segredo foi criado ou salvo.
- Validar a build Web no navegador e Android no aparelho após instalar templates/SDK. Testes locais não substituem esses testes de plataforma.

Referências: [Web](https://docs.godotengine.org/en/4.5/tutorials/export/exporting_for_web.html), [Android](https://docs.godotengine.org/en/stable/tutorials/export/exporting_for_android.html).
