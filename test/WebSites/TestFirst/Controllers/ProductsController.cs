using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TestFirst.Controllers
{
    [Route("api/[controller]")]
    public class ProductsController : Controller
    {
        [HttpPost]
        public IActionResult CreateProduct([FromBody]Product product)
        {
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);

            return Created("api/products/1", null);
        }

        [HttpGet]
        public IActionResult GetProducts([FromQuery]PagingParameters pagingParameters)
        {
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);

            return new ObjectResult(new[] { new Product { Id = 1, Name = "Test product" } });
        }

        [HttpGet("{id}")]
        public IActionResult GetProduct(int id)
        {
            if (id == 0) return NotFound();

            return new ObjectResult(new Product { Id = 1, Name = "Test product" });
        }

        [HttpPut("{id}")]
        public IActionResult GetProduct(int id, [FromBody]Product product)
        {
            if (id == 0) return NotFound();

            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            if (id == 0) return NotFound();

            return NoContent();
        }
    }

    public class Product
    {
        public int Id { get; internal set;}

        [Required]
        public string Name { get; set; }
    }

    public class PagingParameters
    {
        [BindRequired]
        public int PageNo { get; set; }

        public int PageSize { get; set; } = 20;
    }
}
