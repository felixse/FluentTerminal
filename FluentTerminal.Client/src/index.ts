import { Terminal, ITerminalOptions, ILinkMatcherOptions } from 'xterm';
import { FitAddon } from 'xterm-addon-fit';
import { SearchAddon } from 'xterm-addon-search';
import { WebLinksAddon } from 'xterm-addon-web-links';
import { SerializeAddon } from "./xterm-addon-serialize/src/SerializeAddon";

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
  serializeTerminal() : void;
}

declare var window: ExtendedWindow;

let term: any;
let fitAddon: FitAddon;
let searchAddon: SearchAddon;
let serializeAddon: SerializeAddon;
let webLinksAddon: WebLinksAddon;
let linkObject = {
  isLink: false,
  uri: ""
};
const terminalContainer = document.getElementById('terminal-container');

function replaceAll(searchString, replaceString, str) {
  return str.split(searchString).join(replaceString);
}

function DecodeSpecialChars(data: string) {
  data = replaceAll("&quot;", "\"", data);
  data = replaceAll("&squo;", "'", data);
  return replaceAll("&bsol;", "\\", data);
}

window.serializeTerminal = () => {
  let serialized = serializeAddon.serialize();
  return serialized;
}

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
    wordSeparator: DecodeSpecialChars(options.wordSeparator)
  };

  term = new Terminal(terminalOptions);

  const webLinksAddonHandler = (e: MouseEvent, u: string) => window.open(u);

  const linkMatcherOptions: ILinkMatcherOptions = {
    validationCallback: (uri: string, callback: (isValid: boolean) => void) => {
      linkObject.isLink = true;
      linkObject.uri = uri;
      callback(true);
    },
    leaveCallback: () => {
      linkObject.isLink = false;
      linkObject.uri = "";
      term.clearSelection();
    }
  };

  searchAddon = new SearchAddon();
  term.loadAddon(searchAddon);
  fitAddon = new FitAddon();
  term.loadAddon(fitAddon);
  serializeAddon = new SerializeAddon();
  term.loadAddon(serializeAddon);
  webLinksAddon = new WebLinksAddon(webLinksAddonHandler, linkMatcherOptions);
  term.loadAddon(webLinksAddon);

  window.term = term;

  window.terminalBridge.onoutput = (data => {
    term.writeUtf8(data);
  });

  term.onData(data => {
    window.terminalBridge.inputReceived(data);
  });

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
  fitAddon.fit();
  term.focus();

  setPadding(options.padding);

  let resizeTimeout: any;
  window.onresize = function () {
    clearTimeout(resizeTimeout);
    resizeTimeout = setTimeout(() => fitAddon.fit(), 500);
  }

  window.onmouseup = function (e) {
    if (e.button == 1) {
      window.terminalBridge.notifyMiddleClick(e.clientX, e.clientY, term.hasSelection());
    } else if (e.button == 2) {
      if(linkObject.isLink) searchAddon.findNext(linkObject.uri);
      window.terminalBridge.notifyRightClick(e.clientX, e.clientY, term.hasSelection());
    }
  }

  term.attachCustomKeyEventHandler(function (e) {
    if (e.type != "keydown") {
      return true;
    }
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

  window.terminalBridge.initialized();

  return JSON.stringify({
    rows: term.rows,
    columns: term.cols
  });
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
  term.setOption('wordSeparator', DecodeSpecialChars(options.wordSeparator));
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
  fitAddon.fit();
}

window.changeKeyBindings = (keyBindings) => {
  keyBindings = JSON.parse(keyBindings);
  window["keyBindings"] = keyBindings;
}

window.findNext = (content: string) => {
  searchAddon.findNext(content);
}

window.findPrevious = (content: string) => {
  searchAddon.findPrevious(content);
}

document.oncontextmenu = function () {
  return false;
};
