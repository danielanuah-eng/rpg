using NUnit.Framework;

namespace ArenaTatica.Tests
{
    // 020-ux: confirmação em dois toques (anti-missclick) e decisões puras de toque. Sem cena: só as classes puras.
    public class ActionConfirmationTests
    {
        static readonly string Fire=ArenaActionConfirmation.Skill("bola-de-fogo"), Pass=ArenaActionConfirmation.Pass;

        [Test] public void FirstPressOnlyHighlightsAndArmsTheKey()
        {
            var c=new ArenaActionConfirmation();
            Assert.IsFalse(c.Press(Fire,3,1),"o primeiro toque nunca age");
            Assert.IsTrue(c.Armed); Assert.IsTrue(c.Is(Fire),"primeiro toque destaca o botão da skill");
            Assert.AreEqual(3,c.Actor); Assert.AreEqual(1,c.Revision,"a confirmação guarda ator e revisão");
        }

        [Test] public void SecondPressOnTheSameButtonConfirmsAndRearms()
        {
            var c=new ArenaActionConfirmation();
            Assert.IsFalse(c.Press(Fire,3,1));
            Assert.IsTrue(c.Press(Fire,3,1),"o segundo toque no mesmo botão confirma");
            Assert.IsFalse(c.Armed,"depois de confirmar, o próximo toque destaca de novo");
            Assert.IsFalse(c.Press(Fire,3,1));
            Assert.IsTrue(c.Press(Fire,3,1));
        }

        [Test] public void PressingAnotherButtonResetsTheConfirmation()
        {
            var c=new ArenaActionConfirmation();
            Assert.IsFalse(c.Press(Fire,3,1));
            var other=ArenaActionConfirmation.Skill("foice-verde");
            Assert.IsFalse(c.Press(other,3,1),"trocar de botão só destaca o novo");
            Assert.IsFalse(c.Is(Fire)); Assert.IsTrue(c.Is(other));
            Assert.IsFalse(c.Press(Fire,3,1),"voltar no primeiro botão destaca de novo, não confirma");
            c.Reset();
            Assert.IsFalse(c.Armed); Assert.IsFalse(c.Is(Fire),"Mover/Reset zera a confirmação");
        }

        [Test] public void AnotherActorOrStateNeverConfirms()
        {
            var c=new ArenaActionConfirmation();
            Assert.IsFalse(c.Press(Fire,3,1));
            Assert.IsFalse(c.Press(Fire,4,1),"trocar o ator (novo turno/jogador) não confirma");
            Assert.AreEqual(4,c.Actor);
            Assert.IsFalse(c.Press(Fire,4,2),"trocar a revisão (estado novo) não confirma");
            Assert.IsFalse(c.Same(Fire,4,1));
            Assert.IsTrue(c.Press(Fire,4,2),"mesmo botão e mesmo contexto: confirma");
        }

        [Test] public void ResetAndEmptyKeyClearEverything()
        {
            var c=new ArenaActionConfirmation();
            c.Press(Pass,2,7); Assert.IsTrue(c.Is(Pass));
            c.Reset();
            Assert.IsFalse(c.Armed); Assert.IsFalse(c.Same(Pass,2,7)); Assert.IsFalse(c.Is(Pass),"reset zera chave, ator e revisão");
            Assert.IsFalse(c.Press(null,2,7)); Assert.IsFalse(c.Armed,"chave vazia não arma nada");
            Assert.IsFalse(c.Press("",2,7)); Assert.IsFalse(c.Armed);
        }

        [Test] public void PassUsesTheSameConfirmedMethod()
        {
            var c=new ArenaActionConfirmation();
            Assert.AreEqual("passar",Pass);
            Assert.AreEqual("skill:bola-de-fogo",Fire);
            Assert.IsFalse(c.Press(Pass,1,1),"Passar: primeiro toque só destaca");
            Assert.IsTrue(c.Press(Pass,1,1),"segundo toque (ou Espaço) passa o turno");
        }
    }

