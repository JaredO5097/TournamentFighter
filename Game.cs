using TournamentFighter.Models;

namespace TournamentFighter
{
    public enum MessageType
    {
        Empty,
        Game,
        Turn,
        PlayerTurn,
        PlayerInput,
        OpponentTurn,
        GameOver
    }

    public enum Status
    {
        None,
        Bleed,
        Burn,
        Frostbite,
        Immobile,
    }

    public readonly record struct MessageModel(MessageType Type, string Message, Move Move)
    {
        public static readonly MessageModel Empty = new(MessageType.Empty, "", Move.None);
    }

    public class Game
    {
        private readonly Queue<MessageType> Turns = new Queue<MessageType>();

        private static readonly Random rng = new Random();

        private Move PlayerInput = Move.None;

        private Character playerInitial = CharacterList.Default;
        public Character Player { get; private set; } = CharacterList.Default;
        public Character Opponent { get; private set; } = CharacterList.Default;

        public void SetUpWithExistingPlayer()
        {
            SetUp(playerInitial);
        }

        public void SetUp(Character playerModel)
        {
            Player = playerInitial = playerModel;
            Player.Moves = [Move.Punch, Move.SwordSlash, Move.JumpKick, Move.Counter];

            Character[] characters = CharacterList.ToArray();
            Opponent = characters[rng.Next(0, characters.Length)];

            Turns.Clear();
            bool playerFaster = Player.Agility == Opponent.Agility ? rng.Next(0, 2) > 0 : Player.Agility > Opponent.Agility;
            if (playerFaster)
            {
                Turns.Enqueue(MessageType.PlayerTurn);
                Turns.Enqueue(MessageType.OpponentTurn);
            } else
            {
                Turns.Enqueue(MessageType.OpponentTurn);
                Turns.Enqueue(MessageType.PlayerTurn);
            }
        }

        public void RecordPlayerMove(Move move) => PlayerInput = move;

        public void NextTurn(Queue<MessageModel> tracker) 
        {
            if (Turns.Peek() == MessageType.PlayerTurn)
            {
                if (PlayerInput != Move.None)
                {
                    Player.QueueMove(PlayerInput);
                    PlayerInput = Move.None;
                }

                if (!Player.HasActions)
                {
                    tracker.Enqueue(new(MessageType.PlayerInput, "What will you do?", Move.None)); 
                    // signify that game is waiting for player's input
                } else
                {
                    TakeTurn(Player, Opponent, MessageType.PlayerTurn, Player.NextAction(), tracker);
                }
            }
            else
            {
                if (!Opponent.HasActions)
                {
                    Opponent.QueueRandomMove(); // simulate input from Opponent
                }
                TakeTurn(Opponent, Player, MessageType.OpponentTurn, Opponent.NextAction(), tracker);
            }
        }

        private void TakeTurn(Character actor, Character target, MessageType type, Move move, Queue<MessageModel> tracker)
        {
            if (move == Move.None)
            {
                tracker.Enqueue(new(type, actor.Name + " does nothing. Interesting...", Move.None));
            } else
            {
                actor.UpdateStatus();

                int attack = actor.AttackWith(move);
                int damage = target.TakeAttack(attack, move);

                tracker.Enqueue(new(type, actor.Name + " " + move.Messages[0], move));
                tracker.Enqueue(new(type, target.Name + " takes " + damage + " damage!", move));
                if (target.Health <= 0)
                {
                    if (target.DefeatDialogue != null) { tracker.Enqueue(new(type, target.DefeatDialogue, Move.None)); }
                    tracker.Enqueue(new(MessageType.GameOver, target.Name + " has been defeated. " + actor.Name + " wins!", move));
                }
            }
            Turns.Enqueue(Turns.Dequeue()); // front goes to back
        }
    }
}