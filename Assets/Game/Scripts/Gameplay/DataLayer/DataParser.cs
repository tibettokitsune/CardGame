using System;
using System.Collections.Generic;
using Game.Scripts.Gameplay.Lobby.Player;

namespace Game.Scripts.Gameplay.DataLayer
{
    internal static class DataParser
    {
        public static List<(PlayerStat, float)> ParseStats(string input)
        {
            var result = new List<(PlayerStat, float)>();
            var statPairs = input.Split(';', StringSplitOptions.RemoveEmptyEntries);

            foreach (var pair in statPairs)
            {
                var parts = pair.Split(',', StringSplitOptions.RemoveEmptyEntries);
                string typePart = null;
                string valuePart = null;

                foreach (var part in parts)
                {
                    var kv = part.Split('=');
                    if (kv.Length != 2)
                        continue;

                    if (kv[0].Trim().Equals("type", StringComparison.OrdinalIgnoreCase))
                        typePart = kv[1].Trim();

                    else if (kv[0].Trim().Equals("Value", StringComparison.OrdinalIgnoreCase))
                        valuePart = kv[1].Trim();
                }

                if (Enum.TryParse<PlayerStat>(typePart, out var stat) && float.TryParse(valuePart, out var value))
                {
                    result.Add((stat, value));
                }
                else
                {
                    throw new FormatException($"Invalid stat or value in: '{pair}'");
                }
            }

            return result;
        }
    }
}