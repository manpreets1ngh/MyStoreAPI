using ApplicationToSellThings.APIs.Models;
using ApplicationToSellThings.APIs.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationToSellThings.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> AddAddress([FromBody]AddressRequestApiModel addressRequestApiModel)
        {
            var result = await _addressService.AddAddress(addressRequestApiModel);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetAddressByUser(Guid userId)
        {
            var result = await _addressService.GetAddressByUser(userId);
            return Ok(result);
        }
    }
}
