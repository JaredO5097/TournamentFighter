using System.Collections.Generic;
using TournamentFighter.Models;
using TournamentFighter.Data;

namespace TournamentFighter
{
    public enum Skill
    {
        None,
        Health,
        Agility,
        Defense,
        Strength,
        Accuracy,
        Evasion,
    }

    public enum Actor
    {
        Player,
        Opponent,
        Game,
    }

    public enum MessageType
    {
        Description,
        PlayerTurn,
        TurnResult,
        GameEnd,
    }

    public record struct Status(Skill Skill, int Modifier, int TurnsUntilExpire, bool Permanent);

    public readonly record struct MessageModel(string Message, Actor Source, MessageType Type)
    {
        public static readonly MessageModel Default = new("", Actor.Game, MessageType.Description);
        public static readonly MessageModel PlayerTurn = new("What will you do?", Actor.Game, MessageType.PlayerTurn);
    }


    public class Game
    {
        private Queue<MessageModel> Messages = new Queue<MessageModel>();
        private Queue<Actor> Turns = new Queue<Actor>();

        private static readonly System.Random rng = new System.Random();

        public Character Player { get; private set; } = CharacterList.Default;
        public Character Opponent { get; private set; } = CharacterList.Default;


        private static readonly Dictionary<Move, Action<Character, Character>> StandardMoves = new()
        {
            {MoveList.None, (actor, opponent) => { } },
        };
        public static Move[] GetStandardMoves() => [.. StandardMoves.Keys];

        private static readonly Dictionary<Move, Action<Character>> PlacementMoves = new()
        {
            {MoveList.Advance, (actor) => { } },
            {MoveList.Retreat, (actor) => { } },
            {MoveList.Dodge, (actor) => actor.ApplyStatus(Skill.Evasion, 100, 1, false) },
        };
        public static Move[] GetPlacementMoves() => [.. PlacementMoves.Keys];

        public void SetUpGame(Character playerModel)
        {
            Player = playerModel;
            Player.Moves = [MoveList.Punch, MoveList.SwordSlash, MoveList.JumpKick, MoveList.Counter];
            Opponent = CharacterList.Ryalt;
            Messages.Enqueue(new("Let's begin!\n" +
                "You enter the sandy arena. You see a clear blue sky above, and feel warm sunlight on your skin.\n" +
                "Opposite you stands " + Opponent.Description, Actor.Game, MessageType.Description));
            Messages.Enqueue(new(Opponent.OpeningDialogue, Actor.Opponent, MessageType.Description));
            if (FirstIsFaster(Player, Opponent))
            {
                Turns.Enqueue(Actor.Player);
                Turns.Enqueue(Actor.Opponent);
            } else
            {
                Turns.Enqueue(Actor.Opponent);
                Turns.Enqueue(Actor.Player);
            }
        }

        private static bool FirstIsFaster(in Character a, in Character b) =>
            a.Agility == b.Agility ? rng.Next(0, 2) > 0 : a.Agility > b.Agility;

        public MessageModel NextMessage() 
        {
            if (Messages.Count == 0)
            {
                NextTurn();
            }
            return Messages.Dequeue();
        }

        private void NextTurn()
        {
            if (Turns.TryPeek(out Actor actor))
            {
                if (actor == Actor.Player)
                {
                    Messages.Enqueue(MessageModel.PlayerTurn);
                }
                else if (actor == Actor.Opponent)
                {
                    OpponentTurn();
                }
                else
                {
                    Messages.Enqueue(new("Game's over I guess!", Actor.Game, MessageType.GameEnd));
                }
            }
        }

        public void PlayerTurn(Move move)
        {
            if (Player.TurnDelay == 0)
            {
                PerformMove(Player, Opponent, move);
                Turns.Dequeue();
                Turns.Enqueue(Actor.Player);
            }
        }

        private void OpponentTurn()
        {
            if (Opponent.TurnDelay == 0)
            {
                PerformMove(Opponent, Player, MoveList.Punch);
                Turns.Dequeue();
                Turns.Enqueue(Actor.Opponent);
            }
        }

        public void Advance(Character actor)
        {

        }

        public void Retreat(Character actor)
        {

        }

        public void PerformMove(Character actor, Character target, Move move)
        {
            if (StandardMoves.ContainsKey(move))
            {
                StandardMoves[move](actor, target);
            } else
            {
                Actor src = actor == Player ? Actor.Player : Actor.Opponent;
                int attack = actor.AttackWith(move);
                int damage = target.TakeAttack(attack);

                Messages.Enqueue(new(actor.Name + move.Messages[0], src, MessageType.TurnResult));
                Messages.Enqueue(new(target.Name + " takes " + damage + " damage!", src, MessageType.TurnResult));
            }
        }
    }
}