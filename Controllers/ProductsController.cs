using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RbacApi.DTOs;
using RbacApi.Responses;
using RbacApi.Services.Interfaces;

namespace RbacApi.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost("create")]
        [Authorize(Policy = "products:manage")]
        public async Task<ApiResponseBase> Create([FromForm] CreateProductRequest request)
        {
            var response = await _productService.CreateAsync(request);
            HttpContext.Response.StatusCode = response.Status;
            return response;
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "products:manage")]
        public async Task<ApiResponseBase> GetById(string id)
        {
            var response = await _productService.GetByIdAsync(id);
            HttpContext.Response.StatusCode = response.Status;
            return response;
        }
    }
}
