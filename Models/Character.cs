using System.ComponentModel.DataAnnotations;
using TournamentFighter.Data;
using Random = System.Random;

namespace TournamentFighter.Models
{
    public static class CharacterList
    {
        public static Character Default => new()
        {
            Name = "Default",
            Tagline = "Perfect Specimen",
            OpeningDialogue = @"Hello. I don't know why I'm here, but it means something went wrong.
                                Well no matter. If you were looking for a good challenge, you've found it!",
            Description = @"a man with a grey body and no hair. He has a slight smile on his face, 
                            and looks extremely confident. You can tell this will be a tough fight.",
            Health = 100,
            Moves = [Move.LongShot, Move.Punch, Move.SwordSlash, Move.AxeSwing,
                Move.JumpKick, Move.ShortShot, Move.Counter],
            Agility = 100,
            Defense = 100,
            Strength = 100,
            Accuracy = 100,
            Evasion = 100,
        };
        public static Character Ryalt => new()
        {
            Name = "Ryalt",
            Tagline = "Sly Archer",
            OpeningDialogue = @"You there, you must be my opponent. My name is Ryalt. I’m a proud warrior from my country.
                                I’ve never lost a fight, and I certainly won’t be breaking my winning streak today.
                                Now come on, I have no more words for you. Prepare yourself!",
            Description = @"an average-height man. He looks slim but strong, and is wearing a long-sleeve shirt and pants, 
                            but no armor. He is holding a bow in one hand, and has a quiver on his waist.",
            Health = 100,
            Moves = [Move.LongShot, Move.ShortShot, Move.Punch],
            Agility = 90,
            Defense = 30,
            Strength = 60,
            Accuracy = 90,
            Evasion = 70,
        };
        public static Character Dejourn => new()
        {
            Name = "Dejourn",
            Tagline = "Burly Axe Wielder",
            OpeningDialogue = @"Hoo so you're my prey, sorry you ran into me so early in the tournament.
                                My axe will cut you so deep that you'll be in the hospital for weeks.
                                Now bring it, let's see if you can give me a good challenge!",
            Description = @"a tall man with beefy muscles. He has a large axe across his back, and has no armor except for
                            a breast plate. He is wearing a tank top and cargo pants, and his tanned, bald head is reflecting the sun.",
            Health = 100,
            Moves = [Move.AxeSwing, Move.Counter],
            Agility = 60,
            Defense = 80,
            Strength = 90,
            Accuracy = 80,
            Evasion = 50,
        };
        public static Character Hina => new()
        {
            Name = "Hina",
            Tagline = "Fiery Martial Artist",
            OpeningDialogue = @"Greetings, my name is Hina. My weapon of choice is my own body.
                                I respect the strong, and so far no one here has been worthy of my respect.
                                So, I ask you to come at me with all you have- I'm starting to get bored.",
            Description = @"an average-height girl wearing a t-shirt and short shorts. Her hair is pulled back, and she has
                            defined muscles all across her body. Her hands are covered with multiple layers of a cotton wrap",
            Health = 100,
            Moves = [Move.Punch, Move.JumpKick, Move.Counter],
            Agility = 90,
            Defense = 50,
            Strength = 90,
            Accuracy = 90,
            Evasion = 70,
        };

        public static Character[] ToArray() => [Ryalt, Dejourn, Hina];
    }

    public class Character
    {
        [Required(ErrorMessage = "Name must not be blank")]
        [StringLength(10, ErrorMessage = "Max of 10 characters")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Tagline must not be blank")]
        [StringLength(15, ErrorMessage = "Max of 15 characters")]
        public string? Tagline { get; set; }

        public string? Description { get; set; }
        public string OpeningDialogue { get; set; } = "";

        public Move[] Moves = [Move.None];

        private readonly Queue<Status> StatusEffects = new Queue<Status>();
        private readonly Queue<Move> Actions = new Queue<Move>();

        // Health + Agility + Defense + Strength + Accuracy + Evasion = 400
        [Range(0, SkillCap)] public int Health { get; set; }
        [Range(1, SkillCap)]  public int Agility { get; set; }
        [Range(1, SkillCap)] public int Defense { get; set; }
        [Range(1, SkillCap)] public int Strength { get; set; }
        [Range(1, SkillCap)] public int Accuracy { get; set; }
        [Range(1, SkillCap)] public int Evasion { get; set; }

        private const int SkillCap = 100;
        private readonly static Random _rng = new Random();

        public int TakeAttack(int incomingDamage)
        {
            int actualDamage = 0;
            if (_rng.Next(1, SkillCap + 1) > (int)Math.Ceiling(0.15f * Evasion)) // did not evade
            {
                actualDamage = incomingDamage - (int)(0.75f * Defense);
                if (actualDamage < 0) { actualDamage = 0; }
            }
            Health -= actualDamage;
            return actualDamage;
        }

        public int AttackWith(Move move)
        {
            if (_rng.Next(1, SkillCap + 1) <= Accuracy) // character accuracy check
            {
                if (_rng.Next(1, SkillCap + 1) <= move.BaseAccuracy) // move accuracy check
                {
                    return move.BaseDamage + (int)(0.10f * Strength);
                }
            }
            return 0;
        }

        public void QueueMove(Move move)
        {
            Actions.Enqueue(move);
        }
        public bool HasActions => Actions.Count > 0;
        public Move CheckNextAction() => Actions.TryPeek(out Move res) ? res : Move.None;
        public Move NextAction() => Actions.TryDequeue(out Move res) ? res : Move.None;

        public void UpdateStatuses()
        {
            if (StatusEffects.Count > 0)
            {
                Status status = StatusEffects.Dequeue();
                if (status.Name == StatusName.TurnDelay)
                {
                    
                }
            }
        }
    }
}
