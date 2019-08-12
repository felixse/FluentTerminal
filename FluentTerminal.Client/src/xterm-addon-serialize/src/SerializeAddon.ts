/**
 * Copyright (c) 2019 The xterm.js authors. All rights reserved.
 * @license MIT
 */

import { Terminal, ITerminalAddon } from 'xterm';

function crop(value: number, from: number, to: number): number {
  return Math.max(from, Math.min(value, to));
}

export class SerializeAddon implements ITerminalAddon {
  private _terminal: Terminal | undefined;

  constructor() { }

  public activate(terminal: Terminal): void {
    this._terminal = terminal;
  }

  public serialize(rows?: number): string {
    // TODO: Add frontground/background color support later
    if (!this._terminal) {
      throw new Error('Cannot use addon until it has been loaded');
    }
    const terminalRows = this._terminal.buffer.length;
    if (rows === undefined) {
      rows = terminalRows;
    }
    rows = crop(rows, 0, terminalRows);

    const buffer = this._terminal.buffer;
    const lines: string[] = new Array<string>(rows);

    var doRTrim = true;
    for (let i = terminalRows - 1; i >= terminalRows - rows; i--) {
      const line = buffer.getLine(i);
      if (doRTrim === true) {
        if (!line || line.translateToString(true).length == 0) {
          lines.length -= 1;
          continue;
        }
        doRTrim = false;
      }
      lines[i - terminalRows + rows] = line ? line.translateToString(true) : '';
    }

    return lines.join('\r\n');
  }

  public dispose(): void { }
}
