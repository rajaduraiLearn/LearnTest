using System.Collections.Generic;
using System.Web.Mvc;
using TestAuthentication.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Web.Management;
using System.Security.Cryptography.Pkcs;

namespace TestAuthentication.Controllers
{
    public class ApiController : Controller
    {
        [HttpPost]
        public ActionResult SubmitData(TestModel model)
        {
            // Retrieve the bearer token from the Authorization header
            var authHeader = Request.Headers["Authorization"];
            string returnMessage = string.Empty;
            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();

                // Validate the token here
                // For example, you can use a token validation library or custom logic

                if (!IsTokenValid(token))
                {
                    returnMessage = "Invalid token";
                    //return new HttpStatusCodeResult(401, "Invalid token");
                }
            }
            else
            {
                returnMessage += "Authorization header missing or invalid";
                //return new HttpStatusCodeResult(401, "Authorization header missing or invalid");
            }

            if (model == null)
            {
                returnMessage += "Model is null";
                //return new HttpStatusCodeResult(400, "Model is null");
            }
            else
            {
                returnMessage += $"Model is with value {model.Id} received";
            }

            // Process the model here
            // For example, save it to the database

            return Json(new { message = $"Data received successfully with message {returnMessage}" });
        }

        private bool IsTokenValid(string token)
        {
            // Implement your token validation logic here
            // This is just a placeholder for demonstration purposes
            return token == "your_valid_token";
        }

        [HttpPost]
        public ActionResult VerifyPermission()
        {
            var authHeader = Request.Headers["Authorization"];

            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                var accessToken1 = authHeader.Substring("Bearer ".Length).Trim();

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidAudiences = new[] { "api://7111800c-fba6-4c38-9364-aee0dcb19266", "audience2" }, // Add your valid audiences here
                    ValidIssuers = new[] { "https://sts.windows.net/975f013f-7f24-47e8-a7d3-abc4752bf346/", "issuer2" }, // Add your valid issuers here
                    IssuerSigningKeys = GetIssuerSigningKeys()
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                SecurityToken validatedToken;
                try
                {
                    var principal = tokenHandler.ValidateToken(accessToken1, tokenValidationParameters, out validatedToken);

                    // Extract the appid
                    var appIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "appid")?.Value;

                    // List of allowed app IDs or object IDs
                    var allowedIds = new List<string>
                    {
                        "b19896f1-d1af-4c0b-a976-11d4f3aab957",
                        "AppId"
                    };

                    // Check if the token's appid or oid is in the list of allowed IDs
                    if (allowedIds.Contains(appIdClaim))
                    {
                        // Token is valid and the managed identity is allowed
                        var responseInfo = new
                        {
                            AppId = appIdClaim,
                            Success = true
                        };

                        return Json(responseInfo);
                    }
                    else
                    {
                        var errorInfo = new
                        {
                            message = "appIdClaim is not valid"
                        };

                        return Json(errorInfo);
                    }
                }
                catch (SecurityTokenValidationException ex)
                {
                    var errorInfo = new
                    {
                        message = $"Exception  {ex.Message}"
                    };

                    return Json(errorInfo);
                }
            }
            else
            {
                var errorInfo = new
                {
                    message = "Auth Header not found"
                };

                return Json(errorInfo);
            }
        }

        private IEnumerable<Microsoft.IdentityModel.Tokens.SecurityKey> GetIssuerSigningKeys()
        {
            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                "https://login.microsoftonline.com/common/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever());
            var openIdConfig = configurationManager.GetConfigurationAsync().Result;
            return openIdConfig.SigningKeys;
        }
    }
}