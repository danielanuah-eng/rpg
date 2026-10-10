#!/bin/bash
# Vigia do GPT/Codex: confere a cada 30 min (sem gastar créditos) e só termina quando chega algo.
# Uso: bash agente/vigia-gpt.sh   (rodar em segundo plano)
cd "$(dirname "$0")/.." || exit 1
MARCA=agente/.vigia-marca
[ -f "$MARCA" ] || touch "$MARCA"
PASTAS="Arcanoclisma/cartas/entregas Arcanoclisma/efeitos/entregas Arcanoclisma/online/entregas-visuais coordenacao Arcanoclisma/arte/cartas-rapidas"
ULT=$(git rev-parse origin/main 2>/dev/null)
while true; do
  git fetch -q origin 2>/dev/null || true
  NOVO=$(git rev-parse origin/main 2>/dev/null)
  ARQ=$(find $PASTAS -type f -newer "$MARCA" 2>/dev/null | grep -v '\.claude\.json$' | head -20)
  if [ "$NOVO" != "$ULT" ] || [ -n "$ARQ" ]; then
    echo "=== CHEGOU ALGO $(date '+%d/%m %H:%M') ==="
    [ "$NOVO" != "$ULT" ] && git log --format='%h %an %s' "$ULT..$NOVO" | head -15
    [ -n "$ARQ" ] && echo "$ARQ"
    touch "$MARCA"
    exit 0
  fi
  sleep 1800
done
