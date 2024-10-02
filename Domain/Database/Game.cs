namespace Domain.Database;

public class Game : BaseEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public string State { get; set; } = default!;
    public bool Active { get; set; } = false;
}