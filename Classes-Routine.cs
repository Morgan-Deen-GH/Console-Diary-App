using System;
using static BaseInformation;
using static BaseInformation.TemplateResponses;
using System.Text.Json;
using static Routine;
using static Diary;
using static Menus;


public class Routine
{
    private static string habitInformation = getHIFP();

    /* The "Habit" class is meant to track already existing habits and routines
	 *		No inherant positive or negative implication
	 */
    public class Habit
    {
        protected string title;
        protected string description;
        //keeps track of the date and whether you completed the habit or not
        protected Dictionary<DateTime, bool>? completedToday = new Dictionary<DateTime, bool>();
        protected List<object>? allInformation = new List<object>();


        //default constructor
        public Habit(string title, string description, Dictionary<DateTime, bool> completionStatus)
        {
            this.title = title;
            this.description = description;
            this.completedToday = completionStatus;
            return;
        }

        public Habit()
        {
            this.title = string.Empty;
            this.description = string.Empty;
            return;
        }

        //copy constructor
        public Habit(Habit oldHabit)
        {
            this.title = oldHabit.title;
            this.description = oldHabit.description;
            this.completedToday = oldHabit.completedToday;
            return;
        }

        //get
        public string getTitle()
        {
            return this.title;
        }

        public string getDescription()
        {
            return this.description;
        }
        public Dictionary<DateTime, bool> getCompletedToday()
        {
            return completedToday;
        }


        //set
        public void setTitle(string newTitle)
        {
            this.title = newTitle;
            return;
        }
        public void setDescription(string newDescription)
        {
            this.description = newDescription;
            return;
        }
        public void addCompletionStatus(KeyValuePair<DateTime, bool> completionStatus)
        {
            this.completedToday.Add(completionStatus.Key, completionStatus.Value);
            return;

        }
        public void clearCompletionStatus()
        {
            this.completedToday.Clear();
            return;
        } 
        public void setCompletionStatus(Dictionary<DateTime, bool> completionStatus)
        {
            this.completedToday.Clear();
            this.completedToday = completionStatus;
            return;
        }




        //habit methods

        //prepares habit for serialisation
        public List<object> serialisationPrep()
        {
            this.allInformation.Add((object)this.getTitle());
            this.allInformation.Add((object)(this.getDescription()));
            this.allInformation.Add((object)this.getCompletedToday());
            return this.allInformation;
        }

