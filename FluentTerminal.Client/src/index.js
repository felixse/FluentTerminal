import * as Terminal from '../node_modules/xterm/build/xterm';
import * as attach from '../node_modules/xterm/build/addons/attach/attach';
import * as fit from '../node_modules/xterm/build/addons/fit/fit';
import * as winptyCompat from '../node_modules/xterm/build/addons/winptyCompat/winptyCompat';


Terminal.applyAddon(attach);
Terminal.applyAddon(fit);
Terminal.applyAddon(winptyCompat);

var term, socket;
var terminalContainer = document.getElementById('terminal-container');

function createTerminal(theme) {
  while (terminalContainer.children.length) {
    terminalContainer.removeChild(terminalContainer.children[0]);
  }

  theme = JSON.parse(theme);
  theme.background = 'transparent';

  term = new Terminal({
    cursorBlink: true,
    fontFamily: 'consolas',
    fontSize: 13,
    allowTransparency: true,
    theme: theme
  });

  window.term = term;

  term.on('resize', function (size) {
    terminalBridge.notifySizeChanged(term.cols, term.rows);
  });

  term.on('title', function (title) {
    terminalBridge.notifyTitleChanged(title);
  });

  term.open(terminalContainer);
  term.winptyCompatInit();
  term.fit();
  term.focus();

  var resizeTimeout;
  window.onresize = function () {
    clearTimeout(resizeTimeout);
    resizeTimeout = setTimeout(term.fit(), 500);
  }

  return JSON.stringify({
    rows: term.rows,
    columns: term.cols
  });
}

function attachTerminal() {
  term.attach(socket);
  term._initialized = true;
}

function connectToWebSocket(url) {
  socket = new WebSocket(url);
  socket.onopen = attachTerminal;
}

window.createTerminal = createTerminal;
window.connectToWebSocket = connectToWebSocket;