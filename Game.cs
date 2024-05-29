using System.Collections.Frozen;
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

    public record struct Status(Skill Skill, int Modifier, int TurnsUntilExpire, bool Permanent);

    public readonly record struct MessageModel(string Message, Actor Source, Move Move)
    {
        public static readonly MessageModel Default = new("", Actor.Game, Move.None);
        public static readonly MessageModel PlayerTurn = new("What will you do?", Actor.Game, Move.None);
        public static readonly MessageModel GameOver = new("Game over!", Actor.Game, Move.None);
    }

    public readonly record struct TurnResult(string Message, Actor Source, int AddedTurns, int DamageDealt);

    public class Game
    {
        private readonly Queue<MessageModel> Messages = new Queue<MessageModel>();
        private bool _isPlayersTurn = false;

        private static readonly System.Random rng = new System.Random();

        public Character Player { get; private set; } = CharacterList.Default;
        public Character Opponent { get; private set; } = CharacterList.Default;


        private readonly FrozenDictionary<Move, Action<Character, Character>> StandardMoves = new Dictionary<Move, Action<Character, Character>>
        {
            {Move.None, (actor, opponent) => { } },
        }.ToFrozenDictionary();
        public Move[] GetStandardMoves() => [.. StandardMoves.Keys];

        private readonly FrozenDictionary<Move, Action<Character>> PlacementMoves = new Dictionary<Move, Action<Character>>
        {
            {Move.Advance, (actor) => { } },
            {Move.Retreat, (actor) => { } },
            {Move.Dodge, (actor) => actor.ApplyStatus(Skill.Evasion, 100, 1, false) },
        }.ToFrozenDictionary();
        public Move[] GetPlacementMoves() => [.. PlacementMoves.Keys];

        public void SetUpGame(Character playerModel)
        {
            Player = playerModel;
            Player.Moves = [Move.Punch, Move.SwordSlash, Move.JumpKick, Move.Counter];
            Opponent = GetRandom(CharacterList.ToArray());
            Messages.Clear();
            Messages.Enqueue(new("Let's begin!\n" +
                "You enter the sandy arena. You see a clear blue sky above, and feel warm sunlight on your skin.\n" +
                "Opposite you stands " + Opponent.Description, Actor.Game, Move.None));
            Messages.Enqueue(new(Opponent.OpeningDialogue, Actor.Opponent, Move.None));

            _isPlayersTurn = FirstIsFaster(Player, Opponent);
        }

        private static T GetRandom<T>(ref readonly T[] array) => array[rng.Next(0, array.Length)];

        private static bool FirstIsFaster(in Character a, in Character b) =>
            a.Agility == b.Agility ? rng.Next(0, 2) > 0 : a.Agility > b.Agility;

        public MessageModel Next() 
        {
            if (Messages.Count == 0)
            {
                if (_isPlayersTurn)
                {
                    Messages.Enqueue(MessageModel.PlayerTurn);
                } else
                {
                    OpponentTurn();
                }
            }
            return Messages.Dequeue();
        }

        public void PlayerTurn(Move move) => TakeTurn(Actor.Player, move);

        private void OpponentTurn() => TakeTurn(Actor.Opponent, GetRandom(ref Opponent.Moves));

        public void TakeTurn(Actor upNext, Move move)
        {
            Character actor = upNext == Actor.Player ? Player : Opponent;
            Character target = upNext == Actor.Player ? Opponent : Player;
            if (actor.TurnDelay == 0)
            {
                if (StandardMoves.TryGetValue(move, out var action))
                {
                    action(actor, target);
                }
                else
                {
                    int attack = actor.AttackWith(move);
                    int damage = target.TakeAttack(attack);

                    Messages.Enqueue(new(actor.Name + " " + move.Messages[0], upNext, move));
                    Messages.Enqueue(new(target.Name + " takes " + damage + " damage!", upNext, move));
                    if (target.Health <= 0)
                    {
                        Messages.Enqueue(MessageModel.GameOver);
                    }
                }
                _isPlayersTurn = !_isPlayersTurn;
            }
        }
    }
}