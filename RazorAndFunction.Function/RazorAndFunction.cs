using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
    

namespace RazorAndFunction.Function
{
    public static class RazorAndFunction
    {
        [FunctionName("RazorAndFunction")]
        public static async void Run([BlobTrigger("uploads/{name}")]Stream myBlob, 
                               string name, TraceWriter log, ExecutionContext context)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            ComputerVisionClient client = new ComputerVisionClient(
                new ApiKeyServiceClientCredentials(config["computerVisionKey"]),
                new System.Net.Http.DelegatingHandler[] { });


            client.Endpoint = config["computerVisionEndpoint"];

            try
            {
                var result = await client.AnalyzeImageInStreamAsync(myBlob, features);
                DisplayResults(result, log, name);

                Debug.WriteLine(result);
            }
            catch(Exception x)
            {
                Debug.WriteLine(x);
            }
        }

        private static void DisplayResults(ImageAnalysis result, TraceWriter log, string filename)
        {
            if (!(result.Description != null
                && result.Description.Captions != null
                && result.Description.Captions.Count > 0 
                && result.Tags != null
                  && result.Tags.Count > 0))
            {
                return;
            }

            log.Info("---------------------------------------------------------");
            log.Info("Image Uploaded - Filename is " + filename);
            log.Info("---------------------------------------------------------");


            if (result.Description != null 
               && result.Description.Captions != null 
               && result.Description.Captions.Count > 0)
            {
                log.Info("Image Captions:");

                foreach (var caption in result.Description.Captions)
                {
                    log.Info(caption.Text + " (" + caption.Confidence + ")");
                }
            }

            foreach (var tag in result.Tags)
            {
                log.Info(tag.Name + " (" + tag.Confidence + ")");
            }
        }

        private static readonly List<VisualFeatureTypes> features =
            new List<VisualFeatureTypes>()
        {
            VisualFeatureTypes.Description,
            VisualFeatureTypes.Tags
        };
    }
}
