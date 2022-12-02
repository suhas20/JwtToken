using JwtToken4.Configurations;
using JwtToken4.Model;
using JwtToken4.Model.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwtToken4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
       //private readonly JwtConfig _jwtConfig;
        public AuthenticationController(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
            //_jwtConfig = jwtConfig;

        }
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDto requestDto)
        {
            //validate incoming request
            if (ModelState.IsValid)
            {
                //check email exist
                var user_exist = await _userManager.FindByEmailAsync(requestDto.Email);

                if(user_exist != null)
                {
                    return BadRequest(new AuthResult()
                    {
                        Result = false,
                        Errors = new List<string>()
                        {
                            "Email already exist!!"
                        }
                    });
                }

                //create a user
                var new_user = new IdentityUser()
                {
                    Email = requestDto.Email,
                    UserName = requestDto.Email
                };

                var is_created = await _userManager.CreateAsync(new_user,requestDto.Password);
                if (is_created.Succeeded)
                {
                    //generate token
                    var token = GenerateJwtToken(new_user);
                    return Ok(new AuthResult()
                    {
                        Result = true,
                        Token = token
                    });
                }
                return BadRequest(new AuthResult()
                {
                    Errors = new List<string>()
                    {
                        "Server error"
                    },
                    Result = false
                });
            }

            return BadRequest();
        }
        [Route("Login")]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRegistrationRequestDto login)
        {
            if (ModelState.IsValid)
            {
                //check user exist
                var isExist = await _userManager.FindByEmailAsync(login.Email);
                if (isExist == null)
                    return BadRequest(new AuthResult()
                    {
                        Errors = new List<string>()
                        { 
                            "Invalid payload"
                        },
                        Result = false
                    });

                var isCorrect = await _userManager.CheckPasswordAsync(isExist, login.Password);

                if (!isCorrect)
                {
                    return BadRequest(new AuthResult()
                    {
                        Errors = new List<string>()
                        {
                            "Invalid credentials"
                        },
                        Result = false
                    });
                }

                var jwtToken = GenerateJwtToken(isExist);
                return Ok(new AuthResult()
                {
                    Token = jwtToken,
                    Result = true
                });
            }

            return BadRequest(new AuthResult()
            {
                Errors= new List<string>()
                {
                    "Invalid Payload"
                },
                Result = false
            });
        }


        private string GenerateJwtToken(IdentityUser user)
        {
            var jwtTokenhandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration.GetSection(key: "JwtConfig:Secret").Value);

            //token descriptor =>
            //allow all the description to put inside the token
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id",user.Id),
                    new Claim(JwtRegisteredClaimNames.Sub,user.Email),
                    new Claim(JwtRegisteredClaimNames.Email,value:user.Email),
                    new Claim(ClaimTypes.Role,"Admin"),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat,DateTime.Now.ToUniversalTime().ToString()),
                }),

                Expires = DateTime.Now.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256)
            };

            var token = jwtTokenhandler.CreateToken(tokenDescriptor);
            return jwtTokenhandler.WriteToken(token);

        }
    }
}
