import * as Terminal from '../../xterm.js/build/xterm';
import * as attach from '../../xterm.js/build/addons/attach/attach';
import * as fit from '../../xterm.js/build/addons/fit/fit';
import * as winptyCompat from '../../xterm.js/build/addons/winptyCompat/winptyCompat';

Terminal.applyAddon(attach);
Terminal.applyAddon(fit);
Terminal.applyAddon(winptyCompat);

var term, protocol, socketURL, socket;
var terminalContainer = document.getElementById('terminal-container');

createTerminal();

function createTerminal() {
  while (terminalContainer.children.length) {
    terminalContainer.removeChild(terminalContainer.children[0]);
  }
  term = new Terminal({
    cursorBlink: true,
    fontFamily: 'consolas',
    fontSize: 13,
    allowTransparency: true,
    theme: {
      background: "transparent" 
    }
  });
  window.term = term;  // Expose `term` to window for debugging purposes
  term.on('resize', function (size) {
    if (!socketURL) {
      return;
    }
    var id = socketURL.split(':')[2];
    var cols = size.cols,
        rows = size.rows,
        url = 'http://localhost:9000/terminals/' + id + '/size?cols=' + cols + '&rows=' + rows;
    fetch(url, {method: 'POST'});
  });
  protocol = (location.protocol === 'https:') ? 'wss://' : 'ws://';

  term.open(terminalContainer);
  term.winptyCompatInit();
  term.fit();
  term.focus();

  var resizeTimeout;
  window.onresize = function() {
    clearTimeout(resizeTimeout);
    resizeTimeout = setTimeout(term.fit(), 100);
  }

  fetch('http://localhost:9000/terminals?cols=' + term.cols + '&rows=' + term.rows, {method: 'POST'}).then(function (res) {
      console.log("got response");
      res.text().then(function (url) {
        socketURL = JSON.parse(url);
        socket = new WebSocket(socketURL);
        socket.onopen = runRealTerminal;
      });
    });
}

function runRealTerminal() {
  term.attach(socket);
  term._initialized = true;
}
