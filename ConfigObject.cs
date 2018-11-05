using System;

namespace Termors.Serivces.HippoPiPwmLedDaemon
{
    public class ConfigObject
    {
        public string Name { get; set; }
        public ushort Port { get; set; }
        public byte RedChannel { get; set; }
    }
}
