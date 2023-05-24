using BettingBot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingBot.Helpers
{
    public static class InputMessageParser
    {
        public static Bet ParseInputMessage(string message)
        {
            var splittedString = message.Split('\n');

            var sport = splittedString[1].Split(')').Last().Trim();

            var BKs = new List<BK>();

            BKs.Add(ParseBK(splittedString[3]));
            BKs.Add(ParseBK(splittedString[4]));

            return new Bet()
            {
                Sport = sport,
                Title = splittedString[2],
                Bks = BKs
            };

        }

        public static BK ParseBK(string bkRow)
        {
            var splittedString = bkRow.Split('/');

            return new BK()
            {
                Name = splittedString[0].Trim(),
                Title = splittedString[1].Trim(),
                Coefficient = splittedString[2].Trim().Replace('.',','),
            };
        }
    }
}
