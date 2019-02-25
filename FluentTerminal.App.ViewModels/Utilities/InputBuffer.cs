using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace FluentTerminal.App.ViewModels.Utilities
{
    public class InputBuffer
    {
        private readonly Timer _timer = new Timer();
        private readonly Func<string, Task> _writeCallback;
        private readonly List<string> _buffer = new List<string>();
        private readonly object _lock = new object();

        public InputBuffer(Func<string, Task> writeCallback)
        {
            _timer.AutoReset = true;
            _timer.Elapsed += _timer_Elapsed;
            _writeCallback = writeCallback;
            _timer.Interval = 5;
            _timer.Start();
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (_lock)
            {
                var data = string.Concat(_buffer);
                _buffer.Clear();
                _writeCallback.Invoke(data);
            }
        }

        public void Write(string data)
        {
            lock (_lock)
            {
                _buffer.Add(data);
            }
        }
    }
}
