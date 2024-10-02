using BLL;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DAL;
using Domain;
using Domain.Database;

namespace WebApp.Pages_Games
{
    public class IndexModel : PageModel
    {
        private readonly DAL.AppDbContext _context;

        public UnoGameEngine Engine { get; set; }

        public IndexModel(DAL.AppDbContext context)
        {
            _context = context;
            Engine = new UnoGameEngine(new GameRepositoryEF(_context));
        }

        public IList<Game> Games { get;set; } = default!;
        public string Nickname { get; set; } = default!;

        public async Task OnGetAsync(string name)
        {
            Games = await _context.Games
                .OrderByDescending(g => g.UpdatedAt)
                .ToListAsync();
            Nickname = name;
        }
    }
}
