using AutoMapper;
using StressApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace StressApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class StressController : ControllerBase
    {
        private readonly IItemService _itemService;
        private readonly IMapper _mapper;

        public StressController(IItemService itemService, IMapper mapper)
        {
            _itemService = itemService;
            _mapper = mapper;
        }

        [HttpGet("version")]
        public async Task<IActionResult> GetVersion()
        {
            return Ok("1.0.0");
        }
    }
}