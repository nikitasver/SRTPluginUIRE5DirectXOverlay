namespace SRTPluginUIRE5DirectXOverlay
{
    /// <summary>Configuration for the RE5 DirectX overlay plugin — all values serialise as simple true/false or numbers for easy editing.</summary>
    public class PluginConfiguration
    {
        /// <summary>Show debug info (raw IGT in seconds). Default: true.</summary>
        public bool Debug { get; set; }
        /// <summary>Reserved — currently behaves the same as text-only HP display. Default: false.</summary>
        public bool ShowHPBars { get; set; }
        /// <summary>Show stats for both Chris &amp; Sheva; false = Chris only. Default: true.</summary>
        public bool ShowBothPlayers { get; set; }

        /// <summary>Show in-game timer. Default: true.</summary>
        public bool ShowIGT { get; set; }
        /// <summary>Show player HP. Default: true.</summary>
        public bool ShowCharacterHP { get; set; }
        /// <summary>Show kill counters. Default: true.</summary>
        public bool ShowKills { get; set; }
        /// <summary>Show death counter. Default: false.</summary>
        public bool ShowDeaths { get; set; }
        /// <summary>Show money / currency. Default: true.</summary>
        public bool ShowMoney { get; set; }
        /// <summary>Show current chapter name. Default: true.</summary>
        public bool ShowChapter { get; set; }
        /// <summary>Show Death Accuracy &amp; rank for both players. Default: true.</summary>
        public bool ShowDAs { get; set; }
        /// <summary>Show kills needed for S-rank. Default: true.</summary>
        public bool ShowKillsNeeded { get; set; }
        /// <summary>Show enemy HP list. Default: true.</summary>
        public bool ShowEnemyHP { get; set; }

        /// <summary>Base font size in pixels. All text and spacing scale from this. Default: 24.</summary>
        public float FontSize { get; set; }
        /// <summary>Horizontal offset of main HUD from left edge. Default: 5.</summary>
        public float PositionX { get; set; }
        /// <summary>Vertical offset of main HUD from top edge. Default: 50.</summary>
        public float PositionY { get; set; }
        /// <summary>Horizontal offset for enemy HP list. -1 = auto-place below main HUD. Default: -1.</summary>
        public float EnemyHPPositionX { get; set; }
        /// <summary>Vertical offset for enemy HP list. -1 = auto-place below main HUD. Default: -1.</summary>
        public float EnemyHPPositionY { get; set; }

        public PluginConfiguration()
        {
            Debug = true;
            ShowHPBars = false;
            ShowBothPlayers = true;

            ShowIGT = true;
            ShowCharacterHP = true;
            ShowKills = true;
            ShowDeaths = false;
            ShowMoney = true;
            ShowChapter = true;
            ShowDAs = true;
            ShowKillsNeeded = true;
            ShowEnemyHP = true;

            FontSize = 24f;
            PositionX = 5f;
            PositionY = 50f;
            EnemyHPPositionX = -1;
            EnemyHPPositionY = -1;
        }
    }
}