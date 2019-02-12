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

function createTerminal(options, theme, keyBindings, sessionType) {
  while (terminalContainer.children.length) {
    terminalContainer.removeChild(terminalContainer.children[0]);
  }

  theme = JSON.parse(theme);

  window.keyBindings = JSON.parse(keyBindings);

  options = JSON.parse(options);

  setScrollBarStyle(options.scrollBarStyle);

  var terminalOptions = {
    fontFamily: options.fontFamily,
    fontSize: options.fontSize,
    fontWeight: options.boldText ? 'bold' : 'normal',
    fontWeightBold: options.boldText ? 'bolder' : 'bold',
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

  term.on('selection', function () {
    terminalBridge.notifySelectionChanged(term.getSelection());
  });

  term.open(terminalContainer);
  term.fit();

  if (sessionType === 'WinPty') {
    term.winptyCompatInit();
  }

  term.focus();
  
  setPadding(options.padding);

  var resizeTimeout;
  window.onresize = function () {
    clearTimeout(resizeTimeout);
    resizeTimeout = setTimeout(term.fit(), 500);
  }

  window.onmouseup = function (e) {
    if (e.button == 1) {
      terminalBridge.notifyMiddleClick(e.clientX, e.clientY, term.hasSelection());
    } else if (e.button == 2) {
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
          if (keyBinding.command == 'Copy' && term.getSelection() == '') {
            return true;
          }
          if (keyBinding.command == 'Clear') {
            term.clearSelection();
            term.clear();
            return false;
          }
          if (keyBinding.command == 'SelectAll') {
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
  term.setOption('theme', theme);
}

function changeOptions(options) {
  options = JSON.parse(options);

  term.setOption('bellStyle', options.bellStyle);
  term.setOption('cursorBlink', options.cursorBlink);
  term.setOption('cursorStyle', options.cursorStyle);
  term.setOption('fontFamily', options.fontFamily);
  term.setOption('fontSize', options.fontSize);
  term.setOption('fontWeight', options.boldText ? 'bold' : 'normal');
  term.setOption('fontWeightBold', options.boldText ? 'bolder' : 'bold');
  term.setOption('scrollback', options.scrollBackLimit);
  setScrollBarStyle(options.scrollBarStyle);
  setPadding(options.padding);
}

function setScrollBarStyle(scrollBarStyle) {
  switch (scrollBarStyle) {
  	case 'hidden':     return terminalContainer.style['-ms-overflow-style'] = 'none';
  	case 'autoHiding': return terminalContainer.style['-ms-overflow-style'] = '-ms-autohiding-scrollbar';
  	case 'visible':    return terminalContainer.style['-ms-overflow-style'] = 'scrollbar';
  }
}

function setPadding(padding) {
  document.querySelector('.terminal').style.padding = padding + 'px';
  term.fit();
}

function changeKeyBindings(keyBindings) {
  keyBindings = JSON.parse(keyBindings);
  window.keyBindings = keyBindings;
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
window.findNext = findNext;
window.findPrevious = findPrevious;
