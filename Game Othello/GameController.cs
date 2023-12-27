using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.Threading.Tasks;


namespace Othello
{
    public class GameController
    {
        #region Field
        /// <summary>
        /// total row at board
        /// </summary>
        public int Rows { get; private set; }

        /// <summary>
        /// total colomn at board
        /// </summary>
        public int Cols { get; private set; }

        /// <summary>
        /// key for disc color and value for which player wants to play
        /// </summary>
        public Dictionary<Disc, IPlayer> Players { get; private set; }

        /// <summary>212
        /// contains information about disks on the board
        /// </summary>
        public Disc[,] Board { get; private set; }

        /// <summary>
        /// get current player
        /// </summary>
        public IPlayer CurrentPlayer { get { return Players[CurrentDisc]; } }

        /// <summary>
        /// get current disc
        /// </summary>
        public Disc CurrentDisc { get; private set; }

        /// <summary>
        /// contains how many each color disc on board
        /// </summary>
        public Dictionary<Disc, int> CountDisc { get; }

        /// <summary>
        /// contain indicator for each color disc can move at current turn
        /// </summary>
        public Dictionary<Disc, int> CountPossibelMove { get; private set; }

        /// <summary>
        /// contain who player win
        /// </summary>
        public Disc Winner { get; private set; }

        /// <summary>
        /// key is position will be legal move and value is which enemy disc outflanked
        /// </summary>
        public Dictionary<Position, List<Position>> LegalMoves { get; private set; }

        /// <summary>
        /// contain game status
        /// </summary>
        public GameStatus GameStat { get; private set; }

        /// <summary>
        /// list position for enemy disc outflanked
        /// </summary>
        private List<Position> _outflanked = new List<Position>();

        /// <summary>
        /// Instance of GameController logger.
        /// </summary>
        private readonly ILogger<GameController>? _logger;

        /// <summary>
        /// delegate for log move
        /// </summary>
        public Action<IPlayer, Disc> OnPlayerUpdate;

        /// <summary>
        /// delegate for UI update after legal move on board
        /// </summary>
        public Action<Disc, Position, List<Position>, List<Position>> OnDiscUpdate;

        /// <summary>
        /// update for UI legal move position list
        /// </summary>
        public Action<List<Position>> OnPossibleMove;
        #endregion

        #region Constructor
        public GameController(ILogger<GameController>? logger = null)
        {
            _logger = logger;

            Players = new Dictionary<Disc, IPlayer>();

            Rows = 8;
            Cols = 8;

            // put disc on board
            Board = new Disc[Rows, Cols];
            Board[3, 3] = Disc.White;
            Board[3, 4] = Disc.Black;
            Board[4, 3] = Disc.Black;
            Board[4, 4] = Disc.White;

            // intial condition on early game
            CountDisc = new Dictionary<Disc, int>()
            {
                { Disc.White, 0 },
                { Disc.Black, 0 }
            };

            CountPossibelMove = new Dictionary<Disc, int>();
            CountPossibelMove[Disc.Black] = 0;
            CountPossibelMove[Disc.White] = 0;

            LegalMoves = new Dictionary<Position, List<Position>>();

            GameStat = GameStatus.NoReady;
            CurrentDisc = Disc.None;

            _logger?.LogInformation("Creating Game...");
            _logger?.LogInformation("Create Size Board {Rows}x{Cols}", Rows, Cols);
            _logger?.LogInformation("Current turn : {disc}", CurrentDisc);
            _logger?.LogInformation("Status Game : {GameStat}", GameStat);
        }
        #endregion

        #region MethodSetupGame
        /// <summary>
        /// set size board have square shape that row and col have same value. please call
        /// SetDiscOnBoard() for replacing disc on board
        /// </summary>
        /// <param name="size">size board</param>
        /// <returns>if size is even return true and opposite is return false</returns>
        public bool SetSizeBoard(int size)
        {
            if (size % 2 == 0)
            {
                Rows = size;
                Cols = size;
                Board = new Disc[Rows, Cols];
                _logger?.LogInformation("Success Recreate Size Board {Rows}x{Cols}", Rows, Cols);
                return true;
            }
            _logger?.LogWarning("Failed Recreate Size Board {size}x{size}", size, size);
            return false;
        }

