namespace ConnectFour.Domain;

public class Game
{
    private readonly Board _board;
    private Player _currentPlayer;

    public Game(GameLog gameLog)
    {
        _board = Board.LoadBoardFromLog(gameLog);
        _currentPlayer = gameLog.CurrentPlayer;
    }
    
    public MoveResult MakeMove(int column)
    {
        if (!_board.DropToken(column, _currentPlayer, out var droppedRow)) return MoveResult.ColumnFull;

        if (CheckWin(droppedRow, column)) return MoveResult.Win;

        _currentPlayer = _currentPlayer is Player.Red ? Player.Yellow : Player.Red;
        return MoveResult.Ok;
    }

    private bool CheckWin(int row, int col) =>
        CheckDirection(row, col, 1, 0) ||
        CheckDirection(row, col, 0, 1) ||
        CheckDirection(row, col, 1, 1) ||
        CheckDirection(row, col, 1, -1);

    private bool CheckDirection(int row, int col, int rowDir, int colDir)
    {
        var count = 0;
        for (var i = -3; i <= 3; i++)
        {
            var newRow = row + i * rowDir;
            var newCol = col + i * colDir;

            if (!IsInside(newRow, newCol)) continue;

            if (_board.Grid[newRow, newCol] == _currentPlayer)
            {
                count++;
                if (count is 4) return true;
            }
            else count = 0;
        }

        return false;
    }

    private static bool IsInside(int newRow, int newCol) =>
        newRow is >= 0 and < Board.Rows &&
        newCol is >= 0 and < Board.Columns;
    
    public class Board
    {
        public readonly Player[,] Grid = new Player[Rows, Columns];
        public const int Rows = 6;
        public const int Columns = 7;
        
        private Board() { }
        
        public static Board LoadBoardFromLog(GameLog gameLog)
        {
            var board = new Board();
            
            var currentPlayer = Player.Red;
            foreach (var column in gameLog.Log)
            {
                board.DropToken(column, currentPlayer, out _);
                currentPlayer = currentPlayer is Player.Red ? Player.Yellow : Player.Red;
            }

            return board;
        }

        /// <summary>
        /// Drop a token in the specified column
        /// </summary>
        /// <param name="column"></param>
        /// <param name="currentPlayer"></param>
        /// <param name="droppedRow"></param>
        /// <returns>true if successfully placed false if row is full</returns>
        public bool DropToken(int column, Player currentPlayer, out int droppedRow)
        {
            droppedRow = -1;
            for (var row = Rows - 1; row >= 0; row--)
            {
                if (Grid[row, column] is Player.Empty)
                {
                    Grid[row, column] = currentPlayer;
                    droppedRow = row;
                    return true;
                }
            }
            return false;
        }
    }
}