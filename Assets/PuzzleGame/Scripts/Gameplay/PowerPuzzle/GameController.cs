using System;
using System.Collections.Generic;
using PuzzleGame.Sounds;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PuzzleGame.Gameplay.PowerPuzzle
{
    public class GameController : BaseGameController<GameState>
    {
        [Header("Mode fields")]
        public int bricksToMerge = 4;
        public int nextBricksCount = 3;
        public Brick emptyBrickPrefab;

        public Transform nextBricksParent;

        public event Action NoPath;

        NumberedBrick currentBrick;
        Vector2Int currentBrickCoords;

        NumberedBrick[] nextBricks;

        bool isAnimating;

        class BrickPath
        {
            public NumberedBrick brick;
            public List<Vector2Int> path;
        }

        int GetRandomNumber()
        {
            return Mathf.RoundToInt(Mathf.Pow(2f, Random.Range(0, 4)));
        }

        int GetColorIndex(int number)
        {
            return Mathf.RoundToInt(Mathf.Log(number, 2f));
        }

        void Start()
        {
            field = new NumberedBrick[bricksCount.x, bricksCount.y];

            for (int x = 0; x < bricksCount.x; x++)
            {
                for (int y = 0; y < bricksCount.y; y++)
                {
                    SpawnEmptyBrick(new Vector2Int(x, y));
                }
            }
        
            gameState = UserProgress.Current.GetGameState<GameState>(name);

            if (gameState == null)
            {
                gameState = new GameState();
                UserProgress.Current.SetGameState(name, gameState);
            }

            UserProgress.Current.CurrentGameId = name;

            StartGame();
        }

        protected override void StartGame()
        {
            if (LoadGame())
                return;

            gameState.Score = 0;
            gameState.IsGameOver = false;
            SpawnStartingBricks();
            SpawnNextBricks();
            SetStartBoosters();
        
            SaveGame();
        }

        void SpawnStartingBricks()
        {
            List<Vector2Int> positions = new List<Vector2Int>();
            for (int i = 0; i < bricksCount.x; i++)
            {
                for (int j = 0; j < bricksCount.y; j++)
                {
                    positions.Add(new Vector2Int(i, j));
                }
            }

            for (int i = 1; i <= 5; i++)
            {
                int rand = Random.Range(0, positions.Count);
                int number = Mathf.RoundToInt(Mathf.Pow(2, i));
                SpawnBrick(positions[rand], number);
                positions.RemoveAt(rand);
            }
        }

        private bool LoadGame()
        {
            if (gameState == null || gameState.IsGameOver)
                return false;

            int[] numbers = gameState.GetField();
            if (numbers == null || numbers.Length != bricksCount.x * bricksCount.y)
                return false;

            for (int x = 0; x < bricksCount.x; x++)
            {
                for (int y = 0; y < bricksCount.y; y++)
                {
                    if (numbers[x * bricksCount.y + y] > 0)
                        SpawnBrick(new Vector2Int(x, y), numbers[x * bricksCount.y + y]);
                }
            }

            int[] nextNumbers = gameState.GetNextBricks();
            SpawnNextBricks(nextNumbers);
        
            if (IsFieldEmpty())
                SpawnNewBricks(SaveGame);
        
            return true;
        }

        protected override void SaveGame()
        {
            int[] numbers = new int[bricksCount.x * bricksCount.y];
            for (int x = 0; x < bricksCount.x; x++)
            {
                for (int y = 0; y < bricksCount.y; y++)
                {
                    numbers[x * bricksCount.y + y] = field[x, y] != null ? field[x, y].Number : 0;
                }
            }

            int[] nextNumbers = new int[nextBricks.Length];

            for (int i = 0; i < nextBricks.Length; i++)
                nextNumbers[i] = nextBricks[i].Number;

            gameState.SetField(numbers);
            gameState.SetNextBricks(nextNumbers);
            UserProgress.Current.SaveGameState(name);
        }

        void SpawnBrick(Vector2Int coords, int number)
        {
            NumberedBrick brick = Instantiate(brickPrefab, fieldTransform);

            brick.transform.SetParent(fieldTransform, false);
            brick.RectTransform.anchorMin = Vector2.zero;
            brick.RectTransform.anchorMax = Vector2.zero;
            brick.RectTransform.anchoredPosition = GetBrickPosition(new Vector2(coords.x, coords.y));

            brick.Number = number;
            brick.ColorIndex = GetColorIndex(number);

            field[coords.x, coords.y] = brick;

            brick.PointerClick += OnNumberedBrickClick;
        }

        void SpawnEmptyBrick(Vector2Int coords)
        {
            Brick brick = Instantiate(emptyBrickPrefab, fieldTransform);

            brick.transform.SetParent(fieldTransform, false);
            brick.RectTransform.anchorMin = Vector2.zero;
            brick.RectTransform.anchorMax = Vector2.zero;
            brick.RectTransform.anchoredPosition = GetBrickPosition(new Vector2(coords.x, coords.y));

            brick.PointerClick += b => OnEmptyBrickClick(b, coords);
        }

        void SpawnNextBricks(int[] values = null)
        {
            nextBricks = new NumberedBrick[nextBricksCount];
            for (int i = 0; i < nextBricksCount; i++)
            {
                NumberedBrick brick = Instantiate(brickPrefab, nextBricksParent);
                brick.Number = values == null || values.Length <= i ? GetRandomNumber() : values[i];
                brick.ColorIndex = GetColorIndex(brick.Number);
                nextBricks[i] = brick;
            }
        }

        void OnNumberedBrickClick(Brick brick)
        {
            if (isBoosterSelected)
            {
                OnHighlightedTargetClick(brick);
                return;
            }

            if (isAnimating || brick == currentBrick)
                return;

            if (currentBrick != null)
                currentBrick.DoStopBlinking();

            for (int x = 0; x < bricksCount.x; x++)
            {
                for (int y = 0; y < bricksCount.y; y++)
                {
                    if (field[x, y] != brick)
                        continue;

                    field[x, y].DoBlinkingAnimation();
                    currentBrick = field[x, y];
                    currentBrickCoords = new Vector2Int(x, y);

                    soundCollection.GetSfx(SoundId.Click).Play();

                    return;
                }
            }
        }

        void OnEmptyBrickClick(Brick brick, Vector2Int coords)
        {
            if (isBoosterSelected)
            {
                OnHighlightedTargetClick(brick);
                return;
            }
        
            if (isAnimating || currentBrick == null || field[coords.x, coords.y] != null)
                return;

            SaveGameState();
            currentBrick.DoStopBlinking();

            List<Vector2Int> path =
                WaveAlgorithm.GetPath(field, currentBrickCoords, coords, GetAdjacentCoords, b => b == null);

            if (path.Count < 2)
            {
                currentBrick = null;

                NoPath?.Invoke();

                return;
            }

            field[currentBrickCoords.x, currentBrickCoords.y] = null;
            field[coords.x, coords.y] = currentBrick;

            int number = currentBrick.Number;
            List<Vector2> localPath = new List<Vector2>(path.Count);
            path.ForEach(c => localPath.Add(GetBrickPosition(c)));
            currentBrick.DoLocalPath(
                localPath,
                () =>
                {
                    List<Vector2Int> area =
                        WaveAlgorithm.GetArea(field, coords, GetAdjacentCoords, b => b != null && b.Number == number);

                    if (area.Count < bricksToMerge)
                        SpawnNewBricks(
                            () =>
                            {
                                SaveGame();
                                CheckGameOver();
                            }
                        );
                    else
                        Merge(coords, true, SaveGame);
                }
            );

            currentBrick = null;
        }

        void CheckGameOver()
        {
            bool isGameOver = true;
            for (int x = 0; x < bricksCount.x; x++)
            {
                for (int y = 0; y < bricksCount.y; y++)
                {
                    if (field[x, y] == null)
                        isGameOver = false;
                }
            }

            if (!isGameOver)
                return;

            isAnimating = true;

            gameState.IsGameOver = true;
            UserProgress.Current.SaveGameState(name);

            OnGameOver();
        }

        void SpawnNewBricks(Action onComplete)
        {
            isAnimating = true;

            List<int> numbers = new List<int>(nextBricksCount);
            for (int i = 0; i < nextBricksCount; i++)
            {
                numbers.Add(nextBricks[i].Number);
                Destroy(nextBricks[i].gameObject);
            }

            SpawnNextBricks();

            SpawnNewBricks(numbers, () =>
                {
                    isAnimating = false;

                    SaveGame();

                    onComplete?.Invoke();
                }
            );
        }

        void SpawnNewBricks(List<int> numbers, Action onComplete)
        {
            List<Vector2Int> emptyCoords = new List<Vector2Int>();
            for (int x = 0; x < bricksCount.x; x++)
            {
                for (int y = 0; y < bricksCount.y; y++)
                {
                    if (field[x, y] == null)
                        emptyCoords.Add(new Vector2Int(x, y));
                }
            }

            if (numbers.Count == 0 || emptyCoords.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            int rand = Random.Range(0, emptyCoords.Count - 1);

            int number = numbers[0];
            numbers.RemoveAt(0);

            Vector2Int coords = emptyCoords[rand];
            SpawnBrick(coords, number);
            NumberedBrick brick = field[coords.x, coords.y];

            brick.DoLandingAnimation(
                () => Merge(coords, true, () => SpawnNewBricks(numbers, onComplete))
            );

            soundCollection.GetSfx(SoundId.Landing).Play();
        }

        void Merge(Vector2Int toMerge, bool continuous, Action onComplete)
        {
            Merge(new List<Vector2Int> {toMerge}, continuous, onComplete);
        }

        void Merge(List<Vector2Int> toMerge, bool continuous, Action onComplete)
        {
            isAnimating = true;

            List<Vector2Int> newCoords = new List<Vector2Int>();

            int animationsLeft = 0;
            foreach (Vector2Int coords in toMerge)
            {
                if (field[coords.x, coords.y] == null)
                    continue;

                NumberedBrick brick = field[coords.x, coords.y];
                List<Vector2Int> area = WaveAlgorithm.GetArea(
                    field,
                    coords,
                    GetAdjacentCoords,
                    b => b != null && b.Number == brick.Number
                );

                if (area.Count < bricksToMerge)
                    continue;

                newCoords.AddRange(area);

                List<BrickPath> paths = new List<BrickPath>();
                foreach (Vector2Int toMove in area)
                {
                    if (toMove == coords)
                    {
                        continue;
                    }

                    BrickPath brickPath = new BrickPath
                    {
                        brick = field[toMove.x, toMove.y],
                        path = WaveAlgorithm.GetPath(
                            field,
                            toMove,
                            coords,
                            GetAdjacentCoords,
                            b => b != null && b.Number == brick.Number
                        )
                    };
                    brickPath.path.RemoveAt(0);
                    paths.Add(brickPath);
                }

                foreach (Vector2Int toMove in area)
                    if (toMove != coords)
                        field[toMove.x, toMove.y] = null;

                animationsLeft++;

                AnimateMerge(
                    paths,
                    () =>
                    {
                        animationsLeft--;

                        if (animationsLeft > 0)
                            return;

                        soundCollection.GetSfx(SoundId.Merging).Play();

                        brick.Number *= bricksToMerge;
                        brick.ColorIndex = GetColorIndex(brick.Number);
                        brick.transform.SetAsLastSibling();
                        brick.DoMergingAnimation(
                            () =>
                            {
                                if (Random.Range(0f, 1f) < coinProbability)
                                {
                                    UserProgress.Current.Coins++;

                                    GameObject vfx = Resources.Load<GameObject>("CoinVFX");
                                    vfx = Instantiate(vfx, fieldTransform.parent);

                                    vfx.transform.position = brick.transform.position;

                                    Destroy(vfx, 1.5f);
                                }

                                Normalize(
                                    normalized =>
                                    {
                                        newCoords.AddRange(normalized);

                                        if (continuous)
                                            Merge(newCoords, true, onComplete);
                                        else
                                        {
                                            isAnimating = false;
                                            onComplete?.Invoke();
                                        }
                                    }
                                );
                            }
                        );

                        gameState.Score += brick.Number;
                    }
                );
            }

            if (newCoords.Count > 0)
                return;

            isAnimating = false;

            if (onComplete != null)
                onComplete.Invoke();
        }

        void AnimateMerge(List<BrickPath> brickPaths, Action onComplete)
        {
            brickPaths.Sort((p0, p1) => p1.path.Count.CompareTo(p0.path.Count));

            int pathLength = brickPaths[0].path.Count;

            if (pathLength == 0)
            {
                brickPaths.ForEach(p => Destroy(p.brick.gameObject));
                onComplete.Invoke();
                return;
            }

            int animationsLeft = 0;
            foreach (BrickPath brickPath in brickPaths)
            {
                if (brickPath.path.Count < pathLength)
                    break;

                Vector2 position = GetBrickPosition(brickPath.path[0]);

                brickPath.path.RemoveAt(0);

                animationsLeft++;
                brickPath.brick.DoLocalMove(
                    position,
                    () =>
                    {
                        animationsLeft--;
                        if (animationsLeft == 0)
                            AnimateMerge(brickPaths, onComplete);
                    }
                );
            }
        }

        void Normalize(Action<List<Vector2Int>> onComplete)
        {
            onComplete.Invoke(new List<Vector2Int>());
        }

        bool IsFieldEmpty()
        {
            bool fieldIsEmpty = true;
        
            for (int x = 0; x < field.GetLength(0); x++)
            {
                for (int y = 0; y < field.GetLength(1); y++)
                {
                    if (field[x, y] == null)
                        continue;
                
                    fieldIsEmpty = false;
                    break;

                }
            }

            return fieldIsEmpty;
        }

        protected override void OnLastChanceCompleted()
        {
            isAnimating = false;
        
            gameState.IsGameOver = false;
            gameState.ClearSave();
            SaveGame();
        }

        protected override void OnBoostersComplete()
        {
            if (IsFieldEmpty())
            {
                SaveGameState();
                SpawnNewBricks(SaveGame);
            }

            base.OnBoostersComplete();
        }

        protected override void OnClearGame()
        {
            for (int i = 0; i < nextBricksCount; i++)
                Destroy(nextBricks[i].gameObject);
        }
    }
}