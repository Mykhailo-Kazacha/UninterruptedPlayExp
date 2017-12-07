using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UninterruptedPlayExp
{
    //stores state of a player
    public class PlayerState
    {

        public string Name;
        public int EnergeticsAdded;
        public int XpLevel;
        public int CurrentXp;
        public int CurrentEnergy;
        public int MaxEnergy;

        public PlayerState(string Name, int EnergeticsAdded, int XpLevel, int CurrentXp, int CurrentEnergy, int MaxEnergy)
        {
            this.Name = Name;
            this.EnergeticsAdded = EnergeticsAdded;
            this.XpLevel = XpLevel;
            this.CurrentXp = CurrentXp;
            this.CurrentEnergy = CurrentEnergy;
            this.MaxEnergy = MaxEnergy;
        }
    }
}
