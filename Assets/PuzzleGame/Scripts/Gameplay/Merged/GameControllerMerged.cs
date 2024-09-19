using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PuzzleGame.Gameplay.Boosters;
using PuzzleGame.Gameplay.Boosters.Merged;
using PuzzleGame.Sounds;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace PuzzleGame.Gameplay.Merged
{
    public class GameControllerMerged : BaseGameController<GameState>
    {
        [Header("Mode fields")]
        public int bricksToMerge = 3;
        public int nextBrickMaxCount = 2;
        public Brick emptyBrickPrefab;

        public MergedFigureController nextBrickController;
        public GameObject nextBrickAnimator;
    
        Brick[,] backgroundBricks;
        List<NumberedBrick> nextBricks;

        const int MaxNumber = 7;
        const float OffsetBetweenNextBricks = 10;
        bool isAnimating;
    
        readonly BricksHighlighter bricksHighlighter = new BricksHighlighter();

        class BrickPath
        {
            public NumberedBrick brick;
            public List<Vector2Int> path;
        }

        void Start()
        {
            field = new NumberedBrick[bricksCount.x, bricksCount.y];
            backgroundBricks = new Brick[bricksCount.x, bricksCount.y];

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

            nextBrickController.PointerClick += OnNextBrickClick;
            nextBrickController.PointerUp += OnNextBrickPointerUp;
            nextBrickController.PointerDrag += OnNextBrickPointerDrag;

            StartGame();
        }

        protected override void StartGame()
        {
            if (LoadGame())
                return;

            gameState.Score = 0;
            gameState.IsGameOver = false;
            SpawnNextBricks();
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

            int[] figures = gameState.GetNextBricks();

            if (figures == null || figures.Length == 0) return false;
        
            SpawnNextBricks(figures);
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
        
            int[] figures = new int[nextBrickController.bricks.Count];

            for (int i = 0; i < nextBrickController.bricks.Count; i++)
                figures[i] = ((NumberedBrick) nextBrickController.bricks[i]).Number;

            gameState.SetField(numbers);
            gameState.SetNextBricks(figures);
            UserProgress.Current.SaveGameState(name);
        }

        void SpawnEmptyBrick(Vector2Int coords)
        {
            var brick = Instantiate(emptyBrickPrefab, fieldTransform);

            brick.transform.SetParent(fieldTransform, false);
            brick.RectTransform.anchorMin = Vector2.zero;
            brick.RectTransform.anchorMax = Vector2.zero;
            brick.RectTransform.anchoredPosition = GetBrickPosition(new Vector2(coords.x, coords.y));
            brick.PointerClick += OnHighlightedTargetClick;

            backgroundBricks[coords.x, coords.y] = brick;
        }

        void SpawnBrick(Vector2Int coords, int number)
        {
            var brick = Instantiate(brickPrefab, fieldTransform);

            brick.transform.SetParent(fieldTransform, false);
            brick.RectTransform.anchorMin = Vector2.zero;
            brick.RectTransform.anchorMax = Vector2.zero;
            brick.RectTransform.anchoredPosition = GetBrickPosition(new Vector2(coords.x, coords.y));

            brick.Number = number;
            brick.ColorIndex = GetColorIndex(number);
            brick.PointerClick += OnHighlightedTargetClick;

            field[coords.x, coords.y] = brick;
        }

        void SpawnNextBricks(int[] bricks = null)
        {
            nextBrickController.ResetPosition();

            int nextBricksCount = bricks?.Length ?? Random.Range(0, nextBrickMaxCount) + 1;

            if (nextBricksCount > 1 && !IsCanPlaceBricks(nextBricksCount))
                nextBricksCount--;

            for (int i = 0; i < nextBricksCount; i++)
            {
                NumberedBrick brick = Instantiate(brickPrefab, nextBrickController.transform);

                int number;
                do number = GetRandomNumber();
                while (nextBrickController.bricks.Any(b => ((NumberedBrick) b).Number == number));

                brick.Number = bricks?[i] ?? number;
                brick.ColorIndex = GetColorIndex(brick.Number);
                brick.PointerClick += OnHighlightedTargetClick;

                nextBrickController.bricks.Add(brick);

                RectTransform brickRectTransform = brick.RectTransform;

                brickRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                brickRectTransform.anchorMax = new Vector2(0.5f, 0.5f);

                if (nextBricksCount == 1)
                {
                    nextBrickAnimator.SetActive(false);
                    brickRectTransform.anchoredPosition = Vector2.zero;
                    return;
                }

                Vector2 brickSize = brick.RectTransform.rect.size;

                Vector2 coords = i % 2 == 0 ? new Vector2(-1, 0) : new Vector2(i, 0);
                Vector2 brickPosition = Vector2.Scale(brickSize, brickRectTransform.pivot);
                brickPosition = Vector2.Scale(coords, brickPosition);
                brickPosition.x += OffsetBetweenNextBricks * coords.x;
                brickRectTransform.anchoredPosition = brickPosition;
                nextBrickAnimator.SetActive(true);
            }
        }

        void OnNextBrickClick(FigureController controller)
        {
            if (isBoosterSelected)
            {
                OnHighlightedTargetClick(nextBrickController);
                return;
            }
            if (isAnimating) return;
        
            soundCollection.GetSfx(SoundId.Click).Play();
        }

        void OnNextBrickPointerUp(FigureController controller)
        {
            bricksHighlighter.UnhighlightBricks();

            if (!TryGetCoords(controller.bricks, out var coords))
                return;

            SaveGameState();
            soundCollection.GetSfx(SoundId.Landing).Play();

            var bricks = new List<NumberedBrick>();

            for (int i = 0; i < controller.bricks.Count; i++)
            {
                var brick = controller.bricks[i];

                brick.RectTransform.SetParent(fieldTransform, false);
                brick.RectTransform.anchorMin = Vector2.zero;
                brick.RectTransform.anchorMax = Vector2.zero;
                brick.RectTransform.anchoredPosition = GetBrickPosition(coords[i]);

                field[coords[i].x, coords[i].y] = brick as NumberedBrick;
                brick.PointerClick += OnHighlightedTargetClick;

                bricks.Add(brick as NumberedBrick);
            }

            nextBrickController.ResetBricksRotation();
            nextBrickController.bricks.Clear();

            bricks.Sort((a, b) => a.Number.CompareTo(b.Number));

            StartCoroutine(MergeBricks(bricks));
        }

        void OnNextBrickPointerDrag(FigureController controller)
        {
            if (!TryGetCoords(controller.bricks, out var coords))
            {
                bricksHighlighter.UnhighlightBricks();
                return;
            }

            bricksHighlighter.SetHighlight(coords.Select(c => backgroundBricks[c.x, c.y]).ToArray());
        }

        IEnumerator MergeBricks(List<NumberedBrick> bricks)
        {
            bricks.Sort((a, b) => a.Number.CompareTo(b.Number));

            while (bricks.Count > 0)
            {
                var brick = bricks.First();
                if (!brick || brick.IsDestroyed)
                {
                    bricks.Remove(brick);
                    continue;
                }

                var number = brick.Number;
                var pivot = brick.RectTransform.pivot;
                var coords = BrickPositionToCoords(brick.transform.position, pivot);

                var area = WaveAlgorithm.GetArea(field, coords, GetAdjacentCoords, b => b && b.Number == number);

                if (area.Count >= bricksToMerge)
                {
                    var isMerging = true;
                    Merge(coords, true, () =>
                    {
                        bricks.Remove(brick);
                        isMerging = false;
                    });

                    yield return new WaitWhile(() => isMerging);
                }
                else
                    bricks.Remove(brick);
            }

            yield return new WaitForSeconds(0.1f);

            SpawnNextBricks();
            SaveGame();
            CheckGameOver();
        }

        void Merge(Vector2Int toMerge, bool continuous, Action onComplete)
        {
            Merge(new List<Vector2Int> {toMerge}, continuous, onComplete);
        }

        void Merge(List<Vector2Int> toMerge, bool continuous, Action onComplete)
        {
            isAnimating = true;

            var newCoords = new List<Vector2Int>();

            int animationsLeft = 0;
            foreach (Vector2Int coords in toMerge)
            {
                if (field[coords.x, coords.y] == null)
                    continue;

                NumberedBrick brick = field[coords.x, coords.y];
                var area = WaveAlgorithm.GetArea(
                    field,
                    coords,
                    GetAdjacentCoords,
                    b => b != null && b.Number == brick.Number
                );

                if (area.Count < bricksToMerge)
                    continue;

                newCoords.AddRange(area);

                var paths = new List<BrickPath>();
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

                foreach (var toMove in area)
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

                        brick.Number++;
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
                                            AnimateDestroyBrick(brick, coords, () =>
                                            {
                                                isAnimating = false;
                                                onComplete?.Invoke();
                                            });
                                        }
                                    }
                                );
                            }
                        );

                        gameState.Score += brick.Number;
                        AnimateDestroyBrick(brick, coords, () =>
                        {
                            isAnimating = false;
                            onComplete?.Invoke();
                        });
                    }
                );
            }

            if (newCoords.Count > 0)
                return;

            isAnimating = false;
            onComplete?.Invoke();
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

        void AnimateDestroyBrick(NumberedBrick brick, Vector2Int coords, Action onComplete)
        {
            if (brick.Number <= MaxNumber)
            {
                onComplete?.Invoke();
                return;
            }

            AnimateDestroy(coords, onComplete);
        }

        void CheckGameOver()
        {
            if (IsCanPlaceBricks(nextBrickController.bricks.Count)) return;

            gameState.IsGameOver = true;
            UserProgress.Current.SaveGameState(name);

            OnGameOver();
        }
    
        bool IsCanPlaceBricks(int nextBricksCount)
        {
            for (int x = 0; x < bricksCount.x; x++)
            {
                for (int y = 0; y < bricksCount.y; y++)
                {
                    if (IsCanPlaceBricks(x, y, nextBricksCount))
                        return true;
                }
            }

            return false;
        }

        bool IsCanPlaceBricks(int x, int y, int bricksCount)
        {
            if (bricksCount == 1)
                return field[x, y] == null;

            var coords = GetAdjacentCoords(new Vector2Int(x, y));

            return field[x, y] == null && coords.Any(c => field[c.x, c.y] == null);
        }

        protected override void OnLastChanceCompleted()
        {
            gameState.IsGameOver = false;
            gameState.ClearSave();
            SaveGame();
        }
    
        protected override void HighlightFigures(bool active)
        {
            nextBrickController.Interactable = !active;
        
            foreach (var brick in nextBrickController.bricks)
                brick.GetComponentInChildren<Image>().raycastTarget = !active;

            SetSortingOrder(nextBrickController.gameObject, active);
        }

        protected override void BoosterExecute<T>(T target)
        {
            if (boosterType != BoosterType.RemoveFigure)
            {
                base.BoosterExecute(target);
                return;
            }

            SaveGameState();

            RemoveFigureMerged.Execute(nextBrickController);
            OnFigureRemoved(null);
            OnBoostersComplete();
            BoostersController.Instance.OnBoosterProceeded(true);
        }

        protected override void OnClearGame()
        {
            foreach (Brick brick in nextBrickController.bricks)
                Destroy(brick.gameObject);

            nextBrickController.ResetBricksRotation();
            nextBrickController.bricks.Clear();
        }

        protected override void OnFigureRemoved(FigureController figure)
        {
            SpawnNextBricks();
        }

        int GetRandomNumber()
        {
            return Random.Range(1, MaxNumber);
        }

        int GetColorIndex(int number)
        {
            return number - 1;
        }
    
        void Normalize(Action<List<Vector2Int>> onComplete)
        {
            onComplete.Invoke(new List<Vector2Int>());
        }
    }
}
