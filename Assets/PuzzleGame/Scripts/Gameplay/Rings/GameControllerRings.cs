using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PuzzleGame.Gameplay.Boosters;
using PuzzleGame.Gameplay.Boosters.Rings;
using PuzzleGame.Sounds;
using PuzzleGame.Themes;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace PuzzleGame.Gameplay.Rings
{
    public class GameControllerRings : BaseGameController<GameStateRings>
    {
        [Header("Mode fields")]
        public Brick emptyBrickPrefab;
        public NextRingController nextRingPrefab;
        public RectTransform lineParent;
        public RectTransform nextRingParent;
        public int startColorsCount = 3;
        public int colorsCount = 8;
        public int[] scoresForUpgrade;
        public float[] colorsProbabilities;

        RingsController[,] rings;
        NextRingController nextRing;
        List<RingsLine> lines = new List<RingsLine>();
        RingsController currentRing;
    
        int[] colorsIndexes;
        float[] probabilities;
        int currentColorsCount;
        const int maxRingsToSpawn = 2;

        int MaxRingsCount
        {
            get
            {
                int count = 0;
            
                for (int x = 0; x < bricksCount.x; x++)
                {
                    for (int y = 0; y < bricksCount.y; y++)
                        count = rings[x, y].FreeStates > count ? rings[x, y].FreeStates : count;
                }
            
                return Mathf.Min(count, maxRingsToSpawn);
            }
        }

        void Start()
        {
            rings = new RingsController[bricksCount.x, bricksCount.y];

            for (int x = 0; x < bricksCount.x; x++)
            {
                for (int y = 0; y < bricksCount.y; y++)
                    SpawnEmptyBrick(new Vector2Int(x, y));
            }
        
            gameState = UserProgress.Current.GetGameState<GameStateRings>(name);
        
            if (gameState == null)
            {
                gameState = new GameStateRings();
                UserProgress.Current.SetGameState(name, gameState);
            }
            UserProgress.Current.CurrentGameId = name;
        
            StartGame();
        }

        protected override void StartGame()
        {
            for (int x = 0; x < bricksCount.x; x++)
            {
                for (int y = 0; y < bricksCount.y; y++)
                    SpawnRingBrick(new Vector2Int(x, y));
            }
        
            if (LoadGame())
                return;

            gameState.Score = 0;
            gameState.IsGameOver = false;
            OnScoreUpdate();
            SpawnNextRing();
            SetStartBoosters();

            SaveGame();
        }

        private bool LoadGame()
        {
            if (gameState.IsGameOver)
                return false;

            RingState[] numbers = gameState.GetRings();

            if (numbers == null || numbers.Length != bricksCount.x * bricksCount.y || gameState.Colors <= 0)
                return false;
        
            for (int x = 0; x < bricksCount.x; x++)
            {
                for (int y = 0; y < bricksCount.y; y++)
                    rings[x, y].Merge(numbers[x * bricksCount.y + y].State);
            }

            int[] currentNextRing = gameState.GetNextRings();

            if (gameState.Colors <= 0 || currentNextRing.Length == 0) return false;

            currentColorsCount = gameState.Colors;
            GetProbabilities();
            SpawnNextRing(true);
        
            for (int i = 0; i < currentNextRing.Length; i++)
                nextRing.SetState(i, currentNextRing[i]);

            return true;
        }

        protected override void SaveGame()
        {
            RingState[] numbers =  new RingState[bricksCount.x * bricksCount.y];
        
            for (int x = 0; x < bricksCount.x; x++)
            {
                for (int y = 0; y < bricksCount.y; y++)
                {
                    numbers[x * bricksCount.y + y] = new RingState {State = (int[]) rings[x, y].ringStates.ToArray().Clone()};
                }
            }

            gameState.Colors = currentColorsCount;
            gameState.SetRings(numbers);
        
            if(nextRing != null)
                gameState.SetNextRings(nextRing.ringsController.ringStates);
        
            UserProgress.Current.SaveGameState(name);
        }
    
        void SpawnEmptyBrick(Vector2Int coords)
        {
            Brick brick = Instantiate(emptyBrickPrefab, fieldTransform);
            brick.RectTransform.anchorMin = Vector2.zero;
            brick.RectTransform.anchorMax = Vector2.zero;
            brick.RectTransform.anchoredPosition = GetBrickPosition(new Vector2(coords.x, coords.y));
        }

        void SpawnRingBrick(Vector2Int coords)
        {
            RingsController ring = Instantiate(brickPrefab, fieldTransform).GetComponent<RingsController>();
            ring.RectTransform.anchorMin = Vector2.zero;
            ring.RectTransform.anchorMax = Vector2.zero;
            ring.RectTransform.anchoredPosition = GetBrickPosition(new Vector2(coords.x, coords.y));
            ring.MergeColor += i => OnColorMerge(ring, i);
            ring.PointerClick += OnHighlightedTargetClick;
            rings[coords.x, coords.y] = ring;
        }

        void SpawnNextRing(bool loaded = false)
        {
            NextRingController ring = Instantiate(nextRingPrefab, nextRingParent);
            RectTransform rectTransform = ring.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            ring.Init(brickPrefab as RingsController);
        
            ring.PointerDown += controller =>
            {
                if (isBoosterSelected)
                {
                    OnHighlightedTargetClick(nextRing);
                    return;
                }
                HighlightField(controller, true);
            };
            ring.PointerUp += RingOnPointerUp;
            nextRing = ring;

            if(!loaded)
                SetNextRing(ring);
        }

        void SetNextRing(NextRingController ring)
        {
            int ringsCount = Random.Range(0, MaxRingsCount);
            List<int[]> free = FreeBricks(ringsCount);

            if (free.Count == 0 && ringsCount > 0)
            {
                ringsCount--;
                free = FreeBricks(ringsCount);
            }
        
            int[] state = MaxRingsCount > 0 ? free[Random.Range(0, free.Count)].ToArray() : new []{-1, -1, 1};

            List<int> indexes = new List<int>();
            for (int i = 0; i < state.Length; i++)
            {
                if (state[i] == -1)
                    indexes.Add(i);
            }
        
            List<int> colors = new List<int>(colorsIndexes);
            List<float> prob = new List<float>(probabilities);
        
            for (int i = 0; i <= ringsCount; i++)
            {
                if(indexes.Count == 0 || colors.Count == 0) break;
            
                int index = Random.Range(0, indexes.Count);
                int color = Probabilities.GetRandomValue(colors.ToArray(), prob.ToArray());
            
                ring.SetState(indexes[index], color);
            
                prob.RemoveAt(colors.IndexOf(color));
                colors.Remove(color);
                indexes.RemoveAt(index);
            }
        }

        void RingOnPointerUp(NextRingController ring)
        {
            HighlightField(null, false);

            Vector2 pivot = ring.GetComponent<RectTransform>().pivot;
            Vector2Int coords = BrickPositionToCoords(ring.transform.position, pivot);

            if (coords.x < 0 || coords.y < 0 || coords.x >= bricksCount.x || coords.y >= bricksCount.y ||
                !rings[coords.x, coords.y].CanMerge(ring.ringsController.ringStates))
            {
                return;
            }
            SaveGameState();

            currentRing = rings[coords.x, coords.y];
            rings[coords.x, coords.y].Merge(ring.ringsController.ringStates);
            ring.Clear();
            soundCollection.GetSfx(SoundId.Landing).Play();
            nextRing = null;
        
            CheckLines();
            CheckGameOver();
            SaveGame();
        }

        void CheckLines()
        {
            lines.Clear();
        
            ///vertical
            for (int x = 0; x < bricksCount.x; x++)
            {
                List<RingsController> line = new List<RingsController>();
            
                for (int y = 0; y < bricksCount.y; y++)
                    line.Add(rings[x,y]);
            
                CheckLine(rings[x, 0].GetColors(), line);
            }
        
            ///horizontal
            for (int y = 0; y < bricksCount.y; y++)
            {
                List<RingsController> line = new List<RingsController>();

                for (int x = 0; x < bricksCount.x; x++)
                    line.Add(rings[x,y]);
            
                CheckLine(rings[0, y].GetColors(), line);
            }
        
            ///diagonal 1
            List<RingsController> lineD1 = new List<RingsController>();
            for (int x = 0; x < bricksCount.x; x++)
                lineD1.Add(rings[x, x]);

            CheckLine(rings[0, 0].GetColors(), lineD1);

            ///diagonal 2
            List<RingsController> lineD2 = new List<RingsController>();
            for (int x = 0; x < bricksCount.x; x++)
            {
                int y = bricksCount.y - 1 - x;
                lineD2.Add(rings[x, y]);
            }
            CheckLine(rings[0, bricksCount.x - 1].GetColors(), lineD2);

            if (lines.Count == 0)
            {
                SpawnNextRing();
                SaveGame();
                return;
            }
        
            CountScore();
            StartCoroutine(DrawLines());
        }

        void CheckLine(List<int> colors, List<RingsController> ringsLine)
        {
            if (colors.Count == 0) return;

            List<int> colorsToRemove = new List<int>();

            foreach (int color in colors)
            {
                if (ringsLine.Any(r => r.ringStates.All(s => s != color))) continue;

                colorsToRemove.Add(color);
            }

            if (colorsToRemove.Count == 0) return;

            lines.Add(new RingsLine(ringsLine, colorsToRemove));
        }

        void CheckGameOver()
        {
            bool gameOver = true;
        
            for (int x = 0; x < bricksCount.x; x++)
            {
                for (int y = 0; y < bricksCount.y; y++)
                {
                    if (rings[x, y].IsFull) continue;
                
                    gameOver = false;
                    break;
                }
            }

            if (!gameOver) return;

            gameState.IsGameOver = true;
            UserProgress.Current.SaveGameState(name);

            OnGameOver();
        }
    
        void OnColorMerge(RingsController ring, int value)
        {
            List<RingsController> colorToRemove = new List<RingsController>();

            for (int x = 0; x < bricksCount.x; x++)
            {
                for (int y = 0; y < bricksCount.y; y++)
                {
                    if (rings[x, y].ringStates.Any(s => s == value))
                        colorToRemove.Add(rings[x, y]);
                }
            }

            currentRing = ring;
            colorToRemove.ForEach(r => r.Clear(value));
            soundCollection.GetSfx(SoundId.Merging).Play();
            GetCoins();
        }
    
    
        IEnumerator DrawLines()
        {
            yield return new WaitForSeconds(0.15f);

            foreach (RingsLine line in lines)
                CreateLine(line);
        
            soundCollection.GetSfx(SoundId.Merging).Play();

            yield return new WaitForSeconds(0.65f);

            GetCoins();
            SpawnNextRing();
            SaveGame();
        }
    
        void CountScore()
        {
            int ringsCount = 0;

            foreach (RingsLine line in lines)
            {
                foreach (int color in line.colors)
                {
                    line.rings.ForEach(r => ringsCount += r.ringStates.Count(s => s == color));
                    line.rings.ForEach(i => i.Clear(color));
                }
            }

            gameState.Score += ringsCount * lines.Count;
            OnScoreUpdate();
        }

        void GetCoins()
        {
            if (!(Random.Range(0f, 1f) < coinProbability) || currentRing == null) return;
        
            UserProgress.Current.Coins++;

            GameObject vfx = Resources.Load<GameObject>("CoinVFX");
            vfx = Instantiate(vfx, fieldTransform.parent);
            vfx.transform.position = currentRing.transform.position;

            Destroy(vfx, 1.5f);
        }

        void CreateLine(RingsLine line)
        {
            GameObject vfx = Resources.Load<GameObject>("LineVFX");
            Vector3 rotateDirection = line.rings[line.rings.Count - 1].Position - line.rings[0].Position;

            float size = (float)Math.Sqrt(Math.Pow(fieldTransform.rect.width, 2) + Math.Pow(fieldTransform.rect.height, 2));
            vfx = Instantiate(vfx, lineParent);
            vfx.transform.position = line.rings[1].Position;
            vfx.transform.localRotation = Quaternion.FromToRotation(Vector3.up, rotateDirection);
            vfx.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
            vfx.GetComponent<Image>().color = ThemeController.Instance.CurrentTheme.GetColor(ColorType.BrickSprite, line.colors[0]);
        
            Destroy(vfx, 1f);
        }

        void HighlightField(NextRingController ring, bool value)
        {
            for (int x = 0; x < bricksCount.x; x++)
            {
                for (int y = 0; y < bricksCount.y; y++)
                {
                    if(!value)
                        rings[x, y].Highlight(false);
                    else if(rings[x, y].CanMerge(ring.ringsController.ringStates))
                        rings[x, y].Highlight(true);
                }
            }
        }

        protected override void HighlightBricks(bool active)
        {
            for (int x = 0; x < bricksCount.x; x++)
            {
                for (int y = 0; y < bricksCount.y; y++)
                {
                    if(rings[x, y] == null) continue;

                    switch (active)
                    {
                        case true when !rings[x, y].IsEmpty:
                        case false:
                            SetSortingOrder(rings[x, y].gameObject, active);
                            break;
                    }
                }
            }
        }

        protected override void HighlightFigures(bool active)
        {
            if (!nextRing.gameObject.TryGetComponent(out SortingOrderApplier applier))
                applier = nextRing.gameObject.AddComponent<SortingOrderApplier>();

            if (active)
                applier.SetSortingOrder(HighlightSortingOrder);
            else
                applier.Hide();
        }

        public override void LastChance(LastChance lastChance)
        {
            switch (lastChance.LastChanceType)
            {
                case LastChanceType.Numbers:
                    Debug.LogError("Not implemented!");
                    return;
                case LastChanceType.CrossLines:
                    ClearCrossLinesRings.Execute(rings);
                    break;
                case LastChanceType.LinesHorizontal:
                    ClearHorizontalLinesRings.Execute(rings, 1);
                    break;
                case LastChanceType.LinesVertical:
                    ClearVerticalLinesRings.Execute(rings, 1);
                    break;
                case LastChanceType.Explosion:
                    ExplosionRings.Execute(rings);
                    break;
            }

            OnLastChanceCompleted();
        }

        protected override void OnLastChanceCompleted()
        {
            gameState.IsGameOver = false;
            gameState.ClearSave();
            SaveGame();
        }

        protected override void BoosterExecute<T>(T target)
        {
            if (boosterType != BoosterType.Undo) 
                SaveGameState();
            
            bool boosterProceeded = true;

            switch (boosterType)
            {
                case BoosterType.Undo:
                    Undo.ClearGame(rings);
                    OnClearGame();
                    boosterProceeded = Undo.Execute(gameState, StartGame);
                    break;
                case BoosterType.ClearBrick when target is NumberedBrick brick:
                    ClearRing.Execute(rings, GetCoords(brick));
                    break;
                case BoosterType.ClearNumber:
                    Debug.LogError("Not implemented!");
                    boosterProceeded = false;
                    break;
                case BoosterType.Explosion:
                    ExplosionRings.Execute(rings);
                    break;
                case BoosterType.RemoveFigure when target is NextRingController:
                    RemoveFigureRings.Execute(nextRing);
                    OnFigureRemoved(null);
                    break;
                case BoosterType.ClearHorizontalLine when target is NumberedBrick brick:
                    ClearHorizontalLinesRings.Execute(rings, GetCoords(brick).y);
                    break;
                case BoosterType.ClearVerticalLine when target is NumberedBrick brick:
                    ClearVerticalLinesRings.Execute(rings, GetCoords(brick).x);
                    break;
            }

            if(boosterType != BoosterType.Undo)
                OnBoostersComplete();
        
            BoostersController.Instance.OnBoosterProceeded(boosterProceeded);
        }

        protected override void OnFigureRemoved(FigureController figure)
        {
            SpawnNextRing();
        }

        protected override void OnClearGame()
        {
            Destroy(nextRing.gameObject);
        }

        void OnScoreUpdate()
        {
            int currentUpgrade = Array.FindLastIndex(scoresForUpgrade, s => s <= gameState.Score) + 1;
            currentColorsCount = Mathf.Clamp(startColorsCount + currentUpgrade, 0, colorsCount);
            GetProbabilities();
        }

        List<int[]> FreeBricks(int ringsCount)
        {
            List<int[]> states = new List<int[]>();
        
            for (int x = 0; x < bricksCount.x; x++)
            {
                for (int y = 0; y < bricksCount.y; y++)
                {
                    if(ringsCount < 0 || rings[x, y].FreeStates - 1 < ringsCount) continue;
                    states.Add(rings[x, y].ringStates);
                }
            }
        
            return states;
        }

        void GetProbabilities()
        {
            colorsIndexes = new int[currentColorsCount];
            probabilities = new float[currentColorsCount];

            for (int i = 0; i < currentColorsCount; i++)
            {
                colorsIndexes[i] = i;
                probabilities[i] = colorsProbabilities[i];
            }
        }

        class RingsLine
        {
            public List<RingsController> rings;
            public List<int> colors;

            public RingsLine(List<RingsController> rings, List<int> colors)
            {
                this.rings = rings;
                this.colors = colors;
            }
        }
    }
}