    // Limiares e sequência de toque: dois dedos só zoom; 1->2->1 nunca vira clique em casa.
    public class TouchGestureTests
    {
        [Test] public void ThresholdsGrowWithDpiAndInterfaceScale()
        {
            Assert.AreEqual(1f,ArenaTouchGesture.Scale(0,0),1e-4f,"sem DPI nem escala, escala 1");
            Assert.AreEqual(1f,ArenaTouchGesture.Scale(160,1),1e-4f);
            Assert.AreEqual(2f,ArenaTouchGesture.Scale(320,0),1e-4f);
            Assert.AreEqual(4f,ArenaTouchGesture.Scale(0,4),1e-4f,"a escala da interface manda quando o DPI não vem");
            Assert.AreEqual(10f,ArenaTouchGesture.Threshold(1),1e-4f);
            Assert.AreEqual(20f,ArenaTouchGesture.Threshold(2),1e-4f);
            Assert.IsTrue(ArenaTouchGesture.Tap(9f,1),"9 px ainda é toque");
            Assert.IsFalse(ArenaTouchGesture.Tap(11f,1),"passou de ~10 px: virou arrasto");
            Assert.IsFalse(ArenaTouchGesture.Tap(25f,2));
            Assert.IsTrue(ArenaTouchGesture.Tap(25f,3),"com escala 3 o limiar vai a 30 px");
            Assert.IsFalse(ArenaTouchGesture.LongPress(.44f)); Assert.IsTrue(ArenaTouchGesture.LongPress(.45f));
        }

        [Test] public void PinchNeedsItsOwnSlackAndNeverRotatesOrPans()
        {
            Assert.AreEqual(8f,ArenaTouchGesture.PinchSlack(300,1),1e-4f,"300 px * 2% = 6, fica o piso de 8 px");
            Assert.AreEqual(20f,ArenaTouchGesture.PinchSlack(1000,1),1e-4f,"1000 px * 2% = 20 px, acima do piso");
            Assert.IsFalse(ArenaTouchGesture.Zoom(300,304,1),"4 px de tremida não mexem na câmera");
            Assert.IsTrue(ArenaTouchGesture.Zoom(300,320,1));
            Assert.IsFalse(ArenaTouchGesture.Zoom(1000,1010,1),"10 px < 2% da distância inicial");
            Assert.IsTrue(ArenaTouchGesture.Zoom(1000,1040,1));
            Assert.IsTrue(ArenaTouchGesture.Zoom(300,288,1),"aproximar os dedos além do limiar também é zoom");
            Assert.IsTrue(ArenaTouchGesture.ZoomOnly(2)); Assert.IsTrue(ArenaTouchGesture.ZoomOnly(3));
            Assert.IsFalse(ArenaTouchGesture.ZoomOnly(1),"um dedo não é pinça");
        }

        [Test] public void OneToTwoToOneOrStartingOnUiNeverActsOnTheBoard()
        {
            Assert.IsTrue(ArenaTouchGesture.TapAfterGesture(1,false,false,false),"toque limpo age");
            Assert.IsFalse(ArenaTouchGesture.TapAfterGesture(1,true,false,false),"passou por pinça: soltar não é clique");
            Assert.IsFalse(ArenaTouchGesture.TapAfterGesture(1,false,true,false),"já moveu a câmera: soltar não é clique");
            Assert.IsFalse(ArenaTouchGesture.TapAfterGesture(1,false,false,true),"toque nascido sobre a UI não age no tabuleiro");
            Assert.IsFalse(ArenaTouchGesture.TapAfterGesture(2,false,false,false),"toque que começou com dois dedos nunca é clique");
            Assert.IsFalse(ArenaTouchGesture.Camera(true)); Assert.IsTrue(ArenaTouchGesture.Camera(false));
        }
    }
}
