using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UninterruptedPlayExp
{
    // class to represent a task for user to complete
    public class Task : ICloneable
    {
        //implementation of interface. needed in order to each player had separate lists
        //active and completed tasks, taken from a big list of available tasks
        public object Clone()
        {
            List<string> tmp_location = new List<string>();
            foreach (string location in this.Locations)
            {
                tmp_location.Add(String.Copy(location));
            }
            double tmp = Probability / 100;
            var clone = new Task(Id, XpReward, LevelRequired, PreviousTasks, tmp_location,(double)Probability/100);

            return clone;
        }

        public List<int> PreviousTasks;
        public int Id;
        public int XpReward;
        public bool IsActive;
        public int LevelRequired;
        public List<string> Locations;
        public int Probability;

       // Tasks.Add(new Task(tmp[0], tmp[1], tmp[2], tmp[3].Split(','), tmp[5].Split(',')));
        public Task(string Id, string Xp, string Level, string[] PrevTasks, string[] Locations, string Probability)
        {
            this.Id = int.Parse(Id);
            XpReward = int.Parse(Xp);
            LevelRequired = int.Parse(Level);
            int[] tmp=new int[PrevTasks.Length];
            int i = 0;
            foreach (string s in PrevTasks)
            {
                if (s != "")
                {
                    tmp[i] = int.Parse(s);
                }
                else
                {
                    tmp[i] = 0;
                }
                i++;
            }

            PreviousTasks = new List<int>(tmp.Length);
            PreviousTasks.AddRange(tmp);

            this.Locations = new List<string>();

            foreach (var location in Locations)
            {
                this.Locations.Add(String.Copy(location));
            }

            IsActive = false;

            //Probability = r.Next(40, 80);
            SetProbability(Probability);
        }

        public Task(int Id, int Xp, int Level, List<int> PrevTasks, List<string> Locations, double Probability)
        {
            this.Id = Id;
            this.XpReward = Xp;
            this.LevelRequired = Level;
            PreviousTasks = PrevTasks;

            this.Locations = new List<string>();
            // this.Locations.Add(String.Copy(Locations[0]));
            foreach (var tmp1 in Locations)
            {
                this.Locations.Add(String.Copy(tmp1));
            }

            IsActive = false;
            SetProbability(Probability.ToString());
        }

        
        private void SetProbability(string Probability)
        {
            if (Probability == "")
            {
                this.Probability = 100;
            }
            else
            {
                this.Probability = (int)(Double.Parse(Probability)*100);
            }
        }
    }
}
