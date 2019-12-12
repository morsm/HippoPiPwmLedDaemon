using System;

namespace Termors.Services.HippoPiPwmLedDaemon
{
    public class ConfigObject
    {
        public string Name { get; set; }
        public ushort Port { get; set; }
        public byte RedChannel { get; set; }
    }

    public class Configuration
    {
        public string BaseCommand { get; set; }
        public string BaseArgs { get; set; }
        public bool Verbose { get; set; }
        public ConfigObject[] Lamps { get; set; }
    }
}
