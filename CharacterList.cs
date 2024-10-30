using TournamentFighter.Models;

namespace TournamentFighter
{
    public class CharacterList
    {
        public static Character Default => new();
        public Dictionary<string, Character> Characters = new();

        public Character Dejourn => new()
        {
            Name = "Dejourn",
            Tagline = "Burly Axe Wielder",
            OpeningDialogue = "Hoo, sorry you have to face me. My axe will cut you so deep that you'll " +
                              "be in the hospital for weeks. Now, let's see if you can give me a good challenge!!",
            Description = "WHOA now those are some muscles! He makes that axe on his back look small!\n" +
                          "And no armor except for a breast plate? I think we're gonna see some real power from this guy!",
            VictoryLineHighHP = "HA HA HA that just felt like squashing a bug!",
            VictoryLineMediumHP = "Hm, one day you might be able to match my strength!",
            VictoryLineLowHP = "HAAaa..Those were some good moves, you gave me a run for my money!",
            DefeatDialogue = "OHH now that's a surprise, you're not half bad!",
            Moves = [Move.AxeSwing, Move.Counter, Move.RoaringMoon],
        };
        public Character Hina => new()
        {
            Name = "Hina",
            Tagline = "Fiery Martial Artist",
            OpeningDialogue = "My weapon of choice is my own body. No one here has been worthy of my respect, " +
                              "so I ask you to come at me with all you have- I'm bored.",
            Description = "This girl looks fierce, I think she'd kill me if I just looked at her the wrong way!\n" +
                          "It looks like her hands are wrapped too, she's going to deliver some pain with her fists!",
            VictoryLineHighHP = "Pathetic.",
            VictoryLineMediumHP = "Go work on your fundamentals.",
            VictoryLineLowHP = "...so you landed a few good hits... in the end it wasn't enough.",
            DefeatDialogue = "9 times out of 10 I would win. Don't get full of yourself.",
            Moves = [Move.Punch, Move.JumpKick, Move.Counter],
        };
        public Character Grizwald => new()
        {
            Name = "Grizwald",
            Tagline = "Mysterious Professor",
            OpeningDialogue = "Greetings! I'm Professor Grizwald! During my adventures, I have come across many " +
                              "mystical spells from an unidentified civilization! I have been searching for a way to " +
                              "relieve some stress, and I figured, let's try out these spells! Best of luck!",
            Description = "Ladies and gentlemen, it's Professor Grizwald! Looking as gentlemanly as ever.\n" +
                          "What a great hat Professor! The Professor likes to go explore ruins, " +
                          "I wonder if he found some kind of weapon!?",
            VictoryLineHighHP = "It appears these scrolls pack quite the figurative punch!",
            VictoryLineMediumHP = "Great showing old chap. I would like to duel with you once more sometime!",
            VictoryLineLowHP = "My wits haven't quite failed me yet!",
            DefeatDialogue = "It appears I have lost! Blast, I will return once I have collected more scrolls!",
            Moves = [Move.IceSpike, Move.FlameImplosion, Move.ThousandCuts, Move.CursedExcalibur],
        };
        public Character Number5 => new()
        {
            Name = "Number 5",
            Tagline = "Reassembled",
            OpeningDialogue = "Howdy partner",
            Description = "Is that a robot, and is it talking?? It sure is saying some weird stuff but it looks like it's " +
                          "built for combat. This should be an interesting fight!",
            VictoryLineHighHP = "NUMBER 5 IS ALIVE",
            VictoryLineMediumHP = "NUMBER 5 STUPID NAME. WANT TO BE KEVIN OR DAVE",
            VictoryLineLowHP = "ATTRACTIVE! NICE SOFTWARE",
            DefeatDialogue = "NO DISASSEMBLE!",
            Moves = [Move.FocusedLaser, Move.CrushingGrip, Move.GroundSlam, Move.RedirectLightning],
        };

        public CharacterList()
        {
            Characters.Add("Ryalt", new()
            {
                Name = "Ryalt",
                Tagline = "Sly Archer",
                OpeningDialogue = "You must be my opponent. My name is Ryalt, I’m a proud warrior from my country.\n" +
                              "I’ve never lost a fight and I certainly won’t be breaking my winning streak today.\n" +
                              "Now come on, prepare yourself!",
                Description = "This guy looks fast. He's not wearing any armor too, just long sleeves and pants.\n" +
                          "And is that a bow? Looks like we have an archer ladies and gentlemen!",
                VictoryLineHighHP = "My arrows always point me to victory!",
                VictoryLineMediumHP = "You weren't fast enough to outrun my arrows!",
                VictoryLineLowHP = "Your have good aim... good match!",
                DefeatDialogue = "Gah, I wasn't focused enough to beat you...",
                Moves = [Move.LongShot, Move.ShortShot, Move.Punch],
            });
            Characters["Ryalt"].SetStats(100, 90, 30, 60, 90);
            Dejourn.SetStats(100, 60, 80, 90, 80);
            Hina.SetStats(100, 90, 50, 90, 90);
            Grizwald.SetStats(100, 70, 70, 70, 90);
            Number5.SetStats(100, 80, 90, 90, 80);
        }

        public void RefreshAll()
        {
            Ryalt.Refresh(); Dejourn.Refresh(); Hina.Refresh();
            Grizwald.Refresh(); Number5.Refresh();
        }

        private static readonly int NumCharacters = 5;
        public Queue<Character> GetUniqueSet(int n, Random rng)
        {
            int[] indices = Enumerable.Range(0, NumCharacters).ToArray();
            for (int i = 0; i < indices.Length; i++)
            {
                int rand = rng.Next(indices.Length);
                (indices[i], indices[rand]) = (indices[rand], indices[i]);
            }
            Character[] characters = [Ryalt, Dejourn, Hina, Grizwald, Number5];
            Queue<Character> res = new(n);
            for (int i = 0; i < n; i++)
            {
                res.Enqueue(characters[indices[i]]);
            }
            return res;
        }
    }
}
