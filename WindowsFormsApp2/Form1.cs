using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace UninterruptedPlayExp
{
    public partial class Form1 : Form
    {
        public Dictionary<int, Task> Tasks;
        bool allIsCreated = false;
        Games game;
        Dictionary<string, Location> locations;
        List<Player> players;
        Dictionary<int, double> probabilities;

        public Form1()
        {
            InitializeComponent();
        }

        private void CreateTaskList()
        {
            //create list of tasks
            string line;
            Tasks = new Dictionary<int, Task>();

            //string path = String.Format(@"C:\Users\mkazacha\Desktop\UninterruptedPlayExp\tasks ({0}).csv", textBox2.Text);
            string path = String.Format(@"tasks ({0}).csv", textBox2.Text);
            using (StreamReader sr = new StreamReader(path))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    string[] tmp = new string[6];
                    tmp = line.Split('\t');
                    if (tmp.Length == 5)
                    {
                        Tasks.Add(int.Parse(tmp[0]), new Task(tmp[0], tmp[1], tmp[2], tmp[3].Split('.'), tmp[4].Split('.'),""));
                    }
                    else if (tmp.Length == 6)
                    {
                        Tasks.Add(int.Parse(tmp[0]), new Task(tmp[0], tmp[1], tmp[2], tmp[3].Split('.'), tmp[4].Split('.'),tmp[5]));
                    }
                }

            };

        }

        private List<Player> CreateListOfPlayers(Games game)
        {
            //создать игроков
            int playersNumber = int.Parse(textBox1.Text);
            List<Player> players = new List<Player>(playersNumber);
            for (int i = 0; i < playersNumber; i++)
            {
                players.Add(new Player(Tasks, i.ToString(), game));
                players[i].EnergyAdded += EnergyAdded;
            }

            return players;
        }


        private void CreateListOfLocations(List<Player> players, Games game)
        {
            //create list of locations for each player
            locations = new Dictionary<string, Location>();
            //string path = String.Format(@"C:\Users\mkazacha\Desktop\UninterruptedPlayExp\locations ({0}).csv", textBox2.Text);
            string path = String.Format(@"locations ({0}).csv", textBox2.Text);
            
            using (StreamReader sr = new StreamReader(path))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] tmp = new string[2];
                    tmp = line.Split('\t');
                    foreach (var player in players)
                    {
                        player.LockedLocations.Add(tmp[0], new Location(tmp[0], int.Parse(tmp[1]), int.Parse(tmp[2]), game));
                    }
                    //locations.Add(tmp[0], new Location(tmp[0], int.Parse(tmp[1]), int.Parse(tmp[2])));
                }

            };

            //add all available locations from locked to Unlocked
            foreach (var player in players)
            {
                player.CheckLocations(game);
            }

        }

        private void SetChart()
        {
            chart1.Series[0].Points.Clear();
            chart1.ChartAreas[0].AxisX.Maximum = 60;
            chart1.ChartAreas[0].AxisY.Maximum = 140;
            chart1.ChartAreas[0].AxisY.Minimum = -90;
            chart1.ChartAreas[0].AxisY.MajorGrid.Interval = 10;
            chart1.ChartAreas[0].AxisY.IntervalAutoMode = System.Windows.Forms.DataVisualization.Charting.IntervalAutoMode.FixedCount;
            chart1.ChartAreas[0].AxisY.Interval = 10;
            chart1.Series[0].Color = Color.Red;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SetChart();

            //choose a game
            switch (textBox2.Text)
            {
                case "TG":
                    game = Games.TG;
                    break;
                case "TE":
                    game = Games.TE;
                    break;
                default:
                    game = Games.TE;
                    break;
            }

            
            if (!allIsCreated)
            {
                //list of all available tasks from csv
                CreateTaskList();
                allIsCreated=true;
            }


            players = CreateListOfPlayers(game);
            
            //create list of locked and unlocked location for each player
            CreateListOfLocations(players, game);

            //start playing
            foreach (var player in players)
            {
                player.Play();
            }

          //  проход по таскам хидден/ миниигра или нет
            //var tmp = new Dictionary<int, bool>();
            //foreach (var task in Tasks)
            //{
            //    if (task.Value.Locations[0] == "Collection" || task.Value.Locations[0] == "Energy")
            //    {
            //        tmp.Add(task.Key, false);
            //    }
            //    else
            //    {
            //        tmp.Add(task.Key, true);
            //    }
            //}

            //save probabilities for each completed tasks
            probabilities = new Dictionary<int, double>();
            foreach (var player in players)
            {
                foreach (var completedTask in player.CompletedTasks)
                {
                    if (!probabilities.ContainsKey(completedTask.Key))
                    {
                        probabilities.Add(completedTask.Key, completedTask.Value.Probability);
                    }
                }
            }

            //create arrays of players' state (time in game, exp, level)
            var time = new int[players.Count()];
            var xp = new int[players.Count()];
            var level = new int[players.Count()];
            for (int i = 0; i < time.Length; i++)
            {
                time[i]=90*players[i].CompletedTasks.Count;
                xp[i] = players[i].CurrentXp;
                level[i] = players[i].XpLevel;
            }

            //   MessageBox.Show(String.Format("XP: " + xp.Max().ToString() + " / " + LevelUpTable.tableTG[level.Max()+1].ToString()));
            //   MessageBox.Show(String.Format("Level: " + level.Max().ToString()));

            listBox1.Items.Add(String.Format("Median time: " + (time.OrderByDescending(t => t).ToArray())[time.Length / 2].ToString() + " / " + (50 * 60).ToString()));
            listBox1.Items.Add(String.Format("Max time: " + time.Max().ToString() + " / " + (50 * 60).ToString()));
            listBox1.Items.Add(String.Format("Median xp: " + (xp.OrderByDescending(t => t).ToArray())[xp.Length / 2].ToString() + " / " 
                + LevelUpTable.tableTG[(level.OrderByDescending(t => t).ToArray())[level.Length/2] + 1].ToString()));
            listBox1.Items.Add("\n\r")  ;


            if (game == Games.TG)
            {
                Dictionary<int, int> chartData = new Dictionary<int, int>();
                int levelToFindDistributionFor = 5;
                foreach (var player in players)
                {
                    if (!chartData.ContainsKey(player.TasksWithLevelUps[levelToFindDistributionFor]))
                    {
                        chartData.Add(player.TasksWithLevelUps[levelToFindDistributionFor], 0);
                    }


                }

                foreach (var player in players)
                {
                    chartData[player.TasksWithLevelUps[levelToFindDistributionFor]]++;
                }

                chart1.Series["Series1"].Points.Clear();
                //chart1.Series["Series1"].Points.DataBindXY(chartData.Keys, chartData.Values);
                foreach (var point in chartData)
                {
                    chart1.Series["Series1"].Points.AddXY(point.Key.ToString(), point.Value);


                }


                chart1.Series[0].Points.Clear();
                chart1.Update();

                foreach (var player in players)
                {
                    if (chart1.Series.FindByName(player.Name) == null)
                    {
                        chart1.Series.Add(player.Name);
                        chart1.Series[player.Name].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;

                        bool b = false;
                        foreach (var point in player.StateAtTaskCompletion)
                        {
                            if (point.Key == 1610)
                            {

                                b = true;
                            }
                            else
                            {
                                b = false;
                            }

                            chart1.Series[player.Name].Points.AddY(point.Value.CurrentEnergy);
                            chart1.Update();
                            if (player.TasksWhenEnergeticsAdded.Contains(point.Key))
                            {
                                chart1.Series[player.Name].Points[chart1.Series[player.Name].Points.Count - 1].Label =
                                    String.Format(player.Name + " " + point.Key.ToString());

                            }

                            if (b)
                            {
                                chart1.Series[player.Name].Points[chart1.Series[player.Name].Points.Count - 1].Label = point.Key.ToString();
                            }



                            chart1.Update();
                            Thread.Sleep(10);
                        }
                        break;
                    }
                }


                var NumberOfEnergeticsPerLevel = new Dictionary<int, List<int>>();
                for (int i = 5; i <= 9; i++)
                {
                    NumberOfEnergeticsPerLevel.Add(i, new List<int>());
                }

                foreach (var level1 in NumberOfEnergeticsPerLevel.Keys)
                {
                    foreach (var player in players)
                    {
                        int count = 0;
                        foreach (var taskWithEnergetic in player.TasksWhenEnergeticsAdded)
                        {

                            if (player.StateAtTaskCompletion[taskWithEnergetic].XpLevel == level1)
                            {
                                count++;
                            }
                        }
                        if (count != 0)
                        {
                            NumberOfEnergeticsPerLevel[level1].Add(count);
                        }
                    }
                }

                //foreach (var level1 in NumberOfEnergeticsPerLevel.Keys)
                //{
                //    listBox1.Items.Add(String.Format("Median" + level1.ToString() + " " +
                //        NumberOfEnergeticsPerLevel[level1].OrderBy(t => t).ToArray()[NumberOfEnergeticsPerLevel[level1].Count / 2]));
                //}

                //распределение количества использованных энергетиков по уровням
                var NumberOfEnergeticsDistribution = new Dictionary<int, int>();
                for (int i = 5; i <= 9; i++)
                {

                    foreach (var num in NumberOfEnergeticsPerLevel[/*int.Parse(textBox3.Text)*/i])
                    {
                        if (!NumberOfEnergeticsDistribution.ContainsKey(num))
                        {
                            NumberOfEnergeticsDistribution.Add(num, 0);
                        }
                        else
                        {
                            NumberOfEnergeticsDistribution[num]++;
                        }
                    }
                    //foreach (var pair in NumberOfEnergeticsDistribution)
                    //{
                    //    listBox1.Items.Add(String.Format(i.ToString() + " " + pair.Key + " " + pair.Value));
                    //}

                    foreach (var key in NumberOfEnergeticsDistribution.Keys.ToArray())
                    {
                        NumberOfEnergeticsDistribution[key] = 0;
                    }

                }


                //распределение тасок, на которых используются энергетики по уровням
                var DistributionByTasksWithEnergetics = new Dictionary<int, Dictionary<int, int>>();

                foreach (var player in players)
                {
                    foreach (int task_id in player.TasksWhenEnergeticsAdded)
                    {
                        int taskCompletionLevel = player.StateAtTaskCompletion[task_id].XpLevel;
                        if (!DistributionByTasksWithEnergetics.ContainsKey(taskCompletionLevel))
                        {
                            DistributionByTasksWithEnergetics.Add(taskCompletionLevel, new Dictionary<int, int>());
                        }
                        else
                        {
                            if (!DistributionByTasksWithEnergetics[taskCompletionLevel].ContainsKey(task_id))
                            {
                                DistributionByTasksWithEnergetics[taskCompletionLevel].Add(task_id, 0);
                            }
                            else
                            {
                                DistributionByTasksWithEnergetics[taskCompletionLevel][task_id]++;
                            }
                        }
                    }

                }

                foreach (var pair in DistributionByTasksWithEnergetics[int.Parse(textBox3.Text)].OrderByDescending(pair => pair.Value))
                {
                    listBox1.Items.Add(String.Format(textBox3.Text + "  " + pair.Key + " " + pair.Value));
                }


            }
            //MessageBox.Show("It's alive!");
            button2.Enabled = true;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //for (int j = 0; j < 100; j++)
            {

                Dictionary<int, int> counts = new Dictionary<int, int>();
                Dictionary<int, bool> probabilityChanged = new Dictionary<int, bool>();
                foreach (int id in probabilities.Keys)
                {
                    counts.Add(id, 0);
                    probabilityChanged.Add(id, false);
                    foreach (var player in players)
                    {
                        if (player.CompletedTasks.Keys.Contains(id))
                        {
                            counts[id]++;
                        }
                    }
                }
                counts = counts.OrderByDescending(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);


                for (int i = 0; i < int.Parse(textBox3.Text); i++)
                {
                    foreach (var id in counts.Keys)
                    {
                        if (Tasks[id].Probability >= 90 || counts[id] <= 20)
                        {
                            probabilityChanged[id] = true;
                        }

                        if (Tasks[id].Probability <= 85 && probabilityChanged[id] == false)
                        {
                            Tasks[id].Probability += 5;
                            probabilityChanged[id] = true;
                            break;
                        }

                    }
                }


                if (!probabilityChanged.Values.Contains(false))
                {
                    foreach (var id in probabilityChanged.Keys.ToArray())
                    {
                        probabilityChanged[id] = false;
                    }
                }


                




                button1_Click(null, null);

                // MessageBox.Show("It's alive!");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (StreamWriter sr = new StreamWriter(@"C:\Users\mkazacha\Desktop\UninterruptedPlayExp\WindowsFormsApp2\bin\Debug\test.csv"))
            {
                sr.WriteLine("TaskID\tProbability");
                foreach (var pair in probabilities)
                {
                    sr.WriteLine(String.Format(pair.Key.ToString() + "\t" + pair.Value.ToString()));
                }
            };
                //chart1.Series[0].Points.Clear();
                //chart1.Update();

                //foreach (var player in players)
                //{
                //    if (chart1.Series.FindByName(player.Name) == null)
                //    {
                //        chart1.Series.Add(player.Name);
                //        chart1.Series[player.Name].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;

                //        bool b = false;
                //        foreach (var point in player.EnergyAtTask)
                //        {
                //            if (point.Key == 1610)
                //            {

                //                b = true;
                //            }
                //            else
                //            {
                //                b = false;
                //            }

                //            chart1.Series[player.Name].Points.AddY(point.Value);
                //            if (player.TasksWhenEnergeticsAdded.Contains(point.Key))
                //            {
                //                chart1.Series[player.Name].Points[chart1.Series[player.Name].Points.Count - 1].Label = 
                //                    String.Format(player.Name+" "+point.Key.ToString());

                //            }

                //            if (b)
                //            {
                //                chart1.Series[player.Name].Points[chart1.Series[player.Name].Points.Count - 1].Label = point.Key.ToString();
                //            }


                //            chart1.Update();
                //            Thread.Sleep(10);
                //        }
                //        break;
                //    }



                //}
                //listBox1.Items.Add(String.Format("Median time: " + (time.OrderByDescending(t => t).ToArray())[time.Length / 2].ToString() + " / " + (50 * 60).ToString()));
                //var sdlfkgjkjs = new Dictionary<int, int>();
                //foreach (var id in probabilities.Keys)
                //{
                //    var tmp = new Dictionary<string, int>();
                //    foreach (var player in players)
                //    {
                //        if (player.CompletedTasks.ContainsKey(id))
                //        {
                //            tmp.Add(player.Name, player.EnergyAtTask[id]);
                //        }
                //    }
                //    sdlfkgjkjs.Add(id, tmp.OrderByDescending(t => t.Value).ToDictionary(pair => pair.Key, pair => pair.Value).Values.ElementAt(tmp.Count/2) );
                //}

                //chart1.Series[0].Points.Clear();
                //chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
                //foreach (var sa in sdlfkgjkjs)
                //{

                //    chart1.Series[0].Points.AddY(sa.Value);
                //    chart1.Update();
                //    //Thread.Sleep(100);
                //        }
            }

        public void EnergyAdded(Player player)
        {
            //listBox1.Items.Add(String.Format(player.Name+ " " +player.CompletedTasks.ElementAt(player.CompletedTasks.Count-1).Key));
            player.TasksWhenEnergeticsAdded.Add(player.CompletedTasks.ElementAt(player.CompletedTasks.Count - 1).Key);
            //var color = chart1.Series[0].Color;
          //  if (color.R>10) chart1.Series[0].Color= Color.FromArgb(color.A, color.R - 10, color.G, color.B);
        }
    }
    }
