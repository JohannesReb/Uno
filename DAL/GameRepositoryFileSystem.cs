
using System.Runtime.Serialization;
using System.Text.Json;
using Domain;

namespace DAL;

public class GameRepositoryFileSystem : IGameRepository
{
    private const string SaveLocation = @"Users\User\Desktop\UnoSaveGames\FileSystem";

    public void Save(GameState game)
    {
        var content = JsonSerializer.Serialize(game);
        var fileName = Path.ChangeExtension(game.Id.ToString(), ".json");
        if (!Path.Exists(SaveLocation))
        {
            Directory.CreateDirectory(SaveLocation);
        }

        File.WriteAllText(Path.Combine(SaveLocation, fileName), content);
    }

    public List<(Guid id, DateTime dt)> GetSaveGames()
    {
        var data = Directory.EnumerateFiles(SaveLocation);
        var res = data
            .Select(
                path => (
                    Guid.Parse(Path.GetFileNameWithoutExtension(path)),
                    File.GetLastWriteTime(path)
                )
            ).ToList();
        
        return res;
    }
    
    public GameState LoadGame(Guid id)
    {
        var fileName = Path.ChangeExtension(id.ToString(), ".json");

        var jsonStr = File.ReadAllText(Path.Combine(SaveLocation, fileName));
        var res = JsonSerializer.Deserialize<GameState>(jsonStr);
        if (res == null) throw new SerializationException($"Cannot deserialize {jsonStr}");

        return res;
    }

    public bool DeleteGame(Guid id)
    {
        var fileName = Path.ChangeExtension(id.ToString(), ".json");
        if (File.Exists(Path.Combine(SaveLocation, fileName)))
        {
            File.Delete(Path.Combine(SaveLocation, fileName));
            return true;
        }
        return false;
    }
}