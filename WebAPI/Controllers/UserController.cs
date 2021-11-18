using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Data.Entities;
using WebAPI.Enums;
using WebAPI.Models;
using WebAPI.Models.BindingModel;
using WebAPI.Models.DTO;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly JWTConfig _jWTConfig;
        public UserController(UserManager<AppUser> userManager,SignInManager<AppUser> signInManager,IOptions<JWTConfig> jwtConfig)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jWTConfig = jwtConfig.Value;//In this value we have object of jwtconfig and data from appsettings
        }

        [HttpPost("RegisterUser")]
        public async Task<Object> RegisterUser([FromBody] AddUpdateRegisterUserBinding model)
        {
            try
            {
                var user = new AppUser()
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    UserName = model.Email,
                    DateCreated = DateTime.UtcNow,
                    DateModified = DateTime.UtcNow
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    return await Task.FromResult(new ResponseModel(ResponseCode.OK,"User has been Registered",null));
                }
                return await Task.FromResult(new ResponseModel(ResponseCode.Error, "", string.Join(",", result.Errors.Select(x => x.Description).ToArray())));
            }
            catch (Exception e)
            {
                return await Task.FromResult(new ResponseModel(ResponseCode.Error, e.Message, null));
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("GetAllUser")]
        public async Task<Object> GetAllUser()
        {
            try
            {
                var users = _userManager.Users.Select(x => new UserDTO(x.FullName,x.Email,x.UserName,x.DateCreated)).ToList();//using select is same as like using js map keyword
                return await Task.FromResult(users);
            }catch(Exception e)
            {
                return await Task.FromResult(new ResponseModel(ResponseCode.Error, e.Message, null));
            }
        }

        [HttpPost("Login")]
        public async Task<Object> Login([FromBody] LoginBindingModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
                    if (result.Succeeded)
                    {
                        var getUser = await _userManager.FindByEmailAsync(model.Email);
                        var user = new UserDTO(getUser.FullName, getUser.Email, getUser.UserName, getUser.DateCreated);
                        user.Token = GenerateToken(getUser);
          
                        return await Task.FromResult(new ResponseModel(ResponseCode.OK, "", user));
                    }
                }
                return await Task.FromResult(new ResponseModel(ResponseCode.Error, "Invalid Login attempt", null));
            }
            catch (Exception e)
            {
                return await Task.FromResult(new ResponseModel(ResponseCode.Error, e.Message, null));
            }
        }

        private string GenerateToken(AppUser appUser)
        {
            var jwtTokenHolder = new JwtSecurityTokenHandler();
            var secretKey = Encoding.ASCII.GetBytes(_jWTConfig.Key);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[] {
                    new System.Security.Claims.Claim(JwtRegisteredClaimNames.NameId, appUser.Id),
                    new System.Security.Claims.Claim(JwtRegisteredClaimNames.Email, appUser.Email),
                    new System.Security.Claims.Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(12),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature),
                Audience = _jWTConfig.Audience,
                Issuer = _jWTConfig.Issuer
            };
            var token = jwtTokenHolder.CreateToken(tokenDescriptor);
            return jwtTokenHolder.WriteToken(token);
        }
    }
}
