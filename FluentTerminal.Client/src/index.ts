import { Terminal, ITerminalOptions } from 'xterm';
import * as attach from "xterm/lib/addons/attach/attach";
import * as fit from "xterm/lib/addons/fit/fit";
import * as search from "xterm/lib/addons/search/search";

interface ExtendedWindow extends Window {
  keyBindings: any[];
  term: any;
  terminalBridge: any;

  createTerminal(options: any, theme: any, keyBindings: any): void;
  connectToWebSocket(url: string): void;
  changeTheme(theme: any): void;
  changeOptions(options: any): void;
  changeKeyBindings(keyBindings: any): void;
  findNext(content: string): void;
  findPrevious(content: string): void;
}

declare var window: ExtendedWindow;

Terminal.applyAddon(attach);
Terminal.applyAddon(fit);
Terminal.applyAddon(search);

let term: any;
let socket: WebSocket;
const terminalContainer = document.getElementById('terminal-container');

window.createTerminal = (options, theme, keyBindings) => {
  while (terminalContainer.children.length) {
    terminalContainer.removeChild(terminalContainer.children[0]);
  }

  theme = JSON.parse(theme);

  window.keyBindings = JSON.parse(keyBindings);

  options = JSON.parse(options);

  setScrollBarStyle(options.scrollBarStyle);

  var terminalOptions: ITerminalOptions = {
    fontFamily: options.fontFamily,
    fontSize: options.fontSize,
    fontWeight: options.boldText ? 'bold' : 'normal',
    fontWeightBold: options.boldText ? '400' : 'bold',
    cursorStyle: options.cursorStyle,
    cursorBlink: options.cursorBlink,
    bellStyle: options.bellStyle,
    scrollback: options.scrollBackLimit,
    allowTransparency: true,
    theme: theme,
    windowsMode: true,
    experimentalCharAtlas: "dynamic"
  };

  term = new Terminal(terminalOptions);

  window.term = term;

  term.onResize(({ cols, rows }) => {
    window.terminalBridge.notifySizeChanged(cols, rows);
  });

  term.onTitleChange((title: string) => {
    window.terminalBridge.notifyTitleChanged(title);
  });

  term.onSelectionChange(() => {
    window.terminalBridge.notifySelectionChanged(term.getSelection());
  });

  term.open(terminalContainer);
  term.fit();
  term.focus();

  setPadding(options.padding);

  let resizeTimeout: any;
  window.onresize = function () {
    clearTimeout(resizeTimeout);
    resizeTimeout = setTimeout(term.fit(), 500);
  }

  window.onmouseup = function (e) {
    if (e.button == 1) {
      window.terminalBridge.notifyMiddleClick(e.clientX, e.clientY, term.hasSelection());
    } else if (e.button == 2) {
      window.terminalBridge.notifyRightClick(e.clientX, e.clientY, term.hasSelection());
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
          window.terminalBridge.invokeCommand(keyBinding.command);
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

window.connectToWebSocket =  (url: string) => {
  socket = new WebSocket(url);
  socket.onerror = function (event) {
    window.terminalBridge.reportError(`Socket error: ${JSON.stringify(event)}`);
  }
  socket.onclose = function (event) {
    window.terminalBridge.reportError(`Socket closed: ${JSON.stringify(event)}`);
  };
  socket.onopen = attachTerminal;
}

window.changeTheme = (theme) => {
  theme = JSON.parse(theme);
  term.setOption('theme', theme);
}

window.changeOptions = (options) => {
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
    case 'hidden': return terminalContainer.style['-ms-overflow-style'] = 'none';
    case 'autoHiding': return terminalContainer.style['-ms-overflow-style'] = '-ms-autohiding-scrollbar';
    case 'visible': return terminalContainer.style['-ms-overflow-style'] = 'scrollbar';
  }
}

function setPadding(padding) {
  document.querySelector('.terminal')["style"].padding = padding + 'px';
  term.fit();
}

window.changeKeyBindings = (keyBindings) => {
  keyBindings = JSON.parse(keyBindings);
  window["keyBindings"] = keyBindings;
}

window.findNext = (content: string) => {
  term.findNext(content);
}

window.findPrevious = (content: string) => {
  term.findPrevious(content);
}

document.oncontextmenu = function () {
  return false;
};
