using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArenaTatica.Core;

namespace ArenaTatica
{
    public sealed partial class ArenaPresentation
    {
        // Assinatura visual por habilidade para conjurações sem arte dedicada.
        // Cor e desenho são determinísticos pelo id e ficam iguais em todos os clientes.
        IEnumerator SkillGlyph(string skill,Vector3 at)
        {
            if(string.IsNullOrEmpty(skill)||Content.AnySkill(skill)==null||transient.Count>=48) yield break;
            uint signature=2166136261;
            foreach(char c in skill) signature=(signature^c)*16777619;
            var color=Style(skill).color;
            var root=Effect("Sigilo_"+skill);
            var lines=new List<LineRenderer>();
            for(int i=0;i<12;i++) {
                var line=Line(root.transform,color,.028f);
                line.positionCount=3; line.numCapVertices=0; lines.Add(line);
            }
            float elapsed=0, duration=.55f;
            while(elapsed<duration&&root) {
                elapsed+=Time.deltaTime;
                float p=Mathf.Clamp01(elapsed/duration),radius=.25f+.45f*Mathf.Sin(p*Mathf.PI*.65f);
                for(int i=0;i<lines.Count;i++) {
                    float angle=(i*30+(signature%30))*Mathf.Deg2Rad;
                    var dir=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
                    var side=new Vector3(-dir.z,0,dir.x);
                    int mark=(int)((signature>>(i*2))&3);
                    Vector3 center=at+Vector3.up*.06f+dir*radius;
                    float length=.06f+.025f*mark;
                    lines[i].SetPosition(0,center-dir*length);
                    lines[i].SetPosition(1,center+side*(mark%2==0?.045f:-.045f));
                    lines[i].SetPosition(2,center+dir*length);
                    var fade=color; fade.a=Mathf.Sin(p*Mathf.PI);
                    lines[i].startColor=lines[i].endColor=fade;
                }
                yield return null;
            }
            if(root) { transient.Remove(root); Destroy(root); }
        }
    }
}

