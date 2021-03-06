﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using EcommerceApi.ViewModel;
using EcommerceApi.Repositories;
using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;

namespace EcommerceApi.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Products")]
    public class ProductsController : Controller
    {
        private readonly EcommerceContext _context;
        private readonly IProductRepository _productRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private const string ProductsContainerName = "products";
        private const string ProductManualsContainerName = "productmanuals";

        public ProductsController(
            EcommerceContext context, 
            IProductRepository productRepository,
            UserManager<ApplicationUser> userManager,
            IConfiguration config)
        {
            _context = context;
            _productRepository = productRepository;
            _userManager = userManager;
            _config = config;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<IEnumerable<ProductViewModel>> GetProduct()
        {
            return await _productRepository.GetProducts();
        }

        // GET: api/Products/WithInventory
        [HttpGet("WithInventory")]
        public async Task<IEnumerable<ProductWithInventoryViewModel>> GetProductsWithInventory() 
            => await _productRepository.GetProductsWithInventory();

        // GET: api/Products/Available
        [HttpGet("Available")]
        public async Task<IEnumerable<ProductViewModel>> GetAvailableProduct()
        {
            return await _productRepository.GetAvailableProducts();
        }

        // GET: api/Products/Location/{locationId}/Available
        [HttpGet("Locations/{locationId}/Available")]
        public async Task<IEnumerable<ProductViewModelV2>> GetAvailableProductV2([FromRoute] int locationId)
            => await _productRepository.GetAvailableProductsV2(locationId);

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ProductViewModel> GetProduct([FromRoute] int id)
        {
            return await _productRepository.GetProduct(id);
        }

        // GET: api/Products/5/Transactions
        [HttpGet("{id}/Transactions")]
        public async Task<IEnumerable<ProductTransactionViewModel>> GetProductTransactions([FromRoute] int id, int locationId, DateTime fromDate, DateTime toDate)
        {
            if (fromDate == DateTime.MinValue)
                fromDate = DateTime.Now;
            if (toDate == DateTime.MinValue)
                toDate = DateTime.Now;
            else
                toDate = toDate.AddDays(1).AddTicks(-1);


            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user = await _userManager.FindByEmailAsync(currentUser.Identity.Name);

            return await _productRepository.GetProductTransactions(id, fromDate, toDate, user.Id, locationId);
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct([FromRoute] int id, [FromBody] ProductViewModel product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != product.ProductId)
            {
                return BadRequest();
            }

            var exisintgProduct = await _context.Product.FirstOrDefaultAsync(m => m.ProductId == id);
            if(exisintgProduct == null)
            {
                return BadRequest($"ProductId {id} not found.");
            }
            exisintgProduct.PurchasePrice = product.PurchasePrice;
            exisintgProduct.SalesPrice = product.SalesPrice;
            exisintgProduct.ProductTypeId = product.ProductTypeId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Products
        [HttpPost]
        public async Task<IActionResult> PostProduct([FromBody] Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Product.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = product.ProductId }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = await _context.Product.SingleOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }
            product.Disabled = !product.Disabled;
            await _context.SaveChangesAsync();

            return Ok(product);
        }

        [HttpGet("{id}/productpackage")]
        public async Task<IEnumerable<ProductPackage>> GetProductPackage([FromRoute] int id)
        {
            return await _context.ProductPackage.Where(p => p.ProductId == id).AsNoTracking().ToListAsync();
        }

        [HttpPost("{id}/productpackage")]
        public async Task<IActionResult> CreateProductPackage([FromRoute] int id, [FromBody] ProductPackage productPackage)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            productPackage.ProductId = id;
            productPackage.ModifiedDate = DateTime.Now;
            _context.ProductPackage.Add(productPackage);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id}/productpackage")]
        public async Task<IActionResult> UpdateProductPackage([FromRoute] int id, [FromBody] ProductPackage productPackage)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var foundPackage = _context.ProductPackage.FirstOrDefault(p => p.ProductPackageId == productPackage.ProductPackageId);
            if (foundPackage != null)
            {
                foundPackage.Package = productPackage.Package;
                foundPackage.PackagePrice = productPackage.PackagePrice;
                foundPackage.AmountInMainPackage = productPackage.AmountInMainPackage;
                foundPackage.ModifiedDate  = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        [HttpDelete("{id}/productpackage/{productPackageId}/delete")]
        public async Task<IActionResult> DeleteProductPackage([FromRoute] int id, [FromRoute] int productPackageId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var foundPackage = _context.ProductPackage.FirstOrDefault(p => p.ProductPackageId == productPackageId && p.ProductId == id);
            if (foundPackage != null)
            {
                _context.ProductPackage.Remove(foundPackage);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        [HttpPost]
        [Route("{id}/Upload")]
        [AllowAnonymous]
        public async Task<IActionResult> UploadAsync([FromRoute] int id, ICollection<IFormFile> file)
        {
            var exisintgProduct = await _context.Product.FirstOrDefaultAsync(m => m.ProductId == id);
            if (exisintgProduct == null)
            {
                return BadRequest($"ProductId {id} not found.");
            }

            var storageConnectionString = _config.GetConnectionString("AzureStorageConnectionString");

            if (!CloudStorageAccount.TryParse(storageConnectionString, out CloudStorageAccount storageAccount))
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            var blobClient = storageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference(ProductsContainerName);

            await container.CreateIfNotExistsAsync();

            // Delete existing files
            var exisintgProductImages = _context.ProductWebsiteImage.Where(m => m.ProductId == id);
            foreach (var image in exisintgProductImages) {
                Uri uri = new Uri(image.ImagePath);
                if (uri.IsFile)
                {
                    string fileName = System.IO.Path.GetFileName(uri.LocalPath);
                    var picBlob = container.GetBlockBlobReference(fileName);
                    await picBlob.DeleteIfExistsAsync();
                }
                _context.ProductWebsiteImage.Remove(image);
            }

            await _context.SaveChangesAsync();

            if (file == null) {
                return Ok();
            }

            // Add the new files
            foreach (var f in file)
            {
                //MS: Don't rely on or trust the FileName property without validation. The FileName property should only be used for display purposes.
                var picBlob = container.GetBlockBlobReference(Guid.NewGuid().ToString() + "-" + f.FileName);

                await picBlob.UploadFromStreamAsync(f.OpenReadStream());
                _context.ProductWebsiteImage.Add(
                    new ProductWebsiteImage
                    {
                        ProductId = id,
                        ImagePath = picBlob.Uri.AbsoluteUri,
                        ImageSize = f.Length.ToString()
                    });
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        [Route("{id}/UploadHeaderImage")]
        [AllowAnonymous]
        public async Task<IActionResult> UploadHeaderImageAsync([FromRoute] int id, IFormFile file)
        {
            var exisintgProduct = await _context.Product.FirstOrDefaultAsync(m => m.ProductId == id);
            if (exisintgProduct == null)
            {
                return BadRequest($"ProductId {id} not found.");
            }

            var storageConnectionString = _config.GetConnectionString("AzureStorageConnectionString");

            if (!CloudStorageAccount.TryParse(storageConnectionString, out CloudStorageAccount storageAccount))
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            var blobClient = storageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference(ProductsContainerName);

            await container.CreateIfNotExistsAsync();

            // Delete existing files
            var exisintgProductWebsite = await _context.ProductWebsite.FirstOrDefaultAsync(m => m.ProductId == id);
            if (exisintgProductWebsite != null && !string.IsNullOrEmpty(exisintgProductWebsite.HeaderImagePath))
            {
                Uri uri = new Uri(exisintgProductWebsite.HeaderImagePath);
                if (uri.IsFile)
                {
                    string fileName = System.IO.Path.GetFileName(uri.LocalPath);
                    var existingBlob = container.GetBlockBlobReference(fileName);
                    await existingBlob.DeleteIfExistsAsync();
                }
                exisintgProductWebsite.HeaderImagePath = null;
                await _context.SaveChangesAsync();

                if (file == null)
                {
                    return Ok();
                }
            }

            //MS: Don't rely on or trust the FileName property without validation. The FileName property should only be used for display purposes.
            var picBlob = container.GetBlockBlobReference(Guid.NewGuid().ToString() + "-" + file.FileName);
            await picBlob.UploadFromStreamAsync(file.OpenReadStream());

            if (exisintgProductWebsite != null)
            {
                exisintgProductWebsite.HeaderImagePath = picBlob.Uri.AbsoluteUri;
            }
            else
            {
                _context.ProductWebsite.Add(
                    new ProductWebsite
                    {
                        ProductId = id,
                        HeaderImagePath = picBlob.Uri.AbsoluteUri,
                        HeaderImageSize = file.Length.ToString()
                    });
            }
            await _context.SaveChangesAsync();

            return Ok(exisintgProductWebsite);
        }

        [HttpPost]
        [Route("{id}/UserManual")]
        [AllowAnonymous]
        public async Task<IActionResult> UploadUserManualAsync([FromRoute] int id, IFormFile file)
        {
            var exisintgProduct = await _context.Product.FirstOrDefaultAsync(m => m.ProductId == id);
            if (exisintgProduct == null)
            {
                return BadRequest($"ProductId {id} not found.");
            }

            var storageConnectionString = _config.GetConnectionString("AzureStorageConnectionString");

            if (!CloudStorageAccount.TryParse(storageConnectionString, out CloudStorageAccount storageAccount))
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            var blobClient = storageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference(ProductManualsContainerName);

            await container.CreateIfNotExistsAsync();

            // Delete existing files
            var exisintgProductWebsite = await _context.ProductWebsite.FirstOrDefaultAsync(m => m.ProductId == id);
            if (exisintgProductWebsite != null && !string.IsNullOrEmpty(exisintgProductWebsite.UserManualPath))
            {
                Uri uri = new Uri(exisintgProductWebsite.UserManualPath);
                if (uri.IsFile)
                {
                    string fileName = System.IO.Path.GetFileName(uri.LocalPath);
                    var existingBlob = container.GetBlockBlobReference(fileName);
                    await existingBlob.DeleteIfExistsAsync();
                }
                exisintgProductWebsite.UserManualPath = null;
                await _context.SaveChangesAsync();

                if (file == null)
                {
                    return Ok();
                }
            }

            //MS: Don't rely on or trust the FileName property without validation. The FileName property should only be used for display purposes.
            var picBlob = container.GetBlockBlobReference(Guid.NewGuid().ToString() + "-" + file.FileName);
            await picBlob.UploadFromStreamAsync(file.OpenReadStream());

            if (exisintgProductWebsite != null)
            {
                exisintgProductWebsite.UserManualPath = picBlob.Uri.AbsoluteUri;
            }
            else
            {
                _context.ProductWebsite.Add(
                    new ProductWebsite
                    {
                        ProductId = id,
                        UserManualPath = picBlob.Uri.AbsoluteUri
                    });
            }
            await _context.SaveChangesAsync();

            return Ok(exisintgProductWebsite);
        }

        // PUT: api/Products/5/Description
        [HttpPut("{id}/Description")]
        public async Task<IActionResult> PutProductDescription([FromRoute] int id, [FromBody] ProductWebsite product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != product.ProductId)
            {
                return BadRequest();
            }

            var exisintgProductWebsite = await _context.ProductWebsite.FirstOrDefaultAsync(m => m.ProductId == id);
            if (exisintgProductWebsite == null)
            {
                _context.ProductWebsite.Add(new ProductWebsite
                {
                    ProductId= id,
                    Description = product.Description
                });
            }
            else
            {
                exisintgProductWebsite.Description = product.Description;
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
               throw;
            }
            return NoContent();
        }

        // PUT: api/Products/5/Description
        [HttpPut("{id}/AdditionalInfo")]
        public async Task<IActionResult> PutProductAdditionalInfo([FromRoute] int id, [FromBody] ProductWebsite product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != product.ProductId)
            {
                return BadRequest();
            }

            var exisintgProductWebsite = await _context.ProductWebsite.FirstOrDefaultAsync(m => m.ProductId == id);
            if (exisintgProductWebsite == null)
            {
                _context.ProductWebsite.Add(new ProductWebsite
                {
                    ProductId = id,
                    AdditionalInformation = product.AdditionalInformation
                });
            }
            else
            {
                exisintgProductWebsite.AdditionalInformation = product.AdditionalInformation;
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            return NoContent();
        }

        // PUT: api/Products/5/Detail
        [HttpPut("{id}/Detail")]
        public async Task<IActionResult> PutProductDetail([FromRoute] int id, [FromBody] ProductWebsite product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != product.ProductId)
            {
                return BadRequest();
            }

            var exisintgProductWebsite = await _context.ProductWebsite.FirstOrDefaultAsync(m => m.ProductId == id);
            if (exisintgProductWebsite == null)
            {
                _context.ProductWebsite.Add(new ProductWebsite
                {
                    ProductId = id,
                    Detail = product.Detail
                });
            }
            else
            {
                exisintgProductWebsite.Detail = product.Detail;
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            return NoContent();
        }

        // PUT: api/Products/5/Warranty
        [HttpPut("{id}/Warranty")]
        public async Task<IActionResult> PutProductWarrantyInformation([FromRoute] int id, [FromBody] ProductWebsite product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != product.ProductId)
            {
                return BadRequest();
            }

            var exisintgProductWebsite = await _context.ProductWebsite.FirstOrDefaultAsync(m => m.ProductId == id);
            if (exisintgProductWebsite == null)
            {
                _context.ProductWebsite.Add(new ProductWebsite
                {
                    ProductId = id,
                    WarrantyInformation = product.WarrantyInformation
                });
            }
            else
            {
                exisintgProductWebsite.WarrantyInformation = product.WarrantyInformation;
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            return NoContent();
        }

        [HttpPost("{id}/ProductTag")]
        public async Task<IActionResult> CreateProductTags([FromRoute] int id, [FromBody] List<ProductTag> model)
        {
            var tags = _context.ProductTag.Where(p => p.ProductId == id);
            _context.ProductTag.RemoveRange(tags);
            await _context.SaveChangesAsync();

            foreach (var tag in model)
            {
                tag.ProductId = id;
                _context.ProductTag.Add(tag);
            }
            await _context.SaveChangesAsync();
            return Ok();
        }
        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.ProductId == id);
        }
    }
}