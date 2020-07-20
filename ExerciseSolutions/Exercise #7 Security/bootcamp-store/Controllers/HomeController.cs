using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using bootcamp_store.Models;
using Steeltoe.Common.Discovery;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
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

        [Authorize(Policy = "admin-policy")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "admin-policy")]
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

        [Authorize(Policy = "admin-policy")]
        public async Task<ActionResult> Delete(int id)
        {
            var client =  new HttpClient(_handler, true);
            var jsonString = await client.GetStringAsync($"{ApiBaseUrl}/{id}");
            var product = JsonConvert.DeserializeObject<Product>(jsonString);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "admin-policy")]
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

        [HttpPost]
        public async Task<IActionResult> LogOff()
        {
           await HttpContext.SignOutAsync();
           return RedirectToAction(nameof(Index), "Home");
        }

        [HttpGet]
        [Authorize]
        public IActionResult Login()
        {
           return RedirectToAction(nameof(Index), "Home");
        }

        public IActionResult AccessDenied()
        {
           ViewData["Message"] = "Insufficient permissions.";
           return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}