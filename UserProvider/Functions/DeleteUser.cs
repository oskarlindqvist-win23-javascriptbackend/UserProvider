using Infrastructure.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace UserProvider.Functions
{
    public class DeleteUser(ILogger<DeleteUser> logger, DataContext context)
    {
        private readonly ILogger<DeleteUser> _logger = logger;
        private readonly DataContext _context = context;

        [Function("DeleteUser")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "DeleteUser/{userId}")] HttpRequest req, string userId)
        {
            try
            {
                var existingUser = await _context.Users.Include(u => u.Address).FirstOrDefaultAsync(u => u.Id == userId);
                if (existingUser == null)
                {
                    return new NotFoundResult();
                }

                if (existingUser.Address != null)
                {
                    _context.Addresses.Remove(existingUser.Address);
                }

                _context.Users.Remove(existingUser);
                await _context.SaveChangesAsync();

                return new OkResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
