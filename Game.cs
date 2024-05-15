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

    public readonly record struct MessageModel(string Message, Actor Source)
    {
        public static readonly MessageModel Default = new("", Actor.Game);
        public static readonly MessageModel PlayerTurn = new("What will you do?", Actor.Game);
        public static readonly MessageModel OpponentTurn = new("", Actor.Opponent);
        public static readonly MessageModel GameOver = new("Game over!", Actor.Game);
    }

    public readonly record struct TurnResult(string Message, Actor Source, int AddedTurns, int DamageDealt);

    public class Game
    {
        private Queue<MessageModel> Messages = new Queue<MessageModel>();
        private bool _isPlayersTurn = false;

        private static readonly System.Random rng = new System.Random();

        public Character Player { get; private set; } = CharacterList.Default;
        public Character Opponent { get; private set; } = CharacterList.Default;


        private static readonly Dictionary<Move, Action<Character, Character>> _standardMoves = new()
        {
            {MoveList.None, (actor, opponent) => { } },
        };
        private readonly FrozenDictionary<Move, Action<Character, Character>> StandardMoves = _standardMoves.ToFrozenDictionary();
        public Move[] GetStandardMoves() => [.. StandardMoves.Keys];


        private static readonly Dictionary<Move, Action<Character>> _placementMoves = new()
        {
            {MoveList.Advance, (actor) => { } },
            {MoveList.Retreat, (actor) => { } },
            {MoveList.Dodge, (actor) => actor.ApplyStatus(Skill.Evasion, 100, 1, false) },
        };
        private readonly FrozenDictionary<Move, Action<Character>> PlacementMoves = _placementMoves.ToFrozenDictionary();
        public Move[] GetPlacementMoves() => [.. PlacementMoves.Keys];

        public void SetUpGame(Character playerModel)
        {
            Player = playerModel;
            Player.Moves = [MoveList.Punch, MoveList.SwordSlash, MoveList.JumpKick, MoveList.Counter];
            Opponent = CharacterList.Ryalt;
            Messages.Enqueue(new("Let's begin!\n" +
                "You enter the sandy arena. You see a clear blue sky above, and feel warm sunlight on your skin.\n" +
                "Opposite you stands " + Opponent.Description, Actor.Game));
            Messages.Enqueue(new(Opponent.OpeningDialogue, Actor.Opponent));

            _isPlayersTurn = FirstIsFaster(Player, Opponent);
        }

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

        private void OpponentTurn() => TakeTurn(Actor.Opponent, MoveList.Punch);

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

                    Messages.Enqueue(new(actor.Name + move.Messages[0], upNext));
                    Messages.Enqueue(new(target.Name + " takes " + damage + " damage!", upNext));
                }
                _isPlayersTurn = !_isPlayersTurn;
            }
        }
    }
}