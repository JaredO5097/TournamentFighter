using TournamentFighter.Models;

namespace TournamentFighter.Data
{
    public static class SeedData
    {
        public static void Initialize(GameContext db)
        {
            db.Characters.AddRange(CharacterList.ToArray());
        }
    }
}
