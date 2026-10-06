using Microsoft.AspNetCore.Mvc;
using Shortener.Application.Abstractions.Interfaces;
using Shortener.Application.DTOs;
using Shortener.Shared;
using Shortener.Shared.Events;

namespace Shortener.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShortenerController(
        ICodeService codeService,
        IKafkaProducerService kafkaProducerService) : ControllerBase
    {
        [HttpPost]
        [Route("shorten")]
        public async Task<IActionResult> Shorten([FromBody] ShortenRequest request)
        {
            if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
                return BadRequest("Invalid URL");

            var link = await codeService.CreateLinkAsync(request);

            var linkCreated = new LinkCreated(link.Code, request.Url, link.CreatedAt);
            await kafkaProducerService.SendAsync(KafkaTopics.LinkCreated, linkCreated);

            return Ok(new { link.Code, shortUrl = $"http://localhost:7777/{link.Code}" });
        }

        [HttpGet]
        [Route("{code}")]
        public async Task<IActionResult> Code(string code, CancellationToken ct)
        {
            var link = await codeService.HandleClickAsync(code, ct);
            if (link is null)
                return NotFound();

            return Redirect(link.OriginalUrl);
        }
    }
}
