using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EbookLibrary.Models;
using Nest;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EbookLibrary.Controllers
{
    public class HomeController : Controller
    {

        private IConfiguration _iconfiguration;
        private readonly ILogger _logger;

        public HomeController(IConfiguration iconfiguration, ILoggerFactory logger)
        {
            _iconfiguration = iconfiguration;
            _logger = logger.CreateLogger("EbookLibrary");
        }

        public IActionResult Index()
        {
            var viewModel = new BookListViewModel();
            return View("Index", viewModel);
        }


        [HttpPost]
        public JsonResult Search(string searchValue)
        {
            var viewModel = new List<string>();
            var esConnection = _iconfiguration["ElasticSearchConnection"];
            var settings = new ConnectionSettings(new Uri(esConnection));
            var client = new ElasticClient(settings);
            var response = client.Search<Book>(s => s.Index("booksearch").Size(30).Query(q => q.MultiMatch(m => m.Query(searchValue).Fields(f => f.Field(p => p.Author).Field(p => p.Title).Field(p => p.Tags)))));
            
            var result = new BookResult
            {
                Displayed = response.Hits.Count,
                MaxCount = response.HitsMetaData!= null? response.HitsMetaData.Total:0,
                Books = response.Documents.Select(s => new Book
                {
                    Id = s.Id,
                    Author = s.Author,
                    Filename = s.Filename,
                    Image = s.Image,
                    Tags = s.Tags,
                    Title = s.Title,
                }).ToList<Book>()
            };
            
            return Json(result);
        }
              

        public FileResult Download(string id)
        {
            var esConnection = _iconfiguration["ElasticSearchConnection"];
            var settings = new ConnectionSettings(new Uri(esConnection));
            var client = new ElasticClient(settings);
            var request = new GetRequest("booksearch", "book", id);
            var response = client.Get<Book>(request);
         
            if (!response.Found || response.Source.Filename == null) return null; 
            byte[] fileBytes = System.IO.File.ReadAllBytes(response.Source.Filename);
            return File(fileBytes, "application/x-msdownload", "ebook.epub");
        }


        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
