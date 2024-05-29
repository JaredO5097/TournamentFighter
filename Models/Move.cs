using System.ComponentModel.DataAnnotations;

namespace TournamentFighter.Models
{
    public record Move(string Name, string[] Messages, int Range, int BaseDamage, int BaseAccuracy,
        int Priority, int ExtraTurnCost = 1)
    {
        public static readonly Move None = new("None", ["nothing"], 0, 0, 0, Priority: 0);
        public static readonly Move Dodge = new("Dodge", ["dodged just in time!"], 0, 0, 100, Priority: 1);
        public static readonly Move Advance = new("Advance", ["moved towards the opponent"], 0, 0, 100, Priority: 1);
        public static readonly Move Retreat = new("Retreat", ["moved back a few steps"], 0, 0, 100, Priority: 1);
        public static readonly Move LongShot = new("Long Shot", ["fired an arrow from afar!"], 2, 90, 95, Priority: 0);
        public static readonly Move Punch = new("Punch", ["threw a hard punch!"], 0, 70, 100, Priority: 0);
        public static readonly Move SwordSlash = new("Sword Slash", ["sliced the air in two!"], 0, 85, 100, Priority: 0);
        public static readonly Move AxeSwing = new("Axe Swing", ["brought down a heavy axe!"], 1, 95, 95, Priority: -1, ExtraTurnCost: 1);
        public static readonly Move JumpKick = new("Jump Kick", ["brought down a strong kick!"], 1, 80, 100, Priority: 2, ExtraTurnCost: 1);
        public static readonly Move ShortShot = new("Short Shot", ["quickly fired an arrow!"], 1, 70, 95, Priority: 1);
        public static readonly Move Counter = new("Counter", ["dodged then attacked!"], 0, 100, 100, Priority: -1);

        public static Move[] ToArray() => [Dodge, Advance, Retreat, LongShot, Punch, SwordSlash, AxeSwing,
                                         JumpKick, ShortShot, Counter];
    }
}
