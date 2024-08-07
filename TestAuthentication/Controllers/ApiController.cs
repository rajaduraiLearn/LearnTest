using System.Web.Mvc;
using TestAuthentication.Models;

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
    }
}