using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WingNuts.Player;

namespace WingNuts.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Fuel Ring")]
        [SerializeField] Image fuelRing;

        [Header("Shield Rings (outer to inner)")]
        [SerializeField] Image shieldRing1; // 81-100  green
        [SerializeField] Image shieldRing2; // 61-80   yellow-green
        [SerializeField] Image shieldRing3; // 41-60   yellow
        [SerializeField] Image shieldRing4; // 21-40   orange
        [SerializeField] Image shieldRing5; // 1-20    red

        [Header("Text Labels")]
        [SerializeField] TMP_Text shieldsLabel;
        [SerializeField] TMP_Text fuelLabel;
        [SerializeField] TMP_Text scoreLabel;
        [SerializeField] TMP_Text livesLabel;

        void Start()
        {
            if (PlayerStats.Instance == null) return;
            PlayerStats.Instance.OnShieldsChanged += OnShieldsChanged;
            PlayerStats.Instance.OnFuelChanged    += OnFuelChanged;
            PlayerStats.Instance.OnScoreChanged   += OnScoreChanged;
            PlayerStats.Instance.OnLivesChanged   += OnLivesChanged;

            // Populate HUD with starting values immediately.
            OnShieldsChanged(PlayerStats.Instance.Shields);
            OnFuelChanged(PlayerStats.Instance.Fuel);
            OnScoreChanged(PlayerStats.Instance.Score);
            OnLivesChanged(PlayerStats.Instance.Lives);
        }

        void OnDestroy()
        {
            if (PlayerStats.Instance == null) return;
            PlayerStats.Instance.OnShieldsChanged -= OnShieldsChanged;
            PlayerStats.Instance.OnFuelChanged    -= OnFuelChanged;
            PlayerStats.Instance.OnScoreChanged   -= OnScoreChanged;
            PlayerStats.Instance.OnLivesChanged   -= OnLivesChanged;
        }

        static readonly Color RingDimColor   = new Color(1f, 1f, 1f, 0.2f);
        static readonly Color RingLitColor   = new Color(1f, 1f, 1f, 1.0f);

        static readonly Color[] RingColors = new Color[]
        {
            new Color(0.2f, 0.9f, 0.2f), // ring 1  green
            new Color(0.6f, 0.9f, 0.2f), // ring 2  yellow-green
            new Color(0.9f, 0.9f, 0.2f), // ring 3  yellow
            new Color(0.9f, 0.5f, 0.1f), // ring 4  orange
            new Color(0.9f, 0.1f, 0.1f), // ring 5  red
        };

        public void OnShieldsChanged(int shields)
        {
            if (shieldsLabel != null) shieldsLabel.text = shields.ToString();
            UpdateShieldRings(shields);
        }

        public void OnFuelChanged(int fuel)
        {
            if (fuelLabel != null) fuelLabel.text = fuel.ToString();
            if (fuelRing  != null)
            {
                fuelRing.fillAmount = fuel / 100f;
                fuelRing.color      = FuelColor(fuel);
            }
        }

        public void OnScoreChanged(int score)
        {
            if (scoreLabel != null) scoreLabel.text = score.ToString();
        }

        public void OnLivesChanged(int lives)
        {
            if (livesLabel != null) livesLabel.text = lives.ToString();
        }

        void UpdateShieldRings(int shields)
        {
            SetRing(shieldRing1, shields, 81, 100, 0);
            SetRing(shieldRing2, shields, 61,  80, 1);
            SetRing(shieldRing3, shields, 41,  60, 2);
            SetRing(shieldRing4, shields, 21,  40, 3);
            SetRing(shieldRing5, shields,  1,  20, 4);
        }

        void SetRing(Image ring, int shields, int lo, int hi, int colorIdx)
        {
            if (ring == null) return;
            bool lit  = shields >= lo;
            ring.color = lit ? RingColors[colorIdx] : RingDimColor;
        }

        static Color FuelColor(int fuel)
        {
            if (fuel > 50) return Color.Lerp(Color.yellow, Color.green,  (fuel - 50) / 50f);
            if (fuel > 25) return Color.Lerp(new Color(1f, 0.5f, 0f), Color.yellow, (fuel - 25) / 25f);
            return new Color(1f, 0.2f, 0.2f);
        }
    }
}
