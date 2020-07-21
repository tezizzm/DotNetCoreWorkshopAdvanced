using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using bootcamp_store.Models;
using Steeltoe.Common.Discovery;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Steeltoe.Common.Http;

namespace bootcamp_store.Controllers
{
    public class HomeController : Controller
    {
        private const string Initials = "mk";
        private static readonly string ApiBaseUrl = $"https://bootcamp-api-{Initials}/api/products";
        private readonly DiscoveryHttpClientHandler _handler;

        public HomeController(IDiscoveryClient client)
        {
            _handler = new DiscoveryHttpClientHandler(client);
        }

        public async Task<IActionResult> Index()
        {
            var client = new HttpClient(_handler, true);
            var jsonString = await client.GetStringAsync(ApiBaseUrl);
            var products = JsonConvert.DeserializeObject<IList<Product>>(jsonString);
            foreach (var product in products)
            {
                Console.WriteLine(product);
            }
            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            try
            {
                var client = new HttpClient(_handler, true);
                await client.PostAsJsonAsync(ApiBaseUrl, product);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public async Task<ActionResult> Delete(int id)
        {
            var client =  new HttpClient(_handler, true);
            var jsonString = await client.GetStringAsync($"{ApiBaseUrl}/{id}");
            var product = JsonConvert.DeserializeObject<Product>(jsonString);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, IFormCollection collection)
        {
            try
            {
                var client = new HttpClient(_handler, true);
                await client.DeleteAsync($"{ApiBaseUrl}/{id}");
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}