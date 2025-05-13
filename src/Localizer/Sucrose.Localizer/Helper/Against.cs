using System.Text.RegularExpressions;

namespace Sucrose.Localizer.Helper
{
    internal static class Against
    {
        public static void ReindexCsv(string csvDirectory, string reindexerLang)
        {
            string reindexerSource = Path.Combine(csvDirectory, $"{reindexerLang}.csv");

            if (!File.Exists(reindexerSource))
            {
                Console.WriteLine($"Error: {reindexerLang}.csv file not found.");
                return;
            }

            Dictionary<int, string> reindexerLines = FileReadWithLines(reindexerSource);

            List<string> csvFiles = Directory.GetFiles(csvDirectory, "*.csv")
                .Where(filePath => Path.GetFileNameWithoutExtension(filePath).Length == 2)
                .ToList();

            csvFiles.Remove(reindexerSource);

            foreach (string csvFile in csvFiles)
            {
                Console.WriteLine($"-- Reindexing {Path.GetFileName(csvFile)} with {Path.GetFileName(reindexerSource)} --");

                Dictionary<int, string> newLines = new();
                Dictionary<int, string> csvLines = FileReadWithLines(csvFile);

                foreach (KeyValuePair<int, string> pair in csvLines)
                {
                    string[] fields = pair.Value.Split(',');

                    string hash = fields[0];
                    bool found = false;

                    foreach (KeyValuePair<int, string> reindexerPair in reindexerLines)
                    {
                        string[] reindexerFields = reindexerPair.Value.Split(',');

                        string reindexerHash = reindexerFields[0];

                        if (hash == reindexerHash)
                        {
                            if (reindexerPair.Key != pair.Key)
                            {
                                Console.WriteLine($"Success: Hash {hash} found in but not in the same line in reindexer file.");
                            }

                            newLines.Add(reindexerPair.Key, pair.Value);

                            found = true;

                            break;
                        }
                    }

                    if (!found)
                    {
                        Console.WriteLine($"Warning: Hash {hash} not found in reindexer file.");
                    }
                }

                Dictionary<int, string> orderedLines = newLines.OrderBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);

                File.WriteAllLines(csvFile, orderedLines.Values);

                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine("CSV file Reindexing is complete.");
            Console.WriteLine();
        }

        public static void CheckCsv(string csvDirectory)
        {
            string[] csvFiles = Directory.GetFiles(csvDirectory, "*.csv")
                .Where(filePath => Path.GetFileNameWithoutExtension(filePath).Length == 2)
                .ToArray();

            for (int i = 0; i < csvFiles.Length; i++)
            {
                for (int j = i + 1; j < csvFiles.Length; j++)
                {
                    Console.WriteLine($"-- Comparing {Path.GetFileName(csvFiles[i])} and {Path.GetFileName(csvFiles[j])} --");
                    CompareCsvFiles(csvFiles[i], csvFiles[j]);
                    Console.WriteLine();
                }
            }

            Console.WriteLine();
            Console.WriteLine("CSV file checking is complete.");
            Console.WriteLine();
        }

        public static void CheckPoe(string poeDirectory)
        {
            string[] poeFiles = Directory.GetFiles(poeDirectory, "*.csv")
                .Where(filePath => Path.GetFileNameWithoutExtension(filePath).Length == 2)
                .ToArray();

            for (int i = 0; i < poeFiles.Length; i++)
            {
                for (int j = i + 1; j < poeFiles.Length; j++)
                {
                    Console.WriteLine($"-- Comparing {Path.GetFileName(poeFiles[i])} and {Path.GetFileName(poeFiles[j])} --");
                    ComparePoeFiles(poeFiles[i], poeFiles[j]);
                    Console.WriteLine();
                }
            }

            Console.WriteLine();
            Console.WriteLine("POEditor file checking is complete.");
            Console.WriteLine();
        }

        private static void CompareCsvFiles(string filePath1, string filePath2)
        {
            string[] lines1 = File.ReadAllLines(filePath1);
            string[] lines2 = File.ReadAllLines(filePath2);

            int minLineCount = Math.Min(lines1.Length, lines2.Length);

            for (int i = 0; i < minLineCount; i++)
            {
                string[] fields1 = lines1[i].Split(',');
                string[] fields2 = lines2[i].Split(',');

                string file1 = fields1[1];
                string file2 = fields2[1];

                string key1 = fields1[2];
                string key2 = fields2[2];

                bool areInSameRow = GetFilenameWithoutLanguageCode(file1) == GetFilenameWithoutLanguageCode(file2) && key1 == key2;

                if (areInSameRow)
                {
                    //Console.WriteLine($"Row {i + 1}: Present in both files.");
                }
                else
                {
                    Console.WriteLine($"Row {i + 1}: Present in both files but different.");
                }
            }

            if (lines1.Length != lines2.Length)
            {
                Console.WriteLine("Warning: Files are of different lengths!");
            }
        }

        private static void ComparePoeFiles(string filePath1, string filePath2)
        {
            string[] lines1 = File.ReadAllLines(filePath1);
            string[] lines2 = File.ReadAllLines(filePath2);

            int minLineCount = Math.Min(lines1.Length, lines2.Length);

            for (int i = 0; i < minLineCount; i++)
            {
                string[] fields1 = Regex.Replace(lines1[i], @"""(.*?)""", m => m.Value.Replace(",", "")).Split(',');
                string[] fields2 = Regex.Replace(lines2[i], @"""(.*?)""", m => m.Value.Replace(",", "")).Split(',');

                string file1 = fields1[3];
                string file2 = fields2[3];

                string key1 = fields1[0];
                string key2 = fields2[0];

                bool areInSameRow = GetFilenameWithoutLanguageCode(file1) == GetFilenameWithoutLanguageCode(file2) && key1 == key2;

                if (areInSameRow)
                {
                    //Console.WriteLine($"Row {i + 1}: Present in both files.");
                }
                else
                {
                    Console.WriteLine($"Row {i + 1}: Present in both files but different.");
                }
            }

            if (lines1.Length != lines2.Length)
            {
                Console.WriteLine("Warning: Files are of different lengths!");
            }
        }

        private static string GetFilenameWithoutLanguageCode(string filename)
        {
            int index = filename.LastIndexOf('.');

            if (index > 0)
            {
#if NET48
                string extension = filename.Substring(index);
                string nameWithoutExtension = filename.Substring(0, index);
#else
                string extension = filename[index..];
                string nameWithoutExtension = filename[..index];
#endif

                int lastIndex = nameWithoutExtension.LastIndexOf('.');

                if (lastIndex > 0)
                {
#if NET48
                    return nameWithoutExtension.Substring(0, lastIndex) + extension;
#else
                    return nameWithoutExtension[..lastIndex] + extension;
#endif
                }
            }

            return filename;
        }

        private static Dictionary<int, string> FileReadWithLines(string filePath)
        {
            Dictionary<int, string> lines = new();

            try
            {
                using StreamReader sr = new(filePath);
                int satirNumarasi = 1;
                string satir;

                while ((satir = sr.ReadLine()) != null)
                {
                    lines.Add(satirNumarasi, satir);
                    satirNumarasi++;
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Dosya bulunamadı.");
            }
            catch (IOException e)
            {
                Console.WriteLine("Dosya okuma hatası: " + e.Message);
            }

            return lines;
        }
    }
}