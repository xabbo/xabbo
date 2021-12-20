using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Messages;

namespace b7.Xabbo.Commands
{
    public class MoodCommands : CommandModule
    {
        private static (double H, double S, double L) RgbToHsl(int r, int g, int b)
        {
            double h, s, l;
            double rr = r / 255.0;
            double gg = g / 255.0;
            double bb = b / 255.0;

            double max = Math.Max(Math.Max(rr, gg), bb);
            double min = Math.Min(Math.Min(rr, gg), bb);

            double diff = max - min;

            l = (max + min) / 2;
            if (Math.Abs(diff) < 0.00001)
            {
                h = s = 0;
            }
            else
            {
                s = diff / (l <= 0.5 ? (max + min) : (2 - max - min));

                double rd = (max - rr) / diff;
                double gd = (max - gg) / diff;
                double bd = (max - bb) / diff;

                if (rr == max) h = bd - gd;
                else if (gg == max) h = 2 + rd - bd;
                else h = 4 + gd - rd;

                h *= 60;
                if (h < 0) h += 360;
            }

            return (h, s, l);
        }

        private readonly RoomManager _roomManager;

        public MoodCommands(RoomManager roomManager)
        {
            _roomManager = roomManager;
        }

        [Command("mood")]
        protected Task HandleMoodCommand(CommandArgs args)
        {
            if (args.Count > 0)
            {
                switch (args[0].ToLower())
                {
                    case "settings": Send(Out.RoomDimmerEditPresets); break;
                    default: break;
                }
            }
            else
            {
                Send(Out.RoomDimmerChangeState);
            }

            return Task.CompletedTask;
        }

        [Command("bg")]
        protected Task HandleBgCommand(CommandArgs args)
        {
            IRoom? room = _roomManager.Room;
            if (room is null)
            {
                ShowMessage("State is not tracked, please re-enter the room.");
                return Task.CompletedTask;
            }

            IFloorItem? toner = room.FloorItems.OfKind("roombg_color").FirstOrDefault();
            if (toner is null)
            {
                ShowMessage("No background toner found in the room.");
                return Task.CompletedTask;
            }

            if (args.Count > 0)
            {
                if (!int.TryParse(args[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int color))
                    return Task.CompletedTask;

                var (h, s, l) = RgbToHsl(
                    (color >> 16) & 0xFF,
                    (color >> 8) & 0xFF,
                    color & 0xFF
                );

                Send(Out.SetRoomBackgroundColorData,
                    (LegacyLong)toner.Id,
                    (int)Math.Round(h / 360.0 * 255),
                    (int)Math.Round(255 * s),
                    (int)Math.Round(255 * l)
                );
            }
            else
            {
                Send(Out.UseStuff, (LegacyLong)toner.Id, 0);
            }

            return Task.CompletedTask;
        }
    }
}
