using System;
using System.Numerics;
using TournamentFighter.Models;

namespace TournamentFighter
{
    public enum Actor
    {
        Player,
        Opponent,
        Game,
    }

    public enum StatusName
    {
        TurnDelay,
    }

    public readonly record struct Status(StatusName Name, int TurnsUntilExpire);

    public readonly record struct MessageModel(string Message, Actor Source, Move Move)
    {
        public static readonly MessageModel Default = new("", Actor.Game, Move.None);
        public static readonly MessageModel PlayerTurn = new("What will you do?", Actor.Game, Move.None);
        public static readonly MessageModel GameOver = new("Game over!", Actor.Game, Move.None);
    }

    public readonly record struct TurnResult(string Message, Actor Source, int AddedTurns, int DamageDealt);

    public class Game
    {
        private readonly Queue<Actor> Turns = new Queue<Actor>();

        private static readonly Random rng = new Random();

        public Character Player { get; private set; } = CharacterList.Default;
        public Character Opponent { get; private set; } = CharacterList.Default;

        public void SetUp(Character playerModel)
        {
            Player = playerModel;
            Player.Moves = [Move.Punch, Move.SwordSlash, Move.JumpKick, Move.Counter];
            Character[] characters = CharacterList.ToArray();
            Opponent = characters[rng.Next(0, characters.Length)];

            Turns.Clear();
            bool playerFaster = Player.Agility == Opponent.Agility ? rng.Next(0, 2) > 0 : Player.Agility > Opponent.Agility;
            if (playerFaster)
            {
                Turns.Enqueue(Actor.Player);
                Turns.Enqueue(Actor.Opponent);
            } else
            {
                Turns.Enqueue(Actor.Opponent);
                Turns.Enqueue(Actor.Player);
            }
        }

        public void Start(Queue<MessageModel> tracker)
        {
            tracker.Enqueue(new("Let's begin!\n" +
                "You enter the sandy arena. You see a clear blue sky above, and feel warm sunlight on your skin.\n" +
                "Opposite you stands " + Opponent.Description, Actor.Game, Move.None));
            tracker.Enqueue(new(Opponent.OpeningDialogue, Actor.Opponent, Move.None));
        }

        public void RecordPlayerMove(Move move) => move.Action(Player);

        public void NextTurn(Queue<MessageModel> tracker) 
        {
            if (Turns.Peek() == Actor.Player)
            {
                if (!Player.HasActions)
                {
                    tracker.Enqueue(MessageModel.PlayerTurn); // signify that game is waiting for player's input
                } else
                {
                    TakeTurn(Player, Opponent, Actor.Player, Player.NextAction(), tracker);
                }
            }
            else
            {
                if (!Opponent.HasActions)
                {
                    Move move = Opponent.Moves[rng.Next(0, Opponent.Moves.Length)];
                    move.Action(Opponent); // simulate input from Opponent
                }
                TakeTurn(Opponent, Player, Actor.Opponent, Opponent.NextAction(), tracker);
            }
        }

        private void TakeTurn(Character actor, Character target, Actor identifier, Move move, Queue<MessageModel> tracker)
        {
            if (move == Move.None)
            {
                tracker.Enqueue(new(actor.Name + " does nothing.", identifier, Move.None));
            } else
            {
                int attack = actor.AttackWith(move);
                int damage = target.TakeAttack(attack);

                tracker.Enqueue(new(actor.Name + " " + move.Messages[0], identifier, move));
                tracker.Enqueue(new(target.Name + " takes " + damage + " damage!", identifier, move));
                if (target.Health <= 0)
                {
                    tracker.Enqueue(new(target.Name + " has been defeated. " + actor.Name + " wins!", identifier, move));
                    tracker.Enqueue(MessageModel.GameOver);
                }
            }
            Turns.Enqueue(Turns.Dequeue()); // front goes to back
        }
    }
}