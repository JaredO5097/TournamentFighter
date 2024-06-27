using System.ComponentModel.DataAnnotations;

namespace TournamentFighter.Models
{
    public readonly record struct Move(string Name, Action<Character, Character> Action, string[] Messages, int Range, int BaseDamage, int BaseAccuracy,
        int Priority)
    {
        public static readonly Move None = new("None", (actor, opponent) => { },
            ["nothing"], 0, 0, 0, Priority: 0);
        public static readonly Move Dodge = new("Dodge", (actor, opponent) => { },
            ["dodged just in time!"], 0, 0, 100, Priority: 1);
        public static readonly Move Advance = new("Advance", (actor, opponent) => { },
            ["moved towards the opponent"], 0, 0, 100, Priority: 1);
        public static readonly Move Retreat = new("Retreat", (actor, opponent) => { },
            ["moved back a few steps"], 0, 0, 100, Priority: 1);
        public static readonly Move LongShot = new("Long Shot", (actor, opponent) => { actor.QueueMove(LongShot); },
            ["fired an arrow from afar!"], 2, 90, 95, Priority: 0);
        public static readonly Move Punch = new("Punch", (actor, opponent) => { actor.QueueMove(Punch); },
            ["threw a hard punch!"], 0, 70, 100, Priority: 0);
        public static readonly Move SwordSlash = new("Sword Slash", (actor, opponent) => { actor.QueueMove(SwordSlash); },
            ["sliced the air in two!"], 0, 85, 100, Priority: 0);
        public static readonly Move AxeSwing = new("Axe Swing", (actor, opponent) => { actor.QueueMove(None); actor.QueueMove(AxeSwing); },
            ["brought down a heavy axe!"], 1, 95, 95, Priority: -1);
        public static readonly Move JumpKick = new("Jump Kick", (actor, opponent) => { actor.QueueMove(JumpKick); },
            ["brought down a strong kick!"], 1, 80, 100, Priority: 1);
        public static readonly Move ShortShot = new("Short Shot", (actor, opponent) => { actor.QueueMove(ShortShot); },
            ["quickly fired an arrow!"], 1, 70, 95, Priority: 1);
        public static readonly Move Counter = new("Counter", (actor, opponent) => { actor.QueueMove(None); actor.QueueMove(Counter); },
            ["dodged then attacked!"], 0, 100, 100, Priority: -1);

        public static Move[] ToArray() => [Dodge, Advance, Retreat, LongShot, Punch, SwordSlash, AxeSwing,
                                         JumpKick, ShortShot, Counter];
    }
}
