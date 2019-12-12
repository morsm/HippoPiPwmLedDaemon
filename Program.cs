using System;
using System.IO;
using System.Threading;

using Newtonsoft.Json;

namespace Termors.Services.HippoPiPwmLedDaemon
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var config = ReadConfig();
            PwmService.BaseCommand = config.BaseCommand;
            PwmService.BaseArgs = config.BaseArgs;
            PwmService.Verbose = config.Verbose;

            // Initial PWM setup (all lamps off)
            PwmService.Instance.WritePwmData().Wait();

            // Start web services
            SetupServices(config.Lamps);

            // Run until Ctrl+C
            var endEvent = new ManualResetEvent(false);
            Console.WriteLine("HippoPiPwmLedDaemon started");

            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("HippoPiPwmLedDaemon stopped");

                LightService.DisposeAll();                  // Stop all web servers

                endEvent.Set();
            };

            endEvent.WaitOne();
        }

        public static Configuration ReadConfig()
        {
            using (StreamReader rea = new StreamReader("pipwmled.json"))
            {
                string json = rea.ReadToEnd();
                return JsonConvert.DeserializeObject<Configuration>(json);
            }
        }

        public static void SetupServices(ConfigObject[] objects)
        {
            foreach (var o in objects)
            {
                var svc = new LightService(o.Name, o.Port, o.RedChannel);
                svc.StartWebserver();
                svc.RegisterMDNS();
            }
        }
    }
}
