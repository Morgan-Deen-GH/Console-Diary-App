using DiaryProject;
using System;
using System.IO;
using static BaseInformation.TemplateResponses;

public class BaseInformation
{
    private static string baseFilePath = "..\\..\\..\\..\\..";
    private static string diaryFilePath = baseFilePath + "\\Rating Files";
    private static string medDetailsFilePath = baseFilePath + "\\information\\medicationDetails.txt";
    private static string medLogFilePath = baseFilePath + "\\information\\medicationLog";
    private static string habitInformation = baseFilePath + "\\information\\habitInfo";
    public static string textdivider = "------------------------------------------------------";

    //creates the "config.txt" file, which is the core filepaths for the program are stored.
    public static void createConfig()
    {
        string configPath = string.Concat(Directory.GetCurrentDirectory(), "\\config.txt");
        if (!File.Exists(configPath))
        {
            try
            {
                File.Create(configPath).Close();
            }
            catch
            {
                Console.WriteLine("Unable to create \"config.txt\" file.");
                Console.ReadKey();
                return;
            }
        }
        return;
    }

    //reads the specified paths from the file and saves them into a list to be used later
    public static Dictionary<string,string> readPathsFromFile()
    {
        string configPath = string.Concat(Directory.GetCurrentDirectory(), "\\config.txt");
        createConfig();
        Dictionary<string, string> readPaths = new Dictionary<string, string>(5);
        FileStream configInfo = File.Open(configPath, FileMode.Open);
        BinaryReader bOperations = new BinaryReader(configInfo);

        try
        {
            //adds keys, adds values
            readPaths.Add(bOperations.ReadString(), bOperations.ReadString()); //base
            readPaths.Add(bOperations.ReadString(), bOperations.ReadString()); //diary
            readPaths.Add(bOperations.ReadString(), bOperations.ReadString()); //details
            readPaths.Add(bOperations.ReadString(), bOperations.ReadString()); //log
            readPaths.Add(bOperations.ReadString(), bOperations.ReadString()); //habit
        }
        catch (EndOfStreamException)
        {
            try
            {
                //closes then reopens filestream and binary reader, then uses them to add values to dictionary
                configInfo.Close();
                bOperations.Close();

                configInfo = File.Open(configPath, FileMode.Open);
                bOperations = new BinaryReader(configInfo);
                readPaths.Clear();

                readPaths.Add("base", bOperations.ReadString()); //base
                readPaths.Add("diary", bOperations.ReadString()); //diary
                readPaths.Add("details", bOperations.ReadString()); //details
                readPaths.Add("log", bOperations.ReadString()); //log
                readPaths.Add("habit", bOperations.ReadString()); //habit
            }
            catch (Exception)
            {
                bOperations.Close();
                configInfo.Close();
                Console.WriteLine(tr_error_general + tr_try_again);
                Console.ReadKey();
                return null;
            }
        }
        catch (Exception)
        {
            bOperations.Close();
            configInfo.Close();
            Console.WriteLine(tr_error_general + tr_try_again);
            Console.ReadKey();
            return null;
        }
        finally
        {
            bOperations.Close();
            configInfo.Close();
        }
        return readPaths;
    }

    //writes the (presumably changed) path information into the file
    public static void savePathsToFile()
    {
        //config is in the debug folder
        string configPath = string.Concat(Directory.GetCurrentDirectory(), "\\config.txt");

        FileStream configInfo = File.Open(configPath, FileMode.Create);
        BinaryWriter bw = new BinaryWriter(configInfo);
        Dictionary<string, string> fileDefinitions = new Dictionary<string, string> {
            { "base", baseFilePath },
            {"diary",diaryFilePath},
            {"details", medDetailsFilePath},
            {"log", medLogFilePath},
            {"habit", habitInformation }
        };


        try
        {
            foreach (string i in fileDefinitions.Keys)
            {
                bw.Write(string.Concat(i, fileDefinitions[i]));                
            }
        }
        catch
        {
            bw.Close();
            configInfo.Close();
            Console.WriteLine("Unable to write path information to files.");            
            Console.ReadKey();
            Environment.Exit(15);
        }

        bw.Close();
        configInfo.Close();
        Console.WriteLine("The operation is complete.");
        return;
    }
    
    //sets the baseFilePath
    public static void setBFP()
    {
        Console.WriteLine("Please specify a new Directory path.\n");
        string newDirect = Console.ReadLine();
        while (!Path.IsPathFullyQualified(newDirect))
        {
            if (newDirect == "cancel")
            {
                return;
            }
            Console.WriteLine(tr_invalid_input + tr_try_again);
            newDirect = Console.ReadLine();
        }
        newDirect = Path.TrimEndingDirectorySeparator(newDirect);
        if (!Directory.Exists(newDirect))
        {
            Directory.CreateDirectory(newDirect);
        }
        string previousBFP = baseFilePath;
        baseFilePath = newDirect;        
        return;
    }

