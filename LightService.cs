using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

using Makaretu.Dns;
using Microsoft.Owin.Hosting;
using Owin;

namespace Termors.Services.HippoPiPwmLedDaemon
{
    public delegate Task LampSwitchedDelegate(LightService service, bool on);

    public class LightService : IDisposable
    {
        private readonly ushort _port;
        private IDisposable _webapp = null;
        private bool _on;
        private readonly byte _redChannel;

        public LightService(string name, ushort port, byte redChannel)
        {
            Name = name;
            _port = port;
            _redChannel = redChannel;
        }

        public event LampSwitchedDelegate LampSwitched;
        public static readonly IDictionary<ushort, LightService> Registry = new Dictionary<ushort, LightService>();

        protected static readonly ServiceDiscovery Discovery = new ServiceDiscovery();

        public void RegisterMDNS()
        {
            var service = new ServiceProfile("HippoLed-" + Name, "_hippohttp._tcp", _port);

            Discovery.Advertise(service);
        }

        public void StartWebserver()
        {
            string url = "http://*:" + _port;
            _webapp = WebApp.Start(url, new Action<IAppBuilder>(WebConfiguration));

            Registry[_port] = this;
        }

        public async Task SetRGB(byte red, byte green, byte blue)
        {
            Red = red;
            Green = green;
            Blue = blue;

            await InternalSetPWM();
        }

        protected async Task InternalSetPWM()
        {
            // Write to PWM registers on Raspberry Pi
            // RGB values are written if the lamp is on, otherwise zeros.
            PwmService.Instance[_redChannel] = On ? Red : (byte) 0;
            PwmService.Instance[_redChannel + 1] = On ? Green : (byte) 0;
            PwmService.Instance[_redChannel + 2] = On ? Blue : (byte) 0;

            await PwmService.Instance.WritePwmData();
        }

        public bool On
        {
            get
            {
                return _on;
            }
            set
            {
                bool oldStatus = _on;
                if (oldStatus != value)
                {
                    _on = value;
                    InternalSetPWM().Wait();
                }
            }
        }

        public byte Red
        {
            get; protected set;
        }
        public byte Green
        {
            get; protected set;
        }
        public byte Blue
        {
            get; protected set;
        }

        public string Name
        {
            get; set;
        }


        public void Dispose()
        {
            if (_webapp != null) _webapp.Dispose();
        }

        public static void DisposeAll()
        {
            foreach (var svc in Registry.Values) svc.Dispose();
            Registry.Clear();
        }


        // This code configures Web API using Owin
        private void WebConfiguration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();

            // Format to JSON by default
            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.EnsureInitialized();

            appBuilder.UseWebApi(config);
        }

    }
}
