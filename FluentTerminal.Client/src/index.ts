import { Terminal, ITerminalOptions, ILinkMatcherOptions, FontWeight } from 'xterm';
import { FitAddon } from 'xterm-addon-fit';
import { SearchAddon } from 'xterm-addon-search';
import { WebLinksAddon } from 'xterm-addon-web-links';
import { SerializeAddon } from "xterm-addon-serialize";
import { Unicode11Addon } from "xterm-addon-unicode11";

interface ExtendedWindow extends Window {
  keyBindings: any[];
  term: Terminal;
  terminalBridge: any;

  createTerminal(options: any, theme: any, keyBindings: any): void;
  connectToWebSocket(url: string): void;
  changeTheme(theme: any): void;
  changeOptions(options: any): void;
  changeKeyBindings(keyBindings: any): void;
  findNext(content: string, caseSensitive: boolean, wholeWord: boolean, regex: boolean): void;
  findPrevious(content: string, caseSensitive: boolean, wholeWord: boolean, regex: boolean): void;
  serializeTerminal() : void;
}

declare var window: ExtendedWindow;

let term: Terminal;
let fitAddon: FitAddon;
let searchAddon: SearchAddon;
let serializeAddon: SerializeAddon;
let webLinksAddon: WebLinksAddon;
let unicode11Addon: Unicode11Addon;

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
    fontWeight: options.fontWeight,
    fontWeightBold: convertBoldText(options.fontWeight),
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

  const linkMatcherOptions: ILinkMatcherOptions = {
    leaveCallback: () => window.onmouseup = (e) => defaultOnMouseUpHandler(e),
    willLinkActivate: (event: MouseEvent, uri: string) => {
      window.onmouseup = (e) => linkHoverOnMouseUpHandler(event, uri);
      return true;
    }
  };

  function defaultOnMouseUpHandler(e: MouseEvent): void {
    if (e.button == 1) {
      window.terminalBridge.notifyMiddleClick(e.clientX, e.clientY, term.hasSelection());
    } else if (e.button == 2) {
      window.terminalBridge.notifyRightClick(e.clientX, e.clientY, term.hasSelection());
    }
  }
  
  function linkHoverOnMouseUpHandler(e: MouseEvent, u: string): void {
    if (e.button == 1) {
      window.terminalBridge.notifyMiddleClick(e.clientX, e.clientY, term.hasSelection());
    } else if (e.button == 2) {
      let pos = findInMouseRow(u, e.clientY);
      if(pos !== undefined) {
        term.select(pos.col, pos.row, u.length);
        window.terminalBridge.notifyRightClick(e.clientX, e.clientY, term.hasSelection());
      }
    }
  }
  
  function findInMouseRow(str: string, mouseY: number): {col: number, row: number} | undefined {
    const lineHeight: number = Math.round(window.innerHeight / window.term.rows) - 1;
    const mouseRow: number = mouseY / lineHeight;
    let col: number, row: number = (mouseRow === Math.ceil(mouseRow) ? mouseRow : Math.floor(mouseRow)) - 1;
    let line = window.term.buffer.getLine(row);

    if(line === undefined) return;
    
    col = line.translateToString().indexOf(str);
    if (col === -1) return;
    
    return {
      col: col, 
      row: row
    };
  }

  searchAddon = new SearchAddon();
  term.loadAddon(searchAddon);
  fitAddon = new FitAddon();
  term.loadAddon(fitAddon);
  serializeAddon = new SerializeAddon();
  term.loadAddon(serializeAddon);
  webLinksAddon = new WebLinksAddon((_, u) => window.open(u), linkMatcherOptions);
  term.loadAddon(webLinksAddon);
  unicode11Addon = new Unicode11Addon();
  term.loadAddon(unicode11Addon);
  term.unicode.activeVersion = '11';

  window.term = term;

  window.terminalBridge.onoutput = (data => {
    term.writeUtf8(data);
  });

  term.onData(data => {
    window.terminalBridge.inputReceived(data);
  });

  term.onBinary(binary => {
    window.terminalBridge.binaryReceived(binary);
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

  window.onmouseup = (e) => defaultOnMouseUpHandler(e);

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
  term.setOption('fontWeight', options.fontWeight);
  term.setOption('fontWeightBold', convertBoldText(options.fontWeight));
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

window.findNext = (content: string, caseSensitive: boolean, wholeWord: boolean, regex: boolean) => {
  searchAddon.findNext(content, { caseSensitive: caseSensitive, wholeWord: wholeWord, regex: regex });
}

window.findPrevious = (content: string, caseSensitive: boolean, wholeWord: boolean, regex: boolean) => {
  searchAddon.findPrevious(content, { caseSensitive: caseSensitive, wholeWord: wholeWord, regex: regex });
}

document.oncontextmenu = function () {
  return false;
};

function convertBoldText(fontWeight: FontWeight) : FontWeight {
  return parseInt(fontWeight) > 600 ? '900' : 'bold';
}