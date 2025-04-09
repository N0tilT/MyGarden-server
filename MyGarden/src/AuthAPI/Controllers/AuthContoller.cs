using EntitiesLibrary;
using EntitiesLibrary.Security;
using JwtAuthenticationManager;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthContoller : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly JwtTokenHandler _jwtTokenHandler;

        public AuthContoller(RoleManager<IdentityRole> roleManager, UserManager<User> userManager, SignInManager<User> signInManager, JwtTokenHandler jwtTokenHandler)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenHandler = jwtTokenHandler;
        }

        [HttpPost("login")]
        public async Task<ActionResult<SecurityResponse>> Login([FromBody] LoginRequest request)
        {
            var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, false, false);

            if (result.Succeeded)
            {
                var user = _userManager.Users.SingleOrDefault(r => r.Email == request.Email);
                if (user != null)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    return Ok(

                        new SecurityResponse
                        {
                            User = user,
                            Token = _jwtTokenHandler.GenerateJwtToken(user, userRoles.First())
                        });
                }
            }
            return BadRequest(request);
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user != null)
                return Ok(user);

            user = new User
            {
                UserName = request.Email,
                Email = request.Email
            };
            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);
                var roleExists = await _roleManager.RoleExistsAsync(request.RoleName);
                if (!roleExists)
                {
                    await _roleManager.CreateAsync(new IdentityRole
                    {
                        Name = request.RoleName
                    });
                }
                await _userManager.AddToRoleAsync(user, request.RoleName);
                return Ok(

                        new SecurityResponse
                        {
                            User = user,
                            Token = _jwtTokenHandler.GenerateJwtToken(user, request.RoleName)
                        });
            }
            return BadRequest(result.Errors);

        }
        [HttpGet("validate")]
        public IActionResult Validate([FromQuery(Name = "email")] string email, [FromQuery(Name = "token")] string token)
        {
            var u = _userManager.FindByEmailAsync(email);

            if (u == null)
            {
                return NotFound("User not found.");
            }

            var userId = _jwtTokenHandler.ValidateToken(token);

            if (userId != Convert.ToString(u.Id))
            {
                return BadRequest("Invalid token.");
            }

            return Ok(userId);
        }
    }

}
