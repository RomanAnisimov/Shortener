using Microsoft.EntityFrameworkCore;
using Shortener.Application.Abstractions.Interfaces;
using Shortener.Data;
using Shortener.Data.Entities;

namespace Shortener.Repository
{
    public class CodeRepository(AppDbContext context)
    : ICodeRepository
    {
        public async Task<Link?> GetLinkByCode(string code, CancellationToken ct = default)
        {
            return await context.Links
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Code == code, ct);
        }

        public async Task<Link> AddLink(Link link, CancellationToken ct = default)
        {
            context.Links.Add(link);
            await context.SaveChangesAsync(ct);
            
            return link;
        }

        public async Task IncreaseClickCountByCode(string code, int i, CancellationToken ct = default)
        {
            await context.Links
                .Where(l => l.Code == code)
                .ExecuteUpdateAsync(s => s.SetProperty(l => l.ClickCount, l => l.ClickCount + 1), ct);
        }
    }
}
