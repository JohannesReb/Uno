using BLL;
using DAL;
using Domain;
using Domain.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Play;

public class LobbyModel : PageModel
{
    private readonly DAL.AppDbContext _context;

    public UnoGameEngine Engine { get; set; }

    public LobbyModel(DAL.AppDbContext context)
    {
        _context = context;
        Engine = new UnoGameEngine(new GameRepositoryEF(_context));
    }

    public Game Game { get; set; } = default!;
    public void OnGet(Guid gameId, Guid playerId, string? nickname)
    {
        Engine.LoadGame(gameId);
        if (Engine.GetPlayerCount() <= Engine.GetPlayers().Count && nickname != null)
        {
            Response.Redirect($"/Games/Index?name={nickname}");
            return;
        }
        Game = _context.Games.First(g => g.Id == gameId);
        GameId = gameId;
        if (nickname != null && !Engine.GetPlayers().Select(p => p.NickName).Contains(nickname))
        {
            var id = Guid.NewGuid();
            Engine.AddPlayer(nickname, EPlayerType.Human, id);
            Engine.SaveGame();
            playerId = id;
        }
        PlayerId = playerId;
        if (Game.Active)
        {
            Response.Redirect($"/Play/Index?gameId={GameId}&playerId={PlayerId}");
            return;
        }

        if (nickname != null)
        {
            Response.Redirect($"/Play/lobby?gameId={GameId}&playerId={PlayerId}");
        }
    }
    [BindProperty]
    public Guid GameId { get; set; }
    [BindProperty]
    public Guid PlayerId { get; set; }
    public void OnPostStart()
    {
        Engine.LoadGame(GameId);
        Game = _context.Games.First(g => g.Id == GameId);
        if (!Game.Active)
        {
            Game.Active = true;
            _context.Games.Update(Game);
            Engine.InitializeGame();
            Engine.SaveGame();
        }
        Response.Redirect($"/Play/Index?gameId={GameId}&playerId={PlayerId}");
    }
    public void OnPostLeave()
    {
        Engine.LoadGame(GameId);
        var name = Engine.GetPlayerNameById(PlayerId).Split(" ")[1];
        Engine.RemovePlayer(PlayerId);
        Engine.SaveGame();
        Response.Redirect($"/Games/Index?name={name}");
    }
}