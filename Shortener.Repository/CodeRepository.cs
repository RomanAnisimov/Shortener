using Microsoft.EntityFrameworkCore;
using Shortener.Application.Abstractions.Interfaces;
using Shortener.Data;
using Shortener.Data.Entities;

namespace Shortener.Repository
{
    public class CodeRepository(AppDbContext context)
    : ICodeRepository
    {
        public async Task<bool> IsCodeExistsAsync(string code)
        {
            return await context.Links.AnyAsync(l => l.Code == code);
        }

        public async Task<Link> AddLink(Link link)
        {
            context.Links.Add(link);
            await context.SaveChangesAsync();
            
            return link;
        }
    }
}
