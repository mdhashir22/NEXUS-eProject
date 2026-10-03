using Microsoft.AspNetCore.Mvc;
using NEXUS_eProject.Models;
using NEXUS_eProject.Services;

namespace NEXUS_eProject.Controllers
{
    [Route("api/nexus-ai")]
    public class AiAssistantController : Controller
    {
        private readonly INexusAiService _aiService;

        public AiAssistantController(
            INexusAiService aiService)
        {
            _aiService = aiService;
        }


        [HttpPost("chat")]
        public async Task<IActionResult> Chat(
            [FromBody] AiChatRequest request)
        {
            if (
                request == null ||
                string.IsNullOrWhiteSpace(request.Message)
            )
            {
                return BadRequest(
                    new AiChatResponse
                    {
                        Success = false,
                        Message =
                            "Please enter a message."
                    }
                );
            }


            string message =
                request.Message.Trim();


            if (message.Length > 1000)
            {
                return BadRequest(
                    new AiChatResponse
                    {
                        Success = false,
                        Message =
                            "Your message is too long."
                    }
                );
            }


            string response =
                await _aiService.GetResponseAsync(
                    message
                );


            return Ok(
                new AiChatResponse
                {
                    Success = true,
                    Message = response
                }
            );
        }
    }
}