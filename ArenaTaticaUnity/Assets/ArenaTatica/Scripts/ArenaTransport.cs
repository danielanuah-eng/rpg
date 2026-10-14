using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using Unity.Networking.Transport.Utilities;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
#if !UNITY_WEBGL || UNITY_EDITOR
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.IO;
#else
using System.Runtime.InteropServices;
#endif

namespace ArenaTatica
{
    // Only transports text. Match authority and JSON validation live in ArenaGame.
    // Conexões por id: no Host, uma por jogador remoto (1, 2, 3…); no cliente, 0 é o Host.
    public sealed partial class ArenaTransport : MonoBehaviour
    {
        public const int MaxPeers=10;
        public event Action<int,string> Message;
        public event Action<int> Connected, Disconnected;
        public event Action<string> Failed;
        public bool IsHost { get; private set; }
        // Host: algum jogador conectado. Cliente: ligado ao Host.
        public bool IsConnected { get; private set; }
        public string Role { get; private set; }="Local";
        readonly ConcurrentQueue<Action> pending=new ConcurrentQueue<Action>();
        volatile int generation;
        int nextPeer;
#if !UNITY_WEBGL || UNITY_EDITOR
        TcpListener listener;
        WebSocket socket;
        readonly System.Collections.Generic.Dictionary<int,WebSocket> peers=new System.Collections.Generic.Dictionary<int,WebSocket>();
        CancellationTokenSource lifetime;
        SemaphoreSlim writeLock=new SemaphoreSlim(1,1);
        public void Host(int port=9080)
        {
            Close(); IsHost=true; Role="Host"; lifetime=new CancellationTokenSource();
            int gen=generation;
            try { listener=new TcpListener(IPAddress.Any,port); listener.Start(4); _=AcceptLoop(listener,gen,lifetime.Token); }
            catch(Exception e) { Close(); Failed?.Invoke("Não foi possível abrir a porta: "+e.Message); }
        }
        async Task AcceptLoop(TcpListener server,int gen,CancellationToken token)
        {
            int open=0;
            while(!token.IsCancellationRequested)
            {
                TcpClient tcp=null;
                try { tcp=await server.AcceptTcpClientAsync().ConfigureAwait(false); }
                catch(Exception) { if(!token.IsCancellationRequested) Enqueue(gen,()=>Failed?.Invoke("Servidor interrompido.")); break; }
                if(token.IsCancellationRequested) { tcp.Dispose(); break; }
                if(Interlocked.Increment(ref open)>MaxPeers) { Interlocked.Decrement(ref open); tcp.Dispose(); continue; }
                _=Serve(tcp,gen,token,()=>Interlocked.Decrement(ref open));
            }
        }
        async Task Serve(TcpClient tcp,int gen,CancellationToken token,Action release)
        {
            WebSocket ws=null;
            bool opened=false; int id=0;
            try
            {
                tcp.NoDelay=true;
                var stream=tcp.GetStream();
                using(var handshake=CancellationTokenSource.CreateLinkedTokenSource(token))
                {
                    handshake.CancelAfter(5000);
                    var bytes=new byte[1]; var header=new StringBuilder();
                    while(header.Length<8192)
                    {
                        if(await stream.ReadAsync(bytes,0,1,handshake.Token).ConfigureAwait(false)!=1) throw new IOException("Conexão incompleta");
                        header.Append((char)bytes[0]);
                        if(header.Length>=4&&header.ToString(header.Length-4,4)=="\r\n\r\n") break;
                    }
                    var lines=header.ToString().Split(new[]{"\r\n"},StringSplitOptions.None);
                    string key=null,upgrade=null,connection=null,version=null;
                    foreach(var line in lines) {
                        int p=line.IndexOf(':'); if(p<0) continue;
                        string name=line.Substring(0,p).Trim().ToLowerInvariant(),value=line.Substring(p+1).Trim();
                        if(name=="sec-websocket-key") key=value; if(name=="upgrade") upgrade=value; if(name=="connection") connection=value; if(name=="sec-websocket-version") version=value;
                    }
                    if(header.Length>=8192||!lines[0].StartsWith("GET ")||!string.Equals(upgrade,"websocket",StringComparison.OrdinalIgnoreCase)||connection==null||connection.IndexOf("upgrade",StringComparison.OrdinalIgnoreCase)<0||version!="13"||key==null||Convert.FromBase64String(key).Length!=16) throw new IOException("Handshake inválido");
                    string accept;
                    using(var sha=SHA1.Create()) accept=Convert.ToBase64String(sha.ComputeHash(Encoding.ASCII.GetBytes(key+"258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
                    var response=Encoding.ASCII.GetBytes("HTTP/1.1 101 Switching Protocols\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Accept: "+accept+"\r\n\r\n");
                    await stream.WriteAsync(response,0,response.Length,handshake.Token).ConfigureAwait(false);
                }
                ws=WebSocket.CreateFromStream(stream,true,null,TimeSpan.FromSeconds(20));
                opened=true; id=Interlocked.Increment(ref nextPeer);
                var connectedSocket=ws;
                Enqueue(gen,()=> { peers[id]=connectedSocket; IsConnected=true; Connected?.Invoke(id); });
                await ReadLoop(ws,id,gen,token).ConfigureAwait(false);
            }
            // Queda de um jogador só gera Disconnected; o Host continua de pé.
            catch(Exception e) { if(!token.IsCancellationRequested) Debug.Log("Arena LAN: conexão encerrada: "+e.Message); }
            finally
            {
                ws?.Dispose(); tcp.Dispose();
                if(opened) Enqueue(gen,()=> { peers.Remove(id); IsConnected=peers.Count>0; Disconnected?.Invoke(id); });
                release();
            }
        }
        public async void Join(string url)
        {
            Close(); Role="Conectando"; int gen=generation;
            if(!ValidUrl(url,out var uri)) { Role="Local"; Failed?.Invoke("Use ws:// para rede local ou wss:// para conexão segura."); return; }
            lifetime=new CancellationTokenSource(); var token=lifetime.Token; var ws=new ClientWebSocket();
            socket=ws;
            try
            {
                using(var connect=CancellationTokenSource.CreateLinkedTokenSource(token)) { connect.CancelAfter(15000); await ws.ConnectAsync(uri,connect.Token); }
                Enqueue(gen,()=> { IsConnected=true; Role="Client"; Connected?.Invoke(0); });
                await ReadLoop(ws,0,gen,token);
            }
            catch(Exception e) { if(!token.IsCancellationRequested) Enqueue(gen,()=>Failed?.Invoke("Falha ao conectar: "+e.Message)); }
            finally { ws.Dispose(); Enqueue(gen,()=> { IsConnected=false; Disconnected?.Invoke(0); }); }
        }
        async Task ReadLoop(WebSocket ws,int peer,int gen,CancellationToken token)
        {
            var buffer=new byte[8192];
            while(!token.IsCancellationRequested&&ws.State==WebSocketState.Open)
            {
                using(var data=new MemoryStream())
                {
                    WebSocketReceiveResult result;
                    do {
                        result=await ws.ReceiveAsync(new ArraySegment<byte>(buffer),token).ConfigureAwait(false);
                        if(result.MessageType==WebSocketMessageType.Close) {
                            await writeLock.WaitAsync(token).ConfigureAwait(false);
                            try { await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure,"",token).ConfigureAwait(false); }
                            finally { writeLock.Release(); }
                            return;
                        }
                        if(result.MessageType!=WebSocketMessageType.Text||data.Length+result.Count>65536) throw new IOException("Mensagem inválida ou excessiva");
                        data.Write(buffer,0,result.Count);
                    } while(!result.EndOfMessage);
                    string text=new UTF8Encoding(false,true).GetString(data.ToArray());
                    // Bound main-thread backlog even if a remote peer floods the socket.
                    if(pending.Count>128) throw new IOException("Muitas mensagens pendentes");
                    Enqueue(gen,()=>Message?.Invoke(peer,text));
                }
            }
        }
        public async void Send(int peer,string text)
        {
            if(relayActive) { RelaySend(peer,text); return; }
            WebSocket ws=socket;
            if(IsHost&&!peers.TryGetValue(peer,out ws)) return;
            var token=lifetime?.Token??CancellationToken.None; int gen=generation;
            if(!IsConnected||ws==null||text.Length>65536) return;
            bool acquired=false;
            try {
                await writeLock.WaitAsync(token); acquired=true;
                if(gen!=generation||ws.State!=WebSocketState.Open) return;
                var data=Encoding.UTF8.GetBytes(text); await ws.SendAsync(new ArraySegment<byte>(data),WebSocketMessageType.Text,true,token);
            } catch(Exception) { Enqueue(gen,()=>Failed?.Invoke("Não foi possível enviar a mensagem.")); }
            finally { if(acquired) writeLock.Release(); }
        }
        // Host derruba uma conexão (jogador substituído pela reconexão do mesmo assento).
        public void Kick(int peer)
        {
            if(relayActive) { RelayKick(peer); return; }
            if(peers.TryGetValue(peer,out var ws)) ws.Abort();
        }
        public void Close()
        {
            generation++; RelayClose(); lifetime?.Cancel(); listener?.Stop(); listener=null;
            foreach(var ws in peers.Values) ws.Abort(); peers.Clear();
            socket?.Abort(); socket?.Dispose(); socket=null; lifetime?.Dispose(); lifetime=null;
            IsConnected=false; IsHost=false; Role="Local";
            while(pending.TryDequeue(out _)) {}
        }
#else
        [DllImport("__Internal")] static extern void ArenaWSOpen(string url,string objectName);
        [DllImport("__Internal")] static extern void ArenaWSSend(string text);
        [DllImport("__Internal")] static extern void ArenaWSClose();
        public void Host(int port=9080) { Failed?.Invoke("No navegador, use Entrar para conectar a um Host Windows."); }
        public void Join(string url) { Close(); if(!ValidUrl(url,out _)) { Failed?.Invoke("Endereço WebSocket inválido ou inseguro."); return; } Role="Conectando"; ArenaWSOpen(url,gameObject.name); }
        public void Send(int peer,string text) { if(relayActive) { RelaySend(peer,text); return; } if(IsConnected) ArenaWSSend(text); }
        public void Kick(int peer) { if(relayActive) RelayKick(peer); }
        public void Close() { generation++; RelayClose(); ArenaWSClose(); IsConnected=false; IsHost=false; Role="Local"; }
        [UnityEngine.Scripting.Preserve] public void WebOpen(string _) { Role="Client"; IsConnected=true; Connected?.Invoke(0); }
        [UnityEngine.Scripting.Preserve] public void WebMessage(string text) { if(text.Length<=65536) Message?.Invoke(0,text); }
        [UnityEngine.Scripting.Preserve] public void WebClose(string _) { IsConnected=false; Disconnected?.Invoke(0); }
        [UnityEngine.Scripting.Preserve] public void WebError(string text) { Failed?.Invoke(text); }
#endif
        public static bool ValidUrl(string text,out Uri uri)
        {
            if(!Uri.TryCreate(text,UriKind.Absolute,out uri)||!string.IsNullOrEmpty(uri.UserInfo)) return false;
            if(uri.Scheme=="wss") return true;
            if(uri.Scheme!="ws") return false;
            if(Application.absoluteURL.StartsWith("https:",StringComparison.OrdinalIgnoreCase)) return false;
            // Unencrypted WS is limited to local development / LAN, including release builds.
            string h=uri.Host;
            return h=="localhost"||h=="127.0.0.1"||h=="[::1]"||PrivateIpv4(h);
        }
        static bool PrivateIpv4(string host)
        {
            var parts=host.Split('.'); if(parts.Length!=4) return false;
            var n=new int[4]; for(int i=0;i<4;i++) if(!int.TryParse(parts[i],out n[i])||n[i]<0||n[i]>255) return false;
            return n[0]==10||(n[0]==192&&n[1]==168)||(n[0]==172&&n[1]>=16&&n[1]<=31);
        }
        void Enqueue(int gen,Action action) { pending.Enqueue(()=> { if(gen==generation) action(); }); }
        void Update() { RelayTick(); int count=0; while(count++<128&&pending.TryDequeue(out var action)) action(); }
        void OnDestroy() { Close(); }
    }

    // Sala pela internet com Unity Relay. O código da sala é o join code do Relay (6 letras); sem Lobby.
    public sealed partial class ArenaTransport
    {
        const int RelayPayload=80*1024;
        public event Action<string> RoomReady;
        public string RoomCode { get; private set; }
        public bool InRoom => relayActive;
        NetworkDriver relayDriver;
        NetworkPipeline relayPipe;
        NetworkConnection relayConn;
        readonly System.Collections.Generic.Dictionary<int,NetworkConnection> relayPeers=new System.Collections.Generic.Dictionary<int,NetworkConnection>();
        bool relayActive;
#if UNITY_WEBGL && !UNITY_EDITOR
        const string RelayProtocol="wss";
#else
        const string RelayProtocol="dtls";
#endif

        public static bool ValidRoomCode(string text)
        {
            text=text?.Trim(); if(text==null||text.Length!=6) return false;
            foreach(char c in text) if(!char.IsLetterOrDigit(c)||c>127) return false;
            return true;
        }
        static async Task SignIn()
        {
            if(string.IsNullOrEmpty(Application.cloudProjectId)) throw new Exception("o projeto ainda não está ligado ao Unity Cloud (Project Settings > Services)");
            if(UnityServices.State!=ServicesInitializationState.Initialized) await UnityServices.InitializeAsync();
            if(!AuthenticationService.Instance.IsSignedIn) await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        public async void HostRoom(int seats=2)
        {
            Close(); IsHost=true; Role="Host"; int gen=generation;
            try {
                await SignIn();
                // Vaga dobrada: quem cai volta antes de a conexão antiga expirar no Relay.
                var alloc=await RelayService.Instance.CreateAllocationAsync(Math.Min(MaxPeers,Math.Max(2,(seats-1)*2)));
                string code=await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
                if(gen!=generation) return;
                StartDriver(alloc.ToRelayServerData(RelayProtocol));
                if(relayDriver.Bind(NetworkEndpoint.AnyIpv4)!=0||relayDriver.Listen()!=0) throw new Exception("o Relay recusou a sala");
                RoomCode=code; RoomReady?.Invoke(code);
            } catch(Exception e) { if(gen==generation) { Close(); Failed?.Invoke("Não foi possível criar a sala: "+e.Message); } }
        }
        public async void JoinRoom(string code)
        {
            Close(); Role="Conectando"; int gen=generation; code=code.Trim().ToUpperInvariant();
            try {
                await SignIn();
                var join=await RelayService.Instance.JoinAllocationAsync(code);
                if(gen!=generation) return;
                StartDriver(join.ToRelayServerData(RelayProtocol));
                if(relayDriver.Bind(NetworkEndpoint.AnyIpv4)!=0) throw new Exception("o Relay recusou a conexão");
                relayConn=relayDriver.Connect(); RoomCode=code;
            } catch(Exception e) { if(gen==generation) { Close(); Failed?.Invoke("Sala não encontrada ou indisponível: "+e.Message); } }
        }
        void StartDriver(RelayServerData data)
        {
            var settings=new NetworkSettings();
            settings.WithRelayParameters(ref data);
            settings.WithFragmentationStageParameters(payloadCapacity:RelayPayload);
            settings.WithReliableStageParameters(windowSize:64);
#if UNITY_WEBGL && !UNITY_EDITOR
            relayDriver=NetworkDriver.Create(new WebSocketNetworkInterface(),settings);
#else
            relayDriver=NetworkDriver.Create(settings);
#endif
            relayPipe=relayDriver.CreatePipeline(typeof(FragmentationPipelineStage),typeof(ReliableSequencedPipelineStage));
            relayActive=true;
        }
        void RelayTick()
        {
            if(!relayActive) return;
            int gen=generation;
            relayDriver.ScheduleUpdate().Complete();
            if(IsHost) {
                NetworkConnection c;
                while((c=relayDriver.Accept())!=default) {
                    int id=++nextPeer; relayPeers[id]=c; IsConnected=true; Connected?.Invoke(id); if(gen!=generation) return;
                }
                foreach(var id in new System.Collections.Generic.List<int>(relayPeers.Keys)) if(relayPeers.TryGetValue(id,out var conn)) { Pump(id,conn,gen); if(gen!=generation) return; }
            }
            else if(relayConn.IsCreated) Pump(0,relayConn,gen);
        }
        void Pump(int peer,NetworkConnection conn,int gen)
        {
            NetworkEvent.Type type;
            while(relayActive&&gen==generation&&(type=conn.PopEvent(relayDriver,out var reader))!=NetworkEvent.Type.Empty)
            {
                if(type==NetworkEvent.Type.Connect) { Role="Client"; IsConnected=true; Connected?.Invoke(0); }
                else if(type==NetworkEvent.Type.Data) {
                    if(reader.Length>RelayPayload) continue;
                    var buffer=new NativeArray<byte>(reader.Length,Allocator.Temp); reader.ReadBytes(buffer);
                    string text=Encoding.UTF8.GetString(buffer.ToArray()); buffer.Dispose();
                    Message?.Invoke(peer,text);
                }
                else if(type==NetworkEvent.Type.Disconnect) {
                    if(IsHost) { relayPeers.Remove(peer); IsConnected=relayPeers.Count>0; Disconnected?.Invoke(peer); return; }
                    bool was=IsConnected; relayConn=default; IsConnected=false;
                    if(was) Disconnected?.Invoke(0); else Failed?.Invoke("Não foi possível conectar à sala.");
                    return;
                }
            }
        }
        void RelaySend(int peer,string text)
        {
            NetworkConnection conn=relayConn;
            if(IsHost&&!relayPeers.TryGetValue(peer,out conn)) return;
            if(!IsConnected||!conn.IsCreated) return;
            var bytes=Encoding.UTF8.GetBytes(text);
            if(bytes.Length>RelayPayload) { Failed?.Invoke("Mensagem grande demais para a sala."); return; }
            if(relayDriver.BeginSend(relayPipe,conn,out var writer)!=0) { Failed?.Invoke("Não foi possível enviar a mensagem."); return; }
            using(var data=new NativeArray<byte>(bytes,Allocator.Temp)) writer.WriteBytes(data);
            if(relayDriver.EndSend(writer)<0) Failed?.Invoke("Não foi possível enviar a mensagem.");
        }
        // Desconexão local não gera evento no driver: avisa aqui.
        void RelayKick(int peer)
        {
            if(!IsHost||!relayPeers.TryGetValue(peer,out var conn)) return;
            relayDriver.Disconnect(conn); relayPeers.Remove(peer); IsConnected=relayPeers.Count>0; Disconnected?.Invoke(peer);
        }
        void RelayClose()
        {
            if(relayActive) {
                foreach(var conn in relayPeers.Values) relayDriver.Disconnect(conn);
                if(relayConn.IsCreated) relayDriver.Disconnect(relayConn);
                relayDriver.ScheduleUpdate().Complete();
                relayDriver.Dispose();
            }
            relayActive=false; relayConn=default; relayPeers.Clear(); RoomCode=null;
        }
    }
}
