using Microsoft.AspNetCore.Mvc;
using Shortener.Application.Abstractions.Interfaces;
using Shortener.Shared;
using Shortener.Shared.DTO;
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

            var link = await codeService.GenerateLink(request);

            await codeService.AddLink(link);

            var linkCreated = new LinkCreated(link.Code, request.Url, link.CreatedAt);
            await kafkaProducerService.SendAsync(KafkaTopics.LinkCreated, linkCreated);

            return Ok(new { link.Code, shortUrl = $"http://localhost:7777/{link.Code}" });
        }

        [HttpGet]
        [Route("{code}")]
        public async Task<IActionResult> Code(string code)
        {
            var link = await codeService.GetLinkByCode(code);
            if (link is null) return NotFound();

            if (link.ExpiresAt is not null && link.ExpiresAt < DateTimeOffset.UtcNow)
                return NotFound();

            var linkClicked = new LinkClicked(code, DateTimeOffset.UtcNow, null, null, null);
            await kafkaProducerService.SendAsync(KafkaTopics.LinkClicked, linkClicked);

            return Redirect(link.OriginalUrl);
        }
    }
}
