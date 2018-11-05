using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

using Makaretu.Dns;
using Microsoft.Owin.Hosting;
using Owin;

namespace Termors.Serivces.HippoPiPwmLedDaemon
{
    public class LightService : IDisposable
    {
        private readonly ushort _port;
        private readonly string _name;
        private readonly int _redChannel;
        private IDisposable _webapp = null;
        private byte _red = 0, _green = 0, _blue = 0;
        private bool _on;

        public LightService(string name, ushort port, int redChannel)
        {
            _name = name;
            _port = port;
            _redChannel = redChannel;
        }

        public static readonly IDictionary<ushort, LightService> Registry = new Dictionary<ushort, LightService>();

        public void RegisterMDNS()
        {
            var service = new ServiceProfile("HippoLed-" + _name, "_hippohttp._tcp", _port);
            var sd = new ServiceDiscovery();
            sd.Advertise(service);
        }

        public void StartWebserver()
        {
            string url = "http://*:" + _port;
            _webapp = WebApp.Start(url, new Action<IAppBuilder>(WebConfiguration));

            Registry[_port] = this;
        }

        public async Task SetRGB(byte red, byte green, byte blue)
        {
            _red = red;
            _green = green;
            _blue = blue;

            await InternalSetPWM(red, green, blue);
        }

        protected async Task InternalSetPWM(byte red, byte green, byte blue)
        {
            // Write to PWM registers on Raspberry Pi
            PwmService.Instance[_redChannel] = _red;
            PwmService.Instance[_redChannel + 1] = _green;
            PwmService.Instance[_redChannel + 2] = _blue;

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
                    if (_on) InternalSetPWM(_red, _green, _blue).Wait();
                    else InternalSetPWM(0, 0, 0).Wait();
                }
            }
        }

        public byte Red
        {
            get { return _red; }
        }
        public byte Green
        {
            get { return _green; }
        }
        public byte Blue
        {
            get { return _blue; }
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
