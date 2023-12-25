using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;


namespace PogodaLab9
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            System.Windows.Forms.Application.Run(new Form1());
        }
    }
    public partial class Form1 : Form
    {
        private static readonly string API_KEY = "5e9529dfc67ed27293e38ca71793f3c4";
        private ComboBox comboBoxCities;
        private ListBox listBoxCities;
        public Form1()
        {
            InitializeComponent(); //инициализация элементов управления на форме
            Components();
        }

        private void Components()
        {
            Width = 400;
            Height = 300;
            Text = "Weather App";

            comboBoxCities = new ComboBox();
            comboBoxCities.Dock = DockStyle.Top;

            listBoxCities = new ListBox();
            listBoxCities.Width = 500;
            listBoxCities.Height = 500;
            listBoxCities.Left = 150;
            listBoxCities.Top = 200;

            var btnFetchCities = new Button();
            btnFetchCities.Text = "Show Cities";
            btnFetchCities.Width = 500;
            btnFetchCities.Height = 100;
            btnFetchCities.Left = 150;
            btnFetchCities.Top = 100;
            btnFetchCities.Click += BtnFetchCitiesClick;

            var btnFetchWeather = new Button();
            btnFetchWeather.Text = "Show Weather";
            btnFetchWeather.Width = 500;
            btnFetchWeather.Height = 100;
            btnFetchWeather.Left = 850;
            btnFetchWeather.Top = 100;
            //TODO: implement click +=;
            btnFetchWeather.Click += BtnFetchWeatherClick;

            Controls.Add(btnFetchCities);
            Controls.Add(btnFetchWeather);
            Controls.Add(listBoxCities);
        }

        private async void BtnFetchCitiesClick(object sender, EventArgs e) //обработчики событий кнопок #загружает список городов
        {
            listBoxCities.Text = "";
            var cities = await LoadCitiesFromFile("C:\\Users\\user\\Desktop\\city.txt");
            listBoxCities.Items.AddRange(cities.ToArray());
        }

        private async Task<List<string>> LoadCitiesFromFile(string path) //загружает список городов
        {

            List<string> datas = new List<string>();
            using (StreamReader reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    string line = await reader.ReadLineAsync();
                    datas.Add(line);
                }
            }
            List<string> cities = new List<string>();
            for (int i = 0; i < datas.Count(); ++i)
            {
                int count = 0;
                for (int j = 0; j < datas[i].Length; ++j)
                {
                    if (datas[i][j].Equals('\t'))
                    {
                        break;
                    }
                    else
                    {
                        ++count;
                    }
                }
                cities.Add(datas[i].Substring(0, count));
            }
            return cities;
        }

        private async void BtnFetchWeatherClick(object sender, EventArgs e) //обработчики событий кнопок #загружает список городов
        {
            string city = listBoxCities.SelectedItem.ToString();

            if (string.IsNullOrEmpty(city))
            {
                MessageBox.Show("Please select a city.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Weather weather = await FetchWeather(city);
                MessageBox.Show($"{weather.Country} - {weather.Name}: {weather.Temp - 273}°C, {weather.Description}",
                    "Weather Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<Weather> FetchWeather(string city) //отправлеят запрос о погоде
        {
            string apiUrl = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={API_KEY}";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync(); //асинхронный сбор данных из тела запросов в строку
                    JObject json = JObject.Parse(jsonData);

                    string country = json["sys"]["country"].ToString();
                    string name = json["name"].ToString();
                    double temp = Convert.ToDouble(json["main"]["temp"]);
                    string description = json["weather"][0]["description"].ToString();

                    return new Weather
                    {
                        Country = country,
                        Name = name,
                        Temp = temp,
                        Description = description
                    };
                }
                else
                {
                    throw new Exception($"HTTP error: {response.StatusCode}");
                }
            }
        }

    }

    struct Weather
    {
        public string Country { get; set; }
        public string Name { get; set; }
        public double Temp { get; set; }
        public string Description { get; set; }
    }
}
