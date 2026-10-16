using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DungeonBDX
{
    // A tap commits on release. A gesture/HUD crossing latches cancellation until all
    // fingers release, so the remaining finger from a pinch cannot issue a command.
    public sealed class DungeonCameraInput
    {
        bool down,cancelled,multi,touchSequence,cameraDrag,dragBlocked;
        Vector2 start,lastDrag,lastCenter,lastSpan;
        public bool GestureBusy => multi || cameraDrag;

        public void PointerDown(Vector2 point,bool blocked)
        {down=true;cancelled=blocked;start=point;}
        public void PointerMoved(Vector2 point,bool blocked)
        {if(down && (blocked || (point-start).sqrMagnitude>100))cancelled=true;}
        public void CancelGesture() {cancelled=true;multi=true;}
        public bool PointerUp(Vector2 point,bool blocked)
        {
            bool tap=down && !cancelled && !multi && !blocked && (point-start).sqrMagnitude<=100;
            down=false;cancelled=false;return tap;
        }
        public void Reset() {down=cancelled=multi=touchSequence=cameraDrag=dragBlocked=false;}

        public bool Read(DungeonWorldView world,Func<Vector2,bool> hitsHud,out Vector2 tap)
        {
            tap=default;
            var touches=Touchscreen.current?.touches.Where(t=>t.press.isPressed).Take(2).ToArray();
            int count=touches?.Length??0;
            if(count>0 || touchSequence)
            {
                if(count>=2)
                {
                    Vector2 a=touches[0].position.ReadValue(),b=touches[1].position.ReadValue(),center=(a+b)*.5f,span=b-a;
                    bool blocked=hitsHud(a)||hitsHud(b)||hitsHud(center);
                    if(!multi)
                    {
                        bool priorBlocked=down&&cancelled;
                        CancelGesture();dragBlocked=priorBlocked||blocked;lastCenter=center;lastSpan=span;
                    }
                    else
                    {
                        dragBlocked|=blocked;
                        if(!dragBlocked)
                        {
                            if(lastSpan.magnitude>20 && span.magnitude>20)
                            {world.ZoomCamera(span.magnitude/lastSpan.magnitude);world.RotateCamera(Vector2.SignedAngle(lastSpan,span));}
                            world.PanCamera(center-lastCenter);
                        }
                        lastCenter=center;lastSpan=span;
                    }
                    touchSequence=true;return false;
                }
                if(count==1)
                {
                    var point=touches[0].position.ReadValue();
                    if(!touchSequence)PointerDown(point,hitsHud(point));else PointerMoved(point,hitsHud(point));
                    lastDrag=point;touchSequence=true;return false;
                }
                bool accepted=PointerUp(lastDrag,hitsHud(lastDrag));Reset();tap=lastDrag;return accepted;
            }
            var mouse=Mouse.current;if(mouse==null)return false;
            Vector2 position=mouse.position.ReadValue();bool overHud=hitsHud(position);
            if(mouse.rightButton.wasPressedThisFrame || mouse.middleButton.wasPressedThisFrame)
            {cameraDrag=true;dragBlocked=overHud;lastDrag=position;cancelled=true;}
            if(cameraDrag)
            {
                dragBlocked|=overHud;
                if(!dragBlocked)
                {
                    if(mouse.rightButton.isPressed)world.RotateCamera((position.x-lastDrag.x)*.38f);
                    if(mouse.middleButton.isPressed)world.PanCamera(position-lastDrag);
                }
                lastDrag=position;
                if(!mouse.rightButton.isPressed&&!mouse.middleButton.isPressed)cameraDrag=false;
            }
            if(!overHud && Mathf.Abs(mouse.scroll.ReadValue().y)>.01f)
            {
                if(down)cancelled=true;
                world.ZoomCamera(Mathf.Exp(mouse.scroll.ReadValue().y*.0015f));
            }
            if(mouse.leftButton.wasPressedThisFrame)PointerDown(position,overHud||cameraDrag);
            if(mouse.leftButton.isPressed)PointerMoved(position,overHud||cameraDrag);
            if(mouse.leftButton.wasReleasedThisFrame)
            {tap=position;return PointerUp(position,overHud||cameraDrag);}
            return false;
        }
    }
}
