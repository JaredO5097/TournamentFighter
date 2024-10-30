using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using Random = System.Random;

namespace TournamentFighter.Models
{
    public class Character
    {
        [Required(ErrorMessage = "Name must not be blank")]
        [StringLength(10, ErrorMessage = "Max of 10 characters")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Tagline must not be blank")]
        [StringLength(15, ErrorMessage = "Max of 15 characters")]
        public string? Tagline { get; set; } = "";

        public string? Description { get; set; } = "";
        public string OpeningDialogue { get; set; } = "";
        public string VictoryLineHighHP { get; set; } = "";
        public string VictoryLineMediumHP { get; set; } = "";
        public string VictoryLineLowHP { get; set; } = "";
        public string DefeatDialogue { get; set; } = "";

        public Move[] Moves = [Move.None];

        private readonly Queue<Move> Actions = new Queue<Move>();

        public Status CurrentStatus { get; private set; } = Status.None;
        private int TurnsUntilStatusExpire = 0;

        // Health + Agility + Defense + Strength + Accuracy = 350
        public int Health { get; set; }
        private int initHealth;
        public int Agility { get; set; }
        private int initAgility;
        public int Defense { get; set; }
        private int initDefense;
        public int Strength { get; set; }
        private int initStrength;
        public int Accuracy { get; set; }
        private int initAccuracy;

        private const int SKILL_CAP = 100;
        private readonly static Random _rng = new Random();

        public Character()
        {

        }

        public Character(int health, int agility, int defense, int strength, int accuracy)
        {

        }

        public void SetStats(int health, int agility, int defense, int strength, int accuracy)
        {
            Health = initHealth = Math.Clamp(health, 1, SKILL_CAP);
            Agility = initAgility = Math.Clamp(agility, 1, SKILL_CAP);
            Defense = initDefense = Math.Clamp(defense, 1, SKILL_CAP);
            Strength = initStrength = Math.Clamp(strength, 1, SKILL_CAP);
            Accuracy = initAccuracy = Math.Clamp(accuracy, 1, SKILL_CAP);
        }

        public void Refresh()
        {
            Health = initHealth; Agility = initAgility; Defense = initDefense;
            Strength = initStrength; Accuracy = initAccuracy;

            Actions.Clear();
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
                if (CurrentStatus != Status.None)
                {
                    TurnsUntilStatusExpire = 1;
                    UpdateStatus(tracker);
                }

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
            if (_rng.Next(1, SKILL_CAP + 1) > (int)Math.Ceiling(0.15f * Agility)) // did not evade
            {
                actualDamage = incomingDamage - (int)(0.75f * Defense);
                if (actualDamage < 0) { actualDamage = 0; }
            }
            Health -= actualDamage;
            return actualDamage;
        }

        public int AttackWith(Move move)
        {
            int res = move.BaseAccuracy + ((Accuracy * Accuracy / 10000) * (100 - move.BaseAccuracy));
            // res is the result of using the character's accuracy to increase the move's base accuracy
            // The closer the character's accuracy is to SKILL_CAP, or 100, the more significant this increase is
            // res = moveAccuracy + (((charAccuracy^2)/(100^2))(100 - moveAccuracy))
            if (_rng.Next(1, SKILL_CAP + 1) <= res)
            {
                return move.BaseDamage + (int)(0.10f * Strength);
            } else
            {
                return 0;
            }

        }

        public string GetVictoryLine()
        {
            if (Health >= 0.7 * initHealth)
            {
                return VictoryLineHighHP;
            } else if (Health <= 0.15 * initHealth)
            {
                return VictoryLineLowHP;
            } else
            {
                return VictoryLineMediumHP;
            }
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
