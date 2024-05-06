using TournamentFighter.Models;

namespace TournamentFighter.Data
{
    public static class MoveList
    {
        public static Move None => new("None", ["Nothing"], 0, 0, 0, priority: 0);

        // Placement Moves
        public static Move Dodge => new("Dodge", ["Dodged just in time!"], 0, 0, 100, priority: 1);
        public static Move Advance => new("Advance", ["Moved towards the opponent"], 0, 0, 100, priority: 1);
        public static Move Retreat => new("Retreat", ["Moved back a few steps"], 0, 0, 100, priority: 1);
        // ---------------

        public static Move LongShot => new("Long Shot", ["Fired an arrow from afar!"], 2, 90, 95, priority: 0);
        public static Move Punch => new("Punch", ["Threw a hard punch!"], 0, 70, 100, priority: 0);
        public static Move SwordSlash => new("Sword Slash", ["Sliced the air in two!"], 0, 85, 100, priority: 0);
        public static Move AxeSwing => new("Axe Swing", ["Brought down a heavy axe!"], 1, 95, 95, priority: -1, extraTurnCost: 1);
        public static Move JumpKick => new("Jump Kick", ["Brought down a strong kick!"], 1, 80, 100, priority: 2, extraTurnCost: 1);
        public static Move ShortShot => new("Short Shot", ["Quickly fired an arrow!"], 1, 70, 95, priority: 1);
        public static Move Counter => new("Counter", ["Dodged then attacked!"], 0, 100, 100, priority: -1);

        public static Move[] ToArray() => [None, Dodge, Advance, Retreat, LongShot, Punch, SwordSlash, AxeSwing,
                                         JumpKick, ShortShot, Counter];
    }

    public static class CharacterList
    {
        public static Character Default => new Character
        {
            Name = "Default",
            Tagline = "Perfect Specimen",
            OpeningDialogue = @"Hello. I don't know why I'm here, but it means something went wrong.
                                Well no matter. If you were looking for a good challenge, you've found it!",
            Description = @"a man with a grey body and no hair. He has a slight smile on his face, 
                            and looks extremely confident. You can tell this will be a tough fight.",
            Health = 100,
            Moves = [MoveList.LongShot, MoveList.Punch, MoveList.SwordSlash, MoveList.AxeSwing,
                MoveList.JumpKick, MoveList.ShortShot, MoveList.Counter],
            Agility = 100,
            Defense = 100,
            Strength = 100,
            Accuracy = 100,
            Evasion = 100,
        };
        public static Character Ryalt => new Character
        {
            Name = "Ryalt",
            Tagline = "Sly Archer",
            OpeningDialogue = @"You there, you must be my opponent. My name is Ryalt. I’m a proud warrior from my country.
                                I’ve never lost a fight, and I certainly won’t be breaking my winning streak today.
                                Now come on, I have no more words for you. Prepare yourself!",
            Description = @"an average-height man. He looks slim but strong, and is wearing a long-sleeve shirt and pants, 
                            but no armor. He is holding a bow in one hand, and has a quiver on his waist.",
            Health = 100,
            Moves = [MoveList.LongShot, MoveList.ShortShot, MoveList.Punch],
            Agility = 90,
            Defense = 30,
            Strength = 60,
            Accuracy = 90,
            Evasion = 90,
        };
        public static Character Dejourn => new Character
        {
            Name = "Dejourn",
            Tagline = "Burly Axe Wielder",
            OpeningDialogue = @"Hoo so you're my prey, sorry you ran into me so early in the tournament.
                                My axe will cut you so deep that you'll be in the hospital for weeks.
                                Now bring it, let's see if you can give me a good challenge!",
            Health = 100,
            Moves = [MoveList.AxeSwing, MoveList.Counter],
            Agility = 60,
            Defense = 80,
            Strength = 90,
            Accuracy = 80,
            Evasion = 50,
        };
        public static Character Hina => new Character
        {
            Name = "Hina",
            Tagline = "Fiery Martial Artist",
            OpeningDialogue = @"Greetings, my name is Hina. My weapon of choice is my own body.
                                I respect the strong, and so far no one here has been worthy of my respect.
                                So, I ask you to come at me with all you have- I'm starting to get bored.",
            Health = 100,
            Moves = [MoveList.Punch, MoveList.JumpKick, MoveList.Counter],
            Agility = 90,
            Defense = 50,
            Strength = 90,
            Accuracy = 90,
            Evasion = 70,
        };

        public static Character[] ToArray() => [Default, Ryalt, Dejourn, Hina];
    }

    public static class SeedData
    {
        public static void Initialize(GameContext db)
        {
            db.Moves.AddRange(MoveList.ToArray());
            db.Characters.AddRange(CharacterList.ToArray());
        }
    }
}
