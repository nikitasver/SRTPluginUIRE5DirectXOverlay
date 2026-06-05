using GameOverlay.Drawing;
using GameOverlay.Windows;
using SRTPluginBase;
using SRTPluginProviderRE5;
using SRTPluginProviderRE5.Structs;
using SRTPluginProviderRE5.Structs.GameStructs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace SRTPluginUIRE5DirectXOverlay
{
    public class SRTPluginUIRE5DirectXOverlay : PluginBase, IPluginUI
    {
        internal static PluginInfo _Info = new PluginInfo();
        public override IPluginInfo Info => _Info;
        public string RequiredProvider => "SRTPluginProviderRE5";
        private IPluginHostDelegates hostDelegates;
        private IGameMemoryRE5 gameMemory;
        private GameMemoryRE5 gameMemoryConcrete;

        private OverlayWindow _window;
        private Graphics _graphics;

        private Font _consolasBold;

        private SolidBrush _black, _white, _grey, _darkred, _red, _lightred, _lightyellow,
                           _lightgreen, _lawngreen, _goldenrod, _greydark, _greydarker,
                           _darkgreen, _darkyellow, _lightpurple, _darkpurple, _orange;

        public PluginConfiguration config;
        private float FONT => config.FontSize;
        private float LINE => config.FontSize * 1.5f;
        private Process GetProcess() => Process.GetProcessesByName("re5dx9")?.FirstOrDefault();
        private Process gameProcess;
        private IntPtr gameWindowHandle;

        SolidBrush HPBarColor;
        SolidBrush TextColor;
        private string PlayerName = "";

        [STAThread]
        public override int Startup(IPluginHostDelegates hostDelegates)
        {
            this.hostDelegates = hostDelegates;
            config = LoadConfiguration<PluginConfiguration>();

            gameProcess = GetProcess();
            if (gameProcess == default)
                return 1;

            IList<IntPtr> windows = PInvoke.GetWindowHandles(gameProcess);
            if (windows.Count > 0)
                gameWindowHandle = windows[0];
            else
                return 1;

            DEVMODE devMode = default;
            devMode.dmSize = (short)Marshal.SizeOf<DEVMODE>();
            PInvoke.EnumDisplaySettings(null, -1, ref devMode);

            _window = new OverlayWindow(0, 0, devMode.dmPelsWidth, devMode.dmPelsHeight);
            _window?.Create();

            _graphics = new Graphics()
            {
                MeasureFPS = false,
                PerPrimitiveAntiAliasing = false,
                TextAntiAliasing = true,
                UseMultiThreadedFactories = false,
                VSync = false,
                Width = _window.Width,
                Height = _window.Height,
                WindowHandle = _window.Handle
            };
            _graphics?.Setup();

            _consolasBold = _graphics?.CreateFont("Consolas", config.FontSize, true);

            _black = _graphics?.CreateSolidBrush(0, 0, 0);
            _white = _graphics?.CreateSolidBrush(255, 255, 255);
            _grey = _graphics?.CreateSolidBrush(128, 128, 128);
            _greydark = _graphics?.CreateSolidBrush(64, 64, 64);
            _greydarker = _graphics?.CreateSolidBrush(24, 24, 24, 100);
            _darkred = _graphics?.CreateSolidBrush(153, 0, 0, 100);
            _darkgreen = _graphics?.CreateSolidBrush(0, 102, 0, 100);
            _darkyellow = _graphics?.CreateSolidBrush(218, 165, 32, 100);
            _red = _graphics?.CreateSolidBrush(255, 0, 0);
            _lightred = _graphics?.CreateSolidBrush(255, 172, 172);
            _lightyellow = _graphics?.CreateSolidBrush(255, 255, 150);
            _lightgreen = _graphics?.CreateSolidBrush(150, 255, 150);
            _lawngreen = _graphics?.CreateSolidBrush(124, 252, 0);
            _goldenrod = _graphics?.CreateSolidBrush(218, 165, 32);
            _lightpurple = _graphics?.CreateSolidBrush(222, 182, 255);
            _darkpurple = _graphics?.CreateSolidBrush(73, 58, 85, 100);
            _orange = _graphics?.CreateSolidBrush(255, 165, 0);

            HPBarColor = _grey;
            TextColor = _white;

            return 0;
        }

        public override int Shutdown()
        {
            SaveConfiguration(config);
            _black?.Dispose(); _white?.Dispose(); _grey?.Dispose();
            _greydark?.Dispose(); _greydarker?.Dispose();
            _darkred?.Dispose(); _darkgreen?.Dispose(); _darkyellow?.Dispose();
            _red?.Dispose(); _lightred?.Dispose(); _lightyellow?.Dispose();
            _lightgreen?.Dispose(); _lawngreen?.Dispose(); _goldenrod?.Dispose();
            _lightpurple?.Dispose(); _darkpurple?.Dispose(); _orange?.Dispose();
            _consolasBold?.Dispose();
            _graphics?.Dispose(); _graphics = null;
            _window?.Dispose(); _window = null;
            gameProcess?.Dispose(); gameProcess = null;
            return 0;
        }

        public int ReceiveData(object gameMemory)
        {
            this.gameMemory = (IGameMemoryRE5)gameMemory;
            this.gameMemoryConcrete = (GameMemoryRE5)gameMemory;
            _window?.PlaceAbove(gameWindowHandle);
            _window?.FitTo(gameWindowHandle, true);

            try
            {
                _graphics?.BeginScene();
                _graphics?.ClearScene();
                DrawOverlay();
            }
            catch (Exception ex)
            {
                hostDelegates.ExceptionMessage.Invoke(ex);
            }
            finally
            {
                _graphics?.EndScene();
            }
            return 0;
        }

        private void SetColors(GamePlayer player)
        {
            if (player.HealthState == PlayerStatus.Fine)
            { HPBarColor = _darkgreen; TextColor = _lightgreen; return; }
            else if (player.HealthState == PlayerStatus.Caution)
            { HPBarColor = _darkyellow; TextColor = _lightyellow; return; }
            else if (player.HealthState == PlayerStatus.Danger)
            { HPBarColor = _darkred; TextColor = _lightred; return; }
            else
            { HPBarColor = _greydarker; TextColor = _white; return; }
        }

        private void DrawOverlay()
        {
            float baseXOffset = config.PositionX;
            float baseYOffset = config.PositionY;
            float x = baseXOffset + 5f;
            float y = baseYOffset;

            _graphics?.DrawText(_consolasBold, FONT * 1.25f, _white, x + 10, y += LINE, "IGT: ");
            _graphics?.DrawText(_consolasBold, FONT * 1.25f, _lawngreen, x + 10f + GetStringSize("IGT: ", FONT * 1.25f) + 10f, y, gameMemory.IGTFormattedString);

            if (config.Debug)
            {
                _graphics?.DrawText(_consolasBold, FONT, _grey, x, y += LINE, string.Format("Raw IGT: {0:F3}s", gameMemory.IGT));
            }

            if (config.ShowBothPlayers)
            {
                PlayerName = "Chris: ";
                SetColors(gameMemory.Player);
                if (config.ShowHPBars)
                {
                    DrawHealthBar(ref x, ref y, PlayerName,
                        gameMemory.Player.CurrentHP, gameMemory.Player.MaxHP, gameMemory.Player.Percentage);
                }
                else
                {
                    y += LINE;
                    _graphics?.DrawText(_consolasBold, FONT, TextColor, x + 10f, y,
                        string.Format("{0}{1} / {2} ({3:P1})", PlayerName,
                            gameMemory.Player.CurrentHP, gameMemory.Player.MaxHP, gameMemory.Player.Percentage));
                }

                PlayerName = "Sheva: ";
                SetColors(gameMemory.Player2);
                if (config.ShowHPBars)
                {
                    DrawHealthBar(ref x, ref y, PlayerName,
                        gameMemory.Player2.CurrentHP, gameMemory.Player2.MaxHP, gameMemory.Player2.Percentage);
                }
                else
                {
                    y += LINE;
                    _graphics?.DrawText(_consolasBold, FONT, TextColor, x + 10f, y,
                        string.Format("{0}{1} / {2} ({3:P1})", PlayerName,
                            gameMemory.Player2.CurrentHP, gameMemory.Player2.MaxHP, gameMemory.Player2.Percentage));
                }

                x = baseXOffset + 5f;

                _graphics?.DrawText(_consolasBold, FONT, _white, x + 10, y += LINE,
                    string.Format("Chris Kills: {0}", gameMemory.ChrisKills));
                _graphics?.DrawText(_consolasBold, FONT, _white, x + 10, y += LINE,
                    string.Format("Sheva Kills: {0}", gameMemory.ShevaKills));

                if (config.ShowShots)
                {
                    _graphics?.DrawText(_consolasBold, FONT, _white, x + 10, y += LINE,
                        string.Format("Shots: {0} | Hit: {1} | Acc: {2:P1}",
                            gameMemory.ShotsFired, gameMemory.EnemiesHit,
                            gameMemory.ShotsFired > 0 ? (float)gameMemory.EnemiesHit / gameMemory.ShotsFired : 0f));

                    _graphics?.DrawText(_consolasBold, FONT, _grey, x + 10, y += LINE,
                        string.Format("Shots2: {0} | Hit2: {1} | Acc2: {2:P1}",
                            gameMemory.ShotsFired2, gameMemory.EnemiesHit2,
                            gameMemory.ShotsFired2 > 0 ? (float)gameMemory.EnemiesHit2 / gameMemory.ShotsFired2 : 0f));
                }
            }
            else
            {
                PlayerName = "Chris: ";
                SetColors(gameMemory.Player);
                if (config.ShowHPBars)
                {
                    DrawHealthBar(ref x, ref y, PlayerName,
                        gameMemory.Player.CurrentHP, gameMemory.Player.MaxHP, gameMemory.Player.Percentage);
                }
                else
                {
                    y += LINE;
                    _graphics?.DrawText(_consolasBold, FONT, TextColor, x + 10f, y,
                        string.Format("{0}{1} / {2} ({3:P1})", PlayerName,
                            gameMemory.Player.CurrentHP, gameMemory.Player.MaxHP, gameMemory.Player.Percentage));
                }

                _graphics?.DrawText(_consolasBold, FONT, _white, x + 10, y += LINE,
                    string.Format("Kills: {0}", gameMemory.ChrisKills));
                if (config.ShowShots)
                {
                    _graphics?.DrawText(_consolasBold, FONT, _white, x + 10, y += LINE,
                        string.Format("Shots: {0} | Hit: {1} | Acc: {2:P1}",
                            gameMemory.ShotsFired, gameMemory.EnemiesHit,
                            gameMemory.ShotsFired > 0 ? (float)gameMemory.EnemiesHit / gameMemory.ShotsFired : 0f));
                }
            }

            x = baseXOffset + 5f;

            if (config.ShowDeaths)
                _graphics?.DrawText(_consolasBold, FONT, _white, x + 10, y += LINE,
                    string.Format("Deaths: {0}", gameMemory.Deaths));
            else
                y += LINE;

            _graphics?.DrawText(_consolasBold, FONT, _goldenrod, x + 10, y += LINE,
                string.Format("Money: {0}", gameMemory.Money));

            _graphics?.DrawText(_consolasBold, FONT, _grey, x + 10, y += LINE,
                string.Format("Chapter {0}", GetChapterName(gameMemory.Chapter)));

            _graphics?.DrawText(_consolasBold, FONT, _white, x + 10, y += LINE,
                string.Format("Chris DA: {0} (Rank: {1})", gameMemory.ChrisDA, gameMemory.ChrisDARank));

            _graphics?.DrawText(_consolasBold, FONT, _white, x + 10, y += LINE,
                string.Format("Sheva DA: {0} (Rank: {1})", gameMemory.ShevaDA, gameMemory.ShevaDARank));

            if (gameMemoryConcrete.IsSRank)
                _graphics?.DrawText(_consolasBold, FONT, _lawngreen, x + 10, y += LINE, "S-RANK ELIGIBLE (Chris)");

            if (gameMemoryConcrete.IsSRank2)
                _graphics?.DrawText(_consolasBold, FONT, _lightgreen, x + 10, y += LINE, "S-RANK ELIGIBLE (Sheva)");

            _graphics?.DrawText(_consolasBold, FONT, _grey, x + 10, y += LINE,
                string.Format("Kills needed for S: {0}", gameMemoryConcrete.KillsRequired));

float enemyX = config.EnemyHPPositionX == -1 ? baseXOffset + 5f : config.EnemyHPPositionX;
            float enemyY = config.EnemyHPPositionY == -1 ? y + LINE : config.EnemyHPPositionY;

            foreach (EnemyHP enemy in gameMemoryConcrete.EnemyHealth.Where(a => a.IsAlive))
            {
                _graphics?.DrawText(_consolasBold, FONT * 0.75f, _lightred, enemyX + 5f, enemyY,
                    string.Format("{0}/{1}", enemy.CurrentHP, enemy.MaximumHP));
                enemyY += FONT;
            }
        }

private void DrawHealthBar(ref float xOffset, ref float yOffset, string name, float currentHP, float maxHP, float percentage)
        {
            yOffset += LINE;
            float textOffsetX = xOffset + 10f;

            _graphics?.DrawText(_consolasBold, FONT, TextColor, textOffsetX, yOffset,
                string.Format("{0}{1} / {2} ({3:P1})", name, currentHP, maxHP, percentage));
        }

        private string GetChapterName(int chapter)
        {
            switch (chapter)
            {
                case 0: return "1-1: Assembly Place";
                case 1: return "1-2: A Slum";
                case 2: return "2-1: The Underground";
                case 3: return "2-2: The Mines";
                case 4: return "2-3: Execution Path";
                case 5: return "3-1: The Marshlands";
                case 6: return "3-2: The Ruins";
                case 7: return "3-3: The Ship Deck";
                case 8: return "4-1: The Boat";
                case 9: return "4-2: The Cargo";
                case 10: return "5-1: The Island";
                case 11: return "5-2: Laboratory";
                case 12: return "5-3: The Reactor";
                case 13: return "6-1: The City";
                case 14: return "6-2: The Maze";
                case 15: return "6-3: The Roof";
                default: return string.Format("{0}", chapter);
            }
        }

        private float GetStringSize(string str, float size)
            => (float)_graphics?.MeasureString(_consolasBold, size, str).X;
    }
}
