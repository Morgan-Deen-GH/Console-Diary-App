// See https://aka.ms/new-console-template for more information
using Microsoft.VisualBasic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using static BaseInformation;
using static BaseInformation.TemplateResponses;
using static Diary.Medication;
using static Diary.MedicationLog;
using static Diary.DiaryEntry;
using static Routine.Habit;
using static Routine.Goal;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Menus;


//check if information folder exist before doing anything

namespace DiaryProject
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (readPathsFromFile() == null)
            {
                savePathsToFile();
            }
            StartUp();
            string input = string.Concat(args).ToLower();
            if (input.Contains("diary"))
            {
                diaryMain();
            }
            else if (input.Contains("medication"))
            {
                medicationMain();
            }
            else if (input.Contains("routine"))
            {
                routineMain();
            }
            else if (input.Contains("configuration"))
            {
                configurationMain();
            }
            else
            {
                mainMenu();
            }
        }
    }
}