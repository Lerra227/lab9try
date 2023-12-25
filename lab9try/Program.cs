using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace lab9try
{
    
    internal class Program
    {
        private static readonly object fileLock = new object();
        public static async Task Main(string[] args)
        {
            // Чтение списка акций из файла ticker.txt
            List<string> tickers = new List<string>();

            if (File.Exists("C:\\Users\\amana\\Downloads\\ticker.txt"))
            {
                Console.WriteLine("Reading tickers");
                tickers.AddRange(File.ReadAllLines("C:\\Users\\amana\\Downloads\\ticker.txt"));
                if (tickers.Count != 0) Console.WriteLine("Get tickers");
                for (int i = 0; i < 5; i++) Console.WriteLine(tickers[i]);
            }
            else { Console.WriteLine("File does not exist"); }

           //{
            //    string line;
            //    while ((line = reader.ReadLine()) != null)
            //    {
            //        tickers.Add(line);
            //    }
            //}
            // Создание задач для каждой акции
            List<Task> tasks = new List<Task>();
            foreach (string ticker in tickers)
            {
                // Task<double> task = GetAveragePriceForYear(ticker);
                tasks.Add(GetAveragePriceForYear(ticker));
            }
            Console.WriteLine("Задачи выполняются");
            // Ожидание завершения всех задач
            await Task.WhenAll(tasks);
            Console.WriteLine("Задачи выполнились");

            // Запись результатов в файл
            //using (StreamWriter writer = new StreamWriter("result.txt"))
            //{
            //    for (int i = 0; i < tickers.Count; i++)
            //    {
            //        string ticker = tickers[i];
            //        double averagePrice = tasks[i].Result;
            //        writer.WriteLine($"{ticker}:{averagePrice}");
            //    }
            //} 
            //Console.WriteLine("Готово!");

             async Task GetAveragePriceForYear(string ticker)
            {

                DateTime startDate = DateTime.Now.AddYears(-1);
                DateTime endDate = DateTime.Now;

                long startTimestamp = (long)(startDate.Subtract(new DateTime(1970, 1, 1))).TotalSeconds; //unix-время - количество секнуд, прошедших с 1 января 1970 г
                long endTimestamp = (long)(endDate.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;


                //long startTimestamp = new DateTimeOffset(startDate).ToUnixTimeSeconds();
                //long endTimestamp = new DateTimeOffset(endDate).ToUnixTimeSeconds();

                string url = $"https://query1.finance.yahoo.com/v7/finance/download/{ticker}?period1={startTimestamp}&period2={endTimestamp}&interval=1d&events=history&includeAdjustedClose=true";

                using (HttpClient client = new HttpClient())
                {
                    // client.DefaultRequestHeaders.Add("Authorization", "Bearer 5b46292ae01f2ba3b57add22869ad86ed5306bfe");
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync(url);
                        response.EnsureSuccessStatusCode();
                        string csvData = await response.Content.ReadAsStringAsync();
                        //! string csvData = await client.GetStringAsync(url);
                        // Разбор CSV-данных и вычисление среднего значения
                        double sum = 0;
                        int count = 0;
                        string[] lines = csvData.Split('\n');
                        for (int i = 1; i < lines.Length; i++) // Пропускаем заголовок
                        {
                            string line = lines[i];
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                string[] values = line.Split(',');
                                int high = Int32.Parse(values[2].Split('.')[0]);
                                int low = Int32.Parse(values[3].Split('.')[0]);
                                double average = (high + low) / 2;
                                sum += average;
                                count++;
                            }
                        }
                        double averagePrice = sum / count;
                        WriteToFile("result.txt", $"{ticker}: {averagePrice}");



                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine(ex.Message);
                    };

                    await Task.Delay(1000);

                }
            }

             void WriteToFile(string path, string data)
            {

                lock (fileLock)
                {
                    File.AppendAllText(path, data + '\n');
                }
            }
        }
    }
}
