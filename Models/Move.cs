using System.ComponentModel.DataAnnotations;

namespace TournamentFighter.Models
{
    public class Move(string name, string[] messages, int range, int baseDamage, int baseAccuracy,
        int priority, int extraTurnCost = 1)
    {
        public string Name => name;
        public string[] Messages => messages;
        public bool Enabled { get; set; } = true;
        [Range(0, 2)] public int Range => range;
        [Range(0, 100)] public int BaseDamage => baseDamage;
        [Range(0, 100)] public int BaseAccuracy => baseAccuracy;
        [Range(-1, 1)] public int Priority => priority;
        [Range(0, 2)] public int ExtraTurnCost => extraTurnCost;

        private System.Random rng = new();
        public string NextMessage() => Messages[rng.Next(0, Messages.Length)];
    }
}
