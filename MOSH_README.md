# Using `mosh` with FluentTerminal

`mosh` can be used in FluentTerminal in two ways:

 1. Through GUI, by turning on ***Use mosh*** option in *SSH Connection* dialog ('Ctrl+Shift+Y' and 'Ctrl+Alt+Y');
 2. As a custom command, in *Quick Launch* dialog ('Ctrl+Shift+Q' and 'Ctrl+Alt+Q').

The first way, through GUI is very simple and self-descriptive, and it will not be explained here. Using `mosh` in *Quick Launch* is more complicated, but also offers more control. The simplest `mosh` command you can enter is:

    mosh user@host

This will initiate a `mosh` session with default options (i.e. port range will default to `60000:61000`).

`mosh` additionally supports many more command-line arguments. In fact, it's almost completely compliant to [mosh(1) - Linux man page](https://linux.die.net/man/1/mosh). `mosh` is defined there as:

`mosh [options] [--] [user@]host [command...]`

`mosh` embedded in FluentTerminal only lacks support for the last part (`[command...]`). You can find detailed explanation at the man page mentioned above. Also you can execute custom command `mosh --help`, and you'll get the following output:

    Usage: mosh [options] [user@]host
    
          --client=VALUE         Path to client helper on local machine (default:
                                   "mosh-client")
          --server=VALUE         Command to run server helper on remote machine
                                   (default: "mosh-server").
                                    Example: '--server="mosh-server new -v -c 256"-
                                   '.
                                    See https://linux.die.net/man/1/mosh-server for
                                   more details.
          --ssh=VALUE            OpenSSH command to remotely execute mosh-server
                                   on remote machine (default: "ssh").
                                    Example: ''--ssh="ssh -p 2222"'.
                                    See https://man.openbsd.org/ssh for more
                                   details.
          --predict=VALUE        Controls use of speculative local echo. Defaults
                                   to 'adaptive' (show predictions on slower links
                                   and to smooth out network glitches) and can also
                                   be 'always' or 'never'.
      -a                         Synonym 'for --predict=always'.
      -n                         Synonym 'for --predict=never'.
      -p, --port=VALUE           Use a particular server-side UDP port or port
                                   range, for example, if this is the only port
                                   that is forwarded through a firewall to the
                                   server. Otherwise, mosh will choose a port
                                   between 60000 and 61000.
                                    Example: '--port=60000:60100'
          --help                 Show help.
          --no-init              Do not send the smcup initialization string and
                                   rmcup deinitialization string to the client's
                                   terminal. On many terminals this disables
                                   alternate screen mode.
    
    Exit codes:
      254 - Invalid command line arguments
      255 - Initial SSH of Mosh connection setup failed
      All other values - Exit code returned by remote shell
