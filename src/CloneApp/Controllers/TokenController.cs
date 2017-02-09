using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CloneApp.Controllers
{
    [Route("api/[controller]")]
    public class TokenController : Controller
    {
        private readonly TokenAuthOptions tokenOption;
        
        public TokenController(TokenAuthOptions t)
        {
            this.tokenOption = t;
        }
        // GET: api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
      [HttpPost("login")]
        public dynamic Post([FromBody]AuthReq req)
        { var auth = false;
            if(req.username=="test" &&req.password=="test")
            {
                DateTime? expire = DateTime.UtcNow.AddMinutes(2);
                var token = GetToken(req.username,expire);
                return new { auth = true, entityId = 1, token = token, tokenExpires = expire };


            }
            return new { auth = false };
        }

        private string GetToken(string username, DateTime? expire)
        {
            var handler = new JwtSecurityTokenHandler();
            ClaimsIdentity identity = new ClaimsIdentity(new GenericIdentity(username, "TokenAuth"), new[] { new Claim("EntityID", "1", ClaimValueTypes.Integer) });

            var securityToken = handler.CreateToken(new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor()
            {
                Issuer = this.tokenOption.Issuer,
                Audience = tokenOption.Audience,
                SigningCredentials = tokenOption.SigningCredentials,
                Subject = identity,
                Expires = expire
            });
            return handler.WriteToken(securityToken);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
