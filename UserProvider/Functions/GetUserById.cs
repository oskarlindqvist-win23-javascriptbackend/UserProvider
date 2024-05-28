using Infrastructure.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace UserProvider.Functions
{
    public class GetUserById(ILogger<GetUser> logger, DataContext context)
    {
        private readonly ILogger<GetUser> _logger = logger;
        private readonly DataContext _context = context;

        [Function("GetUserById")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetUserById/{userId}")] HttpRequest req, string userId)
        {
            var user = await _context.Users.Include(u => u.Address).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return new NotFoundResult();
            }
            return new OkObjectResult(user);
        }
    }
}
