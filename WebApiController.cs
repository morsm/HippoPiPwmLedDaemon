using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Owin;

namespace Termors.Serivces.HippoPiPwmLedDaemon
{
    public class LampDataObject
    {
        public bool burn { get; set; }
        public byte red { get; set; }
        public byte green { get; set; }
        public byte blue { get; set; }
    }


    public class WebApiController : ApiController
    {
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

        [Route("rgb.json"), HttpPost]
        public void SetStatus(LampDataObject obj)
        {
            // TODO: partial response, not on/off and r,g,b always set simultaneously

            LightService svc = CurrentService;

            svc.On = obj.burn;
            svc.SetRGB(obj.red, obj.green, obj.blue).Wait();
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
