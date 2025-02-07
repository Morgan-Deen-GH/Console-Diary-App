using System;
using static BaseInformation;
using static BaseInformation.TemplateResponses;
using static BaseInformation.ComputerInformation;
using static Diary.Medication;
using static Diary.MedicationLog;
using static Routine.Habit;
using static Routine.Goal;
using static Diary.DiaryEntry;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Runtime.Intrinsics.X86;

public class Menus()
{
    public static string diaryFilePath = BaseInformation.getDFP();
    public static string medDetailsFilePath = BaseInformation.getMDFP();
    public static string medLogFilePath = BaseInformation.getMLFP();
    public static string habitInformation = BaseInformation.getHIFP();

    public static HashSet<string> acceptableResponses_cancel = new HashSet<string> {
    "cancel",
    "exit",
    "0",
    "zero",
    "close"
};

    //tests if answer can be parsed into a bool, loops until it can
    public static bool? testBoolAnswer(string answer)
    {
        bool trueBool = new bool();
        while (!bool.TryParse(answer, out trueBool))
        {
            if (answer.Contains("cancel"))
            {
                Console.WriteLine(tr_cancel);
                Console.ReadKey();
                return null;
            }
            if (answer == "yes")
            {
                answer = "true";
            }
            else if (answer == "no")
            {
                answer = "false";
            }
            else
            {
                Console.WriteLine(tr_invalid_input + tr_try_again);
                answer = Console.ReadLine().Trim().ToLower();
            }
        }
        return trueBool;
    }

