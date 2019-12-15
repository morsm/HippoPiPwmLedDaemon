using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Termors.Services.HippoPiPwmLedDaemon.Extensions;

namespace Termors.Services.HippoPiPwmLedDaemon
{
    /// <summary>
    /// Class to drive the Adafruit 24-channel SPI PWM board,
    /// through the file system interface of the Raspberry Pi
    /// </summary>
    public class PwmService
    {
        protected readonly byte[] _registers = new byte[24];
        protected readonly SemaphoreSlim _mutex = new SemaphoreSlim(1);

        protected PwmService()
        {
            for (int i = 0; i < _registers.Length; i++) _registers[i] = 0;
        }

        public static readonly PwmService Instance = new PwmService();

        public static string BaseCommand { get; set; }
        public static string BaseArgs { get; set; }
        public static bool Verbose { get; set; }
        public static bool Invert { get; set; }

        public byte this[int index]
        {
            get
            {
                return GetValue(index);
            }
            set
            {
                SetValue(index, value);
            }
        }

        public byte GetValue(int index)
        {
            return _registers[index];
        }

        public void SetValue(int index, byte val)
        {
            _registers[index] = val;
        }

        public async Task WritePwmData()
        {
            // Build command line
            StringBuilder sb = new StringBuilder(BaseArgs);
            for (int i = 0; i < _registers.Length; i++)
            {
                // Create 12-bit number
                int twelveBitValue = ((int)_registers[i]) * 16;
                if (Invert) twelveBitValue = 4095 - twelveBitValue;

                sb.Append(" ").Append(twelveBitValue);
            }

            ProcessStartInfo start = new ProcessStartInfo(BaseCommand, sb.ToString());
            start.RedirectStandardOutput = (!Verbose);      // Redirect if verbose flag not set; redirect prevents from showing up on console.
            start.UseShellExecute = false;

            try
            {
                // Obtain the Mutex, so only one thread can write PWM at a time
                await _mutex.WaitAsync();

                var proc = Process.Start(start);
                await proc.WaitForExitAsync();
                
            }
            finally
            {
                // Release the mutex
                _mutex.Release();
            }
        }
    }
}
