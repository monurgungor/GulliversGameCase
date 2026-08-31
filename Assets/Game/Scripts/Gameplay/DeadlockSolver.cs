using System.Collections.Generic;

/// <summary>
/// Answers the only question the board asks after a word is submitted: can the
/// player still build a word that has not been played yet?
///
/// The search walks the free tiles depth first and drops any branch whose
/// letters no longer start a word, which is what keeps a fifty tile board cheap.
/// It mutates one set of arrays and undoes every step on the way back out, so a
/// whole search allocates nothing after the first level is loaded.
/// </summary>
public sealed class DeadlockSolver
{
    /// <summary>
    /// Upper bound on visited states. A real board settles in a few thousand,
    /// so hitting this means the search cannot prove anything either way, and
    /// the board is reported as playable rather than ending the level wrongly.
    /// </summary>
    private const int NodeBudget = 200000;

    private readonly WordDictionary dictionary;

    private char[] characters = new char[0];
    private int[][] children = new int[0][];
    private bool[] onBoard = new bool[0];
    private int[] coveredBy = new int[0];
    private char[] word = new char[0];

    private readonly Dictionary<int, int> indexById = new Dictionary<int, int>();

    private int tileCount;
    private int wordLength;
    private int nodesVisited;
    private ICollection<string> alreadyPlayed;

    public DeadlockSolver(WordDictionary dictionary)
    {
        this.dictionary = dictionary;
    }

    /// <summary>True when the search ran out of budget instead of finishing.</summary>
    public bool LastSearchWasCutShort { get; private set; }

    /// <summary>
    /// Looks for one playable word on the given board.
    /// </summary>
    /// <param name="board">Tiles still on the board, keyed by tile id.</param>
    /// <param name="maxWordLength">Letter slots available, normally <see cref="BoardRules.SlotCount"/>.</param>
    /// <param name="alreadyPlayed">Words the player has already submitted this level.</param>
    /// <param name="found">One word the player could still build.</param>
    public bool TryFindWord(IReadOnlyDictionary<int, TileData> board, int maxWordLength,
                            ICollection<string> alreadyPlayed, out string found)
    {
        found = null;
        LastSearchWasCutShort = false;

        if (dictionary == null || board == null || board.Count == 0 || maxWordLength <= 0)
        {
            return false;
        }

        Load(board);

        this.alreadyPlayed = alreadyPlayed;
        wordLength = 0;
        nodesVisited = 0;

        if (word.Length < maxWordLength)
        {
            word = new char[maxWordLength];
        }

        bool solved = Search(maxWordLength);
        LastSearchWasCutShort = !solved && nodesVisited >= NodeBudget;

        if (solved)
        {
            found = new string(word, 0, wordLength);
        }

        this.alreadyPlayed = null;
        return solved;
    }

    /// <summary>
    /// Copies the board into flat arrays indexed from zero. Cover counts are
    /// rebuilt here rather than passed in, so the search can never disagree with
    /// the board about which tiles are free.
    /// </summary>
    private void Load(IReadOnlyDictionary<int, TileData> board)
    {
        tileCount = board.Count;

        if (characters.Length < tileCount)
        {
            characters = new char[tileCount];
            children = new int[tileCount][];
            onBoard = new bool[tileCount];
            coveredBy = new int[tileCount];
        }

        indexById.Clear();

        int next = 0;
        foreach (var entry in board)
        {
            indexById[entry.Key] = next++;
        }

        next = 0;
        foreach (var entry in board)
        {
            characters[next] = entry.Value.Character;
            onBoard[next] = true;
            coveredBy[next] = 0;
            children[next] = null;
            next++;
        }

        next = 0;
        foreach (var entry in board)
        {
            int[] childIds = entry.Value.Children;
            int covered = 0;

            if (childIds != null)
            {
                for (int i = 0; i < childIds.Length; i++)
                {
                    if (indexById.ContainsKey(childIds[i]))
                    {
                        covered++;
                    }
                }
            }

            int[] childIndices = new int[covered];
            int write = 0;

            if (childIds != null)
            {
                for (int i = 0; i < childIds.Length; i++)
                {
                    int childIndex;
                    if (indexById.TryGetValue(childIds[i], out childIndex))
                    {
                        childIndices[write++] = childIndex;
                        coveredBy[childIndex]++;
                    }
                }
            }

            children[next] = childIndices;
            next++;
        }
    }

    private bool Search(int maxWordLength)
    {
        if (++nodesVisited > NodeBudget)
        {
            return false;
        }

        if (wordLength > 0 && dictionary.Contains(word, wordLength) && !HasBeenPlayed())
        {
            return true;
        }

        if (wordLength >= maxWordLength)
        {
            return false;
        }

        for (int tile = 0; tile < tileCount; tile++)
        {
            if (!onBoard[tile] || coveredBy[tile] > 0)
            {
                continue;
            }

            word[wordLength++] = characters[tile];

            if (dictionary.HasWordWithPrefix(word, wordLength))
            {
                onBoard[tile] = false;
                Uncover(tile, -1);

                bool solved = Search(maxWordLength);

                Uncover(tile, 1);
                onBoard[tile] = true;

                if (solved)
                {
                    return true;
                }
            }

            wordLength--;
        }

        return false;
    }

    private void Uncover(int tile, int delta)
    {
        int[] childIndices = children[tile];

        for (int i = 0; i < childIndices.Length; i++)
        {
            coveredBy[childIndices[i]] += delta;
        }
    }

    private bool HasBeenPlayed()
    {
        return alreadyPlayed != null && alreadyPlayed.Contains(new string(word, 0, wordLength));
    }
}
