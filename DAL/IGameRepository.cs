using Domain;

namespace DAL;

public interface IGameRepository
{
    void Save(GameState game);
    List<(Guid id, DateTime dt)> GetSaveGames();
    GameState LoadGame(Guid id);
    bool DeleteGame(Guid id);
}