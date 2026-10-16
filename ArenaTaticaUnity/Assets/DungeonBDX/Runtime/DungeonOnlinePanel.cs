using System;
using System.Threading.Tasks;
using DungeonBDX.Online;
using TMPro;
using UnityEngine;

namespace DungeonBDX
{
    // Explicit user actions only: constructing the panel never initializes cloud services.
    public sealed class DungeonOnlinePanel : MonoBehaviour
    {
        DungeonOnlineClient client;
        TMP_InputField username, password, code, roomPassword;
        TextMeshProUGUI state;
        public void Mount(DungeonOnlineClient connection,Action back)
        {
            client=connection;
            Label("CONTAS E SALAS PRIVADAS",new Vector2(.04f,.88f),new Vector2(.96f,.98f),23);
            Label("Salas em integração. Combate online e convites no jogo ainda indisponíveis.",new Vector2(.04f,.79f),new Vector2(.96f,.88f),14);
            username=Field("Usuário",new Vector2(.04f,.65f),new Vector2(.48f,.78f),false);
            password=Field("Senha da conta",new Vector2(.50f,.65f),new Vector2(.96f,.78f),true);
            Button("ENTRAR",new Vector2(.04f,.52f),new Vector2(.48f,.64f),()=>Authenticate(false));
            Button("CRIAR CONTA",new Vector2(.50f,.52f),new Vector2(.96f,.64f),()=>Authenticate(true));
            code=Field("Código da sala",new Vector2(.04f,.38f),new Vector2(.48f,.51f),false);
            roomPassword=Field("Senha da sala (opcional)",new Vector2(.50f,.38f),new Vector2(.96f,.51f),true);
            Button("CRIAR SALA",new Vector2(.04f,.25f),new Vector2(.48f,.37f),()=>RunRoom(true));
            Button("ENTRAR NA SALA",new Vector2(.50f,.25f),new Vector2(.96f,.37f),()=>RunRoom(false));
            state=Label("",new Vector2(.04f,.12f),new Vector2(.96f,.24f),14);
            Button("VOLTAR",new Vector2(.04f,.01f),new Vector2(.33f,.12f),back);
            Button("SAIR DA SALA",new Vector2(.34f,.01f),new Vector2(.65f,.12f),()=>Run(client.LeaveRoomAsync));
            Button("DESCONECTAR",new Vector2(.66f,.01f),new Vector2(.96f,.12f),()=>Run(client.LogoutAsync));
            client.Changed+=Refresh;Refresh();
        }
        async void Authenticate(bool register)
        {
            if(client.Busy)return;
            string user=username.text,secret=password.text;password.text="";
            try{await client.LoginAsync(user,secret,register);}finally{secret=null;if(this)Refresh();}
        }
        async void RunRoom(bool create)
        {
            if(client.Busy)return;
            string entry=code.text,secret=roomPassword.text;roomPassword.text="";
            try{if(create)await client.CreateRoomAsync(secret);else await client.JoinRoomAsync(entry,secret);}
            finally{secret=null;if(this)Refresh();}
        }
        async void Run(Func<Task> operation)
        { if(client.Busy)return;try{await operation();}finally{if(this)Refresh();} }
        void Refresh()
        {
            if(state)state.text=client.Status+(string.IsNullOrEmpty(client.RoomCode)?"":"  ·  CÓDIGO "+client.RoomCode);
            foreach(var button in GetComponentsInChildren<UnityEngine.UI.Button>())button.interactable=!client.Busy;
        }
        void OnDestroy()
        {
            if(client)client.Changed-=Refresh;
            if(password)password.text="";if(roomPassword)roomPassword.text="";
        }
        TMP_InputField Field(string placeholder,Vector2 min,Vector2 max,bool secret)
        {
            var rect=Box(placeholder,transform,min,max,new Color(.18f,.15f,.21f));
            rect.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            var field=rect.gameObject.AddComponent<TMP_InputField>();field.characterLimit=secret?64:30;
            field.targetGraphic=rect.GetComponent<UnityEngine.UI.Image>();
            var viewport=new GameObject("Viewport",typeof(RectTransform),typeof(UnityEngine.UI.RectMask2D));
            var view=(RectTransform)viewport.transform;view.SetParent(rect,false);view.anchorMin=Vector2.zero;view.anchorMax=Vector2.one;view.offsetMin=new Vector2(9,3);view.offsetMax=new Vector2(-9,-3);
            var text=Label("",Vector2.zero,Vector2.one,16,view);text.textWrappingMode=TextWrappingModes.NoWrap;
            var hint=Label(placeholder,Vector2.zero,Vector2.one,16,view);hint.color=new Color(.7f,.65f,.72f);
            field.textViewport=view;field.textComponent=text;field.placeholder=hint;
            field.contentType=secret?TMP_InputField.ContentType.Password:TMP_InputField.ContentType.Standard;
            field.lineType=TMP_InputField.LineType.SingleLine;return field;
        }
        void Button(string caption,Vector2 min,Vector2 max,Action action)
        {
            var hit=Box(caption,transform,min,max,Color.clear);var image=hit.GetComponent<UnityEngine.UI.Image>();image.raycastTarget=true;
            var button=hit.gameObject.AddComponent<UnityEngine.UI.Button>();button.targetGraphic=image;button.transition=UnityEngine.UI.Selectable.Transition.None;
            var visual=Box("Visual",hit,Vector2.zero,Vector2.one,new Color(.23f,.18f,.25f));visual.offsetMin=new Vector2(3,3);visual.offsetMax=new Vector2(-3,-3);
            hit.gameObject.AddComponent<DungeonMotion>().Configure(visual,new Color(.23f,.18f,.25f),new Color(.43f,.27f,.34f));
            Label(caption,new Vector2(.03f,.03f),new Vector2(.97f,.97f),15,visual);button.onClick.AddListener(()=>action());
        }
        TextMeshProUGUI Label(string caption,Vector2 min,Vector2 max,float size,Transform parent=null)
        {
            var go=new GameObject("Label",typeof(RectTransform),typeof(TextMeshProUGUI));var rect=(RectTransform)go.transform;rect.SetParent(parent?parent:transform,false);
            rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;var text=go.GetComponent<TextMeshProUGUI>();
            text.font=TMP_Settings.defaultFontAsset;text.text=caption;text.fontSize=size;text.enableAutoSizing=true;text.fontSizeMin=12;text.fontSizeMax=size;text.color=new Color(.94f,.86f,.76f);
            text.raycastTarget=false;return text;
        }
        static RectTransform Box(string name,Transform parent,Vector2 min,Vector2 max,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image));var rect=(RectTransform)go.transform;rect.SetParent(parent,false);
            rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;var image=go.GetComponent<UnityEngine.UI.Image>();image.color=color;image.raycastTarget=false;return rect;
        }
    }
}
