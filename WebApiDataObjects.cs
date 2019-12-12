using System;

namespace Termors.Services.HippoPiPwmLedDaemon
{
    public class LampDataObject
    {
        public bool burn { get; set; }
        public byte red { get; set; }
        public byte green { get; set; }
        public byte blue { get; set; }
    }

    public class VersionDataObject
    {
        public string version { get; set; }
    }

    public enum LampBehavior
    {
        START_OFF,      // Do not power on lamp when power comes on
        START_ON,       // Always power on lamp when power comes on (allows using regular light switch)
        START_LAST      // Power lamp or not depending on whether it was on or off when the power was cut
    }

    public enum LampType
    {
        Unknown,                // Not determined yet
        LampDimmable,           // One color, dimmable 0-100%
        LampColor1D,            // E.g. cool white to warm white, dimmable 0-100%
        LampColorRGB,           // RGB led
        Switch                  // On/off switch (e.g. relay)
    }

    public class ConfigDataObject
    {
        // Internal properties not for JSON sterilization
        internal LampBehavior Behavior { get; set; } = LampBehavior.START_OFF;
        internal LampType TypeOfLamp { get; set; } = LampType.Unknown;

        public string name { get; set; }
        public int behavior
        {
            get
            {
                return Convert.ToInt32(Behavior);
            }
            set
            {
                Behavior = (LampBehavior)value;
            }
        }

        public int type
        {
            get
            {
                return Convert.ToInt32(TypeOfLamp);
            }
            set
            {
                TypeOfLamp = (LampType)value;
            }
        }
    }
}
