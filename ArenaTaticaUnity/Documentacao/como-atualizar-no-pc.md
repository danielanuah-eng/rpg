# Como atualizar a Arena Tática no PC sem perder nada

As fichas editadas, os mapas e as opções **não ficam na pasta do jogo**. Ficam no seu usuário do Windows:

```
C:\Users\<seu usuário>\AppData\LocalLow\Arcanoclisma\Arena Tática\
  arena-edicoes.json   fichas editadas (aba Editor)
  arena-mapas.json     mapas do editor de mapas
  arena-presets.json   presets salvos
```

As opções (time, loja, tempo de turno, som da L.E.L.A) ficam no registro do Windows. Trocar ou apagar a pasta do jogo não mexe em nada disso.

## Pelo app do itch (mais fácil)
1. Abra o app do itch.io.
2. Em **Library**, ache **BDX Master / Arena Tática**. Quando houver versão nova, aparece **Update**.
3. Clique em **Update**. O app troca só os arquivos do jogo.

## Pelo site (zip)
1. Baixe o `.zip` novo na página do jogo no itch.
2. **Feche o jogo.**
3. Extraia o zip numa pasta nova, por exemplo `Arena Tática 1.2.12`.
4. Rode o `.exe` da pasta nova. Fichas e mapas aparecem iguais.
5. Se tudo estiver certo, apague a pasta da versão antiga.

Não extraia o zip por cima da pasta antiga: arquivos velhos podem sobrar misturados.

## Backup (recomendado antes de atualizar)
1. Aperte `Win + R`, cole o caminho abaixo e dê Enter:
   ```
   %USERPROFILE%\AppData\LocalLow\Arcanoclisma\Arena Tática
   ```
2. Copie os arquivos `arena-*.json` para outro lugar (pen drive, nuvem).
3. Para restaurar: feche o jogo e cole os arquivos de volta na mesma pasta.

## Outro PC
Copie os `arena-*.json` para a mesma pasta no outro PC. Abra o jogo uma vez antes, para a pasta ser criada.
