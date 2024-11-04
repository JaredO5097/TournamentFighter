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
        Damage,
        OpponentDialogue,
        PlayerVictory,
        OpponentVictory,
        NewChampion,
    }

    public enum Status
    {
        None,
        Bleed,
        Burn,
        Immobile,
    }

    public record Stats(int Health, int Agility, int Defense, int Strength, int Accuracy);
    public record struct MutableStats(int Health, int Agility, int Defense, int Strength, int Accuracy)
    {
        public void SetAll(Stats stats)
        {
            Health = stats.Health; Agility = stats.Agility; Defense = stats.Defense;
            Strength = stats.Strength; Accuracy = stats.Accuracy;
        }
    }

    public readonly record struct MoveInfo(Move Move, int DamageDealt)
    {
        public static readonly MoveInfo Empty = new(Move.None, 0);
    }

    public readonly record struct MessageModel(MessageType Type, string Message, MoveInfo MoveInfo)
    {
        public static readonly MessageModel Empty = new(MessageType.Empty, "", MoveInfo.Empty);
    }

    public class MsgTracker : Queue<MessageModel> { }

    public class Game
    {
        private readonly Queue<MessageType> Turns = new Queue<MessageType>();

        private static readonly Random _rng = new Random();

        private Move PlayerInput = Move.None;

        private const int MAX_MATCHES = 3;
        public int MatchNumber { get; private set; } = 1;

        private readonly CharacterList OpponentList = new CharacterList();
        private Queue<Character> OpponentQueue = new Queue<Character>();

        private Character Player;
        private Character Opponent;
        public (Character, Character) PlayerAndOpponent => (Player, Opponent);

        private void InitializeTurns()
        {
            Turns.Clear();
            bool playerFaster = Player.Agility == Opponent.Agility ? _rng.Next(0, 2) > 0 : Player.Agility > Opponent.Agility;
            if (playerFaster)
            {
                Turns.Enqueue(MessageType.PlayerTurn);
                Turns.Enqueue(MessageType.OpponentTurn);
            }
            else
            {
                Turns.Enqueue(MessageType.OpponentTurn);
                Turns.Enqueue(MessageType.PlayerTurn);
            }
        }

        public void SetIntroMessages(MsgTracker tracker)
        {
            if (MatchNumber == 1)
            {
                tracker.Enqueue(new(MessageType.Game, "Welcome ladies and gentlemen, to the annual IRONMAN tournament!\n" +
                    "It's a bright sunny day here, and we've got some great fighters for you this year!\n" +
                    "Here comes our first one, " + Player.Name + "! They look like they're here to win!", MoveInfo.Empty));
            }
            else if (MatchNumber == 2)
            {
                tracker.Enqueue(new(MessageType.Game, Player.Name + " has moved on to the second round!\n" +
                    "But can they keep up their momentum??", MoveInfo.Empty));
            }
            else if (MatchNumber == 3)
            {
                tracker.Enqueue(new(MessageType.Game, "This is it, it's the final round! If " + Player.Name +
                    " wins this, they'll be crowned our new champion!", MoveInfo.Empty));
            }

            tracker.Enqueue(new(MessageType.Game, "And here's their opponent. " + Opponent.Description, MoveInfo.Empty));
        }

        public void InitializeNewGame(Character playerModel)
        {
            Player = playerModel;
            Player.Refresh();

            MatchNumber = 1;
            OpponentQueue = OpponentList.GetUniqueSet(MAX_MATCHES, _rng);
            Opponent = OpponentQueue.Dequeue();
            Opponent.Refresh();

            InitializeTurns();
        }

        public void NextMatch()
        {
            MatchNumber++;
            if (MatchNumber <= MAX_MATCHES)
            {
                Opponent = OpponentQueue.Dequeue();
                Opponent.Refresh();
                Player.Refresh();
                InitializeTurns();
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
                    tracker.Enqueue(new(MessageType.PlayerInput, "What will " + Player.Name + " do next??", MoveInfo.Empty)); 
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

        private static string GetDmgMsg(int damage) => damage switch
        {
            > 60 => "took a lethal blow!",
            > 40 => "must've felt that!",
            > 20 => "took some damage there!",
            > 10 => "might've felt that!",
            0 => "wasn't touched!",
            _ => "took a hit!"
        };

        private void HandleDefeat(Character victor, Character target, MessageType type, MsgTracker tracker)
        {
            if (type == MessageType.PlayerTurn) // player won
            {
                tracker.Enqueue(new(MessageType.Game, "WHOA, " + target.Name + " is on the ground! Looks like they have something to say.", MoveInfo.Empty));
                tracker.Enqueue(new(MessageType.OpponentDialogue, target.DefeatDialogue, MoveInfo.Empty));
                if (MatchNumber == MAX_MATCHES)
                {
                    tracker.Enqueue(new(MessageType.NewChampion, victor.Name + " is our new IRONMAN tournament champion!" +
                        " Well done " + victor.Name + "!", MoveInfo.Empty));
                } else
                {
                    tracker.Enqueue(new(MessageType.PlayerVictory, victor.Name + " wins! That's the end of this match, wow that was exciting!", MoveInfo.Empty));
                }
            } else if (type == MessageType.OpponentTurn) // opponent won
            {
                tracker.Enqueue(new(MessageType.Game, "WHOA, " + target.Name + " is on the ground. That was the finishing blow!", MoveInfo.Empty));
                tracker.Enqueue(new(MessageType.OpponentDialogue, victor.GetVictoryLine(), MoveInfo.Empty));
                tracker.Enqueue(new(MessageType.OpponentVictory, victor.Name + " wins! That's the end of this match, wow that was exciting!", MoveInfo.Empty));
            }
        }

        private void TakeTurn(Character actor, Character target, MessageType type, Move move, MsgTracker tracker)
        {
            if (move == Move.None)
            {
                tracker.Enqueue(new(type, actor.Name + " isn't doing anything? Interesting...", MoveInfo.Empty));
            } else
            {
                actor.UpdateStatus(tracker);
                if (actor.Health <= 0)
                {
                    MessageType reverse = type == MessageType.PlayerTurn ? MessageType.OpponentTurn : MessageType.PlayerTurn;
                    HandleDefeat(target, actor, reverse, tracker);
                } else
                {
                    int attack = actor.AttackWith(move);
                    int damage = target.TakeAttack(attack, move, tracker);
                    tracker.Enqueue(new(type, actor.Name + " " + move.Messages[0], new(move, damage)));

                    if (target.Health <= 0)
                    {
                        HandleDefeat(actor, target, type, tracker);
                    }
                    else
                    {
                        tracker.Enqueue(new(type, target.Name + " " + GetDmgMsg(damage), MoveInfo.Empty));
                        if (damage > 0 && move.Status != Status.None)
                        {
                            target.AddStatus(move.Status, tracker);
                        }
                    }
                }
            }
            Turns.Enqueue(Turns.Dequeue()); // front goes to back
        }
    }
}