using System.ComponentModel.DataAnnotations;

namespace TournamentFighter.Models
{
    public readonly record struct Move(string Name, int TurnDelay, string[] Messages, int Range, int BaseDamage, int BaseAccuracy,
        int Priority)
    {
        public static readonly Move None = new("None", 0, ["nothing"], 0, 0, 0, Priority: 0);
        public static readonly Move Dodge = new("Dodge", 0, ["dodged just in time!"], 0, 0, 100, Priority: 1);
        public static readonly Move Advance = new("Advance", 0, ["moved towards the opponent"], 0, 0, 100, Priority: 1);
        public static readonly Move Retreat = new("Retreat", 0, ["moved back a few steps"], 0, 0, 100, Priority: 1);
        public static readonly Move LongShot = new("Long Shot", 0, ["fired an arrow from afar!"], 2, 90, 95, Priority: 0);
        public static readonly Move Punch = new("Punch", 0, ["threw a hard punch!"], 0, 70, 100, Priority: 0);
        public static readonly Move SwordSlash = new("Sword Slash", 0, ["sliced the air in two!"], 0, 85, 100, Priority: 0);
        public static readonly Move AxeSwing = new("Axe Swing", 1, ["brought down a heavy axe!"], 1, 95, 95, Priority: -1);
        public static readonly Move JumpKick = new("Jump Kick", 0, ["brought down a strong kick!"], 1, 80, 100, Priority: 1);
        public static readonly Move ShortShot = new("Short Shot", 0, ["quickly fired an arrow!"], 1, 70, 95, Priority: 1);
        public static readonly Move Counter = new("Counter", 1, ["dodged then attacked!"], 0, 100, 100, Priority: -1);
    }
}