        /// <summary>
        /// placing a disc into a specific empty position. if placing disc on position
        /// has been exit another disc arbitrary color will be failed placing disc. if placing
        /// disc which Disc.None will be failed placing disc.
        /// </summary>
        /// <param name="disc">color disc, dont input Disc.None</param>
        /// <param name="position">disire position placing disc</param>
        /// <returns>if placing disc success return true</returns>
        public bool SetDiscOnBoard(Disc disc, Position position)
        {
            if (Board[position.Row, position.Col] == Disc.None && disc != Disc.None)
            {
                Board[position.Row, position.Col] = disc;
                _logger?.LogInformation("Succes Replacing Disc {disc} on ({row},{col})", disc, position.Row, position.Col);
                return true;
            }
            else
            {
                _logger?.LogWarning("Failed Replacing Disc {disc} on ({row},{col})", disc, position.Row, position.Col);
                return false;
            }
        }

        /// <summary>
        /// removing disc has been placing on board. if try removing at empty disc will be failed
        /// remove
        /// </summary>
        /// <param name="desire disc position want to remove"></param>
        /// <returns>if success remove will return true</returns>
        public bool SetRemoveDiscOnBoard(Position position)
        {
            if (Board[position.Row, position.Col] != Disc.None)
            {
                Board[position.Row, position.Col] = Disc.None;
                _logger?.LogInformation("Success Removed Disc on ({row},{col})", position.Row, position.Col);
                return true;
            }
            else
            {
                _logger?.LogWarning("Failed Removed Disc on ({row},{col})", position.Row, position.Col);
                return false;
            }
        }

        /// <summary>
        /// add player into game and pick color disc. player can be added if game stasus if NoReady. 
        /// if the color has been choosen and try add again with another player will result recent 
        /// player that will pick that color. dont input Disc.None will failed add player. player
        /// have same id other player and want to add will failed add that player
        /// </summary>
        /// <param name="player">player want added</param>
        /// <param name="disc">select color disc</param>
        /// <returns>if add player succes return true</returns>
        public bool AddPlayer(IPlayer player, Disc disc)
        {
            if (GameStat == GameStatus.NoReady && disc != Disc.None)
            {
                if (Players.Count > 0 && Players[disc.Opponent()].Id != player.Id)
                {
                    Players[disc] = player;
                    _logger?.LogInformation("Success Added Player {name}-{id} with a {disc} disc ", player.Name, player.Id, disc);
                    return true;
                }
                else if (Players.Count == 0)
                {
                    Players[disc] = player;
                    _logger?.LogInformation("Success Added Player {name}-{id} with a {disc} disc ", player.Name, player.Id, disc);
                    return true;
                }
                else
                {
                    _logger?.LogWarning("Failed Added Player {name}-{id} with a {disc} disc", player.Name, player.Id, disc);
                    return false;
                }
            }
            else
            {
                _logger?.LogWarning("Failed Added Player {name}-{id} with a {disc} disc", player.Name, player.Id, disc);
                return false;
            }
        }

        /// <summary>
        /// checking this player pick color disc. if the player dont add to the game and
        /// checking with this method will be return Disc.None
        /// </summary>
        /// <param name="player">player want to check</param>
        /// <returns>return color disc</returns>
        public Disc CheckPlayer(IPlayer player)
        {
            foreach (var disc in Players)
            {
                if (disc.Value == player)
                {
                    _logger?.LogInformation("Player {name}-{id} have {disc} disc", player.Name, player.Id, disc.Key);
                    return disc.Key;
                }
            }
            _logger?.LogWarning("Player {name}-{id} have not {disc} disc", player.Name, player.Id, Disc.None);
            return Disc.None;
        }

        /// <summary>
        /// check this disc selected by who player.
        /// </summary>
        /// <param name="disc">check disc</param>
        /// <returns>return player</returns>
        public IPlayer CheckDisc(Disc disc)
        {
            if (Players.ContainsKey(disc))
            {
                _logger?.LogInformation("Player {name}-{id} pick {disc} disc", Players[disc].Name, Players[disc].Id, disc);
                return Players[disc];
            }
            else
            {
                _logger?.LogWarning("No one Player pick {disc} disc", disc);
                return null;
            }
        }

