﻿using System.ComponentModel.DataAnnotations;
using Random = System.Random;

namespace TournamentFighter.Models
{
    public static class CharacterList
    {
        public static Character Default => new()
        {
            Name = "Default",
            Tagline = "",
            OpeningDialogue = "",
            Description = "",
            VictoryDialogue = "",
            DefeatDialogue = "",
            Health = 100,
            Moves = [Move.None],
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
            OpeningDialogue = @"You must be my opponent. My name is Ryalt, I’m a proud warrior from my country.
                                I’ve never lost a fight and I certainly won’t be breaking my winning streak today.
                                Now come on, prepare yourself!",
            Description = @"This guy looks fast. He's not wearing any armor too, just long sleeves and pants. 
                            And is that a bow? Looks like we have an archer ladies and gentlemen!",
            VictoryDialogue = @"My arrows always point me to victory!",
            DefeatDialogue = @"Gah, I wasn't focused enough to beat you...",
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
            OpeningDialogue = @"Hoo, sorry you have to face me. My axe will cut you so deep that you'll 
                                be in the hospital for weeks. Now, let's see if you can give me a good challenge!!",
            Description = @"WHOA now those are some muscles! He makes that axe on his back look small! 
                            And no armor except for a breast plate? I think we're gonna see some real power from this guy!",
            VictoryDialogue = @"HA HA HA NOW THAT WAS A GOOD FIGHT!",
            DefeatDialogue = @"OHH now that's a surprise, you've earned my respect as a fighter!",
            Health = 100,
            Moves = [Move.AxeSwing, Move.Counter, Move.RoaringMoon],
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
            OpeningDialogue = @"My weapon of choice is my own body. No one here has been worthy of my respect,
                                so I ask you to come at me with all you have- I'm bored.",
            Description = @"This girl looks fierce, I think she'd kill me if I just looked at her the wrong way!
                            It looks like her hands are wrapped too, she's going to deliver some pain with her fists!",
            VictoryDialogue = @"Pathetic.",
            DefeatDialogue = @"9 times out of 10 I would win. Don't get full of yourself.",
            Health = 100,
            Moves = [Move.Punch, Move.JumpKick, Move.Counter],
            Agility = 90,
            Defense = 50,
            Strength = 90,
            Accuracy = 90,
            Evasion = 70,
        };
        public static Character Grizwald => new()
        {
            Name = "Grizwald",
            Tagline = "Mysterious Professor",
            OpeningDialogue = @"Greetings! I'm Professor Grizwald! During my adventures, I have come across many
                                mystical spells from an unidentified civilization! I have been searching for a way to
                                relieve some stress, and I figured, let's try out these spells! Best of luck!",
            Description = @"Ladies and gentlemen, it's Professor Grizwald! Looking as gentlemanly as ever.
                            What a great hat Professor! The Professor likes to go explore ruins, 
                            I wonder if he found some kind of weapon!?",
            VictoryDialogue = @"Great showing old chap. It appears these scrolls deal a good deal of damage!",
            DefeatDialogue = @"It appears I have lost! Blast, I will return once I have collected more scrolls!",
            Health = 100,
            Moves = [Move.IceSpike, Move.FlameImplosion, Move.ThousandCuts, Move.CursedExcalibur],
            Agility = 70,
            Defense = 70,
            Strength = 70,
            Accuracy = 90,
            Evasion = 70,
        };
        public static Character Number5 => new()
        {
            Name = "Number 5",
            Tagline = "Reassembled",
            OpeningDialogue = @"Howdy partner",
            Description = @"Is that a robot, and is it talking?? It sure is saying some weird stuff but it looks like it's
                            built for combat. This should be an interesting fight!",
            VictoryDialogue = @"NUMBER 5 IS ALIVE",
            DefeatDialogue = @"NO DISASSEMBLE!",
            Health = 100,
            Moves = [Move.FocusedLaser, Move.CrushingGrip, Move.GroundSlam, Move.RedirectLightning],
            Agility = 80,
            Defense = 90,
            Strength = 90,
            Accuracy = 80,
            Evasion = 70,
        };

        public static Character[] ToArray() => [Ryalt, Dejourn, Hina, Grizwald, Number5];
    }

