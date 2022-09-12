using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Net.Codecrete.QrCodeGenerator;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;

// ReSharper disable once CheckNamespace
namespace Company.Function
{
    public static class QRCodeGen
    {
        [FunctionName("Form")]
        public static HttpResponseMessage Form([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log, ExecutionContext context)
        {
            var indexPage = File.ReadAllText(context.FunctionAppDirectory + "/www/index.html");

            var result = new HttpResponseMessage(HttpStatusCode.OK);

            result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");

            result.Content = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(indexPage));

            return result;

        }

        [FunctionName("QRCodeGen")]
        public static async Task<IActionResult> Generate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request");

            string url = req.Query["url"];

            log.LogInformation("Generating QR Code for {url}", url);
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            url = url ?? data?.url;

            if (string.IsNullOrEmpty(url))
            {
                return new BadRequestResult();
            }

            var qr = QrCode.EncodeText(url, QrCode.Ecc.Medium);
            var qr2 = QrCode.EncodeText(url, QrCode.Ecc.Medium);
            // convert it into a byte array for PNG output
            var pngOut = qr2.ToPng(10, 1, SkiaSharp.SKColors.Black, SkiaSharp.SKColors.White);
            var svgOut = qr.ToSvgString(1);

            // log.LogInformation(svgout);
            // create a new return object
            var ourResult = new ReturnObject { };

            // store our byte array as a string 
            ourResult.Image = svgOut;
            ourResult.Imagepng = Convert.ToBase64String(pngOut);

            // send it as JSON
            return new JsonResult(ourResult);
        }
    }
    public class ReturnObject
    {
        // the only property here is a string for the PNG
        public string Image { get; set; }
        public string Imagepng { get; set; }
    }
}
