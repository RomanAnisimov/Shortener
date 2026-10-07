using Shortener.Application.DTOs;
using Shortener.Data.Entities;

namespace Shortener.Services.Mapping;

public static class LinkMapper
{
    public static LinkResponse ToLinkResponse(Link link, string baseUrl)
    {
        var shortUrl = $"{baseUrl.TrimEnd('/')}/{link.Code}";

        return new LinkResponse(
            link.Code,
            shortUrl,
            link.OriginalUrl,
            link.CreatedAt,
            link.ExpiresAt,
            link.ClickCount
        );
    }
}