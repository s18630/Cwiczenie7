using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Cwiczenie5.DTOs;
using Cwiczenie5.Models;
using Cwiczenie5.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Cwiczenie5.Controllers
{
    
    [Route("api/students")]
    [ApiController]
    

    public class StudentsController : ControllerBase
    {
        private IStudentsDbService _service;
        public IConfiguration Configuration { get; set; }
        public StudentsController(IStudentsDbService service, IConfiguration configuration)
        {
            _service = service;
            Configuration = configuration;
        }

        

        [HttpGet]
        //  [Authorize(Roles ="admin")]
        //   [AllowAnonymous]
        [Authorize]
        public IActionResult GetStudents()
        {

            try
            {
                IEnumerable<Student> students = _service.GetStudents();


                return Ok(students);
            }

            catch (Exception )
            {

                return BadRequest();
            }

        }




        [HttpPost]

        public IActionResult Login(LoginRequestDto request)
        {
            try
            {
                bool log = _service.isLogOk(request);
                if(log== false)
                {
                    return BadRequest();
                }
            }
            catch(Exception)
            {
                return Ok("wyszuciło blad");
            }
            var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "jan123"),
            new Claim(ClaimTypes.Role, "admin"),
            new Claim(ClaimTypes.Role, "student"),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "Gakko",
                audience: "Students",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
                );
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken = Guid.NewGuid()
            });
        }



    }
}
