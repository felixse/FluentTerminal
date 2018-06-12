import * as Terminal from '../node_modules/xterm/dist/xterm';
import * as attach from '../node_modules/xterm/dist/addons/attach/attach';
import * as fit from '../node_modules/xterm/dist/addons/fit/fit';
import * as winptyCompat from '../node_modules/xterm/dist/addons/winptyCompat/winptyCompat';
import * as search from '../node_modules/xterm/dist/addons/search/search';


Terminal.applyAddon(attach);
Terminal.applyAddon(fit);
Terminal.applyAddon(winptyCompat);
Terminal.applyAddon(search);

var term, socket;
var terminalContainer = document.getElementById('terminal-container');

function createTerminal(options, theme, keyBindings) {
  while (terminalContainer.children.length) {
    terminalContainer.removeChild(terminalContainer.children[0]);
  }

  theme = JSON.parse(theme);
  theme.background = 'transparent';

  window.keyBindings = JSON.parse(keyBindings);

  options = JSON.parse(options);

  setScrollBarStyle(options.scrollBarStyle);

  var terminalOptions = {
    fontFamily: options.fontFamily,
    fontSize: options.fontSize,
    cursorStyle: options.cursorStyle,
    cursorBlink: options.cursorBlink,
    bellStyle: options.bellStyle,
    scrollback: options.scrollBackLimit,
    allowTransparency: true,
    theme: theme,
    experimentalCharAtlas: 'dynamic'
  };

  term = new Terminal(terminalOptions);

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

  window.onmouseup = function (e) {
    if (e.button == 2) {
      terminalBridge.notifyRightClick(e.clientX, e.clientY, term.hasSelection());
    }
  }

  term.attachCustomKeyEventHandler(function (e) {
    for (var i = 0; i < window.keyBindings.length; i++) {
      var keyBinding = window.keyBindings[i];
        if (keyBinding.ctrl == e.ctrlKey
        && keyBinding.meta == e.metaKey
        && keyBinding.alt == e.altKey
        && keyBinding.shift == e.shiftKey
        && keyBinding.key == e.keyCode) {
        if (document.visibilityState == 'visible') {
          if (keyBinding.command == 'copy' && term.getSelection() == '') {
            return true;
          }
          if (keyBinding.command == 'clear') {
            term.clearSelection();
            term.clear();
            return false;
          }
          if (keyBinding.command == 'selectAll') {
            term.selectAll();
            return false;
          }


          e.preventDefault();
          terminalBridge.invokeCommand(keyBinding.command);
        }
        return false;
      }
    }
    return true;
  });

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
  socket.onclose = function() {
    terminalBridge.invokeCommand('CloseTab');
  };
}

function changeTheme(theme) {
  theme = JSON.parse(theme);
  theme.background = 'transparent';

  term.setOption('theme', theme);
}

function changeOptions(options) {
  options = JSON.parse(options);

  term.setOption('bellStyle', options.bellStyle);
  term.setOption('cursorBlink', options.cursorBlink);
  term.setOption('cursorStyle', options.cursorStyle);
  term.setOption('fontFamily', options.fontFamily);
  term.setOption('fontSize', options.fontSize);
  term.setOption('scrollback', options.scrollBackLimit);
  setScrollBarStyle(options.scrollBarStyle);
}

function setScrollBarStyle(scrollBarStyle) {
  if (scrollBarStyle == 'hidden') {
    document.getElementById('terminal-container').style['-ms-overflow-style'] = 'none';
  } else if (scrollBarStyle == 'autoHiding') {
    document.getElementById('terminal-container').style['-ms-overflow-style'] = '-ms-autohiding-scrollbar';
  } else if (scrollBarStyle == 'visible') {
    document.getElementById('terminal-container').style['-ms-overflow-style'] = 'scrollbar';
  }
  
}

function changeKeyBindings(keyBindings) {
  keyBindings = JSON.parse(keyBindings);
  window.keyBindings = keyBindings;
}

function paste(content) {
  content = b64DecodeUnicode(content);
  term.send(content);
}

function b64DecodeUnicode(str) {
  return decodeURIComponent(Array.prototype.map.call(atob(str), function (c) {
    return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)
  }).join(''))
}

function findNext(content) {
  term.findNext(content);
}

function findPrevious(content) {
  term.findPrevious(content);
}

document.oncontextmenu = function () {
  return false;
};

window.createTerminal = createTerminal;
window.connectToWebSocket = connectToWebSocket;
window.changeTheme = changeTheme;
window.changeOptions = changeOptions;
window.changeKeyBindings = changeKeyBindings;
window.paste = paste;
window.findNext = findNext;
window.findPrevious = findPrevious;