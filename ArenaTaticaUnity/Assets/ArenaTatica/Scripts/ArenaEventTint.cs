using System;
using System.Collections.Generic;
using ArenaTatica.Core;
using UnityEngine;

namespace ArenaTatica
{
    // Cor do evento de dados ativo (MapEventDef.tint, ex.: Lua de Sangue vermelha): puxa a luz direcional e o ambiente para a cor
    // enquanto o evento durar e devolve as de antes no fim. Isolado de propósito: quando a iluminação global expuser o gancho de
    // cor ambiente, basta ligar Hook nele (com Hook ligado, esta tinta simples não mexe nas luzes).
    public static class ArenaEventTint
    {
        public const float Strength=.55f;
        public static Action<Color?> Hook;
        // Cor em vigor (nulo = nenhum evento com cor).
        public static Color? Current { get; private set; }
        static string shown;
        static bool tinted; static Color ambientBefore, ambientTinted;
        static readonly Dictionary<Light,(Color before,Color tinted)> lights=new Dictionary<Light,(Color,Color)>();

        public static void Sync(string ev)
        {
            var hex=MapEvents.Get(ev)?.tint;
            if(hex==shown) return;
            Restore(); shown=hex; Current=null;
            if(string.IsNullOrEmpty(hex)||!ColorUtility.TryParseHtmlString(hex,out var c)) { Hook?.Invoke(null); return; }
            Current=c;
            if(Hook!=null) { Hook(c); return; }
            tinted=true; ambientBefore=RenderSettings.ambientLight;
            RenderSettings.ambientLight=ambientTinted=Color.Lerp(ambientBefore,c,Strength);
            foreach(var l in UnityEngine.Object.FindObjectsByType<Light>()) { var t=Color.Lerp(l.color,c,Strength); lights[l]=(l.color,t); l.color=t; }
        }
        // Devolve só o que ainda está com a tinta: se o cenário trocou o ambiente no meio (partida nova), vale o do cenário.
        static void Restore()
        {
            if(!tinted) return; tinted=false;
            if(RenderSettings.ambientLight==ambientTinted) RenderSettings.ambientLight=ambientBefore;
            foreach(var kv in lights) if(kv.Key&&kv.Key.color==kv.Value.tinted) kv.Key.color=kv.Value.before;
            lights.Clear();
        }
    }
}
