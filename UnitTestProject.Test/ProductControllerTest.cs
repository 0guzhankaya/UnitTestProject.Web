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
    public class ProductControllerTest
    {
        private readonly Mock<IRepository<Product>> _mockRepo;
        private readonly ProductsController _controller;
        private List<Product> products; // Dummy Data List

        public ProductControllerTest()
        {
            _mockRepo = new Mock<IRepository<Product>>();
            _controller = new ProductsController(_mockRepo.Object);
            products = new List<Product>()
            {
                new Product { Id = 1, Name = "Klavye", Price = 100, Stock = 50, Color = "Beyaz" },
                new Product { Id = 2, Name = "Monitör", Price = 1000, Stock = 50, Color = "Siyah" }
            };
        }

        [Fact]
        public async void IndexActionExecutesReturnView()
        {
            // Index Action'un ViewResult olma durumu test ediliyor.
            var result = await _controller.Index();
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async void IndexActionExecutesReturnProductList()
        {
            _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(products); // mocklandı ve dummy data'ları dönecek.
            var result = await _controller.Index();
            var viewResult = Assert.IsType<ViewResult>(result);
            var productList = Assert.IsAssignableFrom<IEnumerable<Product>>(viewResult.Model);
            Assert.Equal<int>(2, productList.Count());
        }

        [Fact]
        public async void DetailsIdIsNullReturnRedirectToIndexAction()
        {
            var result = await _controller.Details(null); // null test edildiği için null verildi.
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async void DetailsIdInvalidReturnNotFound()
        {
            Product product = null;
            _mockRepo.Setup(x => x.GetByIdAsync(0)).ReturnsAsync(product);
            var result = await _controller.Details(0);
            var redirect = Assert.IsType<NotFoundResult>(result);
            Assert.Equal<int>(404, redirect.StatusCode);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void DetailsValidIdReturnProduct(int productId)
        {
            Product product = products.First(x => x.Id == productId);
            _mockRepo.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(product);
            var result = await _controller.Details(productId);
            var viewResult = Assert.IsType<ViewResult>(result);
            var resultProduct = Assert.IsAssignableFrom<Product>(viewResult.Model);

            Assert.Equal(product.Id, resultProduct.Id);
            Assert.Equal(product.Name, resultProduct.Name);
        }

        [Fact]
        public void CreateActionExecutesReturnView()
        {
            var result = _controller.Create();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async void CreatePostInvalidModelStateReturnViewProduct()
        {
            _controller.ModelState.AddModelError("Name", "Name alanı gereklidir.");
            var result = await _controller.Create(products.First());
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<Product>(viewResult.Model);
        }

        [Fact]
        public async void CreatePostValidModelStateReturnRedirectToIndexAction()
        {
            var result = await _controller.Create(products.First());
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async void CreatePostValidModelStateCreateMethodExecute()
        {
            Product newProduct = null;
            _mockRepo.Setup(repo => repo.CreateAsync(It.IsAny<Product>())).Callback<Product>(x => newProduct = x);
            var result = await _controller.Create(products.First());
            _mockRepo.Verify(repo => repo.CreateAsync(It.IsAny<Product>()), Times.Once);

            Assert.Equal(products.First().Id, newProduct.Id);
        }

        [Fact]
        public async void CreatePostInvalidModalStateNeverCreateExecute()
        {
            _controller.ModelState.AddModelError("Name", "");
            var result = await _controller.Create(products.First());

            _mockRepo.Verify(repo => repo.CreateAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async void EditIdIsNullReturnRedirectToIndexAction()
        {
            var result = await _controller.Edit(null);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Theory]
        [InlineData(99)]
        public async void EditIdInvalidReturnNotFound(int productId)
        {
            Product product = null;
            _mockRepo.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(product);
            var result = await _controller.Edit(productId);
            var redirect = Assert.IsType<NotFoundResult>(result);
            Assert.Equal<int>(404, redirect.StatusCode);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void EditActionExecutesReturnProduct(int productId)
        {
            var product = products.First(p => p.Id == productId);
            _mockRepo.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(product);
            var result = await _controller.Edit(productId);
            var viewResult = Assert.IsType<ViewResult>(result);
            var resultProduct = Assert.IsAssignableFrom<Product>(viewResult.Model);

            Assert.Equal(product.Id, resultProduct.Id);
            Assert.Equal(product.Name, resultProduct.Name);
        }

        [Fact]
        public async void EditPostIdIsNotEqualProductReturnNotFound()
        {
            Product product = new Product();
            product.Id = 99;
            // Edit metodunu çağırın
            var result = await _controller.Edit(product.Id);

            // NotFound döndüğünü doğrulayın
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Theory, InlineData(1)]
        public void EditPostInvalidModelStateReturnView(int productId)
        {
            _controller.ModelState.AddModelError("Name", "");
            var result = _controller.Edit(productId, products.First(x => x.Id == productId));
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<Product>(viewResult.Model);
        }

        [Theory, InlineData(1)]
        public void EditPostValidModelStateUpdateMethodExecute(int productId)
        {
            var product = products.First(x => x.Id == productId);
            _mockRepo.Setup(repo => repo.Update(product));
            _controller.Edit(productId, product);
            _mockRepo.Verify(repo => repo.Update(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async void DeleteIdIsNullReturnNotFound()
        {
            var result = await _controller.Delete(null);
            var resultStatus = Assert.IsType<NotFoundResult>(result);
            Assert.Equal<int>(404, resultStatus.StatusCode);
        }

        [Theory]
        [InlineData(99)]
        public async void DeleteIdIsNotEqualProductReturnNotFound(int productId)
        {
            // Olmayan Id gönderilip, product içerisindeki ile eşleşmediği için NotFound dönecek.
            Product product = null;
            _mockRepo.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(product);
            var result = await _controller.Delete(productId);
            var redirect = Assert.IsType<NotFoundResult>(result);
            Assert.Equal<int>(404, redirect.StatusCode);
        }

        [Theory]
        [InlineData(1)]
        public async void DeleteActionExecutesReturnProduct(int productId)
        {
            var product = products.First(p => p.Id == productId);
            _mockRepo.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(product);
            var result = await _controller.Delete(product.Id);
            var viewResult = Assert.IsType<ViewResult>(result);
            var resultProduct = Assert.IsAssignableFrom<Product>(viewResult.Model);
            Assert.NotNull(result);
            Assert.Equal(product.Id, productId);
            Assert.Equal(product.Name, resultProduct.Name);
        }

        [Theory, InlineData(99)]
        public async void DeleteConfirmedActionExecuteReturnNotFound(int productId)
        {
            var result = await _controller.DeleteConfirmed(productId);
            Assert.IsType<NotFoundResult>(result);
        }

        [Theory, InlineData(1)]
        public async Task DeleteConfirmedActionExecuteDeleteMethodExecute(int productId)
        {
            var product = products.First(p => p.Id == productId);
            _mockRepo.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(product);
            _mockRepo.Setup(repo => repo.Delete(product));
            var result = await _controller.DeleteConfirmed(productId);
            _mockRepo.Verify(repo => repo.Delete(product), Times.Once);
            var response = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", response.ActionName);
        }

        [Theory]
        [InlineData(1)]
        public void ProductExistsIfTrueReturnProduct(int productId)
        {
            var product = products.Exists(p => p.Id.Equals(productId));
        }

        [Fact]
        public void ProductExistsIfNullReturnFalse()
        {
            Product product = null;
            Assert.Null(product);
        }
    }
}
