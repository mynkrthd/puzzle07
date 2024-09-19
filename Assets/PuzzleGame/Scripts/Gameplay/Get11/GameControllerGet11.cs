using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleGame.Sounds;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PuzzleGame.Gameplay.Get11
{
    public class GameControllerGet11 : BaseGameController<GameState>
    {
        float timeSinceMoveDown;

        bool isAnimating;
        bool isMerging;

        class BrickPath
        {
            public NumberedBrick brick;
            public List<Vector2Int> path;
        }

        int GetRandomNumber()
        {
            int maxNumber = 0;

            for (int x = 0; x < bricksCount.x; x++)
            {
                for (int y = 0; y < bricksCount.y; y++)
                {
                    NumberedBrick brick = field[x, y];
                    if (brick != null)
                        maxNumber = Mathf.Max(maxNumber, brick.Number);
                }
            }

            return Random.Range(1, Mathf.Clamp(maxNumber, 4, 6));
        }

        void Start()
        {
            field = new NumberedBrick[bricksCount.x, bricksCount.y];
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
            SetStartBoosters();

            SaveGame();
        }

        protected virtual bool LoadGame()
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

            return true;
        }

        void SpawnStartingBricks()
        {
            for (int x = 0; x < bricksCount.x; x++)
            {
                for (int y = 0; y < bricksCount.y; y++)
                {
                    SpawnBrick(new Vector2Int(x, y), GetRandomNumber());
                }
            }
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

            gameState.SetField(numbers);
            UserProgress.Current.SaveGameState(name);
        }

        void SpawnBrick(Vector2Int coords, int number)
        {
            NumberedBrick brick = Instantiate(brickPrefab, fieldTransform);

            brick.transform.SetParent(fieldTransform, false);
            brick.RectTransform.anchorMin = Vector2.zero;
            brick.RectTransform.anchorMax = Vector2.zero;
            brick.RectTransform.anchoredPosition = GetBrickPosition(new Vector2(coords.x, coords.y));

            SetBrickNumber(brick, number);

            field[coords.x, coords.y] = brick;

            brick.PointerClick += OnClick;
        }

        static void SetBrickNumber(NumberedBrick brick, int number)
        {
            brick.Number = number;
            brick.ColorIndex = number - 1;
        }

        void OnClick(Brick brick)
        {
            if (isBoosterSelected)
            {
                OnHighlightedTargetClick(brick);
                return;
            }
        
            if (isAnimating)
                return;

            for (int x = 0; x < bricksCount.x; x++)
            {
                for (int y = 0; y < bricksCount.y; y++)
                {
                    if (field[x, y] != brick)
                        continue;

                    isMerging = true;
                    Merge(new Vector2Int(x, y), false, () => SpawnNewBricks(CheckGameOver));
                    return;
                }
            }
        }

        void CheckGameOver()
        {
            bool isGameOver = true;
            for (int x = 0; x < bricksCount.x; x++)
            {
                for (int y = 0; y < bricksCount.y; y++)
                {
                    Vector2Int coords = new Vector2Int(x, y);
                    List<Vector2Int> area = WaveAlgorithm.GetArea(
                        field,
                        coords,
                        GetAdjacentCoords,
                        b => b != null && b.Number == field[coords.x, coords.y].Number
                    );

                    if (area.Count > 1)
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

            bool spawned = false;
            int yMin = int.MaxValue;
            for (int y = 0; y < bricksCount.y; y++)
            {
                for (int x = 0; x < bricksCount.x; x++)
                {
                    if (field[x, y] != null)
                        continue;

                    yMin = Mathf.Min(yMin, y);

                    Vector2Int coords = new Vector2Int(x, y);
                    SpawnBrick(coords, GetRandomNumber());
                    NumberedBrick brick = field[coords.x, coords.y];
                
                    brick.RectTransform.anchoredPosition =
                        GetBrickPosition(new Vector2Int(x, bricksCount.y + y - yMin));
                    brick.DoLocalMove(
                        GetBrickPosition(coords),
                        () => brick.DoLandingAnimation(
                            () =>
                            {
                                isAnimating = false;

                                if (onComplete != null)
                                    onComplete.Invoke();
                            }
                        )
                    );

                    spawned = true;
                }
            }

            if (!spawned)
            {
                isAnimating = false;

                if (onComplete != null)
                    onComplete.Invoke();
            }

            SaveGame();
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

                if (area.Count < 2)
                    continue;

                if (isMerging)
                {
                    SaveGameState();
                    isMerging = false;
                }
            
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

                        SetBrickNumber(brick, brick.Number + 1);
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
                                            isMerging = false;

                                            if (onComplete != null)
                                                onComplete.Invoke();
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
            isMerging = false;

            if (onComplete != null)
                onComplete.Invoke();
        }

        void AnimateMerge(List<BrickPath> brickPaths, Action onComplete)
        {
            brickPaths = brickPaths.OrderByDescending(p => p.path.Count).ToList();

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
            List<Vector2Int> normalized = new List<Vector2Int>();
            for (int x = 0; x < field.GetLength(0); x++)
            {
                for (int y = 0; y < field.GetLength(1); y++)
                {
                    NumberedBrick brick = field[x, y];

                    if (brick == null)
                        continue;

                    int yEmpty = y;
                    while (yEmpty > 0 && field[x, yEmpty - 1] == null)
                        yEmpty--;

                    if (yEmpty == y)
                        continue;

                    field[x, y] = null;
                    field[x, yEmpty] = brick;
                    Vector2Int brickCoords = new Vector2Int(x, yEmpty);

                    normalized.Add(brickCoords);

                    bool isFirst = normalized.Count == 1;
                    brick.DoLocalMove(
                        GetBrickPosition(brickCoords),
                        () =>
                        {
                            if (isFirst)
                            {
                                brick.DoLandingAnimation(() => onComplete.Invoke(normalized));
                            }
                            else
                                brick.DoLandingAnimation(null);
                        }
                    );
                }
            }

            if (normalized.Count == 0)
                onComplete.Invoke(normalized);
        }

        protected override void OnLastChanceCompleted()
        {
            gameState.IsGameOver = false;
            gameState.ClearSave();

            OnBoostersComplete();
        }

        protected override void HighlightField(bool active)
        {
            SetSortingOrder(fieldTransform.transform.parent.gameObject, active);
        }

        protected override void HighlightBricks(bool active)
        {
            HighlightField(active);
        }

        protected override void OnBoostersComplete()
        {
            Normalize(
                normalized =>
                {
                    SpawnNewBricks(() =>
                    {
                        base.OnBoostersComplete();
                        CheckGameOver();
                    });
                }
            );
        }
    }
}