        //saves habit information to its own file
        public static void saveHabitToFile(Habit habit)
        {
            if (!Directory.Exists(habitInformation))
            {
                Directory.CreateDirectory(habitInformation);
            }
            string habitFilePath = $"{habitInformation}\\habit_{habit.title}.txt";

            //creating the new file
            if (File.Exists(habitFilePath))
            {
                Console.WriteLine("This file already exists.\nWould you like to delete it?");
                string answer = Console.ReadLine().ToLower().Trim();
                var trueAnswer = testBoolAnswer(answer);
                bool acceptableAnswer = new bool();
                if (trueAnswer.HasValue)
                {
                    acceptableAnswer = trueAnswer.Value;
                }
                if (acceptableAnswer)
                {
                    try
                    {
                        File.Delete(habitFilePath);
                        File.Create(habitFilePath).Close();
                    }
                    catch (Exception exc)
                    {
                        Console.Clear();
                        Console.WriteLine($"The following error has occured:\n{exc}\nPlease try again later.");
                        Console.ReadKey();
                        return;
                    }
                    acceptableAnswer = true;
                }
                else
                {
                    return;
                }
                

            }
            else
            {
                try
                {
                    File.Create(habitFilePath).Close();
                }
                catch (Exception exc)
                {
                    Console.Clear();
                    Console.WriteLine($"The following error has occured:\n{exc.Message}" + tr_try_again);
                    Console.ReadKey();
                    return;
                }
            }

            //serialisation!!
            List<object> serialFinish = habit.serialisationPrep();
            string completionInfo = JsonSerializer.Serialize(serialFinish);

            try { File.AppendAllText(habitFilePath, completionInfo); }
            catch (Exception exc)
            {
                Console.Clear();
                Console.WriteLine($"The following error has occured:\n{exc}\nPlease try again later.");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("\nThis operation has been completed.");
            Console.ReadKey();
            return;
        }

        //reads habit information from specific file
        public static void readHabitFromFile(string filepath)
        {
            // ^ assume already selected habit from list of files
            if (filepath == "cancel")
            {
                return;
            }
            if (!File.Exists(filepath))
            {
                Console.WriteLine(tr_unfound_path + tr_try_again);
                Console.ReadKey();
                return;
            }

            //reads and deserialises information from file
            string fileGuts = File.ReadAllText(filepath);
            //string,string,dictionary<datetime,bool>
            List<object> fileInnards = new List<object>(3);
            try
            {
                fileInnards = JsonSerializer.Deserialize<List<object>>(fileGuts);
            }
            catch (Exception)
            {
                Console.WriteLine(tr_error_general + tr_try_again);
                Console.ReadKey();
                return;
            }

            //Prints to console the routine information
            //could use a dictionary to make the code more adaptive
            Console.Clear();
            if (Path.GetFileNameWithoutExtension(filepath).StartsWith("habit_"))
            {
                Console.WriteLine($"    Habit\nTitle: {fileInnards[0].ToString()}");
            }
            else if (Path.GetFileNameWithoutExtension(filepath).StartsWith("goal_"))
            {
                Console.WriteLine($"    Goal\nTitle: {fileInnards[0].ToString()}");
            }

            Console.WriteLine($"Description: {fileInnards[1].ToString()}");
            Console.WriteLine($"Log: {fileInnards[2]}");
            if (Path.GetFileNameWithoutExtension(filepath).StartsWith("goal_"))
            {
                Console.WriteLine($"Details: {fileInnards[3].ToString()}");
            }
            Console.ReadKey();
            return;
        }

        public static Habit returnHabitFromFile(string filepath)
        {
            //already selected habit from list of files
            if (filepath == "cancel")
            {
                return null;
            }
            if (!File.Exists(filepath))
            {
                Console.WriteLine(tr_unfound_path + tr_try_again);
                Console.ReadKey();
                return null;
            }
            string fileGuts = File.ReadAllText(filepath);
            List<object> fileInnards = new List<object>(3);
            try
            {
                fileInnards = JsonSerializer.Deserialize<List<object>>(fileGuts);
            }
            catch (Exception)
            {
                Console.WriteLine(tr_error_general + tr_try_again);
                Console.ReadKey();
                return null;
            }

            
            Habit placeholder = new Habit();
            placeholder.setTitle(fileInnards[0].ToString());
            placeholder.setDescription(fileInnards[1].ToString());

            //change jsonelement to dictionary
            string toDict = fileInnards[2].ToString();
            toDict = toDict.Replace("{", "").Replace("}", "");
            List<string> listDict = toDict.Split("\"").ToList();

            listDict.RemoveAll(x => x == "");
            Dictionary<DateTime, bool> newDict = new Dictionary<DateTime, bool>();
            foreach (string stringInsert in listDict)
            {
                DateTime soon = new DateTime();
                bool result = new bool();
                if (DateTime.TryParse(stringInsert, out soon))
                {
                    int indexNeeded = listDict.IndexOf(stringInsert);
                    if (bool.TryParse(listDict[indexNeeded+1].Replace(":",""), out result))
                    {
                        newDict.Add(soon, result);
                    }
                }
            }
            placeholder.setCompletionStatus(newDict);

            return placeholder;
        }

        //creates a new habit
        public static void createNewHabit()
        {
            Console.WriteLine("What is the title of your habit?");
            string title = Console.ReadLine();
            if (title.ToLower().Trim() == "cancel")
            {
                Console.WriteLine(tr_cancel);
                Console.ReadKey();
                return;
            }

            Console.WriteLine("\nPlease give a description of your habit.");
            string description = Console.ReadLine();
            if (description.ToLower().Trim() == "cancel")
            {
                Console.WriteLine(tr_cancel);
                Console.ReadKey();
                return;
            }

            Console.WriteLine("\nHave you completed this habit today?");
            string answer = Console.ReadLine().Trim().ToLower();
            bool? finalAnswer = new bool();
            if (testBoolAnswer(answer) != null)
            {
                finalAnswer = testBoolAnswer(answer);
            }
            else
            {
                Console.WriteLine(tr_cancel);
                Console.ReadKey();
                return;
            }
           

            DateTime today = DateTime.Today;

            Dictionary<DateTime, bool> completionStatus = new Dictionary<DateTime, bool>();
            completionStatus.Add(today, finalAnswer.Value);

            saveHabitToFile(new Habit(title, description, completionStatus));

            Console.WriteLine("This habit and the associated file has been logged and created.");
            Console.ReadKey();
            return;

        }

        public static string viewHabits()
        {
            if (!Directory.Exists(habitInformation))
            {
                try
                {
                    Directory.CreateDirectory(habitInformation);
                }
                catch (Exception exc)
                {
                    Console.Clear();
                    Console.WriteLine($"The following error has occured:\n{exc}" + tr_try_again);
                    Console.ReadKey();
                    return "cancel";
                }
            }
            
            Dictionary<string, string> availableFiles = new Dictionary<string, string>();
            List<string> directoryGuts = Directory.GetFiles(habitInformation).ToList();

            if (directoryGuts.Count <= 0)
            {
                Console.WriteLine("There are no available files in this directory.");
                Console.ReadKey();
                return "cancel";
            }

            Console.WriteLine("Your available files are:");
            int counter = 0;
            foreach (string directoryGutsFile in directoryGuts)
            {
                counter++;
                string pathTitle = Path.GetFileNameWithoutExtension(directoryGutsFile);

                availableFiles.Add(counter.ToString(), pathTitle);
                Console.WriteLine($"    {counter} : {pathTitle}");
            }
            Console.WriteLine("\nWhat file would you like to access?");
            string fileAccess = Console.ReadLine().ToLower().Trim();
            while (!availableFiles.Keys.Contains(fileAccess) && !availableFiles.Values.Contains(fileAccess))
            {
                if (fileAccess == "cancel")
                {
                    Console.WriteLine(tr_cancel);
                    Console.ReadKey();
                    return "cancel";
                }
                Console.WriteLine(tr_invalid_input + tr_try_again);
                fileAccess = Console.ReadLine().ToLower().Trim();
            }

            if (availableFiles.Keys.Contains(fileAccess))
            {
                fileAccess = availableFiles.GetValueOrDefault(fileAccess);
            }

            fileAccess = string.Concat(habitInformation, "\\", fileAccess, ".txt");
            return fileAccess;

        }

        public static void deleteHabit(string filepath)
        {
            if (!File.Exists(filepath))
            {
                Console.WriteLine(tr_unfound_path + tr_try_again);
                Console.ReadKey();
                return;
            }
            try
            {
                File.Delete(filepath);
            }
            catch (Exception exe)
            {
                Console.WriteLine($"The following error has occured:\n{exe}" + tr_try_again);
                Console.ReadKey();
                return;
            }
            Console.WriteLine("This Habit has been deleted.");
            Console.ReadKey();
            return;
        }

        public static void routineSummary()
        {
            if (!Directory.Exists(habitInformation))
            {
                try
                {
                    Directory.CreateDirectory(habitInformation);
                }
                catch (Exception exc)
                {
                    Console.Clear();
                    Console.WriteLine($"The following error has occured:\n{exc.Message}" + tr_try_again);
                    Console.ReadKey();
                    return;
                }
            }

            List<string> routineList = new List<string>();
            List<string> routineGoals = new List<string>();
            List<string> routineHabits = new List<string>();
            try
            {
                routineList = Directory.GetFiles(habitInformation).ToList();
                routineGoals = routineList.Select(s => Path.GetFileName(s)).Where(s => s.StartsWith("goal_")).ToList();
                routineHabits = routineList.Select(s => Path.GetFileName(s)).Where(s => s.StartsWith("habit_")).ToList();
            }
            catch (Exception exc)
            {
                Console.WriteLine(tr_error_specific.Replace("SPECIFIC", exc.Message) + tr_try_again);
                Console.ReadKey();
                return;
            }

            List<Habit> habitList = new List<Habit>();
            Habit habit = null;
            Goal goal = null;
            foreach (string routine in routineHabits)
            {
                try
                {
                    habit = returnHabitFromFile(string.Concat(habitInformation, $"\\{routine}"));
                    
                }
                catch
                {
                    Console.WriteLine(tr_error_general + tr_try_again);
                    Console.ReadKey();
                    return;
                }
                finally
                {
                    if (habit != null)
                    {
                        habitList.Add(habit);
                    }
                }
            }
            foreach (string routine in routineGoals)
            {
                try
                {
                    goal = new Goal(returnHabitFromFile(string.Concat(habitInformation, $"\\{routine}")));

                }
                catch
                {
                    Console.WriteLine(tr_error_general + tr_try_again);
                    Console.ReadKey();
                    return;
                }
                finally
                {
                    if (goal != null)
                    {
                        habitList.Add(goal);
                    }
                }
            }
            //get title and description of all habits

            //check if habit list is empty
            if (habitList.Count == 0)
            {
                Console.WriteLine("There are no routines stored.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("The following routines are stored on your devide:\n");

            List<string> habitTitle = new List<string>();
            foreach (Habit habitInfo in habitList)
            {
                Console.WriteLine($@"   {habitInfo.getTitle()}");
            }
            Console.ReadKey();
        }


    }



    /* Goal are meant to track actions that you want to become habits (e.g. I will swim for 30 minutes every saturday)
	 *		Specifically SMART goals
	 */
    public class Goal : Habit
    {
        private string? SMART;

        //SMART goals are Specific, Measurable, Achievable, Relevant, and Time-bound.
        public Goal(Habit oldHabit) : base(oldHabit)
        {
            this.setTitle(oldHabit.getTitle());
            this.setDescription(oldHabit.getDescription());
            return;
        }

        public Goal(Habit oldHabit, string sMART)
        {
            this.title = oldHabit.getTitle();
            this.description = oldHabit.getDescription();
            this.completedToday = oldHabit.getCompletedToday();
            SMART = sMART;
            return;
        }

        public string? getGoalDetails()
        {
            return this.SMART;
        }

        public void deleteGoalDetails()
        {
            this.SMART = null;
            return;
        }

        public void setGoalDetails(string newDetails)
        {
            this.SMART = newDetails;
            return;
        }


        //preparing data for serialisation
        public List<object> serialisationPrep()
        {
            this.allInformation.Add((object)this.getTitle());
            this.allInformation.Add((object)(this.getDescription()));
            this.allInformation.Add((object)this.getCompletedToday());
            this.allInformation.Add((object)this.getGoalDetails());
            return this.allInformation;
        }

        //add goal to files
        public static void saveGoalToFile(Goal goal)
        {
            if (Directory.Exists(habitInformation))
            {
                string goalFilePath = $"{habitInformation}\\goal_{goal.title}.txt";

                //creating the new file
                if (File.Exists(goalFilePath))
                {
                    Console.WriteLine("This file already exists.\nWould you like to delete it?");
                    bool acceptableAnswer = false;
                    string answer = Console.ReadLine().ToLower().Trim();


                    while (!acceptableAnswer)
                    {
                        if (answer == "yes")
                        {
                            try
                            {
                                File.Delete(goalFilePath);
                                File.Create(goalFilePath).Close();
                            }
                            catch (Exception exc)
                            {
                                Console.Clear();
                                Console.WriteLine($"The following error has occured:\n{exc}\nPlease try again later.");
                                Console.ReadKey();
                                return;
                            }
                            acceptableAnswer = true;
                        }
                        else if (answer == "no" || answer == "cancel")
                        {
                            Console.WriteLine("\nCancelling operation.");
                            Console.ReadKey();
                            return;
                        }
                        else
                        {
                            Console.WriteLine("\nThis input is not acceptable.\nPlease try again.");
                            answer = Console.ReadLine().ToLower().Trim();
                        }
                    }

                }
                else
                {
                    try
                    {
                        File.Create(goalFilePath).Close();
                    }
                    catch (Exception exc)
                    {
                        Console.Clear();
                        Console.WriteLine($"The following error has occured:\n{exc.Message}" + tr_try_again);
                        Console.ReadKey();
                        return;
                    }
                }

                //serialises the information
                List<object> serialFinish = goal.serialisationPrep();
                string completionInfo = JsonSerializer.Serialize(serialFinish);

                //save goal info to file
                try { File.AppendAllText(goalFilePath, completionInfo); }
                catch (Exception exc)
                {
                    Console.Clear();
                    Console.WriteLine($"The following error has occured:\n{exc.Message}" + tr_try_again);
                    Console.ReadKey();
                    return;
                }
                Console.WriteLine("\nThis operation has been completed.");
                Console.ReadKey();
                return;

            }
            else
            {
                Console.WriteLine(tr_unfound_path + tr_try_again);
                Console.ReadKey(); return;
            }
        }

        //converts a habit to goal
        public static void turnHabitToGoal(string filepath)
        {
            Habit newHabit = returnHabitFromFile(filepath);
            if (newHabit == null)
            {
                Console.WriteLine(tr_error_general + tr_try_again);
                Console.ReadKey();
                return;
            }
            Goal newGoal = new Goal(newHabit);
            defineSMARTObjectives(newGoal);
            saveGoalToFile(newGoal);
            Console.WriteLine("Your goal was been created.");
            deleteHabit(filepath);
            return;
        }

        //defines SMART objectives
        public static Goal defineSMARTObjectives(Goal goal)
        {
            if (goal.getGoalDetails() != null)
            {
                Console.WriteLine("You have already defined your SMART goals.");
                Console.ReadKey();
                return goal;
            }
            Console.WriteLine(@"To help you achieve your goals, they need to be SMART.
    S - Specific
    M - Measurable
    A - Achievable
    R - Relevant
    T - Time-Based
For example: I want to run 1km, everyday, for a week, in order to be healthier.");
            Console.ReadKey();
            Console.WriteLine("\nYour goal is: " + goal.title);
            Console.WriteLine("Please give your goal a SMART definition.");
            string smartDef = Console.ReadLine();
            if (smartDef.ToLower() == "cancel")
            {
                Console.WriteLine(tr_cancel);
                Console.ReadKey();
                return goal;
            }
            goal.setGoalDetails(smartDef);
            return goal;
        }

        //converts returned habit to goal
        public static Goal returnHabitFromFile(string filepath)
        {
            Goal newGoal = new Goal(Habit.returnHabitFromFile(filepath));
            return newGoal;
        }
    }
}

    
