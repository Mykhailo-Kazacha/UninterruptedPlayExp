using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace UninterruptedPlayExp
{
    public class Player : IDisposable
    {
        public delegate void EnergyAddedHandler(Player player);
        public event EnergyAddedHandler EnergyAdded;

        public List<int> TasksWhenEnergeticsAdded = new List<int>();
        public int ReputationLevel = 0;

        public string Name;
        public Dictionary<int, PlayerState> StateAtTaskCompletion = new Dictionary<int, PlayerState>();
        public Dictionary<int, int> TasksWithLevelUps = new Dictionary<int, int>();
        public int EnergeticsAdded = 0;

        private Dictionary<int, int> levelUpTable;

        private Games game;
        private Random r;// = new Random();
        public int XpLevel;
        public int CurrentXp;
        public int CurrentEnergy;
        public int MaxEnergy;
        public CloneableDictionary<int,Task> ActiveTasks = new CloneableDictionary<int, Task>();
        public CloneableDictionary<int, Task> AvailableTasks;
        public CloneableDictionary<int, Task> CompletedTasks = new CloneableDictionary<int, Task>();
        public Dictionary<string,Location> LockedLocations = new Dictionary<string, Location>();
        public Dictionary<string, Location> UnlockedLocations = new Dictionary<string, Location>();

        public Player(CloneableDictionary<int, Task> tasks, string name, Games game)
        {
            this.game = game;

            r = new Random((int.Parse(name)+1)*DateTime.Now.Millisecond);
            Thread.Sleep(1);

            Name = name;

            AvailableTasks = tasks;

            
            CurrentXp=0;

            switch (game)
            {
                case Games.TE: CreatePlayerForTE();
                    break;
                case Games.TG: CreatePlayerForTG();
                    break;
                default: CreatePlayerForTE();
                    break;
            }
            




            
        }

        private void CreatePlayerForTE()
        {
            XpLevel = 1;
            levelUpTable = LevelUpTable.tableTE;

            MaxEnergy = 5;
            CurrentEnergy = MaxEnergy;

            var firsttask = AvailableTasks[4926];
            ActiveTasks.Add(4926, (Task)firsttask.Clone());
            ActiveTasks[4926].IsActive = true;

        }

        private void CreatePlayerForTG()
        {
            XpLevel = 2;
            levelUpTable = LevelUpTable.tableTG;
            CurrentXp = 240;

            MaxEnergy = 40;
            CurrentEnergy = MaxEnergy;

            var firsttask = AvailableTasks[59];
            ActiveTasks.Add(59, (Task)firsttask.Clone());
            ActiveTasks[59].IsActive = true;
        }

        public void CheckLocations(Games game)
        {
            foreach (var loc in LockedLocations.Values)
            {
                if (!UnlockedLocations.ContainsKey(loc.Name)&& XpLevel >= loc.MinLevel)
                {
                    UnlockedLocations.Add(loc.Name,new Location(loc.Name,loc.MinLevel,loc.XpReward,game));
                }
            }
        }

        private bool CheckIfCanPlay()
        {
            bool canDoSomething = false;
            foreach (var t in ActiveTasks)
            {
                foreach (var l in t.Value.Locations)
                {
                    //если может играть - canDoSomething тру

                    if (UnlockedLocations.ContainsKey(l) && CurrentEnergy >= UnlockedLocations[l].EnergyCost)
                    //if (UnlockedLocations.ContainsKey(l) && CurrentEnergy >= -2)
                    {
                        canDoSomething = true;
                    }
                }
            }
            return canDoSomething;
        }

        public int Play()
        {
            bool canDoSomething=true;

            bool addedPancakesOn5 = false;
            bool addedSaladOn6 = false;

            while (canDoSomething)
            {
                canDoSomething = false;

                //сыграть локацию

                TryCompleteTask();

                

                canDoSomething= CheckIfCanPlay();
                //добавить за время
                //if (!canDoSomething && !addEnergyForTime)
                //{
                //    CurrentEnergy += 10;
                //    canDoSomething = CheckIfCanPlay();
                //    addEnergyForTime = true;
                //}
                
                //навалить энергетик
                if (!canDoSomething && EnergeticsAdded<4)
                {

                    if ((XpLevel == 5 && !addedPancakesOn5)|| (XpLevel == 6 && !addedSaladOn6))
                    {
                        
                        if (!addedPancakesOn5 && XpLevel==5)
                        {
                            addedPancakesOn5 = true;
                            CurrentEnergy = MaxEnergy;
                        }

                        if (!addedSaladOn6 && XpLevel==6)
                        {
                            addedSaladOn6 = true;
                            CurrentEnergy+=90;
                        }
                    }
                    else
                    {
                        CurrentEnergy += 30;
                    }
                    
                    EnergyAdded(this);
                    canDoSomething = CheckIfCanPlay();
                    EnergeticsAdded++;
                }
            }

            Dispose();

            return 0;
        }

        private void TryCompleteTask()
        {
            //выбор случайной таски из активных
            int taskNum=ChooseNextTask();
            Task chosenTask = ActiveTasks.ElementAt(taskNum).Value;


            //выбор случайной локации из этой таски
            string chosenLocationName=ChooseLocationName(chosenTask);




            //если локация открыта и на нее хватает энергии
            if (UnlockedLocations.ContainsKey(chosenLocationName) && CurrentEnergy>=UnlockedLocations[chosenLocationName].EnergyCost)
            //if (UnlockedLocations.ContainsKey(chosenLocationName) && CurrentEnergy>=-30)
                {
                //отминусовать энергию, получить результат игры
                    CurrentEnergy -= UnlockedLocations[chosenLocationName].EnergyCost;

                    

                int result = GetResult();
                if (chosenLocationName == "Energy" && CurrentEnergy >= 15) result = -1;
                    if (result >= 100-chosenTask.Probability)
                    {
                    //если выиграли


                    //если сыграли таску про топор и включился амулет
                    int reward;
                    if (game == Games.TG && CompletedTasks.ContainsKey(886))
                    {
                        reward = (int)(UnlockedLocations[chosenLocationName].XpReward * 1.5);
                    }
                    else
                    {
                        reward = UnlockedLocations[chosenLocationName].XpReward;
                    }
                    CurrentXp += reward;



                    //chosenTask.Locations.RemoveAt(locN);
                    //убрали локацию из активных
                    if (game == Games.TG)
                    {
                        UnlockedLocations[chosenLocationName].AddLocationXP();
                    }
                    chosenTask.Locations.Remove(chosenLocationName);

                    if (chosenLocationName == "Energy")
                    {
                        CurrentEnergy += 30;
                    }

                        
                    //если нашли все, что надо - перенести таску в завершенные, выбрать из активных все, для которых эта таска была предыдущей
                        if (chosenTask.Locations.Count == 0)
                        {
                        if (!CompletedTasks.ContainsKey(chosenTask.Id))
                        {
                            CompletedTasks.Add(chosenTask.Id, chosenTask);
                        }


                        chosenTask.IsActive = false;


                            foreach (Task task in AvailableTasks.Values)
                            {
                                if (XpLevel>= task.LevelRequired && !CompletedTasks.ContainsKey(task.Id) && 
                                !ActiveTasks.ContainsKey(task.Id))
                                {
                                    bool[] completedPreviousTasks = new bool[task.PreviousTasks.Count];
                                    completedPreviousTasks.Initialize();
                                    for (int i = 0; i < completedPreviousTasks.Length; i++)
                                    {
                                        if (CompletedTasks.ContainsKey(task.PreviousTasks[i]))
                                        {
                                            completedPreviousTasks[i] = true;
                                        }
                                    }
                                    if (!completedPreviousTasks.Contains(false))
                                    {
                                       ActiveTasks.Add(task.Id, (Task)task.Clone());
                                        ActiveTasks[task.Id].IsActive = true;
                                    }
                                }
                            }
                        
                        ActiveTasks.Remove(chosenTask.Id);

                        }

                        AddBuns(chosenTask);

                    if (!StateAtTaskCompletion.ContainsKey(chosenTask.Id))
                    {
                        StateAtTaskCompletion.Add(chosenTask.Id, new PlayerState(Name, EnergeticsAdded, XpLevel, CurrentXp, CurrentEnergy, MaxEnergy));
                    }
                }
                }
            

        }

        private void AddBuns(Task task)
        {
            CurrentXp += task.XpReward;
            if (game == Games.TE && task.Id == 9999)
                ReputationLevel++;
            CurrentXp += 5 * ReputationLevel;

            if (CurrentXp >= levelUpTable[XpLevel+1])
            {
                XpLevel++;

                LevelUp(game);
                CurrentEnergy = MaxEnergy;
                TasksWithLevelUps.Add(XpLevel, task.Id);
                CheckLocations(game);
            }
        }

        private void LevelUp(Games game)
        {
            switch (game)
            {
                case Games.TG:
                    {
                        MaxEnergy += 10;
                        foreach (Task xp_task in AvailableTasks.Values)
                        {
                            if (xp_task.LevelRequired <= XpLevel && !ActiveTasks.ContainsKey(xp_task.Id) && 
                                !CompletedTasks.ContainsKey(xp_task.Id))
                            {
                                bool[] canAddTaskToActive = new bool[xp_task.PreviousTasks.Count];
                                canAddTaskToActive.Initialize();
                                for (int i = 0; i < xp_task.PreviousTasks.Count; i++)
                                {
                                    if (CompletedTasks.ContainsKey(xp_task.PreviousTasks[i]))
                                    {
                                        canAddTaskToActive[i] = true;
                                    }
                                }
                                if (!canAddTaskToActive.Contains(false))
                                {
                                    ActiveTasks.Add(xp_task.Id, (Task)xp_task.Clone());
                                    ActiveTasks[xp_task.Id].IsActive = true;
                                }
                            }
                        }
                    }
                    break;
                case Games.TE:
                    break;
                default:
                    break;
            }
        }

        public void Dispose()
        {
            //AvailableTasks = null;
            //ActiveTasks = null;
            //LockedLocations = null;
        }

        //выбор таски, которую будем играть. сейчас - рандомно из активных, в будущем можно добавить более сложное поведение
        private int ChooseNextTask()
        {
            int taskNum = r.Next(ActiveTasks.Count);
            return taskNum;
        }

        //выбор локации из выбранной таски, которую будем играть. сейчас - рандомно из возможных, в будущем можно 
        //добавить более сложное поведение
        private string ChooseLocationName(Task chosenTask)
        {
            int locN = r.Next(chosenTask.Locations.Count);
            string chosenLocationName = "";
            if (chosenTask.Locations.Count != 0)
            {
                chosenLocationName = chosenTask.Locations.ElementAt(locN);
            }
            return chosenLocationName;
        }

        //получить результат прохождения локации (по сути нашли/не нашли предмет), сейчас просто рандомно, потом можно допилить например
        //учет винрейта, аномалии, моды и т. д. (но для этого придется давать в метод локацию и переписать сам класс Location)
        private int GetResult()
        {
            int result = r.Next(0, 100);
            return result;
        }
    }
}
