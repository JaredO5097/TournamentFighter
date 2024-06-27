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
        private readonly Queue<MessageModel> Messages = new Queue<MessageModel>();
        private readonly Queue<Actor> Turns = new Queue<Actor>();

        private static readonly System.Random rng = new System.Random();

        public Character Player { get; private set; } = CharacterList.Default;
        public Character Opponent { get; private set; } = CharacterList.Default;

        public void SetUpGame(Character playerModel)
        {
            Player = playerModel;
            Player.Moves = [Move.Punch, Move.SwordSlash, Move.JumpKick, Move.Counter];
            Character[] characters = CharacterList.ToArray();
            Opponent = GetRandom(ref characters);
            Messages.Clear();
            Turns.Clear();
            Messages.Enqueue(new("Let's begin!\n" +
                "You enter the sandy arena. You see a clear blue sky above, and feel warm sunlight on your skin.\n" +
                "Opposite you stands " + Opponent.Description, Actor.Game, Move.None));
            Messages.Enqueue(new(Opponent.OpeningDialogue, Actor.Opponent, Move.None));

            if (FirstIsFaster(Player, Opponent))
            {
                Turns.Enqueue(Actor.Player);
                Turns.Enqueue(Actor.Opponent);
            } else
            {
                Turns.Enqueue(Actor.Opponent);
                Turns.Enqueue(Actor.Player);
            }
        }

        private static T GetRandom<T>(ref readonly T[] array) => array[rng.Next(0, array.Length)];

        private static bool FirstIsFaster(in Character a, in Character b) =>
            a.Agility == b.Agility ? rng.Next(0, 2) > 0 : a.Agility > b.Agility;

        public MessageModel Next() 
        {
            if (Messages.Count == 0)
            {
                if (Turns.Peek() == Actor.Player)
                {
                    if (Player.HasActions)
                    {
                        PlayerTurn();
                    } else
                    {
                        Messages.Enqueue(MessageModel.PlayerTurn);
                    }
                    Move next = Player.CheckNextAction();
                    if (next == Move.None)
                    {
                        
                    } else
                    {
                        PlayerTurn(next);
                    }
                } else
                {
                    OpponentTurn();
                }
            }
            return Messages.Dequeue();
        }

        public void PlayerTurn()
        {

        }
        public void PlayerTurn(Move move) => TakeTurn(Actor.Player, move);

        private void OpponentTurn() => TakeTurn(Actor.Opponent, GetRandom(ref Opponent.Moves));

        public void TakeTurn(Actor upNext, Move move)
        {
            Character actor = upNext == Actor.Player ? Player : Opponent;
            Character target = upNext == Actor.Player ? Opponent : Player;

            actor.UpdateStatuses();
            move.Action(actor, target);

            Move actorAction = actor.NextAction();
            if (actorAction != Move.None)
            {
                int attack = actor.AttackWith(move);
                int damage = target.TakeAttack(attack);

                Messages.Enqueue(new(actor.Name + " " + move.Messages[0], upNext, move));
                Messages.Enqueue(new(target.Name + " takes " + damage + " damage!", upNext, move));
                if (target.Health <= 0)
                {
                    Messages.Enqueue(new(target.Name + " has been defeated. " + actor.Name + " wins!", upNext, move));
                    Messages.Enqueue(MessageModel.GameOver);
                }
            } else
            {
                Messages.Enqueue(new(actor.Name + " does nothing.", upNext, Move.None));
            }

            Turns.Enqueue(Turns.Dequeue());
        }
    }
}