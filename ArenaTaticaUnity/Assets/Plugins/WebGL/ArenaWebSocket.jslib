mergeInto(LibraryManager.library, {
  ArenaWSOpen: function(urlPtr, objPtr) {
    var url = UTF8ToString(urlPtr), obj = UTF8ToString(objPtr);
    if (Module.arenaSocket) Module.arenaSocket.close();
    var ws;
    try { ws = new WebSocket(url); } catch (e) { SendMessage(obj, 'WebError', 'Endereço WebSocket inválido.'); SendMessage(obj, 'WebClose', ''); return; }
    Module.arenaSocket = ws;
    ws.onopen = function() { if (Module.arenaSocket === ws) SendMessage(obj, 'WebOpen', ''); };
    ws.onmessage = function(e) { if (Module.arenaSocket === ws && typeof e.data === 'string' && e.data.length <= 65536) SendMessage(obj, 'WebMessage', e.data); };
    ws.onclose = function() { if (Module.arenaSocket === ws) SendMessage(obj, 'WebClose', ''); };
    ws.onerror = function() { if (Module.arenaSocket === ws) SendMessage(obj, 'WebError', 'Conexão WebSocket indisponível. Confira o Host e o endereço.'); };
  },
  ArenaWSSend: function(ptr) { var ws = Module.arenaSocket; if (ws && ws.readyState === 1) ws.send(UTF8ToString(ptr)); },
  ArenaWSClose: function() { var ws = Module.arenaSocket; Module.arenaSocket = null; if (ws) ws.close(); }
});
