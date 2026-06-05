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

        private Dictionary<string, SolidBrush> _brushes;

        public PluginConfiguration config;
        private float FONT => config.FontSize;
        private float LINE => config.FontSize * 1.5f;

        private Process _gameProcess;
        private IntPtr _gameWindowHandle;

        private static Process GetProcess() => Process.GetProcessesByName("re5dx9")?.FirstOrDefault();

        private static readonly Dictionary<string, (byte r, byte g, byte b, byte a)> _brushDefs = new()
        {
            ["black"] = (0, 0, 0, 255),
            ["white"] = (255, 255, 255, 255),
            ["grey"] = (128, 128, 128, 255),
            ["greydark"] = (64, 64, 64, 255),
            ["greydarker"] = (24, 24, 24, 100),
            ["darkred"] = (153, 0, 0, 100),
            ["darkgreen"] = (0, 102, 0, 100),
            ["darkyellow"] = (218, 165, 32, 100),
            ["red"] = (255, 0, 0, 255),
            ["lightred"] = (255, 172, 172, 255),
            ["lightyellow"] = (255, 255, 150, 255),
            ["lightgreen"] = (150, 255, 150, 255),
            ["lawngreen"] = (124, 252, 0, 255),
            ["goldenrod"] = (218, 165, 32, 255),
            ["lightpurple"] = (222, 182, 255, 255),
            ["darkpurple"] = (73, 58, 85, 100),
            ["orange"] = (255, 165, 0, 255),
        };

        [STAThread]
        public override int Startup(IPluginHostDelegates hostDelegates)
        {
            this.hostDelegates = hostDelegates;
            config = LoadConfiguration<PluginConfiguration>();

            _gameProcess = GetProcess();
            if (_gameProcess == default)
                return 1;

            IList<IntPtr> windows = PInvoke.GetWindowHandles(_gameProcess);
            if (windows.Count > 0)
                _gameWindowHandle = windows[0];
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

            _brushes = new Dictionary<string, SolidBrush>(_brushDefs.Count);
            foreach (var kv in _brushDefs)
                _brushes[kv.Key] = _graphics?.CreateSolidBrush(kv.Value.r, kv.Value.g, kv.Value.b, kv.Value.a);

            return 0;
        }

        public override int Shutdown()
        {
            SaveConfiguration(config);
            foreach (var brush in _brushes.Values)
                brush?.Dispose();
            _brushes.Clear();
            _consolasBold?.Dispose();
            _graphics?.Dispose();
            _window?.Dispose();
            _gameProcess?.Dispose();
            return 0;
        }

        public int ReceiveData(object gameMemory)
        {
            this.gameMemory = (IGameMemoryRE5)gameMemory;
            this.gameMemoryConcrete = (GameMemoryRE5)gameMemory;
            _window?.PlaceAbove(_gameWindowHandle);
            _window?.FitTo(_gameWindowHandle, true);

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

        private (SolidBrush bar, SolidBrush text) GetStatusColors(PlayerStatus status) => status switch
        {
            PlayerStatus.Fine => (_brushes["darkgreen"], _brushes["lightgreen"]),
            PlayerStatus.Caution => (_brushes["darkyellow"], _brushes["lightyellow"]),
            PlayerStatus.Danger => (_brushes["darkred"], _brushes["lightred"]),
            _ => (_brushes["greydarker"], _brushes["white"]),
        };

        private void DrawOverlay()
        {
            float x = config.PositionX + 5f;
            float y = config.PositionY;
            var g = _graphics;

            if (config.ShowIGT)
                DrawIGT(ref x, ref y, g);

            if (config.ShowCharacterHP)
                DrawCharacterHP(ref x, ref y, g);

            x = config.PositionX + 5f;

            if (config.ShowKills)
                DrawKills(ref x, ref y, g);

            if (config.ShowDeaths)
                g?.DrawText(_consolasBold, FONT, _brushes["white"], x + 10, y += LINE,
                    $"Deaths: {gameMemory.Deaths}");

            if (config.ShowMoney)
                g?.DrawText(_consolasBold, FONT, _brushes["goldenrod"], x + 10, y += LINE,
                    $"Money: {gameMemory.Money}");

            if (config.ShowChapter)
                g?.DrawText(_consolasBold, FONT, _brushes["grey"], x + 10, y += LINE,
                    $"Chapter {GetChapterName(gameMemory.Chapter)}");

            if (config.ShowDAs)
                DrawDeathAccuracy(ref x, ref y, g);

            if (gameMemoryConcrete.IsSRank)
                g?.DrawText(_consolasBold, FONT, _brushes["lawngreen"], x + 10, y += LINE,
                    "S-RANK ELIGIBLE (Chris)");

            if (gameMemoryConcrete.IsSRank2)
                g?.DrawText(_consolasBold, FONT, _brushes["lightgreen"], x + 10, y += LINE,
                    "S-RANK ELIGIBLE (Sheva)");

            if (config.ShowKillsNeeded)
                g?.DrawText(_consolasBold, FONT, _brushes["grey"], x + 10, y += LINE,
                    $"Kills needed for S: {gameMemoryConcrete.KillsRequired}");

            if (config.ShowEnemyHP)
                DrawEnemyHP(y, g);
        }

        private void DrawIGT(ref float x, ref float y, Graphics g)
        {
            float labelWidth = MeasureString("IGT: ", FONT * 1.25f);

            g?.DrawText(_consolasBold, FONT * 1.25f, _brushes["white"], x + 10, y += LINE, "IGT: ");
            g?.DrawText(_consolasBold, FONT * 1.25f, _brushes["lawngreen"], x + 10f + labelWidth + 10f, y,
                gameMemory.IGTFormattedString);

            if (config.Debug)
                g?.DrawText(_consolasBold, FONT, _brushes["grey"], x, y += LINE,
                    $"Raw IGT: {gameMemory.IGT:F3}s");
        }

        private void DrawCharacterHP(ref float x, ref float y, Graphics g)
        {
            DrawPlayerHP("Chris", gameMemory.Player, ref x, ref y, g);

            if (config.ShowBothPlayers)
            {
                DrawPlayerHP("Sheva", gameMemory.Player2, ref x, ref y, g);
                x = config.PositionX + 5f;
            }
        }

        private void DrawPlayerHP(string playerName, GamePlayer player, ref float x, ref float y, Graphics g)
        {
            var (_, textBrush) = GetStatusColors(player.HealthState);

            y += LINE;
            g?.DrawText(_consolasBold, FONT, textBrush, x + 10f, y,
                $"{playerName}: {player.CurrentHP} / {player.MaxHP} ({player.Percentage:P1})");
        }

        private void DrawKills(ref float x, ref float y, Graphics g)
        {
            if (config.ShowBothPlayers)
            {
                g?.DrawText(_consolasBold, FONT, _brushes["white"], x + 10, y += LINE,
                    $"Chris Kills: {gameMemory.ChrisKills}");
                g?.DrawText(_consolasBold, FONT, _brushes["white"], x + 10, y += LINE,
                    $"Sheva Kills: {gameMemory.ShevaKills}");
            }
            else
            {
                g?.DrawText(_consolasBold, FONT, _brushes["white"], x + 10, y += LINE,
                    $"Kills: {gameMemory.ChrisKills}");
            }
        }

        private void DrawDeathAccuracy(ref float x, ref float y, Graphics g)
        {
            g?.DrawText(_consolasBold, FONT, _brushes["white"], x + 10, y += LINE,
                $"Chris DA: {gameMemory.ChrisDA} (Rank: {gameMemory.ChrisDARank})");
            g?.DrawText(_consolasBold, FONT, _brushes["white"], x + 10, y += LINE,
                $"Sheva DA: {gameMemory.ShevaDA} (Rank: {gameMemory.ShevaDARank})");
        }

        private void DrawEnemyHP(float currentY, Graphics g)
        {
            float enemyX = config.EnemyHPPositionX == -1 ? config.PositionX + 5f : config.EnemyHPPositionX;
            float enemyY = config.EnemyHPPositionY == -1 ? currentY + LINE : config.EnemyHPPositionY;

            foreach (EnemyHP enemy in gameMemoryConcrete.EnemyHealth.Where(a => a.IsAlive))
            {
                g?.DrawText(_consolasBold, FONT * 0.75f, _brushes["lightred"], enemyX + 5f, enemyY,
                    $"{enemy.CurrentHP}/{enemy.MaximumHP}");
                enemyY += FONT;
            }
        }

        private float MeasureString(string text, float size) =>
            (float)_graphics?.MeasureString(_consolasBold, size, text).X;

        private static string GetChapterName(int chapter) => chapter switch
        {
            0 => "1-1: Assembly Place",
            1 => "1-2: A Slum",
            2 => "2-1: The Underground",
            3 => "2-2: The Mines",
            4 => "2-3: Execution Path",
            5 => "3-1: The Marshlands",
            6 => "3-2: The Ruins",
            7 => "3-3: The Ship Deck",
            8 => "4-1: The Boat",
            9 => "4-2: The Cargo",
            10 => "5-1: The Island",
            11 => "5-2: Laboratory",
            12 => "5-3: The Reactor",
            13 => "6-1: The City",
            14 => "6-2: The Maze",
            15 => "6-3: The Roof",
            _ => $"{chapter}",
        };
    }
}