    //method function to run when starting, gets the filePaths
    public static void StartUp()
    {
        createConfig();
        getBFP();
        getDFP();
        getHIFP();
        getMDFP();
        getMLFP();
    }



    //returns the baseFilePath - assigns a new value if it cannot read the data from the file
    public static string getBFP()
    {
        Dictionary<string, string> bitPaths = readPathsFromFile();
        try
        {
            baseFilePath = bitPaths.GetValueOrDefault("base");
        }
        catch
        {
            baseFilePath = string.Concat(Directory.GetCurrentDirectory(), "..\\..\\..\\..\\..");
            if (!Directory.Exists(baseFilePath))
            {
                Directory.CreateDirectory(baseFilePath);
            }
        }
        return baseFilePath;

    }

    //returns the diaryFilePath - assigns a new value if it cannot read the data from the file, depends on getBFP()
    public static string getDFP()
    {
        Dictionary<string, string> bitPaths = readPathsFromFile();
        try
        {
            diaryFilePath = bitPaths.GetValueOrDefault("diary");
            
        }
        catch
        {
            diaryFilePath = string.Concat(getBFP(), "\\Rating Files");
            if (!Directory.Exists(diaryFilePath))
            {
                Directory.CreateDirectory(diaryFilePath);
            }
            
        }
        return diaryFilePath;

    }
    //returns the medDetailsFilePath - assigns a new value if it cannot read the data from the file, depends on getBFP()
    public static string getMDFP()
    {
        Dictionary<string, string> bitPaths = readPathsFromFile();
        try
        {
            medDetailsFilePath = bitPaths.GetValueOrDefault("details");
            
        }
        catch
        {
            medDetailsFilePath = string.Concat(getBFP(), "\\information\\medicationDetails.txt");
            if (!Directory.Exists(string.Concat(getBFP(), "\\information")))
            {
                Directory.CreateDirectory(string.Concat(getBFP(), "\\information"));
            }
            if (!File.Exists(medDetailsFilePath))
            {
                File.Create(medDetailsFilePath).Close();
            }
            
        }
        return medDetailsFilePath;
    }

    //returns the medLogFilePath - assigns a new value if it cannot read the data from the file, depends on getBFP()
    public static string getMLFP()
    {
        Dictionary<string, string> bitPaths = readPathsFromFile();
        try
        {
            medLogFilePath = bitPaths.GetValueOrDefault("log");            
            
        }
        catch
        {
            medLogFilePath = string.Concat(getBFP(), "\\information\\medicationLog");
            if (!Directory.Exists(string.Concat(getBFP(), "\\information")))
            {
                Directory.CreateDirectory(string.Concat(getBFP(), "\\information"));
            }
            if (!Directory.Exists(medLogFilePath))
            {
                Directory.CreateDirectory(medLogFilePath);
            }
            
        }
        return medLogFilePath;
    }

    //returns the habitInformation - assigns a new value if it cannot read the data from the file, depends on getBFP()
    public static string getHIFP()
    {
        Dictionary<string, string> bitPaths = readPathsFromFile();
        try
        {
            habitInformation = bitPaths.GetValueOrDefault("habit");
            
        }
        catch
        {
            habitInformation = string.Concat(getBFP(), "\\information\\habitInfo");
            if (!Directory.Exists(habitInformation))
            {
                Directory.CreateDirectory(habitInformation);
            }
            
        }
        return habitInformation;
    }

    public class ComputerInformation
    {
        private static int processor = Environment.ProcessorCount;

        public static int getProcessorCount()
        {
            return processor;
        }
    }

    //templates to use in response to certain results
    public class TemplateResponses
    {
        public static string tr_cancel = "This operation has been cancelled.";
        public static string tr_try_again = "\nPlease try again.";
        public static string tr_invalid_input = "This input is not valid.";
        public static string tr_unfound_path = "This path does not exist.";
        public static string tr_error_general = "An error has occured.";

        public static string tr_error_specific = "The following error has occured:\nSPECIFIC";
        public static string tr_unfound_path_specific = "The \"SPECIFIC\" path does not exist.";
        public static string tr_creating_specific = "\nCreating SPECIFIC.";

        public static string tr_JOINT_unfoundPath_creating_specific = "The \"SPECIFIC\" path does not exist.\nCreating SPECIFIC.";
    }

}
