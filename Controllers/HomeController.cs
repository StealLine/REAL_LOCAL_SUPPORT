using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Support_Bot.CREDENTIALS_CLASSES;
using Support_Bot.DB_INTERFACES;
using Support_Bot.DB_MODELS;
using Support_Bot.SENDTXT;

namespace Support_Bot.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class HomeController : ControllerBase
    {
        private readonly IDB_TICKER_CREATED IDB_TICKER_CREATED;
        private readonly IaddDBcontent iaddDBcontent;

        private readonly DiscordSocketClient discordSocketClient;
        private readonly IdiscordSendTXT idiscordSendTXT;
        private readonly IcheckActivity icheckActivity;
        private readonly ISetStatus setStatus;
        public HomeController(IDB_TICKER_CREATED IDB_TICKER_CREATED,
            IaddDBcontent iaddDBcontent,

            DiscordSocketClient discordSocketClient,
            IdiscordSendTXT idiscordSendTXT,
            IcheckActivity icheckActivity, ISetStatus setStatus)
        {
            this.IDB_TICKER_CREATED = IDB_TICKER_CREATED;
            this.iaddDBcontent = iaddDBcontent;

            this.discordSocketClient = discordSocketClient;
            this.idiscordSendTXT = idiscordSendTXT;
            this.icheckActivity = icheckActivity;
            this.setStatus = setStatus;
        }
        [HttpPost]
        [ActionName("DB_Ticket_Created")]
        public async Task<IActionResult> TicketCreated(string secret, [FromBody] DB_Ticket_Created_Model_Body model)
        {
            if(Secret.Secret_Key != secret)
            {
                return BadRequest("Wrong secret");
            }

            string response = await IDB_TICKER_CREATED.DB_ADD(model.creator_id, model.ticket_id, model.ticket_type);

            bool success = bool.TryParse(response, out _);
            if (!success)
            {
                return BadRequest(response);
            }

            return Ok(response);

        }
        [HttpGet]
        [ActionName("CHECKACTIVE")]
        public async Task<IActionResult> CheckActivity(string secret, string userID)
        {
            if (Secret.Secret_Key != secret)
            {
                return BadRequest("Wrong secret");
            }

            string response = await icheckActivity.Check(userID);

            bool success = bool.TryParse(response, out _);
            if (!success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost]
        [ActionName("SendTicketHistory")]
        public async Task<IActionResult> SendTXT(string secret, ulong userID, string ticketID)
        {
            if (Secret.Secret_Key != secret)
            {
                return BadRequest("Wrong secret");
            }

            bool response = await idiscordSendTXT.Send(userID,ticketID,discordSocketClient);

            return Ok(response);

        }
        [HttpPost]
        [ActionName("SetStatus")]
        public async Task<IActionResult> Status(string secret, string ticketID)
        {
            if (Secret.Secret_Key != secret)
            {
                return BadRequest("Wrong secret");
            }

            bool response = await setStatus.SetStat(ticketID);

            return Ok(response);

        }

    }
}
