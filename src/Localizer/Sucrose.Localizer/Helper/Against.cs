using CsvHelper;
using System.Globalization;
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

        public static void AlphabeticIndexer(string csvDirectory, string languageCode)
        {
            string csvFilePath = Path.Combine(csvDirectory, $"{languageCode}.csv");

            if (!File.Exists(csvFilePath))
            {
                Console.WriteLine($"Error: {languageCode}.csv file not found.");
                return;
            }

            Console.WriteLine($"Processing alphabetic indexer for {languageCode}.csv...");

            List<CsvRecord> records = ReadCsvRecords(csvFilePath);
            List<CsvRecord> processedRecords = ProcessRecordsForSorting(records);

            // Write the processed records back to file
            WriteCsvRecords(csvFilePath, processedRecords);

            Console.WriteLine();
            Console.WriteLine($"Alphabetic indexing for {languageCode}.csv is complete.");
            Console.WriteLine();
        }

        private static List<CsvRecord> ReadCsvRecords(string filePath)
        {
            List<CsvRecord> records = new();

            using StreamReader reader = new(filePath);
            using CsvReader csv = new(reader, CultureInfo.InvariantCulture);

            csv.Read();
            csv.ReadHeader();

            while (csv.Read())
            {
                records.Add(new CsvRecord
                {
                    Hash = csv.GetField("Hash"),
                    File = csv.GetField("File"),
                    Key = csv.GetField("Key"),
                    Value = csv.GetField("Value")
                });
            }

            return records;
        }

        private static List<CsvRecord> ProcessRecordsForSorting(List<CsvRecord> records)
        {
            List<CsvRecord> processedRecords = new(records);

            for (int i = 0; i < processedRecords.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(processedRecords[i].Key) || string.IsNullOrWhiteSpace(processedRecords[i].Value))
                {
                    // Find the range to sort
                    int endIndex = i;
                    int startIndex = FindBackwardRange(processedRecords, i);

                    if (startIndex < endIndex)
                    {
                        // Extract the range to sort
                        List<CsvRecord> rangeToSort = processedRecords.Skip(startIndex).Take(endIndex - startIndex).ToList();

                        // Sort by key with custom natural sorting (handles numbers correctly)
                        rangeToSort = rangeToSort
                            .Where(r => !string.IsNullOrWhiteSpace(r.Key) && !r.Key.Equals("Base", StringComparison.OrdinalIgnoreCase) && !r.Key.Equals("Base64", StringComparison.OrdinalIgnoreCase))
                            .OrderBy(r => GetKeyForSorting(r.Key))
                            .ThenBy(r => r.Key.Length)
                            .ToList();

                        // replace the sorted range back
                        for (int j = 0; j < rangeToSort.Count; j++)
                        {
                            if (startIndex + j < processedRecords.Count)
                            {
                                processedRecords[startIndex + j] = rangeToSort[j];
                            }
                        }
                    }
                }
            }

            return processedRecords;
        }

        private static int FindBackwardRange(List<CsvRecord> records, int currentIndex)
        {
            int startIndex = currentIndex;

            // Go backward from current position
            for (int i = currentIndex - 1; i >= 0; i--)
            {
                // Stop if we find an empty key/value or "Base" key
                if (string.IsNullOrWhiteSpace(records[i].Key) ||
                    string.IsNullOrWhiteSpace(records[i].Value) ||
                    records[i].Key.Equals("Base", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
                startIndex = i;
            }

            return startIndex;
        }

        private static void WriteCsvRecords(string filePath, List<CsvRecord> records)
        {
            using StreamWriter writer = new(filePath);
            using CsvWriter csv = new(writer, CultureInfo.InvariantCulture);

            csv.WriteField("Hash");
            csv.WriteField("File");
            csv.WriteField("Key");
            csv.WriteField("Value");
            csv.NextRecord();

            foreach (CsvRecord record in records)
            {
                csv.WriteField(record.Hash);
                csv.WriteField(record.File);
                csv.WriteField(record.Key);
                csv.WriteField(record.Value);
                csv.NextRecord();
            }
        }

        private class CsvRecord
        {
            public string Hash { get; set; } = string.Empty;
            public string File { get; set; } = string.Empty;
            public string Key { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
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
                string extension = filename[index..];
                string nameWithoutExtension = filename[..index];

                int lastIndex = nameWithoutExtension.LastIndexOf('.');

                if (lastIndex > 0)
                {
                    return nameWithoutExtension[..lastIndex] + extension;
                }
            }

            return filename;
        }

        private static string GetKeyForSorting(string key)
        {
            // Extract text and number parts for natural sorting
            Match match = Regex.Match(key, @"^(.+?)(\d+)$");
            
            if (match.Success)
            {
                string textPart = match.Groups[1].Value;
                int numberPart = int.Parse(match.Groups[2].Value);
                
                // Pad number with leading zeros for proper sorting
                return $"{textPart}{numberPart:D10}";
            }
            
            return key;
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