        /// <summary>
        /// set who first turn, this mathod will true if game status is NoReady, Player registered
        /// more than 1, and disc not None
        /// </summary>
        /// <param name="disc">disc color</param>
        /// <returns>if success will return true;</returns>
        public bool SetIntialTurn(Disc disc)
        {
            if (GameStat == GameStatus.NoReady && Players.Count > 1 && disc != Disc.None)
            {
                CurrentDisc = disc;
                _logger?.LogInformation("Success set intial turn, current turn : {disc}", disc);
                return true;
            }
            else
            {
                _logger?.LogWarning("Failed Set intial turn {disc} disc", disc);
                return false;
            }
        }

        /// <summary>
        /// change status game. satatus game will be change to Start if condition players
        /// more than 1 and current disc not None. game status is on going if count disc 
        /// each color more than 0
        /// </summary>
        /// <returns>status game recent</returns>
        private GameStatus StatusGame()
        {
            // check untuk mid game
            if (CountDisc[Disc.Black] > 0 || CountDisc[Disc.White] > 0)
            {
                GameStat = GameStatus.OnGoing;
                _logger?.LogInformation("Status Game : {GameStat}", GameStat);
                return GameStat;
            }
            // check untuk awal game
            else if (CurrentDisc != Disc.None && Players.Count > 1)
            {
                GameStat = GameStatus.Start;
                _logger?.LogInformation("Status Game : {GameStat}", GameStat);
                return GameStat;
            }
            else
            {
                _logger?.LogInformation("Status Game : {GameStat}", GameStat);
                return GameStat;
            }
        }

        /// <summary>
        /// strating game and status game will be change to be Start.
        /// </summary>
        /// <returns>game will start and return true</returns>
        public bool StartGame()
        {
            if (StatusGame() == GameStatus.Start)
            {
                CountDisc[Disc.White] = 2;
                CountDisc[Disc.Black] = 2;
                GameStat = GameStatus.Start;
                _logger?.LogInformation("Game can strated, status game : {GameStat}", GameStat);
                return true;
            }
            else
            {
                _logger?.LogWarning("Game can not strated, status game : {GameStat}", GameStat);
                return false;
            }
        }

        /// <summary>
        /// get current player at this turn
        /// </summary>
        /// <returns>return current player</returns>
        public IPlayer GetCurrentPlayer()
        {
            if (CurrentDisc != Disc.None)
            {
                _logger?.LogInformation("Success get current turn is Player {name}-{id}", Players[CurrentDisc].Name, Players[CurrentDisc].Id);
                return Players[CurrentDisc];
            }
            else
            {
                _logger?.LogWarning("Failed get current turn");
                return null;
            }
        }

        /// <summary>
        /// get current disc at this turn
        /// </summary>
        /// <returns>return color disc</returns>
        public Disc GetCurrentDisc()
        {
            _logger?.LogInformation("Succes get current disc is {disc} disc", CurrentDisc);
            return CurrentDisc;
        }

        /// <summary>
        /// force change current turn automatically
        /// </summary>
        /// <returns>return true if force change success</returns>
        public bool ChangeTurn()
        {
            if (CurrentDisc != Disc.None)
            {
                CurrentDisc = CurrentDisc.Opponent();
                _logger?.LogInformation("Success change force turn, current turn : {disc}", CurrentDisc);
                return true;
            }
            else
            {
                _logger?.LogWarning("Failed chang force turn");
                return false;
            }
        }

        /// <summary>
        /// force change current turn to desire color disc. force change will failed
        /// if desire disc is None
        /// </summary>
        /// <param name="disc">desire color disc to be current turn</param>
        /// <returns>return true if force change success</returns>
        public bool ChangeTurn(Disc disc)
        {
            if (CurrentDisc != Disc.None && disc != Disc.None)
            {
                CurrentDisc = disc;
                _logger?.LogInformation("Success change force turn, current turn : {disc}", disc);
                return true;
            }
            else
            {
                _logger?.LogWarning("Failed chang force turn");
                return false;
            }
        }

        /// <summary>
        /// force change current turn to desire player. force change will failed if
        /// palyer dont add into game
        /// </summary>
        /// <param name="player">desire player to be current turn</param>
        /// <returns>return true if force change success</returns>
        public bool ChangeTurn(IPlayer player)
        {
            if (Players.ContainsValue(player))
            {
                CurrentDisc = Players.FirstOrDefault(x => x.Value == player).Key;
                _logger?.LogInformation("Success change force turn, current turn : {disc}", CurrentDisc);
                return true;
            }
            else
            {
                _logger?.LogWarning("Failed chang force turn");
                return false;
            }
        }

