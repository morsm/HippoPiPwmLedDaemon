using System;
namespace Termors.Serivces.HippoPiPwmLedDaemon
{
    public class DebugData
    {
        protected DebugData()
        {
            Debug = false;
        }

        public static readonly DebugData Instance = new DebugData();

        public bool Debug { get; set; }

        public static void ProcessCommandline(string[] args)
        {
            foreach (string arg in args)
            {
                Instance.Debug = arg.ToLower().StartsWith("-d", StringComparison.CurrentCulture);
                if (Instance.Debug) break; // Debug argument found
            }
        }
    }
}
