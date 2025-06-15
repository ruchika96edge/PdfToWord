using Aspose.Pdf;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace PdfToWord;

public class Function1
{
    private readonly ILogger<Function1> _logger;
    public IConfiguration _configuration { get; }


    public Function1(ILogger<Function1> logger, IConfiguration configuration)
    {
        this._logger = logger;
        this._configuration = configuration;

    }

    [Function(nameof(Function1))]
    public async Task Run([BlobTrigger("pdfnew/{name}", Connection = "AzureWebJobsStorage")] Stream inputBlob, string name)
    {
        _logger.LogError($"Processing file azure function app: {name}");
        string storageConnectionString = _configuration["AzureWebJobsStorage"];
        // string lisence = _configuration["AsposePdfLisence"];
        //Aspose.Pdf.License license = new Aspose.Pdf.License();
        //var bytes = Convert.FromBase64String(lisence);
        //license.SetLicense(new MemoryStream(bytes));

        Document pdfFile = new Document(inputBlob);
        DocSaveOptions saveOpts = new DocSaveOptions();

        saveOpts.Mode = DocSaveOptions.RecognitionMode.Textbox;
        saveOpts.Format = DocSaveOptions.DocFormat.DocX;


        using (var outputStream = new MemoryStream())
        {
            pdfFile.Save(outputStream, saveOpts);
            outputStream.Position = 0;

            //// Upload the MemoryStream to Azure Blob Storage

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("converted-docs");
            await container.CreateIfNotExistsAsync();

            CloudBlockBlob blockBlob = container.GetBlockBlobReference($"{Path.GetFileNameWithoutExtension(name)}.docx");
            await blockBlob.UploadFromStreamAsync(outputStream, null, null, null);

            // delete the original pdf file
            CloudBlobContainer pdfContainer = blobClient.GetContainerReference("pdfnew");
            CloudBlockBlob pdfBlob = pdfContainer.GetBlockBlobReference(name);
            await pdfBlob.DeleteIfExistsAsync();
            Console.WriteLine("PDF converted to Word successfully!");
        }
    }

}