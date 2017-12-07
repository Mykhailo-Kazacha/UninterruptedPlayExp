using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UninterruptedPlayExp
{
    public class Location
    {
        public string Name;
        public int MinLevel;
        public int EnergyCost;
        public int MasteryLevel=0;
        public bool Unlocked=false;
        public int XpReward;
        public int MasteryXp = 0;
        private Games Game;

        public void AddLocationXP()
        {
            if (Name == "Match3" || Name == "Bubbles" || Name == "Snooker")
            {
                MasteryXp += 100;
            }
            else if (Name != "Collection" && Name != "Energy")
            {
                int xp = LocationLevelUpTable.LocationLevelTable[Name][MasteryLevel];
                MasteryXp +=xp;
            }

            if (MasteryXp >= 100)
            {
                LocationLevelUp();
                MasteryXp = 0;
            }
        }

        public void LocationLevelUp()
        {
            if (Name == "Match3" || Name == "Bubbles" || Name == "Snooker")
            {
                EnergyCost = MasteryLevel+1;
                MasteryLevel++;
                XpReward++;
            }
            else
            {
                EnergyCost = EnergyCost + 5;
                MasteryLevel++;
                XpReward += 10;
            }
        }

        public Location(string name, int minLevel,int reward, Games game)
        {
            Game = game;

            Name = name;
            MinLevel = minLevel;
            XpReward = reward;

            switch (game)
            {
                case Games.TE: CreateTELocation();
                    break;
                case Games.TG: CreateTGLocation();
                    break;
                default: CreateTELocation();
                    break;
            }
        }

        private void CreateTELocation()
        {
            if (Name == "Казино" || Name=="Коллекция" ||Name=="Опросник"|| Name == "Путешествуй_больше")
            {
                EnergyCost = 0;
            }
            else
            {
                EnergyCost = 1;
            }
        }

        private void CreateTGLocation()
        {
            if (Name == "Match3" || Name == "Bubbles" || Name == "Snooker")
            {
                EnergyCost = 10;
            }
            else switch (Name)
            {
                    case "antique shop": EnergyCost = 15;
                        break;
                    case "egypt": EnergyCost = 15;
                        break;
                    case "museum": EnergyCost = 30;
                        break;
                    case "paris": EnergyCost = 10;
                        break;
                }
        }
    }
}
