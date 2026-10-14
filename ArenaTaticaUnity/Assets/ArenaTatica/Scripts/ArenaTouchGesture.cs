namespace ArenaTatica
{
    // Decisões puras de toque (sem UnityEngine, para testar sem cena): dois dedos dão SÓ zoom — nunca giro nem
    // deslocamento —, um dedo move a vista, e o limiar do toque/arrasto é separado do limiar da pinça.
    // O limiar cresce com o DPI e com a escala da interface: 10 px de toque e 8 px de pinça viram 10*escala e
    // 8*escala; a pinça também aceita 2% da distância inicial entre os dedos, o que for maior.
    public static class ArenaTouchGesture
    {
        public const float TapPixels=10f;      // swipe/longpress: até ~10 px (escalados) ainda é toque
        public const float PinchPixels=8f;     // pinça: ~8 px (escalados) de mudança de distância
        public const float PinchRatio=.02f;    // pinça: ou 2% da distância inicial entre os dedos
        public const float LongPressSeconds=.45f;
        public const float DpiBase=160f;       // 160 dpi ≈ 1 (celular comum); DPI 0 (editor/desktop) conta como 1

        // Escala dos limiares: o maior entre DPI e escala da interface, nunca abaixo de 1.
        public static float Scale(float dpi,float uiScale)
        {
            float d=dpi>0?dpi/DpiBase:1f, u=uiScale>0?uiScale:1f;
            return Max(1f,Max(d,u));
        }
        public static float Threshold(float scale) => TapPixels*Max(1f,scale);
        // Pinça: 8 px escalados OU 2% da distância inicial entre os dedos (o que for maior).
        public static float PinchSlack(float startDistance,float scale) => Max(PinchPixels*Max(1f,scale),Max(0f,startDistance)*PinchRatio);
        // Mudança de distância que já vale como zoom; abaixo disso a pinça ainda não encostou na câmera.
        public static bool Zoom(float startDistance,float distance,float scale) => Abs(distance-startDistance)>=PinchSlack(startDistance,scale);
        // Toque: andou menos que o limiar. Swipe/arrasto é o contrário.
        public static bool Tap(float moved,float scale) => moved<Threshold(scale);
        public static bool LongPress(float held) => held>=LongPressSeconds;
        // Toque começado sobre a interface (botão, painel, janela) nunca controla câmera nem tabuleiro.
        public static bool Camera(bool startedOnUi) => !startedOnUi;
        // Dois dedos: só zoom (sem giro e sem deslocamento).
        public static bool ZoomOnly(int fingers) => fingers>=2;
        // Sequência: isto pode agir no tabuleiro ao soltar? Só um toque que começou com UM dedo, que nunca viu pinça
        // nem gesto de câmera e que não nasceu sobre a interface. É o que impede 1->2->1 (zoom) de virar clique em
        // casa ou giro depois de soltar um dedo.
        public static bool TapAfterGesture(int fingersAtPress,bool pinchSeen,bool cameraMoved,bool startedOnUi) =>
            fingersAtPress==1&&!pinchSeen&&!cameraMoved&&!startedOnUi;

        static float Max(float a,float b) => a>b?a:b;
        static float Abs(float v) => v<0?-v:v;
    }
}
