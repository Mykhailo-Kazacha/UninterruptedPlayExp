using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UninterruptedPlayExp
{
    //levelup tables for different games
    static public class LevelUpTable
    {
        public static Dictionary<int, int> tableTE = new Dictionary<int, int>();
        public static Dictionary<int, int> tableTG = new Dictionary<int, int>();

        static LevelUpTable()
        {
            

            tableTE.Add(1, 0);
            tableTE.Add(2, 30);
            tableTE.Add(3, 200);
            tableTE.Add(4, 350);
            for (int i = tableTE.Count+1; i < 100; i++)
            {
                tableTE.Add(i, tableTE[i-1]+500);
            }

            var delta = new int[16] { 0, 50, 200, 500, 1000, 1200, 1300, 1400, 1500, 1600, 1700, 2000, 2500, 3000, 3500, 4000 };
            tableTG.Add(1, 0);
            for (int i = 2; i < 16; i++)
            {
                tableTG.Add(i, tableTG[i - 1] + delta[i-1]);
            }
            for (int i = 16; i < 100;i++)
            {
                tableTG.Add(i,tableTG[i-1]+4000);
            }



        }
    }
}
