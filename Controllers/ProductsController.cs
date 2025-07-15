using Microsoft.AspNetCore.Mvc;
using MyStoreAPI.Models;
using MyStoreAPI.Services.Interface;

namespace MyStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductsService _productService;
        private readonly IConfiguration _config;
        public ProductsController(IProductsService productService, IConfiguration config)
        {
            _productService = productService;
            _config = config;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var products = await _productService.GetProducts();
                var responseModel = new ResponseModel<Product>
                {
                    StatusCode = 200,
                    Status = "Success",
                    Message = "Products retrieved successfully",
                    Items = (List<Product>)products,
                };
                return Ok(responseModel);
            }
            catch(Exception ex)
            {
                var response = new ResponseModel<string>
                {
                    StatusCode = 500,
                    Status = "Error",
                    Message = ex.Message,
                };

                return StatusCode(500, new { status = response.Status, message = response.Message });
            }
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(Guid id)
        {
            try
            {
                var product = await _productService.GetProductById(id);
                var responseModel = new ResponseModel<Product>
                {
                    StatusCode = 200,
                    Status = "Success",
                    Message = "Product Detail Fetched successfully",
                    Data = product
                };

                return Ok(responseModel);
            }
            catch(Exception ex)
            {
                var response = new ResponseModel<string>
                {
                    StatusCode = 500,
                    Status = "Error",
                    Message = ex.Message,
                };

                return StatusCode(500, new { status = response.Status, message = response.Message });
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(Product product)
        {
            try
            {
                var result = await _productService.UpdateProduct(product.ProductId, product);
              
                return Ok(result);
            }
            catch (Exception ex)
            {
                var response = new ResponseModel<string>
                {
                    StatusCode = 500,
                    Status = "Error",
                    Message = ex.Message,
                };

                return StatusCode(500, new { status = response.Status, message = response.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ProductViewResponseModel>> PostProduct(Product product)
        {
            var productData = await _productService.CreateProduct(product);
            return productData;
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var result = _productService.DeleteProduct(id);
            return Ok(result);
        }
    }
}
