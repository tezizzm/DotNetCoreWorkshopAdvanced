using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Steeltoe.Management.Endpoint.Info;

namespace bootcamp_webapi
{

    public class ProductInStockInfoContributor : IInfoContributor
    {
        private readonly ProductContext _context;

        public ProductInStockInfoContributor([FromServices] ProductContext context)
        {
            _context = context;
        }

        public void Contribute(IInfoBuilder builder)
        {
            var count = _context.Products.Count();

            // pass in the info
            builder.WithInfo("productInStock", new { numberOfProductsInStock = count });
        }
    }
}