using TournamentFighter.Models;

namespace TournamentFighter
{
    public class CharacterList
    {
        private readonly List<Character> Characters = new();

        public CharacterList()
        {
            Characters.Add(new(new Stats(100, 90, 30, 60, 90), [Move.LongShot, Move.ShortShot, Move.Punch])
            {
                Name = "Ryalt",
                Tagline = "Sly Archer",
                Description = "This guy looks fast. He's not wearing any armor too, just long sleeves and pants.\n" +
                          "And is that a bow? Looks like we have an archer ladies and gentlemen!",
                VictoryLineHighHP = "My arrows always point me to victory!",
                VictoryLineMediumHP = "You weren't fast enough to outrun my arrows!",
                VictoryLineLowHP = "Your have good aim... good match!",
                DefeatDialogue = "Gah, I wasn't focused enough to beat you...",
            });
            Characters.Add(new(new Stats(100, 60, 80, 90, 80), [Move.AxeSwing, Move.Counter, Move.RoaringMoon])
            {
                Name = "Dejourn",
                Tagline = "Burly Axe Wielder",
                Description = "WHOA now those are some muscles! He makes that axe on his back look small!\n" +
                          "And no armor except for a breast plate? I think we're gonna see some real power from this guy!",
                VictoryLineHighHP = "HA HA HA that just felt like squashing a bug!",
                VictoryLineMediumHP = "Hm, one day you might be able to match my strength!",
                VictoryLineLowHP = "HAAaa..Those were some good moves, you gave me a run for my money!",
                DefeatDialogue = "OHH now that's a surprise, you're not half bad!",
            });
            Characters.Add(new(new Stats(100, 90, 50, 90, 90), [Move.Punch, Move.JumpKick, Move.Counter])
            {
                Name = "Hina",
                Tagline = "Fiery Martial Artist",
                Description = "This girl looks fierce, I think she'd kill me if I just looked at her the wrong way!\n" +
                          "It looks like her hands are wrapped too, she's going to deliver some pain with her fists!",
                VictoryLineHighHP = "Pathetic.",
                VictoryLineMediumHP = "Go work on your fundamentals.",
                VictoryLineLowHP = "...so you landed a few good hits... in the end it wasn't enough.",
                DefeatDialogue = "9 times out of 10 I would win. Don't get full of yourself.",
            });
            Characters.Add(new(new Stats(100, 70, 70, 70, 90), [Move.IceSpike, Move.FlameImplosion, Move.ThousandCuts, Move.CursedExcalibur])
            {
                Name = "Grizwald",
                Tagline = "Mysterious Professor",
                Description = "Ladies and gentlemen, it's Professor Grizwald! Looking as gentlemanly as ever.\n" +
                          "What a great hat Professor! The Professor likes to go explore ruins, " +
                          "I wonder if he found some kind of weapon!?",
                VictoryLineHighHP = "It appears these scrolls pack quite the figurative punch!",
                VictoryLineMediumHP = "Great showing old chap. I would like to duel with you once more sometime!",
                VictoryLineLowHP = "My wits haven't quite failed me yet!",
                DefeatDialogue = "It appears I have lost! Blast, I will return once I have collected more scrolls!",
            });
            Characters.Add(new(new Stats(100, 80, 90, 90, 80), [Move.FocusedLaser, Move.CrushingGrip, Move.GroundSlam, Move.RedirectLightning])
            {
                Name = "Number 5",
                Tagline = "Reassembled",
                Description = "Is that a robot, and is it talking?? It sure is saying some weird stuff but it looks like it's " +
                          "built for combat. This should be an interesting fight!",
                VictoryLineHighHP = "NUMBER 5 IS ALIVE",
                VictoryLineMediumHP = "NUMBER 5 STUPID NAME. WANT TO BE KEVIN OR DAVE",
                VictoryLineLowHP = "ATTRACTIVE! NICE SOFTWARE",
                DefeatDialogue = "NO DISASSEMBLE!",
            });
        }

        public Queue<Character> GetUniqueSet(int n, Random rng)
        {
            int[] indices = Enumerable.Range(0, Characters.Count).ToArray();
            for (int i = 0; i < indices.Length; i++)
            {
                int rand = rng.Next(indices.Length);
                (indices[i], indices[rand]) = (indices[rand], indices[i]);
            }

            Queue<Character> res = new(n);
            for (int i = 0; i < n; i++)
            {
                res.Enqueue(Characters[indices[i]]);
            }
            return res;
        }
    }
}