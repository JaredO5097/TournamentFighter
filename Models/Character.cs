using System.ComponentModel.DataAnnotations;
using TournamentFighter.Data;
using Random = System.Random;

namespace TournamentFighter.Models
{
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

        public Move[] Moves { get; set; } = [MoveList.None];

        private LinkedList<Status> StatusEffects = new LinkedList<Status>();
        public int TurnDelay { get; private set; } = 0;

        private int _posX = 0;
        private int _posY = 0;

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

        public void ChangePosition(int dx, int dy)
        {
            _posX += dx;
            _posY += dy;
        }

        public void ApplyStatus(Skill skill, int modifier, int turnsUntilExpire, bool permanent)
        {
            if (skill != Skill.None) { StatusEffects.Append(new Status(skill, modifier, turnsUntilExpire, permanent)); }
        }

        public void UpdateStatuses()
        {
            LinkedListNode<Status>? node = StatusEffects.First;
            while (node is not null)
            {
                LinkedListNode<Status>? next = node.Next;
                if (node.Value.TurnsUntilExpire < 0)
                {
                    StatusEffects.Remove(node);
                } else
                {
                    switch (node.Value.Skill)
                    {
                        case Skill.Health:
                            Health += node.Value.Modifier;
                            return;
                        case Skill.Agility:
                            Agility += node.Value.Modifier;
                            return;
                        case Skill.Defense:
                            Defense += node.Value.Modifier;
                            return;
                    }
                }
                node = next;
            }
        }
    }
}
