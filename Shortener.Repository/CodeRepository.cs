using Microsoft.EntityFrameworkCore;
using Shortener.Application.Abstractions.Interfaces;
using Shortener.Data;
using Shortener.Data.Entities;

namespace Shortener.Repository
{
    public class CodeRepository(AppDbContext context)
    : ICodeRepository
    {
        public async Task<Link?> GetLinkByCode(string code)
        {
            return await context.Links
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Code == code);
        }

        public async Task<Link> AddLink(Link link)
        {
            context.Links.Add(link);
            await context.SaveChangesAsync();
            
            return link;
        }

        public async Task IncreaseClickCountByCode(string code, int i)
        {
            await context.Links
                .Where(l => l.Code == code)
                .ExecuteUpdateAsync(s => s.SetProperty(l => l.ClickCount, l => l.ClickCount + 1));
        }
    }
}
