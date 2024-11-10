using TournamentFighter.Data;
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
        private readonly Queue<Turn> Turns_G = new();
        private readonly MsgTracker Tracker_G = new();
        private static readonly Random Rng_G = new();

        public Msg? LastMsg { get; private set; }
        private Move PlayerInput = Move.None;

        private const int MAX_MATCHES = 3;
        public int MatchNumber { get; private set; } = 1;

        private readonly CharacterList OpponentList = new CharacterList();
        private Queue<Character> OpponentQueue = new Queue<Character>();

        private Character Player;
        private Character Opponent;
        public (Character, Character) PlayerAndOpponent => (Player, Opponent);

        private readonly IServiceScopeFactory _scopeFactory;

        public Game(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        private void RecordChampion()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetService<GameContext>()!;
                db.Champions.Add(new() { Name = Player.Name, Tagline = Player.Tagline });
                db.SaveChanges();
            }
        }

        public Champion[] GetChampions()
        {
            Champion[] res = [];
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetService<GameContext>()!;
                res = db.Champions.ToArray();
            }
            return res;
        }

        private void InitializeTurns()
        {
            Turns_G.Clear();
            bool playerFaster = Player.Agility == Opponent.Agility ? Rng_G.Next(0, 2) > 0 : Player.Agility > Opponent.Agility;
            if (playerFaster)
            {
                Turns_G.Enqueue(Turn.Player);
                Turns_G.Enqueue(Turn.Opponent);
            }
            else
            {
                Turns_G.Enqueue(Turn.Opponent);
                Turns_G.Enqueue(Turn.Player);
            }
        }

        private void SetIntroMessages()
        {
            if (MatchNumber == 1)
            {
                Tracker_G.AddGame("Welcome ladies and gentlemen, to the annual IRONMAN tournament!\n" +
                    "It's a bright sunny day here, and we've got some great fighters for you this year!\n" +
                    "Here comes our first one, " + Player.Name + "! They look like they're here to win!");
            }
            else if (MatchNumber == 2)
            {
                Tracker_G.AddGame(Player.Name + " has moved on to the second round!\n" +
                    "But can they keep up their momentum??");
            }
            else if (MatchNumber == 3)
            {
                Tracker_G.AddGame("This is it, it's the final round! If " + Player.Name +
                    " wins this, they'll be crowned our new champion!");
            }
            Tracker_G.AddGame("And here's their opponent. " + Opponent.Description);
        }

        public Msg NextMsg()
        {
            if (Tracker_G.Count == 0)
            {
                NextTurn();
            }
            LastMsg = Tracker_G.Dequeue();
            return LastMsg.Value;
        }

        public void InitializeNewGame(Character playerModel)
        {
            Player = playerModel;
            Player.Refresh();

            MatchNumber = 1;
            OpponentQueue = OpponentList.GetUniqueSet(MAX_MATCHES, Rng_G);
            Opponent = OpponentQueue.Dequeue();
            Opponent.Refresh();

            InitializeTurns();
            LastMsg = null;
            Tracker_G.Clear();
            SetIntroMessages();
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
                LastMsg = null;
                Tracker_G.Clear();
                SetIntroMessages();
            }
        }

        public void RecordPlayerMove(Move move) => PlayerInput = move;

        public void NextTurn() 
        {
            if (Turns_G.Peek() == Turn.Player)
            {
                if (PlayerInput != Move.None)
                {
                    Player.QueueMove(PlayerInput);
                    PlayerInput = Move.None;
                }

                if (!Player.HasActions)
                {
                    Tracker_G.Enqueue(new(MsgType.PlayerInput, "What will " + Player.Name + " do next??")); 
                } else
                {
                    TakeTurn(Player, Opponent, Player.NextAction());
                }
            }
            else
            {
                if (!Opponent.HasActions)
                {
                    Opponent.QueueRandomMove(); // simulate input from Opponent
                }
                TakeTurn(Opponent, Player, Opponent.NextAction());
            }
        }

        private void Victory(Character victor)
        {
            if (victor == Player)
            {
                Tracker_G.AddGame("WHOA, " + Opponent.Name + " is on the ground! Looks like they have something to say.");
                Tracker_G.Enqueue(new(MsgType.OpponentDialogue, Opponent.DefeatDialogue));
                if (MatchNumber >= MAX_MATCHES)
                {
                    Tracker_G.Enqueue(new(MsgType.NewChampion, Player.Name + " is our new IRONMAN tournament champion!" +
                        " Congratulations " + Player.Name + "! You will forever be remembered as a champion- now go out and enjoy your " +
                        "newfound fame!"));
                    RecordChampion();
                } else
                {
                    Tracker_G.Enqueue(new(MsgType.PlayerVictory, Player.Name + " wins! That's the end of this match, wow that was exciting!"));
                }
            } else
            {
                Tracker_G.Enqueue(new(MsgType.Game, "WHOA, " + Player.Name + " is on the ground. That was the finishing blow!"));
                Tracker_G.Enqueue(new(MsgType.OpponentDialogue, Opponent.GetVictoryLine()));
                Tracker_G.Enqueue(new(MsgType.OpponentVictory, Opponent.Name + " wins! That's the end of this match, wow that was exciting!"));
            }
        }

        private void TakeTurn(Character actor, Character target, Move move)
        {
            actor.UpdateStatus(Tracker_G);
            if (actor.Health <= 0)
            {
                Victory(target);
            } else
            {
                int attack = actor.AttackWith(move, Tracker_G);
                target.TakeAttack(attack, move, Tracker_G);
                if (target.Health <= 0)
                {
                    Victory(actor);
                } else
                {
                    Turns_G.Enqueue(Turns_G.Dequeue());
                }
            }
        }
    }
}