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

    public readonly record struct CharacterMutableStats(int Health);

    public readonly record struct MessageModel(MessageType Type, string Message, Move Move)
    {
        public static readonly MessageModel Empty = new(MessageType.Empty, "", Move.None);
    }

    public class MsgTracker : Queue<MessageModel> { }

    public class Game
    {
        private readonly Queue<MessageType> Turns = new Queue<MessageType>();

        private static readonly Random rng = new Random();

        private Move PlayerInput = Move.None;

        private CharacterMutableStats playerInit;
        public Character Player { get; private set; } = CharacterList.Default;
        public Character Opponent { get; private set; } = CharacterList.Default;

        public void SetUpWithExistingPlayer()
        {
            Player.Health = playerInit.Health;
            Player.ClearStatus();
            SetUp(Player);
        }

        public void SetUp(Character playerModel)
        {
            playerInit = new CharacterMutableStats(playerModel.Health);
            Player = playerModel;
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

        public void NextTurn(MsgTracker tracker) 
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
                    tracker.Enqueue(new(MessageType.PlayerInput, "What will " + Player.Name + " do next??", Move.None)); 
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

        private string GetDmgMsg(int damage)
        {
            return damage switch
            {
                > 60 => "took a lethal blow!",
                > 40 => "must've felt that!",
                > 20 => "took some damage there!",
                > 10 => "might've felt that!",
                0 => "wasn't touched!",
                _ => "took a hit!"
            };
        }

        private void TakeTurn(Character actor, Character target, MessageType type, Move move, MsgTracker tracker)
        {
            if (move == Move.None)
            {
                tracker.Enqueue(new(type, actor.Name + " isn't doing anything? Interesting...", Move.None));
            } else
            {
                actor.UpdateStatus(tracker);

                int attack = actor.AttackWith(move);
                int damage = target.TakeAttack(attack, move, tracker);
                tracker.Enqueue(new(type, actor.Name + " " + move.Messages[0], move));

                if (target.Health <= 0)
                {
                    tracker.Enqueue(new(type, "WHOA, " + target.Name + " is on the ground! Looks like they have something to say", move));
                    if (target.DefeatDialogue != null) { tracker.Enqueue(new(type, target.DefeatDialogue, Move.None)); }
                    tracker.Enqueue(new(MessageType.GameOver, actor.Name + " wins! That's the end of this match, wow that was exciting!", Move.None));
                } else
                {
                    
                    tracker.Enqueue(new(type, target.Name + " " + GetDmgMsg(damage), move));
                    if (damage > 0 && move.Status != Status.None)
                    {
                        target.AddStatus(move.Status, tracker);
                    }
                }
            }
            Turns.Enqueue(Turns.Dequeue()); // front goes to back
        }
    }
}