using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleGame.Gameplay.Boosters;
using PuzzleGame.Input;
using PuzzleGame.Sounds;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace PuzzleGame.Gameplay.Bricks2048
{
    public class GameController2048Bricks : BaseGameController<GameState2048Bricks>
    {
        [Header("Mode fields")]
        public float speed;
        public float fallSpeed;

        public RectTransform fieldBackGround;
        public Transform nextBrickPoint;
        public Transform columnsParent;

        public Brick columnPrefab;
        public bool spawnColumns;

        NumberedBrick nextBrick;
        NumberedBrick currentBrick;
        Vector2Int currentBrickCoords;

        float timeSinceMoveDown;

        bool isFalling;
        bool isAnimating;

        class BrickPath
        {
            public NumberedBrick brick;
            public List<Vector2Int> path;
        }

        bool IsAnimating
        {
            get => isAnimating;
            set => isAnimating = value;
        }

        public NumberedBrick CurrentBrick
        {
            get
            {
                if(currentBrick == null)
                    CurrentBrick = SpawnBrick(currentBrickCoords, GetRandomNumber());
            
                return currentBrick;
            }
            set => currentBrick = value;
        }

        int GetRandomNumber()
        {
            return Mathf.RoundToInt(Mathf.Pow(2, Random.Range(1, 5)));
        }

        int GetColorIndex(int number)
        {
            return Mathf.RoundToInt(Mathf.Log(number, 2) - 1);
        }

        void Start()
        {
            if (spawnColumns)
                SpawnColumns();

            InputController.Left += OnLeft;
            InputController.Right += OnRight;
            InputController.Down += OnDown;
            InputController.Move += OnTapMove;
        
            field = new NumberedBrick[bricksCount.x, bricksCount.y];
            SpawnNextBrick();

            gameState = UserProgress.Current.GetGameState<GameState2048Bricks>(name);
            if (gameState == null)
            {
                gameState = new GameState2048Bricks();
                UserProgress.Current.SetGameState(name, gameState);
            }

            UserProgress.Current.CurrentGameId = name;

            StartGame();
        }

        protected override void StartGame()
        {
            currentBrickCoords = new Vector2Int(bricksCount.x / 2, bricksCount.y - 1);
            gameState.CurrentBrickCoords = currentBrickCoords;
        
            if (LoadGame())
                return;

            gameState.Score = 0;
            gameState.IsGameOver = false;
            SpawnStartingBricks();
            nextBrick.Number = GetRandomNumber();
            nextBrick.ColorIndex = GetColorIndex(nextBrick.Number);
            SetStartBoosters();

            SaveGame();
        }

        void SpawnStartingBricks()
        {
            currentBrickCoords = new Vector2Int(bricksCount.x / 2, bricksCount.y - 1);
            CurrentBrick = SpawnBrick(currentBrickCoords, GetRandomNumber());

            List<int> numbers = new List<int>(bricksCount.x);
            for (int i = 1; i <= bricksCount.x; i++)
            {
                numbers.Add(Mathf.RoundToInt(Mathf.Pow(2, i)));
            }

            for (int i = 0; i < bricksCount.x; i++)
            {
                int rand = Random.Range(0, numbers.Count);
                var brick = SpawnBrick(new Vector2Int(i, 0), numbers[rand]);
                brick.PointerClick += OnHighlightedTargetClick;
                field[i, 0] = brick;

                numbers.RemoveAt(rand);
            }
        }

        void SpawnColumns()
        {
            for (int i = 0; i < bricksCount.x; i++)
            {
                var columnBrick = Instantiate(columnPrefab, columnsParent);
                var rect = columnBrick.RectTransform;
                var fieldRect = fieldTransform.rect;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.zero;
                rect.anchoredPosition = new Vector2(GetBrickPosition(new Vector2(i, 0)).x, fieldRect.height / 2);

                rect.sizeDelta = new Vector2(fieldRect.width / bricksCount.x, fieldRect.height);
                columnBrick.ColorIndex = i % 2;
            }
        }

        void OnDestroy()
        {
            InputController.Left -= OnLeft;
            InputController.Right -= OnRight;
            InputController.Down -= OnDown;
            InputController.Move -= OnTapMove;
        }

        void OnLeft()
        {
            if (!IsAnimating && !isFalling)
                MoveHorizontally(-1);
        }

        void OnRight()
        {
            if (!IsAnimating && !isFalling)
                MoveHorizontally(1);
        }

        void OnDown()
        {
            if (isBoosterSelected)
            {
                isAnimating = true;
                OnHighlightedTargetClick(this);
                return;
            }
        
            if (IsAnimating || isFalling)
                return;

            isFalling = true;
            timeSinceMoveDown = 0f;
            MoveDown();
        }

        void OnTapMove(int value)
        {
            if (isBoosterSelected)
                return;
        
            if (IsAnimating || isFalling) return;

            int path = 0;
            if (value < currentBrickCoords.x)
            {
                for (int i = currentBrickCoords.x - 1; i >= value; i--)
                {
                    if (field[i, currentBrickCoords.y] != null)
                        break;

                    path++;
                }
            }

            if (value > currentBrickCoords.x)
            {
                for (int i = currentBrickCoords.x + 1; i <= value; i++)
                {
                    if (field[i, currentBrickCoords.y] != null)
                        break;

                    path++;
                }
            }

            int steps = Mathf.Abs(currentBrickCoords.x - value);
            value = path < steps ? currentBrickCoords.x : value;
        
            Move(value);
        }

        void Update()
        {
            if (IsAnimating || isBoosterSelected)
                return;

            timeSinceMoveDown += Time.deltaTime;

            if (isFalling && timeSinceMoveDown >= 1f / fallSpeed)
            {
                timeSinceMoveDown -= 1f / fallSpeed;
                MoveDown();
            }

            if (!isFalling && timeSinceMoveDown >= 1 / speed)
            {
                timeSinceMoveDown -= 1f / speed;
                MoveDown();
            }
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
                        field[x, y] = SpawnBrick(new Vector2Int(x, y), numbers[x * bricksCount.y + y]);
                }
            }

            currentBrickCoords = gameState.CurrentBrickCoords;
            CurrentBrick = SpawnBrick(currentBrickCoords, gameState.CurrentBrick);
            nextBrick.Number = gameState.NextBrick;
            nextBrick.ColorIndex = GetColorIndex(nextBrick.Number);

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

            gameState.SetField(numbers);
            gameState.CurrentBrickCoords = currentBrickCoords;
            gameState.CurrentBrick = CurrentBrick.Number;
            gameState.NextBrick = nextBrick.Number;
            UserProgress.Current.SaveGameState(name);
        }

        NumberedBrick SpawnBrick(Vector2Int coords, int number)
        {
            NumberedBrick brick = Instantiate(brickPrefab, fieldTransform);

            brick.transform.SetParent(fieldTransform, false);
            brick.RectTransform.anchorMin = Vector2.zero;
            brick.RectTransform.anchorMax = Vector2.zero;
            brick.RectTransform.anchoredPosition = GetBrickPosition(new Vector2(coords.x, coords.y));

            brick.Number = number;
            brick.ColorIndex = GetColorIndex(number);
            brick.PointerClick += OnHighlightedTargetClick;

            return brick;
        }

        void SpawnNextBrick()
        {
            nextBrick = Instantiate(brickPrefab, nextBrickPoint);
            nextBrick.RectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            nextBrick.RectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            nextBrick.RectTransform.anchoredPosition = Vector2.zero;
        }

        void MoveDown()
        {
            if (currentBrickCoords.y > 0 && field[currentBrickCoords.x, currentBrickCoords.y - 1] == null)
            {
                currentBrickCoords.y--;
                CurrentBrick.RectTransform.anchoredPosition =
                    GetBrickPosition(new Vector2(currentBrickCoords.x, currentBrickCoords.y));

                SaveGame();
            }
            else
            {
                SaveGameState();

                IsAnimating = true;
                soundCollection.GetSfx(SoundId.Landing).Play();

                CurrentBrick.DoLandingAnimation(
                    () =>
                    {
                        IsAnimating = false;
                        field[currentBrickCoords.x, currentBrickCoords.y] = CurrentBrick;

                        Merge(
                            new List<Vector2Int> {currentBrickCoords},
                            () =>
                            {
                                isFalling = false;

                                currentBrickCoords = new Vector2Int(bricksCount.x / 2, bricksCount.y - 1);

                                if (field[currentBrickCoords.x, currentBrickCoords.y] != null)
                                {
                                    IsAnimating = true;

                                    gameState.IsGameOver = true;
                                    UserProgress.Current.SaveGameState(name);

                                    OnGameOver();
                                    return;
                                }
                            
                                CurrentBrick = SpawnBrick(currentBrickCoords, nextBrick.Number);
                                nextBrick.Number = GetRandomNumber();
                                nextBrick.ColorIndex = GetColorIndex(nextBrick.Number);

                                SaveGame();
                            }
                        );
                    }
                );
            }
        }

        void MoveHorizontally(int value)
        {
            int x = currentBrickCoords.x + value;
            Move(x);
        }

        void Move(int value)
        {
            if (value < 0 || value >= field.GetLength(0) || field[value, currentBrickCoords.y] != null)
                return;
        
            currentBrickCoords.x = value;
            CurrentBrick.RectTransform.anchoredPosition =
                GetBrickPosition(new Vector2(currentBrickCoords.x, currentBrickCoords.y));
        }

        void Merge(List<Vector2Int> toMerge, Action onComplete)
        {
            IsAnimating = true;

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

                int areaSize = area.Count;
                AnimateMerge(
                    paths,
                    () =>
                    {
                        animationsLeft--;

                        if (animationsLeft > 0)
                            return;

                        soundCollection.GetSfx(SoundId.Merging).Play();

                        brick.Number *= Mathf.ClosestPowerOfTwo(areaSize);
                        brick.ColorIndex = GetColorIndex(brick.Number);
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

                                if (newCoords.Count > 0)
                                    Normalize(
                                        normalized =>
                                        {
                                            newCoords.AddRange(normalized);
                                            Merge(newCoords, onComplete);
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

            IsAnimating = false;
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
                                soundCollection.GetSfx(SoundId.Landing).Play();
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

        public override void LastChance(LastChance lastChance)
        {
            field[currentBrickCoords.x, currentBrickCoords.y] = CurrentBrick;

            switch (lastChance.LastChanceType)
            {
                case LastChanceType.Numbers:
                    ClearNumbers.Execute(field, lastChance.MaxNumber, OnLastChanceCompleted);
                    break;
                case LastChanceType.CrossLines:
                    ClearCrossLines.Execute(field, OnLastChanceCompleted);
                    break;
                case LastChanceType.LinesHorizontal:
                    var coordY = bricksCount.y - lastChance.LinesCount;
                    ClearHorizontalLines.Execute(field, coordY, lastChance.LinesCount, OnLastChanceCompleted);
                    break;
                case LastChanceType.LinesVertical:
                    int coordX = (bricksCount.x - lastChance.LinesCount) / 2;
                    ClearVerticalLines.Execute(field, coordX, lastChance.LinesCount, OnLastChanceCompleted);
                    break;
                case LastChanceType.Explosion:
                    var coords = new Vector2Int(bricksCount.x / 2, bricksCount.y / 2);
                    AnimateDestroy(coords, OnLastChanceCompleted);
                    return;
            }
        }

        protected override void OnLastChanceCompleted()
        {
            Normalize(
                normalized =>
                {
                    Merge(normalized, () =>
                    {
                        currentBrickCoords = new Vector2Int(bricksCount.x / 2, bricksCount.y - 1);
                        gameState.CurrentBrickCoords = currentBrickCoords;

                        currentBrick = SpawnBrick(currentBrickCoords, nextBrick.Number);
                        nextBrick.Number = GetRandomNumber();
                        nextBrick.ColorIndex = GetColorIndex(nextBrick.Number);
        
                        gameState.IsGameOver = false;
                        gameState.ClearSave();
        
                        SaveGame();
                    }); 
                }
            );
        }
    
        public override void HighlightBoosterTarget(BoosterType type, bool active)
        {
            isBoosterSelected = active;
            boosterType = type;

            switch (type)
            {
                case BoosterType.ClearBrick:
                case BoosterType.ClearNumber:
                case BoosterType.Explosion:
                case BoosterType.ClearHorizontalLine:
                case BoosterType.ClearVerticalLine:
                    HighlightBricks(active);
                    break;
                case BoosterType.RemoveFigure:
                    break;
                case BoosterType.Undo:
                    HighlightField(active);
                    break;
            }
        }

        protected override void HighlightField(bool active)
        {
            isAnimating = active;
        
            SetSortingOrder(fieldBackGround.gameObject, active);
        }

        protected override void HighlightBricks(bool active)
        {
            base.HighlightBricks(active);

            for (int x = 0; x < bricksCount.x; x++)
            {
                for (int y = 0; y < bricksCount.y; y++)
                {
                    if(field[x, y] == null) continue;

                    field[x, y].GetComponentInChildren<Image>().raycastTarget = active;
                }
            }
        }

        protected override void OnClearGame()
        {
            if(currentBrick != null)
                Destroy(currentBrick.gameObject);
        }

        protected override void OnBoostersComplete()
        {

            Normalize(
                normalized =>
                {
                    Merge(normalized, () =>
                    {
                        base.OnBoostersComplete(); 
                    });
                }
            );
        }
    }
}