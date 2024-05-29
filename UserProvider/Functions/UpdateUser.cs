using Infrastructure.Contexts;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace UserProvider.Functions
{
    public class UpdateUser(ILogger<UpdateUser> logger, DataContext context)
    {
        private readonly ILogger<UpdateUser> _logger = logger;
        private readonly DataContext _context = context;

        [Function("UpdateUser")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "UpdateUser/{userId}")] HttpRequest req, string userId)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var updatedUser = JsonConvert.DeserializeObject<ApplicationUser>(requestBody);

                var existingUser = await _context.Users.Include(u => u.Address).FirstOrDefaultAsync(u => u.Id == userId);
                if (existingUser == null)
                {
                    return new NotFoundResult();
                }

                existingUser.FirstName = updatedUser!.FirstName;
                existingUser.LastName = updatedUser.LastName;
                existingUser.Email = updatedUser.Email;
                existingUser.UserName = updatedUser.Email;
                existingUser.NormalizedUserName = updatedUser.Email;
                existingUser.NormalizedEmail = updatedUser.Email;
                existingUser.PhoneNumber = updatedUser.PhoneNumber;
                existingUser.Biography = updatedUser.Biography;

                if (updatedUser.Address != null)
                {
                    if (existingUser.Address == null)
                    {
                        existingUser.Address = new AddressEntity();
                    }
                    existingUser.Address.AddressLine_1 = updatedUser.Address.AddressLine_1;
                    existingUser.Address.AddressLine_2 = updatedUser.Address.AddressLine_2;
                    existingUser.Address.PostalCode = updatedUser.Address.PostalCode;
                    existingUser.Address.City = updatedUser.Address.City;
                }

                await _context.SaveChangesAsync();

                return new OkObjectResult(existingUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user information.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
