using DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly AppDbContext _ctx;

    public IndexModel(ILogger<IndexModel> logger, AppDbContext ctx)
    {
        _logger = logger;
        _ctx = ctx;
    }

    public int Count { get; set; }

    public void OnGet()
    {
        Count = _ctx.Games.Count();
    }
    [BindProperty]
    public string Nickname { get; set; } = default!;
    public IActionResult OnPost()
    {
        return Redirect("/Games/Index?name=" + Nickname);
    }
}