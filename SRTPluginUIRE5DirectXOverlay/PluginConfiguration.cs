namespace SRTPluginUIRE5DirectXOverlay
{
    public class PluginConfiguration
    {
        public bool Debug { get; set; }
        public bool NoInventory { get; set; }
        public bool ShowHPBars { get; set; }
        public bool ShowDamagedEnemiesOnly { get; set; }
        public bool ShowBossOnly { get; set; }
        public bool ShowDeaths { get; set; }
        public bool ShowShots { get; set; }
        public bool ShowBothPlayers { get; set; }
        public float FontSize { get; set; }
        public float ScalingFactor { get; set; }

        public float PositionX { get; set; }
        public float PositionY { get; set; }

        public float EnemyHPPositionX { get; set; }
        public float EnemyHPPositionY { get; set; }

        public float InventoryPositionX { get; set; }
        public float InventoryPositionY { get; set; }

        public string StringFontName { get; set; }

        public PluginConfiguration()
        {
            Debug = true;
            NoInventory = true;
            ShowHPBars = false;
            ShowDamagedEnemiesOnly = false;
            ShowBossOnly = false;
            ShowDeaths = false;
            ShowShots = false;
            ShowBothPlayers = true;
            FontSize = 24f;
            ScalingFactor = 1f;
            PositionX = 5f;
            PositionY = 50f;
            EnemyHPPositionX = -1;
            EnemyHPPositionY = -1;
            InventoryPositionX = -1;
            InventoryPositionY = -1;
            StringFontName = "Courier New";
        }
    }
}
