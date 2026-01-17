using BLL.Interfaces;
using DTOs.Requests;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfile(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound(new { Message = "User not found" });

            return Ok(user);
        }

        [HttpPut("{id}/payment")]
        public async Task<IActionResult> UpdatePaymentInfo(string id, [FromBody] UpdatePaymentRequest request)
        {
            try
            {
                await _userService.UpdatePaymentInfoAsync(id, request.BankName, request.BankAccountNumber, request.TaxCode);
                return Ok(new { Message = "Payment info updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}