using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace RazorAndFunction.Web.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public IFormFile Upload { get; set; }

        private IHostingEnvironment _environment;
        private IConfiguration _configuration;

        public IndexModel(IHostingEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
        }

        public void OnGet()
        {

        }

        public async Task OnPostAsync()
        {
            using (var memoryStream = new MemoryStream())
            {
                // todo: read from configuration
                string storageConnectionString = _configuration.GetConnectionString("StorageAccountConnectionString");

                CloudStorageAccount account = CloudStorageAccount.Parse(storageConnectionString);
                CloudBlobClient serviceClient = account.CreateCloudBlobClient();

                // Create container. Name must be lower case.
                Console.WriteLine("Creating container...");
                var container = serviceClient.GetContainerReference("uploads");
                container.CreateIfNotExistsAsync().Wait();

                // write a blob to the container
                CloudBlockBlob blob = container.GetBlockBlobReference(Upload.FileName);
                Upload.CopyTo(memoryStream);
                memoryStream.Position = 0;
                await blob.UploadFromStreamAsync(memoryStream);
            }
        }
    }
}
