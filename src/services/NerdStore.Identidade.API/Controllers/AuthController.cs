using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NerdStore.Identidade.API.Extensions;
using NerdStore.Identidade.API.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace NerdStore.Identidade.API.Controllers
{
    [Route("api/identidade")]
    public class AuthController : MainController
    {
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly UserManager<IdentityUser> userManager;
        private readonly AppSettings appSettings;

        public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IOptions<AppSettings> appSettings)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.appSettings = appSettings.Value;
        }

        [HttpPost("nova-conta")]
        public async Task<ActionResult> Registrar(UsuarioRegistro usuarioRegistro)
        {
            if (!ModelState.IsValid) return this.CustomResponse(ModelState);

            IdentityUser user = new IdentityUser()
            {
                UserName = usuarioRegistro.Email,
                Email = usuarioRegistro.Email,
                EmailConfirmed = true
            };

            IdentityResult result = await this.userManager.CreateAsync(user, usuarioRegistro.Senha);

            if(result.Succeeded)
            {
                return this.CustomResponse(await this.GerarJwt(usuarioRegistro.Email));
            }

            foreach (var error in result.Errors)
            {
                this.AddError(error.Description);
            }

            return this.CustomResponse();
        }

        [HttpPost("autenticar")]
        public async Task<ActionResult> Login(UsuarioLogin usuarioLogin)
        {
            if (!ModelState.IsValid) return this.CustomResponse(ModelState);

            SignInResult result = await this.signInManager.PasswordSignInAsync(usuarioLogin.Email, usuarioLogin.Senha, false, true);

            if(result.Succeeded)
            {
                return Ok(await this.GerarJwt(usuarioLogin.Email));
            }

            if (result.IsLockedOut)
            {
                this.AddError("User is temporarily locked out :/");
                return this.CustomResponse();
            }

            this.AddError("Incorrect email or password!");
            return this.CustomResponse();
        }

        private async Task<UsuarioRespostaLogin> GerarJwt(string email)
        {
            IdentityUser user = await this.userManager.FindByEmailAsync(email);
            IList<Claim> claims = await this.userManager.GetClaimsAsync(user);
            IEnumerable<string> userRoles = await this.userManager.GetRolesAsync(user);

            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.UtcNow).ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64));

            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim("role", userRole));
            }

            var identityClaims = new ClaimsIdentity();
            identityClaims.AddClaims(claims);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(this.appSettings.Secret);

            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor 
            {
                Issuer = this.appSettings.Emissor,
                Audience = this.appSettings.ValidoEm,
                Subject = identityClaims,
                Expires = DateTime.UtcNow.AddHours(this.appSettings.ExpiracaoHoras),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            });

            var encodedToken = tokenHandler.WriteToken(token);

            var response = new UsuarioRespostaLogin
            {
                AccessToken = encodedToken,
                ExpiresIn = TimeSpan.FromHours(this.appSettings.ExpiracaoHoras).TotalSeconds,
                UsuarioToken = new UsuarioToken
                {
                    Id = user.Id,
                    Email = user.Email,
                    Claims = claims.Select(c => new UsuarioClaim { Type = c.Type, Value = c.Value})
                }
            };

            return response;
        } 

        private static long ToUnixEpochDate(DateTime date)
            => new DateTimeOffset(date).ToUnixTimeSeconds();
    }
}
