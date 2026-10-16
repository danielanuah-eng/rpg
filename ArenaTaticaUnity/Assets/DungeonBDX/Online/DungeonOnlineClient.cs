using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace DungeonBDX.Online
{
    /// <summary>
    /// Optional real UGS account/session client. Never starts networking on Awake/Start.
    /// A session coordinates membership only: no combat replication or cloud saves.
    /// Input passwords are never retained in component fields, logs or local saves.
    /// </summary>
    public sealed class DungeonOnlineClient : MonoBehaviour
    {
        public const string ProfileName = "dungeonbdx";
        public const string ScopeDescription = "Sala de gestão; combate, saves cloud e convites in-app ainda não sincronizados.";

        public string Status { get; private set; } = "Online opcional. Nenhuma conexão iniciada.";
        public bool Busy => Volatile.Read(ref _busy) != 0;
        public string RoomCode => _room == null ? string.Empty : _room.Code;
        public bool HasRoom => _room != null;
        public event Action Changed;

        public bool Authorized
        {
            get
            {
                if (_destroying || UnityServices.State != ServicesInitializationState.Initialized)
                    return false;
                var auth = AuthenticationService.Instance;
                return DungeonOnlineValidation.CanUseOnline(auth.IsAuthorized, auth.Profile);
            }
        }

        private ISession _room;
        private IAuthenticationService _observedAuth;
        private int _busy;
        private bool _destroying;

        public Task LoginAsync(string username, string password, bool register)
        {
            return RunAsync(async () =>
            {
                if (!DungeonOnlineValidation.IsValidUsername(username))
                {
                    SetStatus("Usuário: 3–20 letras, números ou . _ @ -.");
                    return;
                }
                if (register ? !DungeonOnlineValidation.IsValidSignupPassword(password)
                             : !DungeonOnlineValidation.IsValidLoginPassword(password))
                {
                    SetStatus(register
                        ? "Senha: 8–30 caracteres, com maiúscula, minúscula, número e símbolo."
                        : "Informe a senha da conta (8–30 caracteres).");
                    return;
                }
                if (!await EnsureProfileAsync()) return;
                var auth = AuthenticationService.Instance;
                if (auth.IsSignedIn)
                {
                    SetStatus(auth.IsAuthorized
                        ? "Uma conta já está conectada. Saia para trocar de conta."
                        : "Sessão expirada. Saia e entre novamente.");
                    return;
                }
                if (register)
                    await auth.SignUpWithUsernamePasswordAsync(username, password);
                else
                    await auth.SignInWithUsernamePasswordAsync(username, password);

                if (_destroying)
                {
                    if (string.Equals(auth.Profile, ProfileName, StringComparison.Ordinal))
                        auth.SignOut(clearCredentials: true);
                    return;
                }
                SetStatus(Authorized ? "Conta autenticada pelo Unity Authentication."
                                     : "A autenticação não ficou disponível. Entre novamente.");
            }, "Não foi possível acessar a conta. Confira os dados e a configuração do serviço.");
        }

        public Task CreateRoomAsync(string roomPassword = null)
        {
            return RunAsync(async () =>
            {
                if (!RequireAuthorization()) return;
                if (_room != null) { SetStatus("Saia da sala atual antes de criar outra."); return; }
                if (!DungeonOnlineValidation.IsValidRoomPassword(roomPassword))
                {
                    SetStatus("A senha opcional da sala deve ter 8–64 caracteres.");
                    return;
                }
                var created = await MultiplayerService.Instance.CreateSessionAsync(new SessionOptions
                {
                    Name = "DungeonBDX",
                    MaxPlayers = 4,
                    IsPrivate = true,
                    Password = string.IsNullOrEmpty(roomPassword) ? null : roomPassword
                });
                if (_destroying) { await LeaveBoundedAsync(created); return; }
                _room = created;
                SetStatus("Sala privada criada. " + ScopeDescription);
            }, "Não foi possível criar a sala privada. Verifique a conexão e os serviços.");
        }

        public Task JoinRoomAsync(string code, string roomPassword = null)
        {
            return RunAsync(async () =>
            {
                if (!RequireAuthorization()) return;
                if (_room != null) { SetStatus("Saia da sala atual antes de entrar em outra."); return; }
                if (!DungeonOnlineValidation.IsValidRoomCode(code))
                {
                    SetStatus("Informe um código de sala válido; ele não é uma credencial de conta.");
                    return;
                }
                if (!DungeonOnlineValidation.IsValidRoomPassword(roomPassword))
                {
                    SetStatus("A senha opcional da sala deve ter 8–64 caracteres.");
                    return;
                }
                var joined = await MultiplayerService.Instance.JoinSessionByCodeAsync(code.Trim(),
                    new JoinSessionOptions { Password = string.IsNullOrEmpty(roomPassword) ? null : roomPassword });
                if (_destroying) { await LeaveBoundedAsync(joined); return; }
                _room = joined;
                SetStatus("Entrada confirmada na sala. " + ScopeDescription);
            }, "Não foi possível entrar. Confira código, senha da sala, vagas e conexão.");
        }

        public Task LeaveRoomAsync()
        {
            return RunAsync(async () =>
            {
                if (!RequireAuthorization()) return;
                if (_room == null) { SetStatus("Nenhuma sala aberta neste cliente."); return; }
                await _room.LeaveAsync();
                _room = null;
                SetStatus("Saída da sala confirmada.");
            }, "Não foi possível confirmar a saída da sala. Tente novamente.");
        }

        public Task LogoutAsync()
        {
            return RunAsync(async () =>
            {
                if (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    SetStatus("Nenhuma conta DungeonBDX conectada.");
                    return;
                }
                var auth = AuthenticationService.Instance;
                if (!string.Equals(auth.Profile, ProfileName, StringComparison.Ordinal))
                {
                    SetStatus("Outro perfil está em uso. A conta dele foi preservada.");
                    return;
                }
                bool leaveConfirmed = true;
                if (_room != null)
                {
                    try { await _room.LeaveAsync(); }
                    catch (Exception) { leaveConfirmed = false; }
                    _room = null;
                }
                // Clear SDK cached session credentials only after attempting to leave.
                auth.SignOut(clearCredentials: true);
                SetStatus(leaveConfirmed
                    ? "Conta desconectada e credenciais de sessão removidas pelo SDK."
                    : "Conta desconectada. A saída da sala depende da recuperação ou expiração no serviço.");
            }, "Não foi possível concluir a desconexão. Tente novamente.");
        }

        private async Task<bool> EnsureProfileAsync()
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
                await UnityServices.InitializeAsync(new InitializationOptions().SetProfile(ProfileName));
            else if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                SetStatus("Os serviços estão sendo inicializados. Tente novamente em instantes.");
                return false;
            }
            if (_destroying) return false;
            var auth = AuthenticationService.Instance;
            if (!string.Equals(auth.Profile, ProfileName, StringComparison.Ordinal))
            {
                if (auth.IsSignedIn)
                {
                    SetStatus("Outro perfil está conectado. A conta dele foi preservada.");
                    return false;
                }
                auth.SwitchProfile(ProfileName);
            }
            ObserveAuthentication(auth);
            return true;
        }

        private bool RequireAuthorization()
        {
            if (Authorized) return true;
            SetStatus("Entre em uma conta DungeonBDX válida para usar salas online.");
            return false;
        }

        private async Task RunAsync(Func<Task> operation, string failureStatus)
        {
            if (_destroying || Interlocked.CompareExchange(ref _busy, 1, 0) != 0) return;
            Notify();
            try { await operation(); }
            catch (Exception) { if (!_destroying) SetStatus(failureStatus); }
            finally
            {
                Interlocked.Exchange(ref _busy, 0);
                Notify();
            }
        }

        private void ObserveAuthentication(IAuthenticationService auth)
        {
            if (ReferenceEquals(_observedAuth, auth)) return;
            StopObservingAuthentication();
            _observedAuth = auth;
            auth.Expired += OnExpired;
            auth.SignedIn += OnAuthorizationChanged;
            auth.SignedOut += OnAuthorizationChanged;
        }

        private void StopObservingAuthentication()
        {
            if (_observedAuth == null) return;
            _observedAuth.Expired -= OnExpired;
            _observedAuth.SignedIn -= OnAuthorizationChanged;
            _observedAuth.SignedOut -= OnAuthorizationChanged;
            _observedAuth = null;
        }

        private void OnExpired() => SetStatus("Sessão expirada. Saia e entre novamente.");
        private void OnAuthorizationChanged() => Notify();
        private void SetStatus(string status) { Status = status; Notify(); }
        private void Notify()
        {
            var callbacks = Changed;
            if (_destroying || callbacks == null) return;
            // A UI subscriber must not interrupt logout or leave the busy gate locked.
            foreach (Action callback in callbacks.GetInvocationList())
                try { callback(); } catch (Exception) { }
        }

        private void OnDestroy()
        {
            _destroying = true;
            StopObservingAuthentication();
            var room = _room;
            _room = null;
            if (room != null) _ = LeaveBoundedAsync(room);
        }

        private static async Task LeaveBoundedAsync(ISession room)
        {
            try
            {
                Task leave = room.LeaveAsync();
                if (await Task.WhenAny(leave, Task.Delay(1500)) == leave)
                    await leave;
                else
                    _ = ObserveCompletionAsync(leave);
            }
            catch (Exception) { }
        }

        private static async Task ObserveCompletionAsync(Task pending)
        {
            try { await pending; } catch (Exception) { }
        }
    }

    /// <summary>Pure validation, with no Editor or Development authentication bypass.</summary>
    public static class DungeonOnlineValidation
    {
        public static bool CanUseOnline(bool isAuthorized, string profile) =>
            isAuthorized && string.Equals(profile, DungeonOnlineClient.ProfileName, StringComparison.Ordinal);

        public static bool IsValidUsername(string username)
        {
            if (username == null || username.Length < 3 || username.Length > 20) return false;
            foreach (char c in username)
                if (!AsciiLetter(c) && !AsciiDigit(c) && c != '.' && c != '_' && c != '@' && c != '-')
                    return false;
            return true;
        }

        public static bool IsValidLoginPassword(string password) =>
            password != null && password.Length >= 8 && password.Length <= 30;

        public static bool IsValidSignupPassword(string password)
        {
            if (!IsValidLoginPassword(password)) return false;
            bool upper = false, lower = false, digit = false, symbol = false;
            foreach (char c in password)
            {
                if (char.IsControl(c)) return false;
                upper |= c >= 'A' && c <= 'Z';
                lower |= c >= 'a' && c <= 'z';
                digit |= AsciiDigit(c);
                symbol |= !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c);
            }
            return upper && lower && digit && symbol;
        }

        public static bool IsValidRoomPassword(string password) =>
            string.IsNullOrEmpty(password) || (password.Length >= 8 && password.Length <= 64);

        public static bool IsValidRoomCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;
            code = code.Trim();
            if (code.Length > 32) return false;
            foreach (char c in code) if (!AsciiLetter(c) && !AsciiDigit(c)) return false;
            return true;
        }

        private static bool AsciiLetter(char c) => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
        private static bool AsciiDigit(char c) => c >= '0' && c <= '9';
    }
}
