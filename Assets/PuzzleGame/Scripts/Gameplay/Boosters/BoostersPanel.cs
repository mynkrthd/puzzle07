using PuzzleGame.Ads;
using PuzzleGame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Gameplay.Boosters
{
    public class BoostersPanel : Panel
    {
        [SerializeField] private Image icon;
        [SerializeField] private Text description;
        [SerializeField] private PriceLabel priceLabel;
        [SerializeField] private Text count;
    
        [SerializeField]
        MonetizeButton monetizeButton;

        private BoosterPreset preset;

        string Item { get; set; }

        void Start()
        {
            monetizeButton.PurchaseComplete += OnPurchaseComplete;
        }

        public void Init(BoosterPreset preset)
        {
            this.preset = preset;
            Show();
        }

        protected override void Awake()
        {
            foreach (Button hideButton in hideButtons)
                hideButton.onClick.AddListener(Hide);
        }

        protected override void Show()
        {
            UpdatePanel();
            content.SetActive(true);
        }
    
        protected override void Hide()
        {
            content.SetActive(false);
        }

        private void UpdatePanel()
        {
            Item = UserProgress.Current.CurrentGameId + preset.name;

            icon.sprite = preset.Icon;
            count.text = "x " + preset.CountToBuy;
            description.text = preset.Description;
            priceLabel.SetPrice(Item, preset.Price);

            monetizeButton.SetPrice(Item, preset.Price);
            monetizeButton.gameObject.SetActive(true);
        }
    
        void OnPurchaseComplete()
        {
            BoostersController.Instance.OnBoosterPurchased(preset);

            Hide();
        }
    }
}
