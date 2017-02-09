using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

namespace CloneApp
{
    public class TokenAuthOptions
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public SigningCredentials SigningCredentials { get; set; }
    }
    public class Startup
    {
        RsaSecurityKey  key;
        public TokenAuthOptions _tokenAuth;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }
       

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(2048);
            //Save the public key information to an RSAParameters structure.  
            
            RSAParameters KeyParam = RSA.ExportParameters(true);
            key = new RsaSecurityKey(KeyParam);
            _tokenAuth = new TokenAuthOptions
            {
                Audience = "TokenAudience",
                Issuer= "Issuer",
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256Signature)
            };

            services.AddSingleton<TokenAuthOptions>(_tokenAuth);

            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme‌​)
                    .RequireAuthenticatedUser().Build());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            _tokenAuth = new TokenAuthOptions
            {
                Audience = "TokenAudience",
                Issuer = "Issuer",
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256Signature)
            };

            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = key,
                    ValidAudience = _tokenAuth.Audience,
                    ValidIssuer = _tokenAuth.Issuer,

                    // When receiving a token, check that it is still valid.
                    ValidateLifetime = true,

                    // This defines the maximum allowable clock skew - i.e.
                    // provides a tolerance on the token expiry time 
                    // when validating the lifetime. As we're creating the tokens 
                    // locally and validating them on the same machines which 
                    // should have synchronised time, this can be set to zero. 
                    // Where external tokens are used, some leeway here could be 
                    // useful.
                    ClockSkew = TimeSpan.FromMinutes(0)
                }
                

            });
            
            app.UseMvc();
        }
    }
}
