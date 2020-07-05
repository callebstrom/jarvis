using System.Windows.Forms;
using WindowsInput;
using System.Drawing;
using System.Collections.Immutable;
using System;

namespace jarvis {
    class CSGOCommandConsumer : ICommandConsumer {
        private InputSimulator inputSimulator = new InputSimulator();
        private const String GameTitle = "Counter-Strike: Global Offensive";

        ImmutableDictionary<String, Action> commandMap;

        public CSGOCommandConsumer() {
            var commandMapBuilder = ImmutableDictionary.CreateBuilder<String, Action>();
            commandMapBuilder.Add("accept game", AcceptGame);
            commandMap = commandMapBuilder.ToImmutable();
        }

        public ImmutableHashSet<String> GetGrammar() {
            return commandMap.Keys.ToImmutableHashSet();
        }

        public void Consume(string command, object context) {
            commandMap[command]();
        }

        private void AcceptGame() {

            System.IntPtr csgoWindowPtr = User32.FindWindow(null, GameTitle);
            User32.LockSetForegroundWindow(2);

            User32.WindowPlacement placement = new User32.WindowPlacement();
            User32.GetWindowPlacement(csgoWindowPtr, ref placement);

            if (placement.showCmd == User32.ShowWindowCommands.ShowMinimized)
            {
                User32.ShowWindow(csgoWindowPtr, User32.ShowWindowCommands.Restore);
            }

            User32.SetForegroundWindow(csgoWindowPtr);

            var screenCenter = this.GetScreenCenter();
            inputSimulator.Mouse.MoveMouseTo(screenCenter.X, screenCenter.Y + (screenCenter.Y * 0.1));
            inputSimulator.Mouse.LeftButtonClick();
        }

        private Point GetScreenCenter() {

            var resolution = Screen.PrimaryScreen.Bounds;

            var screenWidth = resolution.Width;
            var screenHeight = resolution.Height;
            var X = screenWidth / 2 * 65535 / screenWidth;
            var Y = screenHeight / 2 * 65535 / screenHeight;

            return new Point(X, Y);
        }
    }
}
