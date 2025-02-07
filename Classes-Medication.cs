using System;
using static BaseInformation;
using static BaseInformation.TemplateResponses;
using static BaseInformation.ComputerInformation;
using static Menus;
using System.Text.Json;
using System.Text;
using System.Linq;
using System.IO;
using System.Reflection.Metadata;


public class Diary
{
    private static string diaryFilePath = getDFP();
    private static string medDetailsFilePath = getMDFP();
    private static string medLogFilePath = getMLFP();

    //Daily Diary
    /* Users can rate how they felt today
     that rating can then be compared to other entries to hopefully give the users an understanding of their general mood
     Averages can be calculated to help with this
    */

    public class DiaryEntry
    {
        private string description = "";
        private int rating = -1;
        private DateTime date = new DateTime();
        private bool medStatus = false;

        public string getDescription()
        {
            return description;
        }
        public int getRating()
        {
            return rating;
        }
        public DateTime getDate()
        {
            return date;
        }
        public bool getMStatus()
        {
            return medStatus;
        }

        public void setDescription(string newDescription)
        {
            description = newDescription;
            return;
        }
        public void setRating(int newRating)
        {
            rating = newRating;
            return;
        }
        public void setDate(DateTime newDate)
        {
            date = newDate;
            return;
        }

        public void setMStatus(bool newMStatus)
        {
            medStatus = newMStatus;
            return;
        }

        public DiaryEntry()
        {
        }

        public DiaryEntry(DiaryEntry oldDiaryEntry)
        {
            this.description = oldDiaryEntry.description;
            this.rating = oldDiaryEntry.rating;
            this.date = oldDiaryEntry.date;
            this.medStatus = oldDiaryEntry.medStatus;
        }

        public DiaryEntry(string description, int rating, DateTime date, bool medStatus)
        {
            this.description = description;
            this.rating = rating;
            this.date = date;
            this.medStatus = medStatus;
        }

        public DiaryEntry(string description, int rating, DateTime date)
        {
            this.description = description;
            this.rating = rating;
            this.date = date;
        }


        //preparation for serialising, takes a DiaryEntry instance and saves it to a list
        public static List<string> serialisePrep(DiaryEntry entry)
        {
            List<string> result = new List<string>(3);
            string ratingDate = $"Today's date and time is: {entry.getDate().ToString("MMMM dd yyyy - hhtt")}";
            string rating = $"I would rate today: {entry.getRating()}";
            string description = $"This is because: {entry.getDescription()}";
            result.Add(ratingDate);
            result.Add(rating);
            result.Add(description);
            return result;
        }

        //preparation for serialising, takes a DiaryEntry instance and saves it to a list
        //used when adding to past files
        public static List<string> serialisePrep(DiaryEntry entry, DateTime currentDate)
        {
            List<string> result = new List<string>(4);
            string editedDate = $"\nThis additional entry was added on the following date: {currentDate.ToString("MMMM dd yyyy - hhtt")}";
            string ratingDate = $"Today's date and time is: {entry.getDate().ToString("MMMM dd yyyy")}";
            string rating = $"I would rate today: {entry.getRating()}";
            string description = $"This is because: {entry.getDescription()}";
            result.Add(editedDate);
            result.Add(ratingDate);
            result.Add(rating);
            result.Add(description);
            return result;
        }

