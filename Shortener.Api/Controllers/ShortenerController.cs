using Microsoft.AspNetCore.Mvc;
using Shortener.Application.Abstractions.Interfaces;
using Shortener.Application.DTOs;

namespace Shortener.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShortenerController(
        ICodeService codeService) : ControllerBase
    {
        [HttpPost]
        [Route("shorten")]
        public async Task<IActionResult> Shorten([FromBody] ShortenRequest request, CancellationToken ct)
        {
            if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
                return BadRequest("Invalid URL");

            var response = await codeService.CreateLinkAsync(request, ct);
            return Ok(response);
        }

        [HttpGet]
        [Route("/{code}")]
        public async Task<IActionResult> Code(string code, CancellationToken ct)
        {
            var link = await codeService.HandleClickAsync(code, ct);
            if (link is null)
                return NotFound();

            return Redirect(link.OriginalUrl);
        }
    }
}
