namespace ArenaTatica
{
    // Confirmação em dois toques (anti-missclick): o PRIMEIRO toque no botão só destaca; o SEGUNDO toque, no mesmo
    // botão e com o mesmo contexto (ator + revisão da partida), é que age. Puro: sem UnityEngine, para testar sem cena.
    // Trocar de botão, de ator, de revisão/turno, perder CanAct, abrir menu/picker/loja ou começar um gesto de câmera
    // é só chamar Reset() (o HUD faz isso) — a confirmação nunca sobrevive a um contexto novo.
    public sealed class ArenaActionConfirmation
    {
        // Chave do botão Passar; skills usam Skill(id).
        public const string Pass="passar";
        public static string Skill(string id) => "skill:"+id;

        public string Key { get; private set; }
        public int Actor { get; private set; }
        public int Revision { get; private set; }
        public bool Armed => !string.IsNullOrEmpty(Key);

        // É o mesmo botão no mesmo contexto (o que valida o segundo toque)?
        public bool Same(string key,int actor,int revision) => Armed&&Key==key&&Actor==actor&&Revision==revision;
        // Está armado para esse botão (destaque na interface), sem afirmar nada sobre o contexto?
        public bool Is(string key) => Armed&&Key==key;

        // Um toque no botão: true = é o segundo toque no mesmo botão/contexto (pode agir agora); false = só destacou.
        // Confirmação sem prazo: vale enquanto o contexto (ator + revisão) não mudar; confirmar zera para o próximo toque.
        public bool Press(string key,int actor,int revision)
        {
            if(string.IsNullOrEmpty(key)) { Reset(); return false; }
            if(Same(key,actor,revision)) { Reset(); return true; }
            Key=key; Actor=actor; Revision=revision; return false;
        }

        public void Reset() { Key=null; Actor=0; Revision=0; }
    }
}