        public static string translateFromFile(string filepath)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                List<string> fileContents = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(filepath));
                foreach (object i in fileContents)
                {
                    string content = i.ToString();
                    sb.Append(content + "\n");
                }
            }
            catch (JsonException ex)
            {
                string fileContents = File.ReadAllText(filepath);
                sb.Append(fileContents);
            }
            return sb.ToString();
        }

        //ensures answer is an integer and is within the needed range
        public static int testAcceptableAnswer(string answer)
        {
            bool acceptableAnswer = false;
            int ratingToday = 0;

            while (!acceptableAnswer)
            {
                if (answer == null || answer.ToLower().Contains("cancel") || answer == "")
                {
                    Console.WriteLine(tr_cancel);
                    Console.ReadKey();
                    return -1;
                }
                if (Int32.TryParse(answer, out ratingToday))
                {
                    if (!(ratingToday >= 0 && ratingToday <= 5))
                    {
                        Console.WriteLine(tr_invalid_input + "\nThe input needs to be between 0 and 5 (inclusive)." + tr_try_again);
                        answer = Console.ReadLine();
                    }
                    else
                    {
                        acceptableAnswer = true;
                    }
                }
                else
                {
                    Console.WriteLine(tr_invalid_input + tr_try_again);
                    answer = Console.ReadLine();
                }
            }
            return ratingToday;
        }

        //saves the entry to a txt file
        public static void saveRateToFile(DateTime fileDate, int ratingInt, string descriptionUnfull)
        {
            DiaryEntry entry = new DiaryEntry { date = fileDate,rating = ratingInt,description = descriptionUnfull };

            //The files are organised so that a year is a folder, and all the entries that are included in that year are stored there
            //creates a new directory for each year, if one doesn't exist already
            string yearFilePath = diaryFilePath + $"\\{fileDate.Year.ToString()}";

            if (!Directory.Exists(yearFilePath))
            {
                Directory.CreateDirectory(yearFilePath);
            }

            string currentFilePath = yearFilePath + $"\\{fileDate.ToString("MMMM dd yyyy")}" + ".txt";
            string cerealResult = "";
            StringBuilder sb = new StringBuilder();

            //creates a new file for  the day if one doesn't exist, then writes serialised entry to the file
            if (!File.Exists(currentFilePath))
            {                
                File.Create(currentFilePath).Close();

                cerealResult = JsonSerializer.Serialize(serialisePrep(entry));

                File.WriteAllText(currentFilePath, cerealResult);
                Console.WriteLine("\nThis file has been created and your work has been saved.");
            }
            else if (File.Exists(currentFilePath))
            {
                Console.WriteLine("\nThis file already exists.\nWould you like to add to it?");
                string answer = Console.ReadLine().ToLower();
                var trueAnswer = testBoolAnswer(answer);
                bool truebool = new bool();
                if (trueAnswer.HasValue)
                {
                    truebool = trueAnswer.Value;
                }
                else
                {
                    Console.WriteLine(tr_cancel);
                    Console.ReadKey();
                    return;
                }
                if (truebool)
                {
                    try
                    {
                        string fileGuts = File.ReadAllText(currentFilePath);
                        List<string> jsonResult = JsonSerializer.Deserialize<List<string>>(fileGuts);
                        List<string> newInfo = serialisePrep(entry, DateTime.Now);
                        jsonResult.AddRange(newInfo);
                        //deserialize it, append new info, serialize them both

                        //if jsonResult doesn't throw an exception, that means the filedata already stored is serialised and we can continue
                        cerealResult = JsonSerializer.Serialize(jsonResult);
                        File.WriteAllText(currentFilePath, cerealResult);
                    }
                    catch (JsonException)
                    {
                        try
                        {
                            List<string> placeholderResult = serialisePrep(entry, DateTime.Now);
                            sb.AppendJoin("\n", placeholderResult);
                            cerealResult = string.Concat("\n", sb.ToString());
                            File.AppendAllText(currentFilePath, cerealResult);
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Unable to write to file." + tr_try_again);
                            Console.ReadKey();
                            return;
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Unable to write to file." + tr_try_again);
                        Console.ReadKey();
                        return;
                    }
                    Console.WriteLine("\nThis file has been edited and your work has been saved.");
                }
                else
                {
                    Console.WriteLine("\nThis file has been left as is.");
                }

            }
            else
            {
                Console.WriteLine(tr_error_general + tr_try_again);
            }
            Console.ReadKey();
            return;
        }


        //used to rate the current day
        public static void rateToday(DateTime? possibleTime)
        {
            Console.WriteLine("How was your day?\nPlease rate it on a scale of 0-5.\n0 being \"horrible\" and 5 being \"amazing\"");
            
            int ratingToday = testAcceptableAnswer(Console.ReadLine());
            if (ratingToday == -1)
            {
                //runs if the user entered the keyword 'cancel'
                return;
            }

            Console.WriteLine("\nWhy do you feel like this?\nPlease tell me what is on your mind?");
            string ratingDescription = Console.ReadLine();

            //runs if the word 'cancel' is the only word given in the answer
            if (ratingDescription.ToLower().Trim() == "cancel")
            {
                Console.WriteLine(tr_cancel);
                Console.ReadKey();
                return;
            }

            //used when editing older diary entries
            //if there is a DateTime parameter given, it will ask for medication info too
            if (!possibleTime.HasValue)
            {
                DateTime currentDate = DateTime.Now;
                string yearFilePath = diaryFilePath + $"\\{currentDate.Year.ToString()}";
                string currentFilePath = yearFilePath + $"\\{currentDate.ToString("MMMM dd yyyy")}" + ".txt";
                if (!File.Exists(currentFilePath))
                {
                    //saves the rating given for the day in a file
                    MedicationLog.newEntry_ConsultMedication();
                }
                saveRateToFile(DateTime.Now, ratingToday, ratingDescription);
                return;
            }
            else if (possibleTime.HasValue)
            {
                saveRateToFile(possibleTime.Value, ratingToday, ratingDescription);
                return;
            }
            else
            {
                Console.WriteLine(tr_error_general + tr_try_again);
                Console.ReadKey();
                return;
            }
        }


        //OPEN AND READ PREVIOUS FILE
        //searches folders for the specified filepath
        public static string? searchFolders(string filePath)
        {

            //Present available folders
            if(!Directory.Exists(filePath) )
            {
                Console.WriteLine(tr_unfound_path + tr_try_again);
                Console.ReadKey();
                return "cancel";
            }
            if (!Directory.GetDirectories(filePath).Any())
            {
                Console.WriteLine("There are no available folders in this directory.");
                return "cancel";
            }
            Console.WriteLine("Your available folders are:");
                        
            HashSet<string> yearDirectory = new HashSet<string>();
            foreach (string innerDirectory in Directory.GetDirectories(filePath))
            {
                //this is assigns the variable "yearInQuestion" to the last section of the directory path (i.e. the year of the files saved)
                string yearInQuestion = innerDirectory.Split(new char[] { '\\' }).Last();

                //adds the year to the hashset "yearDirectory" to query later
                //      - used a hashset to ensure no repeating values just as another layer of protection in case any errors occured like duplicate folders (even though that shouldn't be possible)
                //prints the year to the console for easier navigation/use for the user
                Console.WriteLine(yearInQuestion);
                yearDirectory.Add(yearInQuestion.ToLower());
            }

            //Choose folders
            Console.WriteLine("\nWhat folder would you like to access?");
            string yearFolderAccess = Console.ReadLine().ToLower();

            while (!yearDirectory.Contains(yearFolderAccess))
            {
                if (yearFolderAccess.Contains("cancel"))
                {
                    Console.WriteLine(tr_cancel);
                    Console.ReadKey();
                    return "cancel";
                }
                Console.WriteLine($"\n{tr_unfound_path + tr_try_again}");
                yearFolderAccess = Console.ReadLine().ToLower();
            }
            string fullFilepath = filePath + $"\\{yearFolderAccess}";

            if (!Directory.GetFiles(fullFilepath).Any())
            {
                Console.WriteLine("There are no available files in this directory.");
                Console.ReadKey();
                return "cancel";
            }


            //Present available files
            Console.WriteLine("\nYour available files are:");

            //Used a dictionary because I wanted to be able to accept the counter variable as an input as well, as this would make navigating the files easier for the user.
            //Especially in the case where there are a large amount of files with long file names
            Dictionary<string, string> ratingDirectory = new Dictionary<string, string>();
            int counter = 0;

            //loops over files and presents them in a way that is easy for the user to use
            foreach (string innerFiles in Directory.GetFiles(fullFilepath))
            {
                //string file = innerFiles.Split(new char[] { '\\' }).Last();
                string file = Path.GetFileNameWithoutExtension(innerFiles);
                counter++;
                Console.WriteLine($"    {counter}: {file}");

                //adds the counter and the file name to the dictionary "ratingDirectory"
                ratingDirectory.Add(counter.ToString(), file.ToLower().Trim());
            }


            //Choose file
            Console.WriteLine("\nWhat file would you like to access?");
            string fileAccess = Console.ReadLine().ToLower();

            List<string> trimmedValues = ratingDirectory.Values.ToList();

            // runs if the input is not a key nor a value in ratingDIrectory
            while (!(ratingDirectory.ContainsKey(fileAccess)) && !(trimmedValues.Contains(fileAccess)))
            {
                if (fileAccess.Contains("cancel"))
                {
                    Console.WriteLine(tr_cancel);
                    Console.ReadKey();
                    return "cancel";
                }
                Console.WriteLine($"\n{tr_unfound_path + tr_try_again}\nPlease remember to include the present whitespace.\nOr, use the assigned number.");
                fileAccess = Console.ReadLine();
            }

            if (ratingDirectory.Keys.Contains(fileAccess))
            {
                fileAccess = ratingDirectory.GetValueOrDefault(fileAccess);
            }

            //if the first letter of fileAccess isn't a capital letter, it capitalizes it
            //this matters because all the file names start with capital letters
            //so a name that starts with a lowercase letter will mess with the file identification system
            fileAccess = string.Concat(string.Concat(fileAccess.Where(x => fileAccess.IndexOf(x) == 0)).ToUpper(), string.Concat(fileAccess.Where(x => fileAccess.IndexOf(x) > 0)));            
            
            //adds the .txt back
            if (!fileAccess.EndsWith(".txt"))
            {
                if (fileAccess.Contains(".")) { fileAccess = fileAccess.Remove('.'); }

                fileAccess = string.Concat(fileAccess, ".txt");
            }
            string finalFilepath = string.Concat(fullFilepath, $"\\{fileAccess}");
            return finalFilepath;
        }

        //menu for accessing older files
        public static void editFoundFile(string fullFilepath)
        {
            if (fullFilepath == null || fullFilepath == "cancel" || fullFilepath == "")
            {
                return;
            }

            string newFilePath = Path.GetFileNameWithoutExtension(fullFilepath);

            if (!File.Exists(fullFilepath))
            {
                Console.WriteLine(tr_unfound_path + tr_try_again);
                Console.ReadKey();
                return;
            }

            bool finished = false;
            Console.Clear();

            while (!finished)
            {
                Console.WriteLine($"You are currently accessing file:\n{newFilePath}");
                Console.WriteLine(@"
What do you want to do with this file?
    0. Cancel Operation
    1. Read File
    2. Add to File
    3. Delete File");
                string answer = Console.ReadLine().ToLower();
                if (answer.Contains("cancel") || answer.Contains("0") || answer.Contains("zero"))
                {
                    Console.WriteLine(tr_cancel);
                    Console.ReadKey();
                    return;
                }
                else if (answer.Contains("read") || answer.Contains("1") || answer.Contains("one"))
                {
                    Console.Clear();
                    Console.WriteLine($"You are currently accessing file:\n{newFilePath}\n");
                    Console.WriteLine(textdivider);
                    Console.WriteLine(translateFromFile(fullFilepath));
                    Console.WriteLine(textdivider);
                    Console.ReadKey();
                }
                else if (answer.Contains("add") || answer.Contains("append") || answer.Contains("2") || answer.Contains("two"))
                {
                    Console.Clear();
                    string getDateFromFileTitle = Path.GetFileNameWithoutExtension(fullFilepath).Replace("-","").Trim();
                    try
                    {
                        rateToday(DateTime.Parse(getDateFromFileTitle));
                    }
                    catch(Exception exc)
                    {
                        Console.WriteLine(tr_error_specific.Replace("SPECIFIC", exc.Message) + tr_try_again);
                        Console.ReadKey();                        
                    }
                }
                else if (answer.Contains("delete") || answer.Contains("3") || answer.Contains("three"))
                {
                    File.Delete(fullFilepath);
                    Console.WriteLine("This file has been deleted.");
                    return;
                }

                Console.Clear();
            }
        }




        //GET AVERAGE RATE
        //takes inputs like "1 month, 2 months, 18 weeks, 5 days, 1 year" and counts them and saves to a dictionary
        public static Dictionary<string, int> rangeOfAverage(string duration)
        {
            if (duration == null || duration.Trim() == "")
            {
                Console.WriteLine(tr_invalid_input + tr_try_again);
                Console.ReadKey();
                return null;
            }

            duration = duration.ToLower().Trim();
            List<string> durationSplit = duration.Replace(" ", "").Split(",").ToList();
            Dictionary<string, int> averageDuration = new Dictionary<string, int>();
            foreach (string entry in durationSplit)
            {
                //look and separate the numbers from the letters and save to dictionary
                int number = 0;
                string numberInString = "";
                string time = "";

                List<char> entryListNumbers = entry.Where(x => Char.IsDigit(x)).ToList();
                List<char> entryListStrings = entry.Where(x => Char.IsLetter(x)).ToList();

                //build the entire number from the characters saved
                foreach (char entryNumber in entryListNumbers)
                {
                    numberInString = string.Concat(numberInString, entryNumber);
                }

                bool trueParse = int.TryParse(numberInString, out number);
                if (!trueParse)
                {
                    Console.WriteLine("This operation has failed.\nPlease try again.");
                    Console.ReadKey();
                    return new Dictionary<string, int>();
                }

                //assigns the timespans the specified amount
                //"time" is the timespan (days,months,years, etc.)
                time = string.Join("", entryListStrings).TrimEnd('s');
                HashSet<string> timespans = new HashSet<string> { "day", "month", "week", "year", "hour" };
                foreach(string timeBlock in timespans)
                {
                    if (time.ToLower().Contains(timeBlock.ToLower()))
                    {
                        if (!averageDuration.TryAdd(timeBlock, number))
                        {
                            averageDuration[timeBlock] += number;
                        }
                    }
                }
                
            }

            return averageDuration;


        }

        //prepares entered dates to be parsed, parses them into a dateTime variable, and uses that to decide if files are in the specified timespan
        public static List<string> rangeOfAverage_withDates(string date1, string date2)
        {
            List<string> validPaths = new List<string>();
            DateTime laterTime = new DateTime();
            DateTime earlyTime = new DateTime();

            //removes characters that might interfere with DateTime.TryParse
            if (date1.Contains("\\") || date1.Contains("-") || date1.Contains(".txt"))
            {
                date1 = date1.Split(new char[] { '\\' }).Last().Replace(".txt", "").Replace("- ", "");
            }
            if (date2.Contains("\\") || date2.Contains("-") || date2.Contains(".txt"))
            {
                date2 = date2.Split(new char[] { '\\' }).Last().Replace(".txt", "").Replace("- ", "");
            }

            //parses user input into DateTime variables
            if (!DateTime.TryParse(date1, out DateTime dateTime1) || !DateTime.TryParse(date2, out DateTime dateTime2))
            {
                return new List<string> { "cancel" };
            }

            //checks the diary folder, gets fileName from directory, parses it into a DateTime, and compares it to the input dates
            foreach (string dire in Directory.GetDirectories(diaryFilePath))
            {
                foreach (string file in Directory.GetFiles(dire))
                {
                    if (DateTime.TryParse(file.Split(new char[] { '\\' }).Last().Replace(".txt", "").Replace("- ", ""), out DateTime currentFile))
                    {
                        if (DateTime.Compare(dateTime1, dateTime2) >= 0)
                        {
                            laterTime = dateTime1;
                            earlyTime = dateTime2;
                        }
                        if (DateTime.Compare(dateTime1, dateTime2) == -1)
                        {
                            laterTime = dateTime2;
                            earlyTime = dateTime1;
                        }

                        if (DateTime.Compare(earlyTime, currentFile) < 0 && DateTime.Compare(currentFile, laterTime) < 0)
                        {
                            validPaths.Add(file);
                        }
                    }

                }
            }
            return validPaths;
        }




        //using the timespan (from rangeOfAverage), returns the list of files within the timeframe
        public static List<string> filesInAverageRange(Dictionary<string, int> timeRange) //input from rangeOfAverage
        {
            DateTime dateTime = DateTime.Now;
            if (timeRange == null || timeRange.Count == 0)
            {
                return null;
            }

            HashSet<string> timespans = new HashSet<string> { "week", "day", "month", "year", "hour" };

            //calculates the oldest day (from now) that is still within the specified timeframe
            foreach(string timeBlock in timespans)
            {
                if (timeRange.ContainsKey(timeBlock))
                {
                    //converts weeks listed to days
                    if (timeBlock == "week")
                    {
                        int weekHours = timeRange["week"] * 7;
                        if (!timeRange.ContainsKey("day"))
                        {
                            timeRange.Add("day", 0);
                        }
                        timeRange["day"] += weekHours;
                    }
                }
            }
            if (timeRange.ContainsKey("hour"))
            {
                dateTime = dateTime.AddHours(-timeRange["hour"]);
            }
            if (timeRange.ContainsKey("day"))
            {
                dateTime = dateTime.AddDays(-timeRange["day"]);
            }
            if (timeRange.ContainsKey("month"))
            {
                dateTime = dateTime.AddMonths(-timeRange["month"]);
            }
            if (timeRange.ContainsKey("year"))
            {
                dateTime = dateTime.AddYears(-timeRange["year"]);
            }


            string yearSearchBaseline = diaryFilePath + $"\\{dateTime.Year}";
            List<string> correctFilePath = new List<string>();


            foreach (string dire in Directory.GetDirectories(diaryFilePath))
            {
                //loops through all the files and finds the files that are within the timeframe
                //would be better if it only looped through the relevant folders

                foreach (string fileN in Directory.GetFiles(dire))
                {
                    //takes the file name and parses it into a dateTime
                    string file = fileN.Split("\\").ToList().Last().Replace(" - ", " ").Replace(".txt", "");
                    bool working = DateTime.TryParse(file, out DateTime fileDT);
                    if (working)
                    {
                        //fileDT is the date of the stored file, dateTime is the oldest date in the range given
                        // eg. if range is 4 weeks, dateTime is the date exactly 4 weeks ago
                        //compares the dates, if dateTime is younger than or the same as fileDT
                        //adds the file to List
                        int comparisonResult = DateTime.Compare(fileDT, dateTime);
                        if (comparisonResult == 1 || comparisonResult == 0)
                        {
                            correctFilePath.Add(fileN);
                        }
                    }
                }
            }
            if (correctFilePath.Count == 0)
            {
                correctFilePath.Add("cancel");
            }
            return correctFilePath;
        }

        //get the ratings from the files
        public static List<int> extractCorrectInfo(List<string> correctFilePath) //input from rangeOfAverage_withDates and filesInAverageRange
        {
            if (correctFilePath == null)
            {
                return null;
            }
            Console.Clear();
            if (correctFilePath.Contains("cancel"))
            {
                Console.WriteLine(tr_cancel + tr_try_again);
                return null;
            }

            List<int> trueRatings = new List<int>(366);

            if (correctFilePath.Count <= 0)
            {
                Console.WriteLine("There are no ratings within this time range.\nPlease try again.");
                return null;
            }
            Console.WriteLine("\nThe following files were accessed:\n");

            //using the list of acceptable files, reads the files and gets the rating info from them
            foreach (string i in correctFilePath)
            {
                if (File.Exists(i))
                {
                    //displays file so its easy for the user to read and understand
                    Console.WriteLine(Path.GetFileNameWithoutExtension(i));

                    //takes file content and converts it to a list
                    List<string> fileGuts = translateFromFile(i).Split("\n\n").ToList();
                    List<string> fileRatings = new List<string>();
                    int ratingInt = -1;

                    //for each line in a diary entry: 
                    /*
                     * Today's date and time is: November 28 2024 - 05PM
                        I would rate today: 2
                        This is because: sad
                    */
                    //gets the 2nd to last line, and splits the info in half by the :
                    /*
                     * [I would rate today, 2]
                     */
                    //gets the last value (the number) from that, and trims off whitespace. Saves to trueRatings list
                    foreach (string entry in fileGuts)
                    {
                        List<string> rating = entry.Split("\n").ToList();
                        string actualRating = "";

                        //could instead do rating[rating.Count-2].Split(":").Last().Trim();
                        if (rating.Count == 4)
                        {
                            actualRating = rating[2].Split(":").Last().Trim();
                        }
                        else if (rating.Count == 3)
                        {
                            actualRating = rating[1].Split(":").Last().Trim();
                        }
                        if (int.TryParse(actualRating, out ratingInt))
                        {
                            trueRatings.Add(ratingInt);
                        }
                    }
                }
            }
            return trueRatings;
        }




        //actually calculates the averages
        public static void calculateAverage(List<int> trueRatings) //input from extractCorrectInfo
        {
            //calculate average
            if (trueRatings == null || trueRatings.Count == 0)
            {
                return;
            }

            double meanAverage = new double();
            try
            {
                //uses multiple processors to calculate mean if there are more than 366 entries
                //supposted to be 1 a day, but given you can write into a file multiple times, can easily be more or less
                if (getProcessorCount() > 2 && trueRatings.Count() > 366)
                {
                    meanAverage = trueRatings.AsParallel().Average();
                }
                else
                {
                    meanAverage = trueRatings.Average();
                }
            }
            catch (System.ArgumentNullException)
            {               
                Console.WriteLine("There are no ratings within this time range." + tr_try_again);
                Console.ReadKey();
                return;
            }
            catch
            {
                Console.WriteLine(tr_error_general + tr_try_again);
                Console.ReadKey();
                return;
            }

            //use index to determine the rating: -0,1,2,3,4,5
            List<int> listCalc = new List<int> { 0,0,0,0,0,0};

            foreach (int i in trueRatings)
            {
                listCalc[i] += 1;
            }
            int modeKey = listCalc.IndexOf(listCalc.Max());
            

            Console.WriteLine($"\nYour mean rating is: {meanAverage}");
            Console.WriteLine($"Your most common rating is: {modeKey}");

            return;
        }



    }

    /* Medication information is saved in the medicationDetails.txt file in the root directory
	 *		This is where details about the medication the user takes is stored - things like name, how much and how often
	 *		The DateTime information of the last time the medication was take is stored there to, and this is used to calculate which medications should be taken when.
	 *		Each medication is seperated by the following symbol ";;"
	 *		
	 * Each diary entry has a space where details about the medication can be filled, e.g. have you taken your medication today??
	*/
    public class Medication
	{
		//I used the private access modifier for most of the class attributes as i didn't want direct access to be possible
		//The only exception is the name as I wanted to be able to set the variable on initialisation
		internal string name { get; set; }
        private int amount; //in milligrams
        private string purpose;
        private int frequency; //no of times a week you take this medication

        //constructors
        public Medication(string name, int dose, string purpose, int frequency)
        {
            this.name = name;
            this.amount = dose;
            this.purpose = purpose;
            this.frequency = frequency;
			return;
        }
        public Medication()
        {
            name = "default";
            amount = 0;
            purpose = "default";
            frequency = 0;
            return;
        }
        public Medication(Medication oldMedication)
        {
            name = oldMedication.name;
            amount = oldMedication.amount;
            purpose = oldMedication.purpose;
            frequency = oldMedication.frequency;
            return;
        }

        //setters + getters
        public void setName(string name)
		{
			this.name = name.ToLower().Trim();
		}
		public string getName()
		{
			return this.name;
		}
		public void setAmount(int amount)
		{
			this.amount = amount;
		}
		public int getAmount()
		{
			return this.amount;
		}
		public void setPurpose(string purpose)
		{
			this.purpose = purpose;
		}
		public string getPurpose()
		{
			return this.purpose;
		}
		public void setFrequency(int frequency)
		{
			this.frequency = frequency;
		}
		public int getFrequency()
		{
			return this.frequency;
		}


		




        //if used here - "cereal" refers to serialisation, or the serialised file (context dependant)
        //REMEMBER:
        //medicationDetails is serialised
        //medicationLog is not





        //medication methods

        //prepares medication data to be serialised to medicationDetails.txt file
        public static List<object> serializePrep(Medication medication)
        {
            List<object> list = new List<object>(5);
            try
            {
                list.Add(medication.getName());
                list.Add(medication.getAmount());
                list.Add(medication.getPurpose());
                list.Add(medication.getFrequency());
            }
            catch (Exception exc)
            {
                Console.WriteLine(tr_error_specific.Replace("SPECIFIC", exc.Message) + tr_try_again);
                return null;
            }
            return list;
        }

        //delete specific medication from medicationDetails and medicationLog files
        public static bool deleteMedicationFromCereal(string medicationName)
        {
            //find file
            //see if medicationName is there
            //delete bit with medication
            //save rest of info
            if (medicationName.ToLower() == "cancel")
            {
                Console.WriteLine(tr_cancel);
                Console.ReadKey();
                return false;
            }
            if (!File.Exists( medDetailsFilePath))
            {
                try
                {
                    File.Create(medDetailsFilePath).Close();
                }
                catch (Exception exc)
                {
                    Console.Clear();
                    Console.WriteLine(tr_error_specific.Replace("SPECIFIC", exc.Message) + tr_try_again);
                    Console.ReadKey();
                    return false;
                }

            }
            if (!Directory.Exists(medLogFilePath))
            {
                try
                {
                    Directory.CreateDirectory(medLogFilePath);
                }
                catch (Exception exc)
                {
                    Console.Clear();
                    Console.WriteLine(tr_error_specific.Replace("SPECIFIC", exc.Message) + tr_try_again);
                    Console.ReadKey();
                    return false;
                }

            }

            string textGuts = File.ReadAllText(medDetailsFilePath);
            int indexOfMedication = -1;
            List<List<object>> fileInfo = new List<List<object>>(4);
            try
            {
                fileInfo = JsonSerializer.Deserialize<List<List<object>>>(textGuts);
            }
            catch
            {
                return false;
            }

            //looks for the medication name in the file data, saves the index if found
            foreach (List<object> medicationInfo in fileInfo)
            {
                if (medicationInfo[0].ToString().ToLower() == medicationName.ToLower())
                {
                    indexOfMedication = fileInfo.IndexOf(medicationInfo);
                    break;
                }
            }

            //removes data related to medication from fileInfo
            try
            {
                fileInfo.RemoveAt(indexOfMedication);
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("There is no information already saved about this medication.");
                Console.ReadKey();
                return false;
            }
            catch
            {
                Console.WriteLine(tr_error_general + tr_try_again);
                Console.ReadKey();
                return false;
            }
            
            //fileInfo is already a list, so no need to use serialisationPrep
            string cereal = JsonSerializer.Serialize(fileInfo);
            try
            {
                File.WriteAllText(medDetailsFilePath, cereal);
            }
            catch
            {
                Console.WriteLine(tr_error_general + tr_try_again);
                Console.ReadKey();
                return false;
            }
            return true;

        }

        //save medication information to the medicationDetails file
        public static void saveCerealToFile(List<object> cereal)
        {
            //check if file exists
            if (!File.Exists( medDetailsFilePath))
            {
                try
                {
                    File.Create(medDetailsFilePath).Close();
                }
                catch (Exception exc)
                {
                    Console.Clear();
                    Console.WriteLine(tr_error_specific.Replace("SPECIFIC", exc.Message) + tr_try_again);
                    Console.ReadKey();
                    return;
                }

            }

            //delete cereal if already there
            deleteMedicationFromCereal((string)cereal[0]);
            string textGuts = File.ReadAllText(medDetailsFilePath);

            List<List<object>> fileInfo = new List<List<object>>(5);
            if (textGuts.Trim().Length > 0) 
            {
                fileInfo = JsonSerializer.Deserialize<List<List<object>>>(textGuts);
            }

            fileInfo.Add(cereal);
            string storedCereal = JsonSerializer.Serialize(fileInfo);

            //overwrites file info with serialized string storedCereal
            try
            {
                File.WriteAllText(medDetailsFilePath, storedCereal);
            }
            catch
            {
                Console.WriteLine(tr_error_general + tr_try_again);
                Console.ReadKey();
                return;
            }
            Console.WriteLine("Your information has been saved.");
            return;


        }


     

        //view serialised medication information
        public static void viewMedicalCereal()
        {
            if (!File.Exists(medDetailsFilePath))
            {
                try
                {
                    File.Create(medDetailsFilePath).Close();
                }
                catch (Exception exc)
                {
                    Console.Clear();
                    Console.WriteLine($"The following error has occured:\n{exc.Message}" + tr_try_again);
                    Console.ReadKey();
                    return;
                }
            }

            //deserialises file contents, if empty, returns
            string fileInsides = File.ReadAllText(medDetailsFilePath);
            List<List<object>> fileInfo = new List<List<object>>(5);
            try
            {
                fileInfo = JsonSerializer.Deserialize<List<List<object>>>(fileInsides);
            }
            catch (JsonException)
            {
                Console.WriteLine("There is no medication information stored.");
                Console.ReadKey();
                return;
            }
            catch (Exception exc)
            {
                Console.WriteLine(tr_error_specific.Replace("SPECIFIC", exc.Message) + tr_try_again);
                Console.ReadKey();
                return;
            }

            //remove empty/null list<object> and objects from fileInfo
            fileInfo.RemoveAll(x => x == null);


            if (fileInfo.Count == 0)
            {
                Console.WriteLine("There is no medication information stored.");
                Console.ReadKey();
                return;
            }
            foreach (List<object> medicationDetails in fileInfo) 
            {
                Console.WriteLine($"Your medication is called: {medicationDetails[0].ToString().Replace("[","")}");
                Console.WriteLine($"Your dose is: {medicationDetails[1]}mg");
                Console.WriteLine($"This medication is for: {medicationDetails[2].ToString().Replace("\"", "")}");
                Console.WriteLine($"You need to take your medication: {medicationDetails[3]} times a week");

                if (fileInfo.IndexOf(medicationDetails) == fileInfo.Count() - 1)
                {
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine(textdivider);
                }
                
            }

            Console.ReadKey();
            return;
        }

        //create new medication
        public static void medicationCreationMain(string filePath)
        {
            string medAnswer = "";

            List<string> questions = new List<string> {
                    "What is the name of your medication?",
                    "How much is one dose (in milligrams)?",
                    "What is your medication for?",
                    "How many times should you take your medication in a week?"
                };
            List<object> answers = new List<object>(4);

            foreach (string q in questions)
            {
                //general response
                Console.WriteLine(q);
                medAnswer = Console.ReadLine().ToLower();
                while (medAnswer.Trim() == "")
                {
                    Console.WriteLine(tr_invalid_input + tr_try_again);
                    medAnswer = Console.ReadLine().ToLower();
                }
                if (medAnswer.Contains("cancel"))
                {
                    Console.WriteLine(tr_cancel);
                    return;
                }

                object objAnswer = medAnswer;

                //runs if answer needs to be an int
                if (questions.IndexOf(q) == 1 || questions.IndexOf(q) == 3)
                {
                    bool parseResult = int.TryParse(medAnswer, out int doseAnswer);
                    while (!parseResult)
                    {
                        if (medAnswer.Contains("cancel"))
                        {
                            Console.WriteLine(tr_cancel);
                            return;
                        }
                        Console.WriteLine(tr_invalid_input + tr_try_again);
                        medAnswer = Console.ReadLine().ToLower();
                        parseResult = int.TryParse(medAnswer, out doseAnswer);
                    }
                    objAnswer = medAnswer;
                }
                answers.Add(objAnswer);
                Console.WriteLine();
            }

            if (medAnswer.Contains("cancel"))
            {
                Console.WriteLine(tr_cancel);
                return;
            }
            else
            {
                try
                {
                    //makes new medication instance and saves info to file
                    Medication newMed = new Medication((string)answers[0], Int32.Parse((string)answers[1]), (string)answers[2], Int32.Parse((string)answers[3]));
                    saveCerealToFile(serializePrep(newMed));
                    MedicationLog newLog = new MedicationLog(newMed);
                    MedicationLog.saveLogToFile(newLog);
                }
                catch (Exception)
                {
                    Console.WriteLine(tr_error_general + tr_try_again);
                    Console.ReadKey();
                    return;
                }
                
            }
            Console.WriteLine("Your medication has been created.");
            Console.ReadKey();
            return;
        }


      

        //asks user for medication to delete
        public static string deleteMedication()
        {
            viewMedicalCereal();
            Console.WriteLine("Which medication would you like to delete?\n(Please input a name)");
            string answer = Console.ReadLine().ToLower().Trim();
            return answer;
        }

    }

    public class MedicationLog : Medication
    {
        private Dictionary<DateTime, bool> record;


        //getters
        public Dictionary<DateTime,bool> getRecord()
        {
            return this.record;
        }


        //setters
        public void setRecord(Dictionary<DateTime, bool> newRecord)
        {
            record = newRecord;
            return;
        }
        public Dictionary<DateTime,bool> defaultRecord()
        {
            DateTime today = DateTime.Today;
            Dictionary<DateTime, bool> recordDict = new Dictionary<DateTime, bool>(7);
            for (int i = 7; i > 0; i--)
            {
                DateTime lastWeekValue = today.Subtract(new TimeSpan(i, 0, 0, 0));
                recordDict.Add(lastWeekValue, false);
            }
            return recordDict;
        }


        //constructors
        public MedicationLog(Medication med)
        {
            this.setName(med.getName());
            this.setFrequency(med.getFrequency());
            record = defaultRecord();
        }
        public MedicationLog()
        {
            return;
        }
        public MedicationLog(string name, int frequency, Dictionary<DateTime, bool> record)
        {
            this.setName(name.ToLower());
            this.setFrequency(frequency);
            this.record = record;
        }
        public MedicationLog(string name, int frequency)
        {
            this.setName(name.ToLower());
            this.setFrequency(frequency);
            record = defaultRecord();
        }


        //serialise things
        //parameters - medicationLog, returns - null, List<object>
        public static List<object> serializePrep(MedicationLog log)
        {
            List<object> list = new List<object>(3);
            list.Add(log.getName());
            list.Add(log.getFrequency());
            list.Add(log.getRecord());
            return list;
        }

        //deserialise things
        //parameters - contents of file, returns - medicationLog, null
        public static MedicationLog deserializingInfo(string fileContents)
        {
            //name,frequency, Dictionary<DateTime,bool>
            List<object> jsonResult = new List<object>();

            try
            {
                jsonResult = JsonSerializer.Deserialize<List<object>>(fileContents);
            }
            catch (Exception exc)
            {
                Console.WriteLine(tr_error_specific.Replace("SPECIFIC", exc.Message) + tr_try_again);
                Console.ReadKey();
                Console.Clear();
                return null;
            }

            //extracts name and frequency from list jsonResult
            string name = Convert.ToString(jsonResult[0]);
            int frequency = -1;
            int.TryParse(Convert.ToString(jsonResult[1]), out frequency);

            //extracts weeklyRecord from list jsonResult
            Dictionary<DateTime,bool> record = new Dictionary<DateTime,bool>();
            string recordString = Convert.ToString(jsonResult.Last());

            List<string> recordList = recordString.Split(",").ToList();
            recordList = recordList.Select(x => x.Replace("[", "").Replace("{", "").Replace("}", "").Replace("]", "").Replace("\"", "")).ToList();

            foreach(string i in recordList)
            {
                /*split by the last : in the string
                string iA = string.Join("",i.Reverse());
                List<string> iB = iA.Split(":", 2).ToList();
                string iC = iB.Select(x => string.Join("",x.Reverse()));
                List<string> iD = iC.Reverse().ToList();
                */

                List<string> thisList = string.Join("", i.Reverse()).Split(":", 2).ToList().Select(x => string.Join("", x.Reverse())).Reverse().ToList();

                DateTime trueDate = new DateTime();
                bool trueBool = new bool();
                bool dateAnswer = DateTime.TryParse(thisList.First(), out trueDate);
                bool boolAnswer = bool.TryParse(thisList.Last(), out trueBool);

                if (boolAnswer && dateAnswer)
                {
                    if (!record.Keys.Contains(trueDate) && record.Count < 7)
                    {
                        record.Add(trueDate, trueBool);
                    }
                }
                
            }
            
            MedicationLog log = new MedicationLog(name, frequency, record);
            return log;
        }


        //saves information to the MedicationLog file
        //parameters - medicationLog
        public static void saveLogToFile(MedicationLog med)
        {
            //check if directory exists
            //check if file exists
            /* if it doesn't:
             *      create new file
             *      medicationName_log.txt
             * if it does:
             *      reads file data
             *      overwrites information
             *      writes onto file
             */

            //checks for directory
            if (!Directory.Exists(medLogFilePath))
            {
                try
                {
                    Directory.CreateDirectory(medLogFilePath);
                }
                catch (Exception exc)
                {
                    Console.Clear();
                    Console.WriteLine($"The following error has occured:\n{exc.Message}" + tr_try_again);
                    Console.ReadKey();
                    return;
                }
            }

            string filepath = medLogFilePath + $"\\{med.getName().ToLower()}_log.txt";

            if (!File.Exists(filepath))
            {
                try
                {
                    File.Create(filepath).Close();
                }
                catch (Exception exc)
                {
                    Console.Clear();
                    Console.WriteLine($"The following error has occured:\n{exc.Message}" + tr_try_again);
                    Console.ReadKey();
                    return;
                }

            }

            
            List<object> serialisedInfo = serializePrep(med);
            string finalForm = JsonSerializer.Serialize(serialisedInfo);
            File.WriteAllText(filepath, finalForm);
            return;
        }

        public static void saveLogToFile(Medication med)
        {
            //check if directory exists
            //check if file exists
            /* if it doesn't:
             *      create new file
             *      medicationName_log.txt
             * if it does:
             *      reads file data
             *      overwrites information
             *      writes onto file
             */

            //checks for directory
            if (!Directory.Exists(medLogFilePath))
            {
                try
                {
                    Directory.CreateDirectory(medLogFilePath);
                }
                catch (Exception exc)
                {
                    Console.Clear();
                    Console.WriteLine($"The following error has occured:\n{exc.Message}" + tr_try_again);
                    Console.ReadKey();
                    return;
                }
            }

            MedicationLog log = new MedicationLog(med);

            string filepath = medLogFilePath + $"\\{log.getName().ToLower()}_log.txt";

            if (!File.Exists(filepath))
            {
                try
                {
                    File.Create(filepath).Close();
                }
                catch (Exception exc)
                {
                    Console.Clear();
                    Console.WriteLine($"The following error has occured:\n{exc.Message}" + tr_try_again);
                    Console.ReadKey();
                    return;
                }

            }


            List<object> serialisedInfo = serializePrep(log);

            string finalForm = JsonSerializer.Serialize(serialisedInfo);
            File.WriteAllText(filepath, finalForm);
            return;
        }


        //delete medicationlog
        public static void deleteMedicationLog(MedicationLog med)
        {
            //check if file exists
            //if so, delete the file
            //if not, return

            if (med.getName().ToLower() == "cancel")
            {
                Console.WriteLine(tr_cancel);
                Console.ReadKey();
                return;
            }

            string filepath = medLogFilePath + $"\\{med.getName().ToLower()}_log.txt";
            if (File.Exists(filepath))
            {
                try
                {
                    File.Delete(filepath);
                }
                catch (Exception exc)
                {
                    Console.WriteLine(tr_error_specific.Replace("SPECIFIC", exc.ToString()) + tr_try_again);
                    Console.ReadKey();
                    Console.Clear();
                    return;
                }
            }
            else
            {
                Console.WriteLine(tr_unfound_path + tr_try_again);
                Console.ReadKey();
                Console.Clear();
                return;
            }
            return;

        }


        public static void deleteMedicationLog(Medication med)
        {
            //check if file exists
            //if so, delete the file
            //if not, return

            if (med.getName().ToLower() == "cancel")
            {
                Console.WriteLine(tr_cancel);
                Console.ReadKey();
                return;
            }

            string filepath = medLogFilePath + $"\\{med.getName().ToLower()}_log.txt";
            if (File.Exists(filepath))
            {
                try
                {
                    File.Delete(filepath);
                }
                catch (Exception exc)
                {
                    Console.WriteLine(tr_error_specific.Replace("SPECIFIC", exc.ToString()) + tr_try_again);
                    Console.ReadKey();
                    Console.Clear();
                    return;
                }
            }
            else
            {
                Console.WriteLine(tr_unfound_path + tr_try_again);
                Console.ReadKey();
                Console.Clear();
                return;
            }
            return;

        }

        public static void deleteMedicationLog(string med)
        {
            //check if file exists
            //if so, delete the file
            //if not, return

            if (med.ToLower() == "cancel")
            {
                Console.WriteLine(tr_cancel);
                Console.ReadKey();
                return;
            }

            string filepath = medLogFilePath + $"\\{med.ToLower()}_log.txt";
            if (File.Exists(filepath))
            {
                try
                {
                    File.Delete(filepath);
                }
                catch (Exception exc)
                {
                    Console.WriteLine(tr_error_specific.Replace("SPECIFIC", exc.ToString()) + tr_try_again);
                    Console.ReadKey();
                    Console.Clear();
                    return;
                }
            }
            else
            {
                Console.WriteLine(tr_unfound_path + tr_try_again);
                Console.ReadKey();
                Console.Clear();
                return;
            }
            return;

        }


        //medication log file - logs whether medication has been taken in the past week
        //med name, number of times a week, true/false to the corresponding days (1 week, excluding current day)
        public static void checkMedicationLog(string medicationName)
        {
            string coreFilePath = $"{medLogFilePath}\\{medicationName.ToLower()}_log.txt";
            if (!File.Exists(coreFilePath))
            {
                Console.Clear();
                Console.WriteLine(tr_unfound_path);
                Console.ReadKey();
                return;
            }
            if (medicationName.ToLower().Trim() == "cancel")
            {
                Console.Clear();
                Console.WriteLine(tr_cancel);
                Console.ReadKey();
                return;
            }

            string fileGuts = File.ReadAllText(coreFilePath);
            MedicationLog coreLog = new MedicationLog();
            try
            {
                coreLog = deserializingInfo(fileGuts);
            }
            catch (Exception exc)
            {
                Console.WriteLine(tr_error_general + tr_try_again);
                Console.ReadKey();
                Console.Clear();
                return;
            }

            int takenMedication = coreLog.getRecord().Values.Count(x => x == true);

            Console.Clear();
            Console.WriteLine($@"For your {medicationName}:
For the past week (from {DateTime.Today.Subtract(new TimeSpan(7, 0, 0, 0)).Date.ToString("MMMM dd yyyy")} to {DateTime.Today.Subtract(new TimeSpan(1, 0, 0, 0)).Date.ToString("MMMM dd yyyy")}), you have taken your medication {takenMedication} times.");
            if (takenMedication > coreLog.getFrequency())
            {
                Console.WriteLine($"This is more than your recommended weekly dosage, which is {coreLog.getFrequency()}.\nPlease consult your primary care doctor if you are having any issues.");
            }
            else if (takenMedication == coreLog.getFrequency())
            {
                Console.WriteLine("Congrats! You are up to date on your medication.");
            }
            else
            {
                Console.WriteLine($"This is less than your recommended weekly dosage, which is {coreLog.getFrequency()}.\nPlease remember to take your medication on time.");
            }
            Console.ReadKey();
            return;
        }

        //checks if user has taken their medication today
        public static void newEntry_ConsultMedication()
        {
            if (!File.Exists(medDetailsFilePath))
            {
                Console.WriteLine(tr_JOINT_unfoundPath_creating_specific.Replace("SPECIFIC", medDetailsFilePath));
                File.Create(medDetailsFilePath).Close();
                Console.ReadKey();
            }
            if (!Directory.Exists(medLogFilePath))
            {
                Console.WriteLine(tr_JOINT_unfoundPath_creating_specific.Replace("SPECIFIC", medLogFilePath));
                Directory.CreateDirectory(medLogFilePath);
                Console.ReadKey();
            }

            //tries to deserialise medication details
            string fileGuts = File.ReadAllText(medDetailsFilePath);
            List<List<object>> detailsCereal = new List<List<object>>(5);
            try
            {
                detailsCereal = JsonSerializer.Deserialize<List<List<object>>>(fileGuts);
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("There is no medical information stored in the \"medicationDetails.txt\" file.");
                Console.ReadKey();
                return;
            }
            catch (Exception)
            {
                Console.WriteLine(tr_error_general + tr_try_again);
                Console.ReadKey();
                return;
            }

            //creates Medication instances for the meds in medicationDetails
            List<Medication> medications = new List<Medication>();
            foreach (List<object> meds in detailsCereal)
            {
                Medication newMed = new Medication();
                try
                {
                    //string int string int datetime
                    newMed.setName(meds[0].ToString());
                    newMed.setAmount(int.Parse(meds[1].ToString()));
                    newMed.setPurpose(meds[2].ToString());
                    newMed.setFrequency(int.Parse(meds[3].ToString()));
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine("\nIncomplete information" + tr_try_again);
                    Console.ReadKey();
                    return;
                }
                catch (Exception)
                {
                    Console.WriteLine(tr_error_general + tr_try_again);
                    Console.ReadKey();
                    return;
                }
                finally
                {
                    medications.Add(newMed);
                }
            }

            //reads contents from medlog
            //[citalopram,4,true,true,false]...

            List<MedicationLog> logs = new List<MedicationLog>();
            foreach (string file in Directory.GetFiles(medLogFilePath))
            {
                string logGuts = File.ReadAllText(file);
                MedicationLog log = deserializingInfo(logGuts);

                bool medResult = new bool();
                Console.WriteLine($"\nFor your {log.getName()}:\nHave you taken your medication today?");
                string medicationAnswer = Console.ReadLine().Trim().ToLower();
                bool newAnswer = new bool();
                var testAnswer = testBoolAnswer(medicationAnswer);
                if (testAnswer.HasValue)
                {
                    medResult = testAnswer.Value;
                }

                Dictionary<DateTime, bool> dictRecord = log.getRecord();
                if (!dictRecord.ContainsKey(DateTime.Today.Date))
                {
                    dictRecord.Remove(dictRecord.Keys.Min());
                    dictRecord.Add(DateTime.Today.Date, medResult);
                }

                log.setRecord(dictRecord);

                saveLogToFile(log);
            }
        }



    }
}
