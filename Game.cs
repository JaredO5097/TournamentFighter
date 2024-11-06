using TournamentFighter.Models;

namespace TournamentFighter
{
    public enum Turn
    {
        Player,
        Opponent,
    }

    public enum MsgType
    {
        Empty,
        Game,
        PlayerInput,
        Move,
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

    public readonly record struct MsgPckg(Move Move, int DamageDealt);
    public readonly record struct Msg(MsgType Type, string Message, MsgPckg? Data = null);

    public class MsgTracker : Queue<Msg>
    {
        public void AddGame(string Message, MsgPckg? Data = null) => Enqueue(new(MsgType.Game, Message, Data));
        public void AddMove(string Message, Move Move) => Enqueue(new(MsgType.Move, Message, new(Move, 0)));
        public void AddDamage(string Message, int Damage) => Enqueue(new(MsgType.Damage, Message, new(Move.None, Damage)));
    }

    public class Game
    {
        private readonly Queue<Turn> Turns = new Queue<Turn>();

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
                Turns.Enqueue(Turn.Player);
                Turns.Enqueue(Turn.Opponent);
            }
            else
            {
                Turns.Enqueue(Turn.Opponent);
                Turns.Enqueue(Turn.Player);
            }
        }

        public void SetIntroMessages(MsgTracker tracker)
        {
            if (MatchNumber == 1)
            {
                tracker.AddGame("Welcome ladies and gentlemen, to the annual IRONMAN tournament!\n" +
                    "It's a bright sunny day here, and we've got some great fighters for you this year!\n" +
                    "Here comes our first one, " + Player.Name + "! They look like they're here to win!");
            }
            else if (MatchNumber == 2)
            {
                tracker.AddGame(Player.Name + " has moved on to the second round!\n" +
                    "But can they keep up their momentum??");
            }
            else if (MatchNumber == 3)
            {
                tracker.AddGame("This is it, it's the final round! If " + Player.Name +
                    " wins this, they'll be crowned our new champion!");
            }
            tracker.AddGame("And here's their opponent. " + Opponent.Description);
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
            if (Turns.Peek() == Turn.Player)
            {
                if (PlayerInput != Move.None)
                {
                    Player.QueueMove(PlayerInput);
                    PlayerInput = Move.None;
                }

                if (!Player.HasActions)
                {
                    tracker.Enqueue(new(MsgType.PlayerInput, "What will " + Player.Name + " do next??")); 
                } else
                {
                    TakeTurn(Player, Opponent, Player.NextAction(), tracker);
                }
            }
            else
            {
                if (!Opponent.HasActions)
                {
                    Opponent.QueueRandomMove(); // simulate input from Opponent
                }
                TakeTurn(Opponent, Player, Opponent.NextAction(), tracker);
            }
        }

        private void Victory(Character victor, MsgTracker tracker)
        {
            if (victor == Player)
            {
                tracker.AddGame("WHOA, " + Opponent.Name + " is on the ground! Looks like they have something to say.");
                tracker.Enqueue(new(MsgType.OpponentDialogue, Opponent.DefeatDialogue));
                tracker.Enqueue(MatchNumber >= MAX_MATCHES ?
                    new(MsgType.NewChampion, Player.Name + " is our new IRONMAN tournament champion!" +
                        " Congratulations " + Player.Name + "! You will forever be remembered as a champion- now go out and enjoy your " +
                        "newfound fame. We hope you'll come back to defend your title!")
                    : new(MsgType.PlayerVictory, Player.Name + " wins! That's the end of this match, wow that was exciting!"));
            } else
            {
                tracker.Enqueue(new(MsgType.Game, "WHOA, " + Player.Name + " is on the ground. That was the finishing blow!"));
                tracker.Enqueue(new(MsgType.OpponentDialogue, Opponent.GetVictoryLine()));
                tracker.Enqueue(new(MsgType.OpponentVictory, Opponent.Name + " wins! That's the end of this match, wow that was exciting!"));
            }
        }

        private void TakeTurn(Character actor, Character target, Move move, MsgTracker tracker)
        {
            actor.UpdateStatus(tracker);
            if (actor.Health <= 0)
            {
                Victory(target, tracker);
            } else
            {
                int attack = actor.AttackWith(move, tracker);
                target.TakeAttack(attack, move, tracker);
                if (target.Health <= 0)
                {
                    Victory(actor, tracker);
                } else
                {
                    Turns.Enqueue(Turns.Dequeue());
                }
            }
        }
    }
}