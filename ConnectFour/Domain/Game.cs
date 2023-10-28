namespace ConnectFour.Domain;

public class Game
{
    private readonly Board _board;
    private PlayerColor _currentPlayerColor;

    public Game(GameLog gameLog)
    {
        _board = Board.LoadBoardFromLog(gameLog);
        _currentPlayerColor = gameLog.CurrentPlayerColor;
    }
    
    public MoveResult MakeMove(int column)
    {
        if (!_board.DropToken(column, _currentPlayerColor, out var droppedRow)) return MoveResult.ColumnFull;

        if (CheckWin(droppedRow, column)) return MoveResult.Win;

        _currentPlayerColor = _currentPlayerColor is PlayerColor.Red ? PlayerColor.Yellow : PlayerColor.Red;
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

            if (_board.Grid[newRow, newCol] == _currentPlayerColor)
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
        public readonly PlayerColor[,] Grid = new PlayerColor[Rows, Columns];
        public const int Rows = 6;
        public const int Columns = 7;
        
        private Board() { }
        
        public static Board LoadBoardFromLog(GameLog gameLog)
        {
            var board = new Board();
            
            var currentPlayer = PlayerColor.Red;
            foreach (var column in gameLog.Log)
            {
                board.DropToken(column, currentPlayer, out _);
                currentPlayer = currentPlayer is PlayerColor.Red ? PlayerColor.Yellow : PlayerColor.Red;
            }

            return board;
        }

        /// <summary>
        /// Drop a token in the specified column
        /// </summary>
        /// <param name="column"></param>
        /// <param name="currentPlayerColor"></param>
        /// <param name="droppedRow"></param>
        /// <returns>true if successfully placed false if row is full</returns>
        public bool DropToken(int column, PlayerColor currentPlayerColor, out int droppedRow)
        {
            droppedRow = -1;
            for (var row = Rows - 1; row >= 0; row--)
            {
                if (Grid[row, column] is PlayerColor.Empty)
                {
                    Grid[row, column] = currentPlayerColor;
                    droppedRow = row;
                    return true;
                }
            }
            return false;
        }
    }
}