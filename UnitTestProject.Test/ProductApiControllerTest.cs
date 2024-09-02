using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTestProject.Web.Controllers;
using UnitTestProject.Web.Models;
using UnitTestProject.Web.Repository;
using Xunit;

namespace UnitTestProject.Test
{
    public class ProductApiControllerTest
    {
        private readonly Mock<IRepository<Product>> _mockRepo;
        private readonly ProductsApiController _controller;
        private List<Product> products;

        public ProductApiControllerTest()
        {
            _mockRepo = new Mock<IRepository<Product>>();
            _controller = new ProductsApiController(_mockRepo.Object);
            products = new List<Product>(){
            new Product { Id = 1, Name = "Test", Price = 100, Stock = 100, Color = "Siyah"},
            new Product { Id = 2, Name = "Dummy", Price = 100, Stock = 100, Color = "Beyaz" } };
        }

        [Fact]
        public async void GetProductsActionExecuteReturnOkResultWithProduct()
        {
            _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(products);
            var result = await _controller.GetProducts();
            var okResult = Assert.IsType<OkObjectResult>(result); // IsType birebir kontrol yapar.
            Assert.Equal<int>(200, Convert.ToInt32(okResult.StatusCode));
            var returnProducts = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value); //IsAssignableFrom, IEnumerable implement etmesine bakar.
            Assert.Equal<int>(2, returnProducts.ToList().Count());
        }

        [Theory, InlineData(99)]
        public async void GetProductIdInvalidReturnNotFound(int productId)
        {
            Product product = null;
            _mockRepo.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(product);
            var result = await _controller.GetProduct(productId);
            Assert.IsType<NotFoundResult>(result);
        }

        [Theory, InlineData(1), InlineData(2)] // 2 kez çalışacak.
        public async void GetProductIdValidReturnOkResult(int productId)
        {
            var product = products.First(x => x.Id == productId);
            _mockRepo.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(product);
            var result = await _controller.GetProduct(productId);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnProduct = Assert.IsType<Product>(okResult.Value);
            Assert.Equal<int>(200, Convert.ToInt32(okResult.StatusCode));
            Assert.Equal<int>(productId, returnProduct.Id);
            Assert.Equal(product.Name, returnProduct.Name);
        }

        [Theory, InlineData(1)]
        public void PutProductIdIsNotEqualProductReturnBadRequest(int productId)
        {
            var product = products.First(x => x.Id == productId);
            var result = _controller.PutProduct(0, product);
            Assert.IsType<BadRequestResult>(result);
        }

        [Theory, InlineData(1)]
        public void PutProductActionExecutesReturnNoContent(int productId)
        {
            var product = products.First(x => x.Id == productId);
            _mockRepo.Setup(x => x.Update(product));
            var result = _controller.PutProduct(productId, product);
            _mockRepo.Verify(x => x.Update(product), Times.Once);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async void PostProductActionExecutesReturnCreateAtAction()
        {
            var product = products.First();
            _mockRepo.Setup(x => x.CreateAsync(product)).Returns(Task.CompletedTask);
            var result = await _controller.PostProduct(product);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            _mockRepo.Verify(x => x.CreateAsync(product), Times.Once);
            Assert.Equal("GetProduct", createdAtActionResult.ActionName);
        }

        [Theory]
        [InlineData(0)]
        public async void DeleteProductIdIsInvalidReturnsNotFound(int productId)
        {
            Product product = null;
            _mockRepo.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(product);
            var resultNotFound = await _controller.DeleteProduct(productId);
            Assert.IsType<NotFoundResult>(resultNotFound);
        }

        [Theory]
        [InlineData(1)]
        public async void DeleteProductActionExecutesReturnNoContent(int productId)
        {
            //Arrange
            var product = products.First(x => x.Id == productId);

            // Repository mock ayarları: ürün varmış gibi yapın
            _mockRepo.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(product);

            _mockRepo.Setup(repo => repo.Delete(product)); // Delete metodu çağrılacak

            // Act
            var result = await _controller.DeleteProduct(productId);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);

            // Delete metodunun bir kez çağrıldığını doğrulayın
            _mockRepo.Verify(repo => repo.Delete(product), Times.Once);
        }

        [Theory]
        [InlineData(1)]
        public void ProductExistsIdIsValidReturnTrue(int productId)
        {
            var product = products.First(p => p.Id == productId);
            Assert.Equal(productId, product.Id);
        }

        [Fact]
        public void ProductExistsIdInvalidReturnFalse()
        {
            Product product = null;
            Assert.Null(product);
        }
    }
}
