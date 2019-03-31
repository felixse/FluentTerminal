using FluentTerminal.App.Views.NativeFrontend.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentTerminal.App.Views.NativeFrontend.Ansi
{
    internal class AnsiParser
    {
        private ParserState _state;
        private int _position;
        private object _currentParam;
        private List<object> _params = new List<object>();
        private List<TerminalCode> _terminalCodes = new List<TerminalCode>();

        /// <summary>
        /// CSI codes can have prefixes of '?', '>'
        /// </summary>
        private string _prefix;

        /// <summary>
        /// CSI codes can have postfixes of '$',
        /// </summary>
        private string _postfix;

        private readonly StringBuilder _currentTextEmit = new StringBuilder(capacity: 128);

        public TerminalCode[] Parse(ArrayReader<char> reader)
        {
            _terminalCodes.Clear();
            while (reader.TryRead(out char ch))
            {
                int code = ch;
                // if (0xD800 <= code && code <= 0xDBFF)
                // {
                //     // we got a surrogate high
                //     // get surrogate low (next 2 bytes)
                //     if (!reader.TryPeek(out char low))
                //     {
                //         // end of data stream, save surrogate high
                //         // this._terminal.surrogate_high = ch;
                //         continue;
                //     }
                //     code = ((code - 0xD800) * 0x400) + (low - 0xDC00) + 0x10000;
                //     // ch += low;
                // }
                // // surrogate low - already handled above
                // if (0xDC00 <= code && code <= 0xDFFF)
                // {
                //     continue;
                // }

                switch (_state)
                {
                    case ParserState.Normal:
                        if (!NormalStateHandler(ch))
                        {
                            Emit(ch);
                        }
                        break;
                    case ParserState.Escaped:
                        if (!EscapedStateHandler(ch))
                        {
                            switch (ch)
                            {
                                // ESC (,),*,+,-,. Designate G0-G2 Character Set.
                                case '(': // <-- this seems to get all the attention
                                          // this._terminal.gcharset = 0;
                                    _state = ParserState.Charset;
                                    break;
                                case ')':
                                case '-':
                                    // this._terminal.gcharset = 1;
                                    _state = ParserState.Charset;
                                    break;
                                case '*':
                                case '.':
                                    // this._terminal.gcharset = 2;
                                    _state = ParserState.Charset;
                                    break;
                                case '+':
                                    // this._terminal.gcharset = 3;
                                    _state = ParserState.Charset;
                                    break;

                                // Designate G3 Character Set (VT300).
                                // A = ISO Latin-1 Supplemental.
                                // Not implemented.
                                case '/':
                                    // _terminal.gcharset = 3;
                                    _state = ParserState.Charset;
                                    // _position--
                                    break;

                                // ESC N
                                // Single Shift Select of G2 Character Set
                                // ( SS2 is 0x8e). This affects next character only.
                                case 'N':
                                    break;
                                // ESC O
                                // Single Shift Select of G3 Character Set
                                // ( SS3 is 0x8f). This affects next character only.
                                case 'O':
                                    break;
                                // ESC n
                                // Invoke the G2 Character Set as GL (LS2).
                                case 'n':
                                    // this._terminal.setgLevel(2);
                                    break;
                                // ESC o
                                // Invoke the G3 Character Set as GL (LS3).
                                case 'o':
                                    // this._terminal.setgLevel(3);
                                    break;
                                // ESC |
                                // Invoke the G3 Character Set as GR (LS3R).
                                case '|':
                                    // this._terminal.setgLevel(3);
                                    break;
                                // ESC }
                                // Invoke the G2 Character Set as GR (LS2R).
                                case '}':
                                    // this._terminal.setgLevel(2);
                                    break;
                                // ESC ~
                                // Invoke the G1 Character Set as GR (LS1R).
                                case '~':
                                    // this._terminal.setgLevel(1);
                                    break;

                                // ESC 7 Save Cursor (DECSC).
                                case '7':
                                    Emit(TerminalCodeType.SaveCursor);
                                    _state = ParserState.Normal;
                                    break;

                                // ESC 8 Restore Cursor (DECRC).
                                case '8':
                                    Emit(TerminalCodeType.RestoreCursor);
                                    _state = ParserState.Normal;
                                    break;

                                // ESC # 3 DEC line height/width
                                case '#':
                                    _state = ParserState.Normal;
                                    // _position++;
                                    break;

                                // ESC H Tab Set (HTS is 0x88).
                                case 'H':
                                    Emit(TerminalCodeType.TabSet);
                                    _state = ParserState.Normal;
                                    break;

                                // ESC = Application Keypad (DECKPAM).
                                case '=':
                                    // this._terminal.log('Serial port requested application keypad.');
                                    // this._terminal.applicationKeypad = true;
                                    // this._terminal.viewport.syncScrollArea();
                                    _state = ParserState.Normal;
                                    break;

                                // ESC > Normal Keypad (DECKPNM).
                                case '>':
                                    // this._terminal.log('Switching back to normal keypad.');
                                    // this._terminal.applicationKeypad = false;
                                    // this._terminal.viewport.syncScrollArea();
                                    _state = ParserState.Normal;
                                    break;

                                default:
                                    _state = ParserState.Normal;
                                    // _terminal.error('Unknown ESC control: %s.', ch);
                                    break;
                            }
                        }
                        break;
                    case ParserState.Charset:
                        // if (ch in CHARSETS)
                        // {
                        //     cs = CHARSETS[ch];
                        //     if (ch === '/')
                        //     { // ISOLatin is actually /A
                        //         this.skipNextChar();
                        //     }
                        // }
                        // else
                        // {
                        //     cs = DEFAULT_CHARSET;
                        // }
                        // this._terminal.setgCharset(this._terminal.gcharset, cs);
                        // this._terminal.gcharset = null;
                        _state = ParserState.Normal;
                        break;
                    case ParserState.Osc:
                        // OSC Ps ; Pt ST
                        // OSC Ps ; Pt BEL
                        //   Set Text Parameters.
                        if (ch == C0.ESC || ch == C0.BEL)
                        {
                            if (ch == C0.ESC)
                            {
                                _position++;
                            }
                            _params.Add(_currentParam);
                            switch ((int)_params[0])
                            {
                                case 0:
                                case 1:
                                case 2:
                                    if (_params.Count >= 1 && _params[1] != null)
                                    {
                                        Emit(new TerminalCode(TerminalCodeType.SetTitle, (string)_params[1]));
                                    }
                                    break;
                                case 3:
                                    // set X property
                                    break;
                                case 4:
                                case 5:
                                    // change dynamic colors
                                    break;
                                case 10:
                                case 11:
                                case 12:
                                case 13:
                                case 14:
                                case 15:
                                case 16:
                                case 17:
                                case 18:
                                case 19:
                                    // change dynamic ui colors
                                    break;
                                case 46:
                                    // change log file
                                    break;
                                case 50:
                                    // dynamic font
                                    break;
                                case 51:
                                    // emacs shell
                                    break;
                                case 52:
                                    // manipulate selection data
                                    break;
                                case 104:
                                case 105:
                                case 110:
                                case 111:
                                case 112:
                                case 113:
                                case 114:
                                case 115:
                                case 116:
                                case 117:
                                case 118:
                                    // reset colors
                                    break;
                            }

                            _params.Clear();
                            _currentParam = 0;
                            _state = ParserState.Normal;
                        }
                        else
                        {
                            if (_params.Count == 0)
                            {
                                if (ch >= '0' && ch <= '9')
                                {
                                    _currentParam = ((int)_currentParam * 10) + ch - 48;
                                }
                                else if (ch == ';')
                                {
                                    _params.Add(_currentParam);
                                    _currentParam = "";
                                }
                            }
                            else
                            {
                                _currentParam = (string)_currentParam + ch;
                            }
                        }
                        break;
                    case ParserState.CsiParam:
                        if (!CsiParamStateHandler(ch))
                        {
                            FinaliseParam();
                            _state = ParserState.Csi;
                            goto case ParserState.Csi;
                        }
                        break;
                    case ParserState.Csi:
                        if (!CsiStateHandler(ch))
                        {
                            //throw new Exception($"Unknown CSI code: {ch}"); // conpty is sending 'X' ?!
                        }
                        _state = ParserState.Normal;
                        _prefix = "";
                        _postfix = "";
                        break;

                    case ParserState.Dcs:
                        if (ch == C0.ESC || ch == C0.BEL)
                        {
                            if (ch == C0.ESC)
                            {
                                // _position++;
                            }
                            switch (_prefix)
                            {
                                // User-Defined Keys (DECUDK)
                                case "":
                                    break;

                                // Request Status String (DECRQSS).
                                // test: echo -e '\eP$q"p\e\\'
                                case "$q":
                                    {
                                        string pt = (string)_currentParam;
                                        bool valid = false;
                                        switch (pt)
                                        {
                                            // DECSCA
                                            case "\"q":
                                                pt = "0\"q";
                                                break;

                                            // DECSCL
                                            case "\"p":
                                                pt = "61\"p";
                                                break;

                                            // DECSTBM
                                            case "r":
                                                // pt = ""
                                                //     + (this._terminal.scrollTop + 1)
                                                //     + ";"
                                                //     + (this._terminal.scrollBottom + 1)
                                                //     + "r";
                                                break;

                                            // SGR
                                            case "m":
                                                pt = "0m";
                                                break;

                                            default:
                                                throw new Exception($"Unknown DCS Pt: {pt}.");
                                                pt = "";
                                                break;
                                        }

                                        // this._terminal.send(C0.ESC + 'P' + +valid + '$r' + pt + C0.ESC + '\\');
                                        break;
                                    }

                                // Set Termcap/Terminfo Data (xterm, experimental).
                                case "+p":
                                    break;

                                // Request Termcap/Terminfo String (xterm, experimental)
                                // Regular xterm does not even respond to this sequence.
                                // This can cause a small glitch in vim.
                                // test: echo -ne '\eP+q6b64\e\\'
                                case "+q":
                                    {
                                        string pt = (string)_currentParam;
                                        bool valid = false;
                                        // this._terminal.send(C0.ESC + 'P' + +valid + '+r' + pt + C0.ESC + '\\');
                                        break;
                                    }
                                default:
                                    throw new Exception($"Unknown DCS prefix: {_prefix}");
                                    break;
                            }

                            _currentParam = 0;
                            _prefix = "";
                            _state = ParserState.Normal;
                        }
                        else if (_currentParam == null)
                        {
                            if (_prefix.Length == 0 && ch != '$' && ch != '+')
                            {
                                _currentParam = ch;
                            }
                            else if (_prefix.Length == 2)
                            {
                                _currentParam = ch;
                            }
                            else
                            {
                                _prefix += ch;
                            }
                        }
                        else
                        {
                            _currentParam = (string)_currentParam + ch;
                        }
                        break;
                    case ParserState.Ignore:
                        // For PM and APC.
                        if (ch == C0.ESC || ch == C0.BEL)
                        {
                            if (ch == C0.ESC)
                            {
                                _position++;
                            }
                            _state = ParserState.Normal;
                        }
                        break;
                }
            }
            FinialiseEmit();
            return _terminalCodes.ToArray();
        }

        private bool NormalStateHandler(char c)
        {
            switch (c)
            {
                case C0.BEL:
                    Emit(TerminalCodeType.Bell);
                    return true;
                case C0.LF:
                case C0.VT:
                case C0.FF:
                    Emit(TerminalCodeType.LineFeed);
                    return true;
                case C0.CR:
                    Emit(TerminalCodeType.CarriageReturn);
                    return true;
                case C0.BS:
                    Emit(TerminalCodeType.Backspace);
                    return true;
                case C0.HT:
                    Emit(TerminalCodeType.Tab);
                    return true;
                case C0.SO:
                    Emit(TerminalCodeType.ShiftOut);
                    return true;
                case C0.SI:
                    Emit(TerminalCodeType.ShiftIn);
                    return true;
                case C0.ESC:
                    _state = ParserState.Escaped;
                    return true;
                default:
                    return false;
            }
        }

        private bool EscapedStateHandler(char ch)
        {
            switch (ch)
            {
                case '[':
                    // ESC [ Control Sequence Introducer (CSI  is 0x9b)
                    _params.Clear();
                    _currentParam = 0;
                    _state = ParserState.CsiParam;
                    return true;
                case ']':
                    // ESC ] Operating System Command (OSC is 0x9d)
                    _params.Clear();
                    _currentParam = 0;
                    _state = ParserState.Osc;
                    return true;
                case 'P':
                    // ESC P Device Control String (DCS is 0x90)
                    _params.Clear();
                    _currentParam = 0;
                    _state = ParserState.Dcs;
                    return true;
                case '_':
                    // ESC _ Application Program Command ( APC is 0x9f).
                    _state = ParserState.Ignore;
                    return true;
                case '^':
                    // ESC ^ Privacy Message ( PM is 0x9e).
                    _state = ParserState.Ignore;
                    return true;
                case 'c':
                    // ESC c Full Reset (RIS).
                    Emit(TerminalCodeType.Reset);
                    return true;
                case 'E':
                    // ESC E Next Line ( NEL is 0x85).
                    // terminal.x = 0;
                    // termina.index();
                    _state = ParserState.Normal;
                    return true;
                case 'D':
                    // ESC D Index ( IND is 0x84).
                    // terminal.index();
                    _state = ParserState.Normal;
                    return true;
                case 'M':
                    // ESC M Reverse Index ( RI is 0x8d).
                    // terminal.reverseIndex();
                    _state = ParserState.Normal;
                    return true;
                case '%':
                    // ESC % Select default/utf-8 character set.
                    // @ = default, G = utf-8
                    // terminal.setgLevel(0);
                    // terminal.setgCharset(0, DEFAULT_CHARSET); // US (default)
                    _state = ParserState.Normal;
                    // parser.skipNextChar();
                    return true;
                case C0.CAN:
                    _state = ParserState.Normal;
                    return true;
                default:
                    return false;
            }
        }

        private bool CsiStateHandler(char ch)
        {
            switch (ch)
            {
                case 'A':
                    Emit(new TerminalCode(TerminalCodeType.CursorUp, (int)_params[0], 0));
                    return true;
                case 'G':
                    Emit(new TerminalCode(TerminalCodeType.CursorCharAbsolute, 0, (int)_params[0] - 1));
                    return true;
                case 'H':
                    var line = _params.Count > 1 ? (int)_params[1] - 1 : 0; // make conpty happy
                    Emit(new TerminalCode(TerminalCodeType.CursorPosition, line, (int)_params[0] - 1));
                    return true;
                case 'K':
                    Emit(TerminalCodeType.EraseInLine);
                    return true;
                case 'J':
                    Emit(TerminalCodeType.EraseInDisplay);
                    return true;
                case 'h':
                    Emit(TerminalCodeType.SetMode);
                    return true;
                case 'l':
                    Emit(TerminalCodeType.ResetMode);
                    return true;
                case 'n':
                    return true;
                case 'm':
                    Emit(new TerminalCode(TerminalCodeType.CharAttributes, GetCharAttributes()));
                    return true;
                default:
                    return false;
            }
        }

        private bool CsiParamStateHandler(char ch)
        {
            switch (ch)
            {
                case '?':
                case '>':
                case '!':
                case '$':
                case '"':
                case ' ':
                case '\'':
                    _prefix = ch.ToString();
                    return true;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    _currentParam = (int)_currentParam * 10 + (ch - '0');
                    return true;
                case ';':
                    FinaliseParam();
                    return true;
                case C0.CAN:
                    _state = ParserState.Normal;
                    return true;
                default:
                    return false;
            }
        }

        private void FinaliseParam()
        {
            _params.Add(_currentParam);
            _currentParam = 0;
        }

        private CharAttributes GetCharAttributes()
        {
            var attributes = new CharAttributes();
            for (int i = 0; i < _params.Count; i++)
            {
                int p = (int)_params[i];
                if (p >= 30 && p <= 37)
                {
                    // fg color 8
                    attributes.ForegroundColor = p - 30;
                }
                else if (p >= 40 && p <= 47)
                {
                    // bg color 8
                    attributes.BackgroundColor = p - 40;
                }
                else if (p >= 90 && p <= 97)
                {
                    // fg color 16
                    p += 8;
                    attributes.ForegroundColor = p - 90;
                }
                else if (p == 0)
                {
                    attributes.Flags = 0;
                    attributes.ForegroundColor = 0;
                    attributes.BackgroundColor = 0;
                }
                else if (p == 1)
                {
                    attributes.Flags |= 1;
                }
                else
                {

                }
            }
            return attributes;
        }

        private void Emit(TerminalCodeType type)
        {
            Emit(new TerminalCode(type));
        }

        private void Emit(char c)
        {
            _currentTextEmit.Append(c);
        }

        private void Emit(TerminalCode code)
        {
            FinialiseEmit();
            _terminalCodes.Add(code);
        }

        private void FinialiseEmit()
        {
            if (_currentTextEmit.Length != 0)
            {
                string text = _currentTextEmit.ToString();
                _currentTextEmit.Clear();
                _terminalCodes.Add(new TerminalCode(TerminalCodeType.Text, text));
            }
        }

        private enum ParserState
        {
            Normal,
            Escaped,
            CsiParam,
            Csi,
            Osc,
            Charset,
            Dcs,
            Ignore
        }
    }
}
