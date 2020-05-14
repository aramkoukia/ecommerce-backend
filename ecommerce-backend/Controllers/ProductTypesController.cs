﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using EcommerceApi.Untilities;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using EcommerceApi.Repositories;
using EcommerceApi.ViewModel;

namespace EcommerceApi.Controllers
{
    [Produces("application/json")]
    [Route("api/ProductTypes")]
    [Authorize]
    public class ProductTypesController : Controller
    {
        private readonly EcommerceContext _context;
        private readonly IConfiguration _config;
        private readonly IProductTypeRepository _productTypeRepository;
        private const string ContentContainerName = "content";

        public ProductTypesController(EcommerceContext context,
                                      IProductTypeRepository productTypeRepository,
                                      IConfiguration config)
        {
            _context = context;
            _config = config;
            _productTypeRepository = productTypeRepository;
        }

        // GET: api/ProductTypes
        [HttpGet]
        public async Task<IEnumerable<ProductTypeViewModel>> GetProductTypes()
            => await _productTypeRepository.GetProductTypes();

        [HttpGet("updateslugs")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateSlugs()
        {
            var productTypes = _context.ProductType;
            foreach (var item in productTypes)
            {
                item.SlugsUrl = SlugGenerator.ToSlug(item.ProductTypeName);
            }
            _context.SaveChanges();

            var products = _context.Product;
            foreach (var item in products)
            {
                var slugsUrl = SlugGenerator.ToSlug(item.ProductName);
                var productWebsite = await _context.ProductWebsite.FirstOrDefaultAsync(m => m.ProductId == item.ProductId && m.SlugsUrl == slugsUrl);
                if (productWebsite == null)
                {
                    var newProductWebsite = new ProductWebsite
                    {
                        ProductId = item.ProductId,
                        SlugsUrl = slugsUrl
                    };
                    _context.ProductWebsite.Add(newProductWebsite);
                }
                else 
                {
                    productWebsite.SlugsUrl = slugsUrl;
                }
                _context.SaveChanges();
            }
            return Ok();
        }

        // GET: api/ProductTypes/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductType([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var productType = await _context.ProductType.SingleOrDefaultAsync(m => m.ProductTypeId == id);

            if (productType == null)
            {
                return NotFound();
            }

            return Ok(productType);
        }

        // PUT: api/ProductTypes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProductType([FromRoute] int id, [FromBody] ProductType productType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != productType.ProductTypeId)
            {
                return BadRequest();
            }

            var exisintgProductType = await _context.ProductType.FirstOrDefaultAsync(m => m.ProductTypeId == id);
            if (exisintgProductType == null)
            {
                return BadRequest($"ProductTypeId {id} not found.");
            }
            exisintgProductType.ShowOnWebsite = productType.ShowOnWebsite;
            exisintgProductType.ProductTypeName = productType.ProductTypeName;
            exisintgProductType.ModifiedDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Pacific Standard Time");
            exisintgProductType.SlugsUrl = SlugGenerator.ToSlug(productType.ProductTypeName);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductTypeExists(id))
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

        // PUT: api/ProductTypes/5/Description
        [HttpPut("{id}/Description")]
        public async Task<IActionResult> PutProductTypeDescription([FromRoute] int id, [FromBody] ProductType productType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != productType.ProductTypeId)
            {
                return BadRequest();
            }

            var exisintgProductType = await _context.ProductType.FirstOrDefaultAsync(m => m.ProductTypeId == id);
            if (exisintgProductType == null)
            {
                return BadRequest($"ProductTypeId {id} not found.");
            }
            exisintgProductType.Description = productType.Description;
            exisintgProductType.ModifiedDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Pacific Standard Time");
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductTypeExists(id))
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

        // POST: api/ProductTypes
        [HttpPost]
        public async Task<IActionResult> PostProductType([FromBody] ProductType productType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            productType.ProductTypeId = _context.ProductType.Max(l => l.ProductTypeId) + 1;
            productType.SlugsUrl = SlugGenerator.ToSlug(productType.ProductTypeName);
            _context.ProductType.Add(productType);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ProductTypeExists(productType.ProductTypeId))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetProductType", new { id = productType.ProductTypeId }, productType);
        }

        // DELETE: api/ProductTypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductType([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var productType = await _context.ProductType.SingleOrDefaultAsync(m => m.ProductTypeId == id);
            if (productType == null)
            {
                return NotFound();
            }

            _context.ProductType.Remove(productType);
            await _context.SaveChangesAsync();

            return Ok(productType);
        }

        [HttpPost]
        [Route("{id}/Upload")]
        [AllowAnonymous]
        public async Task<IActionResult> UploadAsync([FromRoute] int id, IFormFile file)
        {
            var exisintgProductType = await _context.ProductType.FirstOrDefaultAsync(m => m.ProductTypeId == id);
            if (exisintgProductType == null)
            {
                return BadRequest($"ProductTypeId {id} not found.");
            }

            var storageConnectionString = _config.GetConnectionString("AzureStorageConnectionString");

            if (!CloudStorageAccount.TryParse(storageConnectionString, out CloudStorageAccount storageAccount))
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            var blobClient = storageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference(ContentContainerName);

            await container.CreateIfNotExistsAsync();

            //MS: Don't rely on or trust the FileName property without validation. The FileName property should only be used for display purposes.
            var picBlob = container.GetBlockBlobReference(Guid.NewGuid().ToString() + "-" + file.FileName);

            await picBlob.UploadFromStreamAsync(file.OpenReadStream());

            exisintgProductType.ThumbnailImagePath = picBlob.Uri.AbsoluteUri;
            await _context.SaveChangesAsync();
            return Ok(exisintgProductType);
        }

        private bool ProductTypeExists(int id)
        {
            return _context.ProductType.Any(e => e.ProductTypeId == id);
        }
    }
}