    public class Character
    {
        [Required(ErrorMessage = "Name must not be blank")]
        [StringLength(10, ErrorMessage = "Max of 10 characters")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Tagline must not be blank")]
        [StringLength(15, ErrorMessage = "Max of 15 characters")]
        public string? Tagline { get; set; }

        public string? Description { get; set; }
        public string OpeningDialogue { get; set; } = "";
        public string VictoryDialogue { get; set; } = "";
        public string DefeatDialogue { get; set; } = "";

        public Move[] Moves = [Move.None];

        private readonly Queue<Move> Actions = new Queue<Move>();

        public Status CurrentStatus { get; private set; } = Status.None;
        private int TurnsUntilStatusExpire = 0;

        // Health + Agility + Defense + Strength + Accuracy + Evasion = 400
        public int Health { get; set; }
        public int Agility { get; set; }
        public int Defense { get; set; }
        public int Strength { get; set; }
        public int Accuracy { get; set; }
        public int Evasion { get; set; }

        private const int SkillCap = 100;
        private readonly static Random _rng = new Random();

        public void SetStats(int health, int agility, int defense, int strength, int accuracy, int evasion)
        {
            Health = Math.Clamp(health, 1, SkillCap);
            Agility = Math.Clamp(agility, 1, SkillCap);
            Defense = Math.Clamp(defense, 1, SkillCap);
            Strength = Math.Clamp(strength, 1, SkillCap);
            Accuracy = Math.Clamp(accuracy, 1, SkillCap);
            Evasion = Math.Clamp(evasion, 1, SkillCap);
        }

        public void ClearStatus()
        {
            CurrentStatus = Status.None;
            TurnsUntilStatusExpire = 0;
        }

        public void UpdateStatus(MsgTracker tracker)
        {
            if (CurrentStatus == Status.Bleed)
            {
                Health -= 10;
                tracker.Enqueue(new(MessageType.Game, "Looks like " + Name + " lost some blood...", Move.None));
            }
            TurnsUntilStatusExpire--;
            if (TurnsUntilStatusExpire == 0)
            {
                if (CurrentStatus == Status.Bleed)
                {
                    tracker.Enqueue(new(MessageType.Game, "Looks like " + Name + " stopped bleeding!", Move.None));
                } else if (CurrentStatus == Status.Burn)
                {
                    Strength += 15;
                    tracker.Enqueue(new(MessageType.Game, "Looks like " + Name + " got their strength back!", Move.None));
                } else if (CurrentStatus == Status.Frostbite)
                {
                    Evasion += 15;
                    tracker.Enqueue(new(MessageType.Game, "Looks like " + Name + " can move their body again!", Move.None));
                } else if (CurrentStatus == Status.Immobile)
                {
                    Agility += 15;
                    tracker.Enqueue(new(MessageType.Game, "Looks like " + Name + " is moving faster!", Move.None));
                }
                CurrentStatus = Status.None;
            }
        }

        public void AddStatus(Status status, MsgTracker tracker)
        {
            if (CurrentStatus != status)
            {
                CurrentStatus = status;
                if (status == Status.Bleed)
                {
                    tracker.Enqueue(new(MessageType.Game, "Looks like " + Name + " started to bleed...", Move.None));
                }
                else if (status == Status.Burn)
                {
                    Strength -= 15;
                    tracker.Enqueue(new(MessageType.Game, "Looks like " + Name + " lost some strength...", Move.None));
                }
                else if (status == Status.Frostbite)
                {
                    Evasion -= 15;
                    tracker.Enqueue(new(MessageType.Game, "Looks like " + Name + "'s body is tense...", Move.None));
                }
                else if (status == Status.Immobile)
                {
                    Agility -= 15;
                    tracker.Enqueue(new(MessageType.Game, "Looks like " + Name + " is moving slower...", Move.None));
                }
            }
            TurnsUntilStatusExpire = 2;  
        }

        public int TakeAttack(int incomingDamage, Move move, MsgTracker tracker)
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

        public void QueueRandomMove()
        {
            Move move = Moves[_rng.Next(0, Moves.Length)];
            QueueMove(move);
        }

        public void QueueMove(Move move)
        {
            if (move.Priority == -1)
            {
                Actions.Enqueue(Move.None);
            }
            Actions.Enqueue(move);
        }
        public bool HasActions => Actions.Count > 0;
        public Move NextAction() => Actions.TryDequeue(out Move res) ? res : Move.None;
    }
}
