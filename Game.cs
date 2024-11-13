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

    public readonly record struct ClientState(string Theme, bool PlayMusic);

    public class Game(IServiceScopeFactory scopeFactory)
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

        private readonly Queue<Turn> _turns = new();
        private readonly MsgTracker _tracker = new();
        private static readonly Random _rng = new();

        public ClientState? StoredState;
        public Msg? LastMsg { get; private set; }
        public Move PlayerInput = Move.None;

        private const int MAX_MATCHES = 3;
        private int _matchNumber = 1;

        private readonly CharacterList Characters = new();
        private Queue<Character> OpponentQueue = new();

        private Character Player = new();
        private Character Opponent = new();
        public (Character, Character) PlayerAndOpponent => (Player, Opponent);

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
            _turns.Clear();
            bool playerFaster = Player.Agility == Opponent.Agility ? _rng.Next(0, 2) > 0 : Player.Agility > Opponent.Agility;
            if (playerFaster)
            {
                _turns.Enqueue(Turn.Player);
                _turns.Enqueue(Turn.Opponent);
            }
            else
            {
                _turns.Enqueue(Turn.Opponent);
                _turns.Enqueue(Turn.Player);
            }
        }

        private void SetIntroMessages()
        {
            if (_matchNumber == 1)
            {
                _tracker.AddGame("Welcome ladies and gentlemen, to the annual IRONMAN tournament!\n" +
                    "It's a bright sunny day here, and we've got some great fighters for you this year!\n" +
                    "Here comes our first one, " + Player.Name + "! They look like they're here to win!");
            }
            else if (_matchNumber == 2)
            {
                _tracker.AddGame(Player.Name + " has moved on to the second round!\n" +
                    "But can they keep up their momentum??");
            }
            else if (_matchNumber == 3)
            {
                _tracker.AddGame("This is it, it's the final round! If " + Player.Name +
                    " wins this, they'll be crowned our new champion!");
            }
            _tracker.AddGame("And here's their opponent. " + Opponent.Description);
        }

        public Msg NextMsg()
        {
            if (_tracker.Count == 0)
            {
                NextTurn();
            }
            LastMsg = _tracker.Dequeue();
            return LastMsg.Value;
        }

        public void StartNextMatch(Character? newPlayer = null)
        {
            if (newPlayer is not null)
            {
                Player = newPlayer;
                _matchNumber = 1;
                OpponentQueue = Characters.GetUniqueSet(MAX_MATCHES, _rng);
            } else
            {
                _matchNumber++;
            }
            
            if (_matchNumber <= MAX_MATCHES)
            {
                Player.Refresh();
                Opponent = OpponentQueue.Dequeue();
                Opponent.Refresh();

                InitializeTurns();
                LastMsg = null;
                _tracker.Clear();
                SetIntroMessages();
            }
        }

        public void NextTurn() 
        {
            if (_turns.Peek() == Turn.Player)
            {
                if (PlayerInput != Move.None)
                {
                    Player.QueueMove(PlayerInput);
                    PlayerInput = Move.None;
                }

                if (!Player.HasActions)
                {
                    _tracker.Enqueue(new(MsgType.PlayerInput, "What will " + Player.Name + " do next??")); 
                } else
                {
                    TakeTurn(Player, Opponent, Player.NextAction());
                }
            }
            else
            {
                if (!Opponent.HasActions)
                {
                    Opponent.QueueRandomMove(_rng); // simulate input from Opponent
                }
                TakeTurn(Opponent, Player, Opponent.NextAction());
            }
        }

        private void Victory(Character victor)
        {
            if (victor == Player)
            {
                _tracker.AddGame("WHOA, " + Opponent.Name + " is on the ground! Looks like they have something to say.");
                _tracker.Enqueue(new(MsgType.OpponentDialogue, Opponent.DefeatDialogue));
                if (_matchNumber >= MAX_MATCHES)
                {
                    _tracker.Enqueue(new(MsgType.NewChampion, Player.Name + " is our new IRONMAN tournament champion!" +
                        " Congratulations " + Player.Name + "! You will forever be remembered as a champion- now go out and enjoy your " +
                        "newfound fame!"));
                    RecordChampion();
                } else
                {
                    _tracker.Enqueue(new(MsgType.PlayerVictory, Player.Name + " wins! That's the end of this match, wow that was exciting!"));
                }
            } else
            {
                _tracker.Enqueue(new(MsgType.Game, "WHOA, " + Player.Name + " is on the ground. That was the finishing blow!"));
                _tracker.Enqueue(new(MsgType.OpponentDialogue, Opponent.GetVictoryLine()));
                _tracker.Enqueue(new(MsgType.OpponentVictory, Opponent.Name + " wins! That's the end of this match, wow that was exciting!"));
            }
        }

        private void TakeTurn(Character actor, Character target, Move move)
        {
            actor.UpdateStatus(_tracker);
            if (actor.Health <= 0)
            {
                Victory(target);
            } else
            {
                int attack = actor.AttackWith(move, _tracker, _rng);
                target.TakeAttack(attack, move, _tracker, _rng);
                if (target.Health <= 0)
                {
                    Victory(actor);
                } else
                {
                    _turns.Enqueue(_turns.Dequeue());
                }
            }
        }
    }
}