        /// <summary>
        /// get next turn
        /// </summary>
        /// <returns>return player that next turn</returns>
        public IPlayer GetNextTurn()
        {
            if (CurrentDisc != Disc.None && Players.Count > 1)
            {
                _logger?.LogInformation("Success get next turn for player {name}-{id}", Players[CurrentDisc.Opponent()].Name, Players[CurrentDisc.Opponent()].Id);
                return Players[CurrentDisc.Opponent()];
            }
            else
            {
                _logger?.LogWarning("Failed get next turn");
                return null;
            }
        }
        #endregion

        #region MethodMidGame
        /// <summary>
        /// checking if position worth to test
        /// </summary>
        /// <param name="r">row</param>
        /// <param name="c">col</param>
        /// <returns>if worth it return true</returns>
        private bool IsInsideBoard(int r, int c)
        {
            return r >= 0 && r < Rows && c >= 0 && c < Cols;
        }

        /// <summary>
        /// checking tile all direction and get list position that enemy disc is outflanked
        /// </summary>
        /// <param name="pos">position candidate legal move</param>
        /// <param name="disc">color disc</param>
        /// <param name="rDelta">direction checking row</param>
        /// <param name="cDelta">direction checking col</param>
        /// <returns>return list of position enemy disc outflanked</returns>
        private List<Position> OutflankedInDir(Position pos, Disc disc, int rDelta, int cDelta)
        {
            List<Position> outflanked = new List<Position>();
            int r = pos.Row + rDelta;
            int c = pos.Col + cDelta;

            while (IsInsideBoard(r, c) && Board[r, c] != Disc.None)
            {
                if (Board[r, c] == disc.Opponent())
                {
                    outflanked.Add(new Position(r, c));
                    r += rDelta;
                    c += cDelta;
                }
                else
                {
                    return outflanked;
                }
            }

            return new List<Position>();
        }


        /// <summary>
        /// do looping check position candidate legal move to all direction
        /// </summary>
        /// <param name="pos">position candidate legal move</param>
        /// <param name="disc">color disc</param>
        /// <returns>return list of position enemy disc all direction</returns>
        private List<Position> Outflanked(Position pos, Disc disc)
        {
            List<Position> outflanked = new List<Position>();

            for (int rDelta = -1; rDelta <= 1; rDelta++)
            {
                for (int cDelta = -1; cDelta <= 1; cDelta++)
                {
                    if (rDelta == 0 && cDelta == 0)
                    {
                        continue;
                    }
                    else
                    {
                        outflanked.AddRange(OutflankedInDir(pos, disc, rDelta, cDelta));
                    }
                }
            }

            return outflanked;
        }

        /// <summary>
        /// checking try one position candidate legel move
        /// </summary>
        /// <param name="disc">color disc</param>
        /// <param name="position">position candidate legal move</param>
        /// <returns>if it legal move return true</returns>
        public bool IsMoveLegal(Disc disc, Position position)
        {
            if (Board[position.Row, position.Col] != Disc.None)
            {
                _logger?.LogWarning("Ilegal move at position ({row},{col})", position.Row, position.Col);
                return false;
            }
            else
            {
                _outflanked = Outflanked(position, disc);
                _logger?.LogInformation("Legal move at position ({row},{col})", position.Row, position.Col);
                return _outflanked.Count > 0;
            }
        }

        /// <summary>
        /// find all legal moves
        /// </summary>
        /// <returns>return list legal move and which enemy disc outflanked</returns>
        public Dictionary<Position, List<Position>> FindLegalMoves()
        {
            LegalMoves = new Dictionary<Position, List<Position>>();

            LoopFindLegalMove(Rows - 1, Cols - 1, Cols - 1);

            CountPossibelMove[CurrentDisc] = LegalMoves.Count;
            OnPossibleMove.Invoke(LegalMoves.Keys.ToList());
            _logger?.LogInformation("Total legal move is {totalLegalMove}", LegalMoves.Count);
            return LegalMoves;
        }

