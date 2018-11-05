using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Termors.Serivces.HippoPiPwmLedDaemon
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

        public static string Filename
        {
            get
            {
                if (DebugData.Instance.Debug) return "/tmp/pwm.bin"; else return "/dev/spidev0.0";
            }
        }

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
            byte[] output = GetAdafruitBytes();

            try
            {
                // Obtain the Mutex, so only one thread can write PWM at a time
               await _mutex.WaitAsync();

                using (FileStream fs = new FileStream(Filename, FileMode.Create, FileAccess.Write))
                {
                    await fs.WriteAsync(output, 0, output.Length);
                }
            } 
            finally 
            {
                // Release the mutex
                _mutex.Release();
            }
        }

        protected byte[] GetAdafruitBytes()
        {
            byte[] nibbles = new byte[72];
            byte[] output = new byte[36];

            // Output sequence is 24 12-bit values, MSB of 23rd PWM output first
            // This equates to 36 bytes, or 72 half-bytes (nibbles)
            // that the 24 8-bit register values have to be written into.
            //
            // Output byte 0 is register 23, high and low nibble
            // Output byte 1 is a 0-nibble, followed by the most significant nibble of register 22
            // Output byte 2 is the least significant nibble of 22, then a 0 nibble
            // ...
            // and so on.

            // Fill nibbles
            for (int i = 0; i < _registers.Length; i++)
            {
                nibbles[nibbles.Length - 1 - 3 * i] = (byte) ((_registers[i] & 0xf0) >> 4);     // MSB
                nibbles[nibbles.Length - 1 - 3 * i - 1] = (byte)((_registers[i] & 0x0f));
                nibbles[nibbles.Length - 1 - 3 * i - 2] = 0;                                    // LSB
            }

            // Fill bytes
            for (int j = 0; j < output.Length; j++)
            {
                output[j] = (byte) ((nibbles[2 * j + 1] << 4) | nibbles[2 * j]);
            }

            return output;
        }
    }
}
