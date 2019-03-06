using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace FluentTerminal.App.ViewModels.Utilities
{
    public class InputBuffer
    {
        private readonly Timer _timer = new Timer();
        private readonly Func<byte[], Task> _writeCallback;
        private readonly List<byte> _buffer = new List<byte>();
        private readonly object _lock = new object();

        public InputBuffer(Func<byte[], Task> writeCallback)
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
                var data = _buffer.ToArray();
                _buffer.Clear();
                _writeCallback.Invoke(data);
            }
        }

        public void Write(byte[] data)
        {
            lock (_lock)
            {
                _buffer.AddRange(data);
            }
        }
    }
}