        /// <summary>
        /// looping  for find legal move
        /// </summary>
        /// <param name="row">rows board</param>
        /// <param name="col">cols board</param>
        /// <param name="colReset">cols board to reset for looping</param>
        private void LoopFindLegalMove(int row, int col, int colReset)
        {
            Position position = new Position(row, col);
            if (IsMoveLegal(CurrentDisc, position) && (GameStat == GameStatus.Start || GameStat == GameStatus.OnGoing))
            {
                LegalMoves[position] = _outflanked;
            }

            if (col >= 1)
            {
                LoopFindLegalMove(row, col - 1, colReset);
            }
            else if (row > 0)
            {
                if (col == 0)
                {
                    col = colReset;
                }
                LoopFindLegalMove(row - 1, col, colReset);
            }
        }

        /// <summary>
        /// get all legal moves
        /// </summary>
        /// <returns>return list of legal moves</returns>
        public List<Position> GetLegalMove()
        {
            _logger?.LogInformation("Success get list of legal move");
            return LegalMoves.Keys.ToList();
        }

        /// <summary>
        /// do try move is desire position is legal move or not
        /// </summary>
        /// <param name="position">desire position to move</param>
        /// <returns>if desire position legal move return true</returns>
        public bool TryMove(Position position)
        {
            foreach (var item in GetLegalMove())
            {
                if (position.Row == item.Row && position.Col == item.Col)
                {
                    _logger?.LogInformation("Position at ({row},{col} can be moved", position.Row, position.Col);
                    return true;
                }
            }

            _logger?.LogWarning("Position at ({row},{col} can not be moved", position.Row, position.Col);
            return false;
        }

        /// <summary>
        /// do make move. if move is not legal will return false; 
        /// </summary>
        /// <param name="position">desire position legal move</param>
        /// <returns>if success move return true</returns>
        public bool MakeMove(Position position)
        {
            StatusGame();
            if (!LegalMoves.ContainsKey(position))
            {
                _logger?.LogWarning("Position at ({row},{col} can not be moved", position.Row, position.Col);
                return false;
            }

            _outflanked = LegalMoves[position];
            Board[position.Row, position.Col] = CurrentDisc;
            _logger?.LogInformation("Position at ({row},{col} can be moved", position.Row, position.Col);
            FlipDiscs();
            UpdateCountDisc(_outflanked.Count);
            OnDiscUpdate.Invoke(CurrentDisc, position, GetLegalMove(), _outflanked);


            return true;
        }

        /// <summary>
        /// flip all enemy disc capture
        /// </summary>
        private void FlipDiscs()
        {
            foreach (Position pos in _outflanked)
            {
                Board[pos.Row, pos.Col] = Board[pos.Row, pos.Col].Opponent();
                _logger?.LogInformation("Position disc at ({row},{col}) has been changed", pos.Row, pos.Col);
            }
        }

        /// <summary>
        /// update count disc each color
        /// </summary>
        /// <param name="outflanked">total outflanked enemy disc</param>
        private void UpdateCountDisc(int outflanked)
        {
            CountDisc[CurrentDisc] += outflanked + 1;
            CountDisc[CurrentDisc.Opponent()] -= outflanked;
            _logger?.LogInformation("total {disc} disc is {totalDisc}", CurrentDisc, outflanked);
        }
        #endregion

        #region MethodLastGame
        /// <summary>
        /// find winner
        /// </summary>
        /// <returns>return color disc who winner</returns>
        private Disc FindWinner()
        {
            if (CountDisc[Disc.Black] > CountDisc[Disc.White])
            {
                return Disc.Black;
            }
            else if (CountDisc[Disc.Black] < CountDisc[Disc.White])
            {
                return Disc.White;
            }
            else
            {
                return Disc.None;
            }
            _logger?.LogInformation("Finding winner");
        }

        /// <summary>
        /// do change turn normally and check this game is end or not
        /// </summary>
        /// <returns>if game can continue return true</returns>
        public bool PassTurn()
        {
            if (CountPossibelMove[Disc.White] > 0 || CountPossibelMove[Disc.Black] > 0)
            {
                CurrentDisc = CurrentDisc.Opponent();
                _logger?.LogInformation("Game continue, next turn is {disc} disc", CurrentDisc);
                return true;
            }
            else
            {
                CurrentDisc = Disc.None;
                Winner = FindWinner();
                GameStat = GameStatus.End;
                _logger?.LogInformation("{disc} disc winner", Winner);
                _logger?.LogInformation("Game end, status game : {gameStat}", GameStat);
                return false;
            }
        }
        #endregion

    }
}