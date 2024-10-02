using System.Text.Json;
using Domain;
using Domain.Database;

namespace DAL;

public class GameRepositoryEF : IGameRepository
{
    private readonly AppDbContext _ctx;

    public GameRepositoryEF(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public void Save(GameState gameState)
    {
        var game = _ctx.Games.FirstOrDefault(g => g.Id == gameState.Id);

        if (game == null)
        {
            game = new Game()
            {
                Id = gameState.Id,
                State = JsonSerializer.Serialize(gameState)
            };
            _ctx.Games.Add(game);
        }
        else
        {
            game.UpdatedAt = DateTime.Now;
            game.State = JsonSerializer.Serialize(gameState);
        }

        var changeCount = _ctx.SaveChanges();
        Console.WriteLine("SaveChanges: " + changeCount);


    }

    public List<(Guid id, DateTime dt)> GetSaveGames()
    {
        return _ctx.Games
            .OrderByDescending(g => g.UpdatedAt)
            .ToList()
            .Select(g => (g.Id, g.UpdatedAt))
            .ToList();
    }

    public GameState LoadGame(Guid id)
    {
        var game = _ctx.Games.First(g => g.Id == id);
        return JsonSerializer.Deserialize<GameState>(game.State)!;
    }

    public bool DeleteGame(Guid id)
    {
        var game = _ctx.Games.FirstOrDefault(g => g.Id == id);
        if (game != null)
        {
            _ctx.Games.Remove(game);
            var changeCount = _ctx.SaveChanges();
            Console.WriteLine("SaveChanges: " + changeCount);
            return true;
        }
        return false;
        
    }
}