using System.Runtime.InteropServices.JavaScript;
using BLL;
using DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Play;

public class GameEnd : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Winner { get; set; }
    
    private readonly DAL.AppDbContext _context;
    public UnoGameEngine Engine { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public Guid GameId { get; set; }

    public bool Error { get; set; }

    public GameEnd(AppDbContext context)
    {
        _context = context;
        Engine = new UnoGameEngine(new GameRepositoryEF(_context));
    }
    
    public void OnGet()
    {
        try
        {
            Engine.LoadGame(GameId);
        }
        catch (Exception e)
        {
            Error = true;
        }
    }
}