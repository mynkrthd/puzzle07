using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PuzzleGame.Gameplay.Boosters;
using PuzzleGame.Sounds;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace PuzzleGame.Gameplay.Make10
{
    public class GameControllerMake10 : BaseGameController<GameStateMake10>
    {
        [Header("Mode fields")]
        public Brick emptyBrickPrefab;
        public FigureController[] figureControllers;

        public RectTransform numbersParent;
        public NumberCounter numberCounterPrefab;
        public int minValue = 1;
        public int maxValue = 6;
        public int maxNumber = 10;
    
        Brick[,] backgroundBricks;
        int[] figures = Array.Empty<int>();
        float[] figureRotations = Array.Empty<float>();

        NumberCounter[] rawNumbers = Array.Empty<NumberCounter>();
        NumberCounter[] columnNumbers = Array.Empty<NumberCounter>();

        readonly BricksHighlighter bricksHighlighter = new();

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
                    SpawnEmptyBrick(new Vector2Int(x, y));
            }

            SpawnNumbersText();
        
            gameState = UserProgress.Current.GetGameState<GameStateMake10>(name);
            if (gameState == null)
            {
                gameState = new GameStateMake10();
                UserProgress.Current.SetGameState(name, gameState);
            }

            UserProgress.Current.CurrentGameId = name;

            foreach (var figureController in figureControllers)
            {
                figureController.PointerUp += FigureOnPointerUp;
                figureController.PointerClick += OnHighlightedTargetClick;
                figureController.PointerDrag += FigureOnPointerDrag;
            }
        
            StartGame();
        }

        protected override void StartGame()
        {
            if (LoadGame())
                return;

            gameState.Score = 0;
            gameState.IsGameOver = false;
            SpawnNewFigures();
            UpdateCounters();
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

            UpdateCounters();

            figures = gameState.GetFigures();
            figureRotations = gameState.GetFigureRotations();
            var figureIndexes = gameState.GetFiguresIndexes();
        
            if (figures.Length != figureControllers.Length || figureIndexes.Length != figureControllers.Length)
                return false;

            for (int i = 0; i < figureControllers.Length; i++)
            {
                if (figures[i] >= 0)
                    SpawnFigure(figureControllers[i], figures[i], figureRotations[i], figureIndexes[i].State);
            }

            return true;
        }

        protected override void SaveGame()
        {
            int[] numbers = new int[bricksCount.x * bricksCount.y];
            for (int x = 0; x < bricksCount.x; x++)
            {
                for (int y = 0; y < bricksCount.y; y++)
                    numbers[x * bricksCount.y + y] = field[x, y] != null ? field[x, y].Number : 0;
            }

            FigureState[] indexes = new FigureState[figures.Length];
            for (int i = 0; i < figures.Length; i++)
            {
                int[] figureIndexes = new int[figureControllers[i].bricks.Count];

                if (figureIndexes.Length > 0)
                {
                    for (int j = 0; j < figureControllers[i].bricks.Count; j++)
                        figureIndexes[j] = ((NumberedBrick) figureControllers[i].bricks[j]).Number;
                }
            
                indexes[i] = new FigureState { State = figureIndexes.ToArray()};
            }

            gameState.SetField(numbers);
            gameState.SetFigures(figures, indexes, figureRotations);
        
            UserProgress.Current.SaveGameState(name);
        }

        void SpawnBrick(Vector2Int coords, int index)
        {
            var brick = Instantiate(brickPrefab, fieldTransform);

            brick.transform.SetParent(fieldTransform, false);
            brick.RectTransform.anchorMin = Vector2.zero;
            brick.RectTransform.anchorMax = Vector2.zero;
            brick.RectTransform.anchoredPosition = GetBrickPosition(coords);

            brick.Number = index;
            brick.ColorIndex = GetColorIndex(index);
            brick.PointerClick += OnHighlightedTargetClick;

            field[coords.x, coords.y] = brick;
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

        void SpawnNumbersText()
        {
            columnNumbers = new NumberCounter[bricksCount.x];
            rawNumbers = new NumberCounter[bricksCount.y];
        
            Vector2 brickSize = GetBrickSize();

            for (int i = 0; i < bricksCount.x; i++)
            {
                Vector2 position = new Vector2(
                    GetBrickPosition(new Vector2(i, 0)).x,
                    fieldTransform.rect.height + brickSize.y / 4
                );
                columnNumbers[i] = SpawnText(position);
            }

            for (int j = 0; j < bricksCount.y; j++)
            {
                Vector2 position = new Vector2(
                    fieldTransform.rect.width + brickSize.x / 4,
                    GetBrickPosition(new Vector2(0, j)).y
                );
                rawNumbers[j] = SpawnText(position);
            }
        }

        private NumberCounter SpawnText(Vector2 position)
        {
            NumberCounter counter = Instantiate(numberCounterPrefab, numbersParent);
            RectTransform rect = counter.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.anchoredPosition = position;

            return counter;
        }

        void UpdateCounters()
        {
            for (int x = 0; x < bricksCount.x; x++)
            {
                int value = 0;

                for (int y = 0; y < bricksCount.y; y++)
                {
                    if(field[x, y] != null)
                        value += field[x, y].Number;
                }
                columnNumbers[x].UpdateValue(value, maxNumber);
            }
        
            for (int y = 0; y < bricksCount.y; y++)
            {
                int value = 0;

                for (int x = 0; x < bricksCount.x; x++)
                {
                    if(field[x, y] != null)
                        value += field[x, y].Number;
                }
                rawNumbers[y].UpdateValue(value, maxNumber);
            }
        }

        void SpawnNewFigures()
        {
            figures = new int[figureControllers.Length];
            figureRotations = new float[figureControllers.Length];
        
            for (int i = 0; i < figureControllers.Length; i++)
            {
                int figure = Random.Range(0, FiguresMake10.Figures.Length);
                float rotation = Random.Range(0, 4) * 90f;

                SpawnFigure(figureControllers[i], figure, rotation);

                figures[i] = figure;
                figureRotations[i] = rotation;
            }
        }

        void SpawnFigure(FigureController figureController, int figureIndex, float rotation, int[] indexes = null)
        {
            figureController.transform.localRotation = Quaternion.identity;
            int[,] figure = FiguresMake10.Figures[figureIndex];

            int iLenght = figure.GetLength(0);
            int jLenght = figure.GetLength(1);
        
            Vector2Int[,] numbersSum = new Vector2Int[iLenght, jLenght];
            numbersSum[0, 0] = new Vector2Int(0, 0);
        
            for (int i = 0; i < iLenght; i++)
            {
                for (int j = 0; j < jLenght; j++)
                {
                    if (figure[figure.GetLength(0) - i - 1, j] == 0)
                    {
                        numbersSum[i, j] = new Vector2Int(
                            numbersSum[Mathf.Clamp(i - 1, 0, i - 1), j].x,
                            numbersSum[i, Mathf.Clamp(j - 1, 0, j - 1)].y
                        );
                    
                        continue;
                    }

                    NumberedBrick brick = Instantiate(brickPrefab, figureController.transform);
                    figureController.bricks.Add(brick);
                
                    brick.RectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    brick.RectTransform.anchorMax = new Vector2(0.5f, 0.5f);

                    Rect rect = figureController.GetComponent<RectTransform>().rect;
                    Vector2 brickSize = new Vector2
                    {
                        x = rect.width / 2,
                        y = rect.height / 2
                    };

                    Vector2 coords = new Vector2(j - jLenght / 2f, i - iLenght / 2f);
                    Vector2 brickPosition = Vector2.Scale(coords, brickSize);
                    brickPosition += Vector2.Scale(brickSize, brick.RectTransform.pivot);
                    brick.RectTransform.anchoredPosition = brickPosition;
        
                    int index;

                    if (indexes == null || indexes.Length < figureController.bricks.Count)
                    {
                        int sumI = i == 0 ? 0 : numbersSum[Mathf.Clamp(i - 1, 0, i - 1), j].x;
                        int sumJ = j == 0 ? 0 : numbersSum[i, Mathf.Clamp(j - 1, 0, j - 1)].y;

                        int maxVal = Mathf.Min(maxNumber - iLenght + i - sumI, maxNumber - jLenght + j - sumJ);
                        maxVal = Mathf.Min(maxVal, maxValue);

                        index = Random.Range(minValue, maxVal + 1);
                        numbersSum[i, j] = new Vector2Int(sumI + index, sumJ + index);
                    }
                    else
                        index = indexes[figureController.bricks.Count - 1];
                
                    brick.Number = index;
                    brick.ColorIndex = GetColorIndex(brick.Number);
                }
            }

            figureController.Rotate(rotation);
        }

        void FigureOnPointerUp(FigureController figureController)
        {
            bricksHighlighter.UnhighlightBricks();

            if (!TryGetCoords(figureController.bricks, out var coords))
            {
                bricksHighlighter.UnhighlightNumberedBricks();
                return;
            }

            SaveGameState();

            for (int i = 0; i < figureController.bricks.Count; i++)
            {
                Brick brick = figureController.bricks[i];

                brick.transform.localRotation = Quaternion.identity;

                brick.RectTransform.SetParent(fieldTransform, false);
                brick.RectTransform.anchorMin = Vector2.zero;
                brick.RectTransform.anchorMax = Vector2.zero;
                brick.RectTransform.anchoredPosition = GetBrickPosition(coords[i]);

                field[coords[i].x, coords[i].y] = brick as NumberedBrick;
                brick.PointerClick += OnHighlightedTargetClick;

                gameState.Score++;
            }

            soundCollection.GetSfx(SoundId.Landing).Play();

            int index = Array.IndexOf(figureControllers, figureController);
            figures[index] = -1;

            figureController.bricks.Clear();
            figureController.ResetPosition();

            CheckLines(coords, () =>
            {
                if (figureControllers.All(c => c.bricks.Count == 0))
                    SpawnNewFigures();
            
                UpdateCounters();
                SaveGame();
                CheckGameOver();
            });
        }

        void FigureOnPointerDrag(FigureController figureController)
        {
            if (!TryGetCoords(figureController.bricks, out var coords))
            {
                bricksHighlighter.UnhighlightBricks();
                bricksHighlighter.UnhighlightNumberedBricks();
                return;
            }

            for (var i = 0; i < coords.Length; i++)
            {
                field.Set(coords[i], figureController.bricks[i] as NumberedBrick);
            }

            var lines = GetCompleteLines(coords, out var toCoords, out var coordsToMerge);

            if (lines > 0)
            {
                var colorIndex = field.Get(toCoords).ColorIndex;
                bricksHighlighter.SetHighlight(coordsToMerge.Select(c => field.Get(c)).ToArray(), colorIndex);
            }
            else
            {
                bricksHighlighter.UnhighlightNumberedBricks();
            }

            bricksHighlighter.SetHighlight(coords.Select(c => backgroundBricks.Get(c)).ToArray());

            foreach (var c in coords)
                field.Set(c, null);
        }

        void CheckLines(Vector2Int[] coords, Action onComplete)
        {
            int lines = GetCompleteLines(coords, out var toCoords, out var bricksToMerge);

            if (lines == 0)
            {
                CheckLines(onComplete.Invoke);
                return;
            }

            Merge(toCoords, bricksToMerge, onComplete.Invoke);
        }

        void CheckLines(Action onComplete)
        {
            int lines = GetCompleteLines(out var toCoords, out var bricksToMerge);
            if (lines == 0)
            {
                onComplete.Invoke();
                return;
            }

            this.DelayedCall(0.25f, () => { Merge(toCoords, bricksToMerge, onComplete.Invoke); });
        }

        int GetCompleteLines(out Vector2Int toCoords, out List<Vector2Int> coordsToMerge)
        {
            var coords = Enumerable.Empty<Vector2Int>();
            for (int x = 0; x < bricksCount.x; x++)
            {
                for (int y = 0; y < bricksCount.y; y++)
                {
                    coords = coords.Append(new Vector2Int(x, y));
                }
            }

            return GetCompleteLines(coords, out toCoords, out coordsToMerge);
        }

        int GetCompleteLines(IEnumerable<Vector2Int> coords, out Vector2Int toCoords, out List<Vector2Int> coordsToMerge)
        {
            toCoords = Vector2Int.zero;
            coordsToMerge = new List<Vector2Int>();
            var lines = 0;

            foreach (var c in coords)
            {
                if (field[c.x, c.y] == null) continue;

                var brickLines = GetCompleteLines(c, out var coordsToMerge0);

                if (brickLines <= lines)
                    continue;

                lines = brickLines;
                toCoords = c;
                coordsToMerge = coordsToMerge0;
            }

            return lines;
        }

        int GetCompleteLines(Vector2Int coords, out List<Vector2Int> coordsToMerge)
        {
            coordsToMerge = new List<Vector2Int>();

            int brickLines = 0;
            int lineValue = 0;

            var lineBricks = new List<Vector2Int>();

            // Check horizontal line
            for (int x = 0; x < bricksCount.x; x++)
            {
                if (field[x, coords.y] == null)
                    continue;

                lineValue += field[x, coords.y].Number;

                if (lineBricks.Any(c => c.x == x && c.y == coords.y))
                    continue;

                lineBricks.Add(new Vector2Int(x, coords.y));
            }

            if (lineValue == maxNumber)
            {
                brickLines++;
                coordsToMerge.AddRange(lineBricks);
            }

            lineValue = 0;
            lineBricks.Clear();

            // Check vertical line
            for (int y = 0; y < bricksCount.y; y++)
            {
                if (field[coords.x, y] == null)
                    continue;

                lineValue += field[coords.x, y].Number;

                if (lineBricks.Any(c => c.x == coords.x && c.y == y))
                    continue;

                lineBricks.Add(new Vector2Int(coords.x, y));
            }

            if (lineValue == maxNumber)
            {
                brickLines++;
                coordsToMerge.AddRange(lineBricks);
            }

            return brickLines;
        }

        void Merge(Vector2Int toMerge, List<Vector2Int> coordsToMerge, Action onComplete)
        {
            coordsToMerge.RemoveAll(c => c == toMerge || field[c.x, c.y] == null);

            NumberedBrick brick = field[toMerge.x, toMerge.y];
            field[toMerge.x, toMerge.y] = null;
            bricksHighlighter.Unhighlight(brick);

            brick.Number = maxNumber;
            brick.ColorIndex = GetColorIndex(brick.Number);
            brick.transform.SetAsLastSibling();

            List<BrickPath> paths = new List<BrickPath>();

            foreach (Vector2Int toMove in coordsToMerge)
            {
                NumberedBrick currentBrick = field[toMove.x, toMove.y];
                currentBrick.ColorIndex = GetColorIndex(brick.Number);
                field[toMove.x, toMove.y] = null;
                bricksHighlighter.Unhighlight(currentBrick);

                BrickPath brickPath = new BrickPath {brick = currentBrick};
                paths.Add(brickPath);
            }

            for (var i = 0; i < coordsToMerge.Count; i++)
            {
                paths[i].path = WaveAlgorithm.GetPath(
                    field,
                    coordsToMerge[i],
                    toMerge, GetAdjacentCoords,
                    b => b == null
                );
            }

            StartCoroutine(AnimateMerge(paths, () =>
            {
                soundCollection.GetSfx(SoundId.Merging).Play();

                brick.DoMergingAnimation(() =>
                {
                    Vector2 vfxPosition = brick.transform.position;
                    GetCoin(vfxPosition);

                    Destroy(brick.gameObject);
                    field[toMerge.x, toMerge.y] = null;

                    gameState.Score += 1;
                    UpdateCounters();
                    SaveGame();

                    CheckLines(onComplete.Invoke);
                });
            }));
        }

        IEnumerator AnimateMerge(List<BrickPath> brickPaths, Action onComplete)
        {
            int animationsLeft = 0;
        
            foreach (BrickPath brickPath in brickPaths)
            {
                animationsLeft++;
                brickPath.brick.DisableNumber();

                AnimateMerge(brickPath, () =>
                {
                    animationsLeft--;
                    Destroy(brickPath.brick.gameObject);
                });
            }

            yield return new WaitWhile(() => animationsLeft > 0);

            yield return new WaitForSeconds(0.1f);
        
            onComplete.Invoke();
        }

        void AnimateMerge(BrickPath brickPath, Action onComplete)
        {
            if (brickPath.path.Count == 0)
            {
                onComplete.Invoke();
                return;
            }
        
            Vector2 position = GetBrickPosition(brickPath.path[0]);
            brickPath.path.RemoveAt(0);
        
            brickPath.brick.DoLocalMove(
                position,
                () =>
                {
                    if (brickPath.path.Count == 0)
                    {
                        onComplete.Invoke();
                        return;
                    }
                
                    AnimateMerge(brickPath, onComplete);
                });
        }
    
        void CheckGameOver()
        {
            foreach (FigureController figureController in figureControllers)
            {
                if (figureController.bricks.Count == 0)
                    continue;

                for (int x = 0; x < bricksCount.x; x++)
                {
                    for (int y = 0; y < bricksCount.y; y++)
                    {
                        if (IsCanPlaceFigure(x, y, figureController))
                            return;
                    }
                }
            }

            gameState.IsGameOver = true;
            UserProgress.Current.SaveGameState(name);

            OnGameOver();
        }

        void GetCoin(Vector2 position)
        {
            UserProgress.Current.Coins++;

            GameObject vfx = Resources.Load<GameObject>("CoinVFX");
            vfx = Instantiate(vfx, fieldTransform.parent);
            vfx.transform.position = position;

            Destroy(vfx, 1.5f);
        }

        bool IsCanPlaceFigure(int x, int y, FigureController figureController)
        {
            Quaternion rotation = figureController.transform.localRotation;

            Vector2 minPosition = new Vector2(float.MaxValue, float.MaxValue);
            foreach (Brick brick in figureController.bricks)
            {
                Vector2 localPosition = rotation * brick.RectTransform.anchoredPosition;

                minPosition.x = Mathf.Min(minPosition.x, localPosition.x);
                minPosition.y = Mathf.Min(minPosition.y, localPosition.y);
            }

            foreach (Brick brick in figureController.bricks)
            {
                RectTransform rectTransform = brick.RectTransform;

                Vector2 position = rotation * rectTransform.anchoredPosition;
                position -= minPosition;

                Vector2Int coords = Vector2Int.RoundToInt(position / rectTransform.rect.size);
                coords.x += x;
                coords.y += y;

                if (coords.x < 0 || coords.y < 0 || coords.x >= bricksCount.x || coords.y >= bricksCount.y ||
                    field[coords.x, coords.y] != null)
                    return false;
            }

            return true;
        }
    
        int GetColorIndex(int number)
        {
            return number - 1;
        }

        protected override void HighlightFigures(bool active)
        {
            foreach (var figure in figureControllers)
            {
                figure.Interactable = !active;
            
                foreach (var brick in figure.bricks)
                    brick.GetComponentInChildren<Image>().raycastTarget = !active;
            
                SetSortingOrder(figure.gameObject, active);
            }
        }

        protected override void OnLastChanceCompleted()
        {
            gameState.IsGameOver = false;
            gameState.ClearSave();

            UpdateCounters();

            CheckLines(() =>
            {
                UpdateCounters();
                SaveGame();
                CheckGameOver();
            });
        }
    
        protected override void OnBoostersComplete()
        {
            CheckLines(() =>
            {
                UpdateCounters();
                base.OnBoostersComplete();
            });
        }
    
        protected override void OnClearGame()
        {
            foreach (FigureController figureController in figureControllers)
                RemoveFigure.Execute(figureController);
        }

        protected override void OnFigureRemoved(FigureController figure)
        {
            int index = Array.IndexOf(figureControllers, figure);
            figures[index] = -1;
        
            if (figureControllers.All(c => c.bricks.Count == 0))
                SpawnNewFigures();
            
            CheckGameOver();
            SaveGame();
        }
    }
}