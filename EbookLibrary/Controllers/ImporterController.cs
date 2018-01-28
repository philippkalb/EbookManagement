using System;
using System.Linq;
using System.Threading.Tasks;
using Elasticsearch.Net;
using ImageMagick;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Nest;
using VersOne.Epub;

namespace EbookLibrary.Controllers
{
    public class ImporterController : Controller
    {
        private IHubContext<ImporterHub> _chatHubContext;
        private IConfiguration _iconfiguration;
        private readonly ILogger _logger;

        public ImporterController(IHubContext<ImporterHub> chatHubContext, IConfiguration iconfiguration, ILoggerFactory logger)
        {
            _chatHubContext = chatHubContext;
            _iconfiguration = iconfiguration;
            _logger = logger.CreateLogger("EbookLibrary");
        }



        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public void ReImport()
        {
            var libPath = _iconfiguration["Basefolder"];
            var provider = new PhysicalFileProvider(libPath);
            var esConnection = _iconfiguration["ElasticSearchConnection"];
            var settings = new ConnectionSettings(new Uri(esConnection));
            var client = new ElasticClient(settings);
            var content = provider.GetDirectoryContents("");
            var max = content.Count();

            var counter = 0;
            Parallel.ForEach(content, (item, loopstate) =>
            {
                if (counter > 100)
                {
                    loopstate.Stop();
                }
                try
                {
                    counter++;
                    var epubBook = EpubReader.ReadBook(item.PhysicalPath);
                    var thumbstring = "";

                    if (epubBook.CoverImage.Length > 0)
                    {
                        using (var image = new MagickImage(epubBook.CoverImage))
                        {
                            image.Resize(60, 0);
                            image.Strip();

                            string fileType;
                            if (image.Compression.ToString().Contains("PNG)"))
                            {
                                fileType = "data:image/png;base64, ";
                            }
                            else
                            {
                                fileType = "data:image/jpeg;base64, ";
                            }
                            thumbstring = fileType + image.ToBase64();
                        }
                    }

                    if (string.IsNullOrEmpty(thumbstring))
                    {
                        using (var image = new MagickImage("images/NoBook.jpg"))
                        {
                            image.Resize(60, 0);
                            image.Strip();
                            thumbstring = image.ToBase64();
                        }
                    }

                    var book = new 
                    {
                        id = Guid.NewGuid(),
                        title = epubBook.Title,
                        author = epubBook.Author,
                        tags = epubBook.AuthorList,
                        filename = epubBook.FilePath,
                        image = thumbstring
                    };

                    var response = client.Index(book, i => i.Index("booksearch").Type("book").Id(book.id));
                    _chatHubContext.Clients.All.InvokeAsync("Send", $"{counter} von {max} importiert");
                }
                catch (ElasticsearchClientException e)
                {
                    throw e;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Error message when importing library books {item.Name}");
                }
            });
        }


    }
}