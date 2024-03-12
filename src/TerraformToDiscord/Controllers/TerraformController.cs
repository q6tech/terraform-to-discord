using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TerraformToDiscord.Core;
using TerraformToDiscord.Core.Models;

namespace TerraformToDiscord.Controllers
{
    [Route("api/v1/terraform"), ApiController]
    public class TerraformController : ControllerBase
    {
        private readonly DiscordClient _client;

        public TerraformController(DiscordClient client)
        {
            _client = client;
        }

        [HttpPost]
        public async Task<IActionResult> Accept([FromBody] NotificationPayload payload)
        {
            await _client.Forward(payload);
            return Accepted();
        }
    }
}
