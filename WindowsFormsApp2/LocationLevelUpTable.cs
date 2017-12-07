using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UninterruptedPlayExp
{
    //class to represent changes in amount of mastery experience gained per one game in location
    //(for TG only)
    public static class LocationLevelUpTable
    {
        public static Dictionary<string, int[]> LocationLevelTable = new Dictionary<string, int[]>();
        static LocationLevelUpTable()
        {
            LocationLevelTable.Add("antique shop", new int[5] { 20, 12, 9, 7, 1});
            LocationLevelTable.Add("museum", new int[5] { 13, 11, 9, 3, 1 });
            // old 
            LocationLevelTable.Add("egypt", new int[5] { 25, 15, 8, 3, 1 });
            // new            LocationLevelTable.Add("egypt", new int[5] { 50, 15, 8, 3, 1 });
            LocationLevelTable.Add("underwater", new int[5] { 13, 11, 9, 3, 1 });
            LocationLevelTable.Add("paris", new int[5] { 25, 15, 10, 8, 1 });
            LocationLevelTable.Add("chinese tomb", new int[5] { 12, 10, 8, 3, 1 });
            LocationLevelTable.Add("attic", new int[5] { 12, 10, 8, 3, 1 });
            LocationLevelTable.Add("camboja", new int[5] { 12, 10, 8, 3, 1 });
        }
    }
}