    //medication menu
    public static void medicationMain()
    {
        Console.Clear();
        while (true)
        {
            Console.WriteLine(@"What do you want to do?
    0. Cancel Operation
    1. View Medication Information
    2. Create new Medication profile
    3. Delete existing Medication profile
    4. Check your Medication log for the past week
");
            string answer = Console.ReadLine().ToLower().Trim();
            if (acceptableResponses_cancel.Contains(answer))
            {
                Console.Clear();
                return;
            }
            if (answer.Contains("view") || answer == "1")
            {
                Console.Clear();
                viewMedicalCereal();
            }
            else if (answer.Contains("create") || answer == "2")
            {
                Console.Clear();
                medicationCreationMain(medDetailsFilePath);

            }
            else if (answer.Contains("delete") || answer == "3")
            {
                Console.Clear();
                try
                {
                    string chosenMeds = deleteMedication();
                    deleteMedicationFromCereal(chosenMeds);
                    deleteMedicationLog(chosenMeds);
                    Console.WriteLine("Your information has been removed.");
                }
                catch (Exception)
                {
                    Console.WriteLine(tr_error_general + tr_try_again);
                    Console.ReadKey();                    
                }                
            }
            else if (answer.Contains("check") || answer.Contains("log") || answer == "4")
            {
                Console.Clear();
                viewMedicalCereal();
                Console.WriteLine("\nWhich medication do you want to check?");
                string medName = Console.ReadLine().ToLower().Trim();
                checkMedicationLog(medName);
            }

            Console.Clear();
        }
    }

    //routine menu
    public static void routineMain()
    {
        Console.Clear();
        while (true)
        {
            Console.WriteLine(@"What do you want to do?
    0. Cancel Operation
    1. View Routine Information
    2. Create New Habit
    3. Delete Habit
    4. Change Habit to Goal
    5. List all Routines
");
            string answer = Console.ReadLine().ToLower().Trim();
            if (acceptableResponses_cancel.Contains(answer))
            {
                Console.Clear();
                return;
            }
            if (answer.Contains("view") || answer == "1")
            {
                Console.Clear();
                readHabitFromFile(viewHabits());
            }
            else if (answer.Contains("create") || answer == "2")
            {
                Console.Clear();
                createNewHabit();
            }
            else if (answer.Contains("delete") || answer == "3")
            {
                Console.Clear();
                deleteHabit(viewHabits());
            }
            else if (answer.Contains("change") || answer == "4")
            {
                Console.Clear();
                turnHabitToGoal(viewHabits());
            }
            else if (answer.Contains("list") || answer == "5")
            {
                Console.Clear();
                routineSummary();
            }

            Console.Clear();
        }
    }

    //configuration menu
    public static void configurationMain()
    {
        Console.Clear();
        while (true)
        {
            Console.WriteLine(@"What do you want to do?
    0. Cancel Operation
    1. Change Base Directory
    2. Bake Configuration Changes
");
            string answer = Console.ReadLine().ToLower().Trim();
            if (acceptableResponses_cancel.Contains(answer))
            {
                Console.Clear();
                return;
            }
            if (answer.Contains("change") || answer == "1")
            {
                Console.Clear();
                setBFP();
            }
            else if (answer.Contains("bake") || answer == "2")
            {
                Console.Clear();
                savePathsToFile();
                Console.WriteLine("Please reopen application to continue use.");
                Console.ReadKey();
                System.Environment.Exit(0);
            }
            else
            {
                Console.Clear();
                Console.WriteLine(tr_invalid_input + tr_try_again);
                Console.ReadKey();
            }
            Console.Clear();
        }
    }

    public static void diaryMain()
    {
        Console.Clear();
        while (true)
        {
            Console.WriteLine(@"What do you want to do?
    0. Cancel Operation
    1. Rate Today
    2. Edit or Delete Existing File
    3. Calculate Average Rating
");
            string answer = Console.ReadLine().ToLower().Trim();
            if (acceptableResponses_cancel.Contains(answer))
            {
                Console.Clear();
                return;
            }
            else if (answer == "1" || answer.Contains("rate"))
            {
                Console.Clear();
                DateTime? dateTime = null;
                rateToday(dateTime);
            }
            else if (answer == "2" || answer.Contains("edit"))
            {
                Console.Clear();
                editFoundFile(searchFolders(diaryFilePath));
            }
            else if (answer == "3" || answer.Contains("calc"))
            {
                string calcAnswer = "";
                while (!calcAnswer.ToLower().Contains("cancel"))
                {
                    Console.Clear();
                    Console.WriteLine(@"
Would you like to:
    0. Cancel Operation
    1. Search between two dates
(e.g. Between 02/05/2014 and 15/02/2035)
    2. Enter a timespan
(e.g. 3 months, 1 week, 6 days ago)
");
                    calcAnswer = Console.ReadLine().ToLower().Trim();
                    if (calcAnswer.Contains("cancel") || calcAnswer == "0")
                    {
                        Console.WriteLine(tr_cancel);
                        Console.ReadKey();
                        break;
                    }
                    else if (calcAnswer == "1" || calcAnswer.Contains("search") || calcAnswer.Contains("date"))
                    {
                        Console.WriteLine("\nWhat is your first date?:");
                        string firstDateTrial = Console.ReadLine();
                        while (!DateTime.TryParse(firstDateTrial, out DateTime firstDate))
                        {
                            Console.WriteLine(tr_invalid_input + "\nPlease enter your date in the \"dd/MM/yyyy\" format." + tr_try_again);
                            firstDateTrial = Console.ReadLine();
                        }

                        Console.WriteLine("\nWhat is your second date?:");
                        string secondDateTrial = Console.ReadLine();
                        while (!DateTime.TryParse(secondDateTrial, out DateTime secondDate))
                        {
                            Console.WriteLine(tr_invalid_input + "\nPlease enter your date in the \"dd/MM/yyyy\" format." + tr_try_again);
                            secondDateTrial = Console.ReadLine();
                        }

                        calculateAverage(extractCorrectInfo(rangeOfAverage_withDates(firstDateTrial, secondDateTrial)));
                    }
                    else if (calcAnswer == "2" || calcAnswer.Contains("timespan"))
                    {
                        Console.WriteLine("Enter a timespan:");
                        calculateAverage(extractCorrectInfo(filesInAverageRange(rangeOfAverage(Console.ReadLine()))));
                    }
                    else
                    {
                        Console.WriteLine(tr_invalid_input + tr_try_again);
                    }

                    Console.ReadKey();
                }

            }
            Console.Clear();
        }
    }

    public static void helpMenu()
    {
        while (true)
        {
            Console.WriteLine(@"The function of this program is to act as a diary or journal, as well as record your medication and habits.
You can choose your responses to the prompts shown by the console through:
    A: Choosing the number to the left of the prompt.
    B: Including the first word of the prompt in your answer.
Please be aware you can only chose one response at a time.");
            Console.ReadKey();
            Console.WriteLine("\n" + @"What would you like to know?:
    0. Cancel Operation
    1. How to use this program?
    2. Main Menu
    3. Diary Entries
    4. Medication
    5. Routines
    6. Configuration
");
            string answer = Console.ReadLine().ToLower().Trim();
            Console.Clear();
            if (acceptableResponses_cancel.Contains(answer))
            {
                Console.Clear();
                return;
            }
            else if (answer.Contains("1") || answer.Contains("how") || answer.Contains("use")) 
            {
                //use for daily entries - console app
                //add medication
                //add routines - habits and goals
                //configure stuff
                //use 0 or cancel to stop the operation at any point
                Console.WriteLine("How to use this program:\n");
                Console.WriteLine(@"As already stated, you can choose your responses to the prompts shown by the console through:
    A: Choosing the number to the left of the prompt.
    B: Including the first word of the prompt in your answer.
Please be aware you can only chose one response at a time.");
                Console.ReadKey();
                Console.WriteLine("\n"+@"If there is a pause in the program where a menu is not being shown, please press any key to continue the program.
The pause occurs to ensure the user has tnough time to read through what has been displayed.");
                Console.ReadKey();
                Console.WriteLine("\nAt any point in the program, even when it is not displayed as an option in the menu, you can input \"cancel\" to stop the ongoing operation.");
                Console.ReadKey();
            }
            else if (answer.Contains("2") || answer.Contains("main"))
            {
                Console.WriteLine("Main Menu:\n");
                Console.WriteLine(@"The following is the main menu:

            What do you want to do?
                0. Cancel Operation
                1. Diary Entries
                2. Medication Information
                3. Routine Information
");
                Console.ReadKey();
                Console.WriteLine("\nAll of the aformentioned prompts direct you to their respective menus.");
                Console.ReadKey();
                Console.WriteLine("\n\"Cancel operation\" will take you back to the main menu.");
                Console.ReadKey();
                Console.WriteLine("\nNot every acceptable response is listed.");
                Console.ReadKey();
                Console.WriteLine("\nTo use the aforementioned functions, include their numbers (e.g. \"0\" to Cancel operation) or the first word of the function name (e.g. \"cancel\" to Cancel operation) in your responses.");
                Console.ReadKey();
            }
            else if (answer.Contains("3") || answer.Contains("diary"))
            {
                Console.WriteLine("Diary Entries:\n");
                Console.WriteLine("Each entry allows you to give the day a score - 0 being the worst, 5 being the best - and write a comment.\n\nIf you take medication, you will also be questioned on whether you have taken it on that day.");
                Console.ReadKey();
                Console.WriteLine(@"The following is the menu for diaries:

            What do you want to do?
                0. Cancel Operation
                1. Rate Today
                2. Edit or Delete Existing File
                3. Calculate Average Rating
");
                Console.ReadKey();
                Console.WriteLine("\nUsing the \"Rate Today\" function, users can enter a diary entry for today, which is then saved onto your computer in the \"Rating Files\" folder.");
                Console.ReadKey();
                Console.WriteLine("\nExisting files can be read, altered, or deleted through the \"Edit or Delete Existing File\" function.");
                Console.ReadKey();
                Console.WriteLine("\n\"Calculate Average Rating\" can be used to calculate the mean and the mode for days within a specified entered time range.");
                Console.ReadKey();
                Console.WriteLine("\n\"Cancel operation\" will take you back to the main menu.");
                Console.ReadKey();
                Console.WriteLine("\nTo use the aforementioned functions, include their numbers (e.g. \"0\" to Cancel operation) or the first word of the function name (e.g. \"cancel\" to Cancel operation) in your responses.");
                Console.ReadKey();
            }
            else if (answer.Contains("4") || answer.Contains("medication"))
            {
                Console.WriteLine("Medication:\n");
                Console.WriteLine("This program can store information about your medications and allow you to keep track of them over the past week.");
                Console.ReadKey();
                Console.WriteLine(@"The following is the menu for medication:

            What do you want to do?
                0. Cancel Operation
                1. View Medication Information
                2. Create new Medication profile
                3. Delete existing Medication profile
                4. Check your Medication log for the past week
");
                Console.ReadKey();
                Console.WriteLine("\nUsing the \"View Medication Information\" function, users can see the details of medication they have entered into the system.");
                Console.ReadKey();
                Console.WriteLine("\nIf there are no medications saved, you will not be able to view anything. \nIf a medication has been saved, then you will be asked if you have taken it in when submitting a new entry for the current day.");
                Console.ReadKey();
                Console.WriteLine("\nUse \"Create new Medication profile\" and \"Delete existing Medication profile\" to change the details of your medication.");
                Console.ReadKey();
                Console.WriteLine("\n\"Check your Medication log\" returns the number of times you have successfully taken your medication over the last week.");
                Console.ReadKey();
                Console.WriteLine("\n\"Cancel operation\" will take you back to the main menu.");
                Console.ReadKey();
                Console.WriteLine("\nTo use the aforementioned functions, include their numbers (e.g. \"0\" to Cancel operation) or the first word of the function name (e.g. \"cancel\" to Cancel operation) in your responses.");
                Console.ReadKey();
            }
            else if (answer.Contains("5") || answer.Contains("routine"))
            {
                Console.WriteLine("Routine:\n");
                Console.WriteLine("Routines are either \"habits\" - actions that you do regularly - or \"goals\" - actions that you want to do regularly.\n\nEach are saved in their own individual file.");
                Console.ReadKey();
                Console.WriteLine("\n"+@"The following is the menu for routines:

            What do you want to do?
                0. Cancel Operation
                1. View Routine Information
                2. Create New Habit
                3. Delete Habit
                4. Change Habit to Goal
                5. List all Routines
");
                Console.ReadKey();
                Console.WriteLine("\nUsing the \"View Routine Information\" function, users can access and read the details of any habits they have entered into the system.");
                Console.WriteLine("If there are no habits or goals saved, you will not be able to view anything.");
                Console.ReadKey();
                Console.WriteLine("\nUse \"Create New Habit\" and \"Delete Habit\" to change your habits and goals.");
                Console.ReadKey();
                Console.WriteLine("\n\"Change Habit to Goal\" turns a habit into a goal and allows you to include SMART goals.");
                Console.ReadKey();
                Console.WriteLine("\n\"List all Routines\" returns a list of the habit and goal files stored. You are not able to access the files using this feature.");
                Console.ReadKey();
                Console.WriteLine("\n\"Cancel operation\" will take you back to the main menu.");
                Console.ReadKey();
                Console.WriteLine("\nTo use the aforementioned functions, include their numbers (e.g. \"0\" to Cancel operation) or the first word of the function name (e.g. \"cancel\" to Cancel operation) in your responses.");
                Console.ReadKey();
            }
            else if (answer.Contains("6") || answer.Contains("configuration"))
            {
                Console.WriteLine("Configuration:\n");
                Console.WriteLine("The configuration menu is for vital information and should not be changed if it can be helped.\n\nHowever, if necessary, I've added the config menu to change this information.");
                Console.ReadKey();
                Console.WriteLine("\nThe configuration menu can be accessed by typing \"-1\" on the main menu.\n");
                Console.ReadKey();
                Console.WriteLine(@"The following is the menu for configuration:

            What do you want to do?
                0. Cancel Operation
                1. Change Base Directory
                2. Bake Configuration Changes
");
                Console.ReadKey();
                Console.WriteLine("\nUsing the \"Change Base Directory\" function allows you to change the directory in which files are written to and read from.");
                Console.ReadKey();
                Console.WriteLine("\nPlease keep in mind, this does not move any already existing files, just tells the system to create new ones at a specified location.");
                Console.ReadKey();
                Console.WriteLine("\nNone of the changes made with the \"Change Base Directory\" function are finialised until you use the \"Bake Configuration Changes\" function.\nYou need to use this function in order for any changes to be saved.");
                Console.ReadKey();
                Console.WriteLine("\n\"Cancel operation\" will take you back to the main menu.");
                Console.ReadKey();
                Console.WriteLine("\nTo use the aforementioned functions, include their numbers (e.g. \"0\" to Cancel operation) or the first word of the function name (e.g. \"cancel\" to Cancel operation) in your responses.");
                Console.ReadKey();
            }
            Console.Clear();
        }
    }

    //main menu
    public static void mainMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine(@"What do you want to do?
    0. Close Application
    1. Diary Entries
    2. Medication Information
    3. Routine Information

Type -help- for more information.
");
            string answer = Console.ReadLine().ToLower().Trim();

            if (acceptableResponses_cancel.Contains(answer))
            {
                Console.Clear();
                Console.WriteLine("Shutting down...");
                return;
            }

            if (answer.Contains("help"))
            {
                Console.Clear();
                helpMenu();
            }
            else if (answer == "-1")
            {
                Console.Clear();
                configurationMain();
            }
            else if (answer == "1" || answer.Contains("diary"))
            {
                Console.Clear();
                diaryMain();
            }
            else if (answer == "2" || answer.Contains("medication"))
            {
                Console.Clear();
                medicationMain();
            }
            else if (answer == "3" || answer.Contains("routine"))
            {
                Console.Clear();
                routineMain();
            }
            else
            {
                Console.WriteLine(tr_invalid_input + tr_try_again);
                Console.ReadKey();
            }
        }
    }


}
