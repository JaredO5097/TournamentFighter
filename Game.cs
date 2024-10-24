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

        private static readonly Random _rng = new Random();

        private Move PlayerInput = Move.None;

        private const int MAX_MATCHES = 3;
        public int MatchNumber { get; private set; } = 1;

        private Queue<Character> OpponentQueue = new Queue<Character>();
        public Character Player { get; private set; } = CharacterList.Default;
        public Character Opponent { get; private set; } = CharacterList.Default;

        private void RefreshPlayer()
        {
            Player.ResetStats();
            Player.ClearActions();
        }

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
                    "Here comes our first one, " + Player.Name + "! They look like they're here to win!", Move.None));
            }
            else if (MatchNumber == 2)
            {
                tracker.Enqueue(new(MessageType.Game, Player.Name + " has moved on to the second round!\n" +
                    "But can they keep their momentum??", Move.None));
            }
            else if (MatchNumber == 3)
            {
                tracker.Enqueue(new(MessageType.Game, "It's the final round ladies and gentlemen! If " + Player.Name +
                    " wins this, they'll be crowned our new champion!", Move.None));
            }

            tracker.Enqueue(new(MessageType.Game, "And here's their opponent. " + Opponent.Description, Move.None));
        }

        public void InitializeNewGame(Character playerModel)
        {
            Player = playerModel;
            RefreshPlayer();

            MatchNumber = 1;
            OpponentQueue = CharacterList.GetUniqueSet(MAX_MATCHES, _rng);
            Opponent = OpponentQueue.Dequeue();

            InitializeTurns();
        }

        public void NextMatch()
        {
            MatchNumber++;
            if (MatchNumber <= MAX_MATCHES)
            {
                Opponent = OpponentQueue.Dequeue();
                RefreshPlayer();
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
                    tracker.Enqueue(new(MessageType.PlayerInput, "What will " + Player.Name + " do next??", Move.None)); 
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
                tracker.Enqueue(new(MessageType.Game, "WHOA, " + target.Name + " is on the ground! Looks like they have something to say.", Move.None));
                tracker.Enqueue(new(MessageType.OpponentDialogue, target.DefeatDialogue, Move.None));
                if (MatchNumber == MAX_MATCHES)
                {
                    tracker.Enqueue(new(MessageType.NewChampion, victor.Name + " is our new IRONMAN tournament champion!" +
                        " Well done " + victor.Name + "!", Move.None));
                } else
                {
                    tracker.Enqueue(new(MessageType.PlayerVictory, victor.Name + " wins! That's the end of this match, wow that was exciting!", Move.None));
                }
            } else if (type == MessageType.OpponentTurn) // opponent won
            {
                tracker.Enqueue(new(MessageType.Game, "WHOA, " + target.Name + " is on the ground. That was the finishing blow!", Move.None));
                tracker.Enqueue(new(MessageType.OpponentDialogue, victor.VictoryDialogue, Move.None));
                tracker.Enqueue(new(MessageType.OpponentVictory, victor.Name + " wins! That's the end of this match, wow that was exciting!", Move.None));
            }
        }

        private void TakeTurn(Character actor, Character target, MessageType type, Move move, MsgTracker tracker)
        {
            if (move == Move.None)
            {
                tracker.Enqueue(new(type, actor.Name + " isn't doing anything? Interesting...", Move.None));
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
                    tracker.Enqueue(new(type, actor.Name + " " + move.Messages[0], move));

                    if (target.Health <= 0)
                    {
                        HandleDefeat(actor, target, type, tracker);
                    }
                    else
                    {
                        tracker.Enqueue(new(type, target.Name + " " + GetDmgMsg(damage), Move.None));
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