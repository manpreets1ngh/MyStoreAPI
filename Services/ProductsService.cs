using ApplicationToSellThings.APIs.Data;
using ApplicationToSellThings.APIs.Models;
using ApplicationToSellThings.APIs.Services.Interface;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace ApplicationToSellThings.APIs.Services
{
    public class ProductsService : IProductsService
    {
        private readonly ApplicationToSellThingsAPIsContext _dbContext;
        private readonly IImageService _imageService;

        public ProductsService(ApplicationToSellThingsAPIsContext dbContext,  IImageService imageService)
        {
            _dbContext = dbContext;
            _imageService = imageService;
        }

        public async Task<IEnumerable<Product>> GetProducts()
        {
            var products = _dbContext.Products.ToList();

            foreach (var product in products)
            {
                if (!string.IsNullOrEmpty(product.ProductImage))
                {
                    // Ensure we pass only the filename, not a full/partial path
                    string fileName = product.ProductImage.Contains("/images/") 
                        ? product.ProductImage.Replace("/images/", "") 
                        : product.ProductImage;
                    
                    product.ProductImage = _imageService.GetImageUrl(fileName);
                }
            }

            return products;
        }


        public async Task<ProductViewResponseModel> CreateProduct(Product product)
        {
            if (product != null)
            {
                string savedImageFileName = await _imageService.SaveImageAsync(product.ProductImage);
                string imageUrl = $"/images/{savedImageFileName}";
                var productRequest = new Product()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = product.ProductName,
                    BrandName = product.BrandName,
                    Price = product.Price,
                    Discount = product.Discount,
                    Description = product.Description,
                    Category = product.Category,
                    QuantityInStock = product.QuantityInStock,
                    CreatedAt = DateTime.Now,
                    ProductImage = imageUrl,
                };
                
                _dbContext.Products.Add(productRequest);
                await _dbContext.SaveChangesAsync();

                var productResponse = new ProductViewResponseModel()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = product.ProductName,
                    BrandName = product.BrandName,
                    Price = product.Price,
                    Discount = product.Discount,
                    Description = product.Description,
                    Category = product.Category,
                    QuantityInStock = product.QuantityInStock,
                    CreatedAt = DateTime.Now,
                    ProductImage = imageUrl,
                };

                return productResponse;
            }

            return null;
        }

        public async Task<Product> GetProductById(Guid id)
        {
            var product = await _dbContext.Products.FindAsync(id);
             if (product == null) return null;

            // Convert relative path to full URL
            string baseUrl = "http://192.168.1.106:5000"; // Replace with your API base URL
            string fullImageUrl = product.ProductImage != null
                ? $"{baseUrl}{product.ProductImage}" // Ensure full URL
                : null;

                product.ProductImage = fullImageUrl;

            return product;
        }

        public async Task<ResponseModel<Product>> UpdateProduct(Guid productId, Product productModel)
        {
            try
            {
                var product = await _dbContext.Products
                .FirstOrDefaultAsync(p => p.ProductId == productId);

                if (product == null)
                {
                    return new ResponseModel<Product>
                    {
                        StatusCode = 404,
                        Status = "Not Found",
                        Message = "Product not found"
                    };
                }

                product.ProductName = productModel.ProductName;
                product.BrandName = productModel.BrandName;
                product.Price = productModel.Price;
                product.Description = productModel.Description;
                product.Category = productModel.Category;
                product.QuantityInStock = productModel.QuantityInStock;
                product.Discount = productModel.Discount;
                product.CreatedAt = productModel.CreatedAt;
                product.ProductImage = productModel.ProductImage;

                _dbContext.Products.Update(product);
                await _dbContext.SaveChangesAsync();

                return new ResponseModel<Product>
                {
                    StatusCode = 200,
                    Status = "Success",
                    Data = product
                };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return new ResponseModel<Product>
                {
                    StatusCode = 409,
                    Status = "Concurrency Error",
                    Message = "The record you attempted to edit was modified by another user after you got the original value. The edit operation was canceled."
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<Product>
                {
                    StatusCode = 500,
                    Status = "Error",
                    Message = ex.Message
                };
            }
        }
        
        public async Task DeleteProduct(Guid id)
        {
            var product = await _dbContext.Products.FindAsync(id);
            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();
        }
    }
}
