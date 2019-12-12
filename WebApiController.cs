using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Text;


namespace Termors.Services.HippoPiPwmLedDaemon
{

    public class WebApiController : ApiController
    {
        public static readonly string API_VERSION = "3.3.0";

        [Route("status.json"), HttpGet]
        public LampDataObject GetStatus()
        {
            LightService svc = CurrentService;
            LampDataObject obj = new LampDataObject
            {
                burn = svc.On,
                red = svc.Red,
                blue = svc.Blue,
                green = svc.Green
            };

            return obj;
        }

        [Route("version.json"), HttpGet]
        public VersionDataObject GetVersion()
        {
            return new VersionDataObject { version = API_VERSION };
        }


        [Route("rgb.json"), HttpPost]
        public void SetStatus(LampDataObject obj)
        {
            // TODO: partial response, not on/off and r,g,b always set simultaneously

            LightService svc = CurrentService;

            svc.On = obj.burn;
            svc.SetRGB(obj.red, obj.green, obj.blue).Wait();
        }

        [Route("config.json"), HttpGet]
        public ConfigDataObject GetConfig()
        {
            ConfigDataObject cfg = new ConfigDataObject
            {
                name = CurrentService.Name,
                Behavior = LampBehavior.START_OFF,
                TypeOfLamp = LampType.LampColorRGB
            };

            return cfg;
        }

        [Route("config.json"), HttpPost]
        public void SetConfig()
        {
            // Do nothing. Nothing to set
        }

        // Simple on/off commands
        [Route("on.html"), HttpGet]
        public HttpResponseMessage On()
        {
            CurrentService.On = true;

            return HtmlStatusResponse();
        }
        [Route("off.html"), HttpGet]
        public HttpResponseMessage Off()
        {
            CurrentService.On = false;

            return HtmlStatusResponse();
        }

        protected HttpResponseMessage HtmlStatusResponse()
        {
            var response = new HttpResponseMessage();

            StringBuilder sb = new StringBuilder("<html><head><title>Lamp status</title></head><body>Lamp is ");
            if (CurrentService.On) sb.Append("ON"); else sb.Append("OFF");
            sb.Append("<br/>");

            sb.AppendFormat("Red = {0}<br/>", CurrentService.Red);
            sb.AppendFormat("Green = {0}<br/>", CurrentService.Green);
            sb.AppendFormat("Blue = {0}<br/>", CurrentService.Blue);
            sb.Append("</body></html>");

            response.Content = new StringContent(sb.ToString());
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }


        protected ushort RequestPort
        {
            get
            {
                return Convert.ToUInt16(Request.RequestUri.Port);
            }
        }

        protected LightService CurrentService
        {
            get
            {
                var svc = LightService.Registry[RequestPort];
                if (svc == null)
                {
                    // Shouldn't happen. Port doesn't match any instance
                    throw new HttpResponseException(
                        new HttpResponseMessage(HttpStatusCode.NotFound) { 
                        ReasonPhrase = "Service instance not found in LightService registry" 
                    });
                }
                return svc;
            }
        }
    }
}
