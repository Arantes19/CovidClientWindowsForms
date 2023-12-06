using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Button to leave the aplication
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #region Local-Services

        #region WebAPI - No JWT
        //https://localhost:44331/

        private const string localurl = "https://localhost:44331/api/covidServices/[ACTION1]/";

        /// <summary>
        /// Registar mortes
        /// url: https://localhost:44331/api/covidServices/[ACTION1]/
        ///      https://localhost:44331/api/covidServices/deaths?v=13/
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void button3_Click(object sender, EventArgs e)
        {
            if(textBox3.Text.Length == 0) 
            {
                MessageBox.Show("Insert the number of deaths!"); return;
            }

            string url = localurl.Replace("[ACTION1]", "deaths");
            //url = url.Replace("[V]", textBox1.Text); //Caso existam mais parâmetros em operações GET

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            //Result type defined to Json
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //Parâmetro em Json
            //Converts object in Json
            int model = int.Parse(textBox3.Text);
            string jsonString = JsonSerializer.Serialize(model);
            var stringContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            //waits result
            HttpResponseMessage response = await client.PostAsync(url, stringContent);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Done!");
            }
        }


        /// <summary>
        /// Regist indicadors (infects, recovers and deaths)
        /// url: https://localhost:44331/api/covidList/[ACTION1]
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void button2_Click(object sender, EventArgs e)
        {
            string url = "https://localhost:44331/api/covidList/[ACTION1]";
            if(textBox1.Text.Length == 0)
            {
                MessageBox.Show("Define value!"); return;
            }

            url = url.Replace("[ACTION1]", "newIndicator");

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            //Define result type: json
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //Parâmetro em JSON

            //var payload = "{\"CustomerId\": 5,\"CustomerName\": \"Pepsi\"}";
            Indicador ind = new Indicador();
            //ind.date = DateTime.Today;
            //ind.date = DateTime.Today.ToString();
            ind.infetados = int.Parse(textBox1.Text);
            ind.mortes = int.Parse(textBox3.Text);
            ind.recuperados = int.Parse(textBox2.Text);

            // Converte objeto para formato Json
            string jsonString = JsonSerializer.Serialize(ind);
            var stringContent = new StringContent(jsonString, Encoding.UTF8, "application/json");   //Header

            // Espera o resultado
            HttpResponseMessage response = await client.PostAsync(url, stringContent);  //Post

            string result = response.Content.ReadAsStringAsync().Result;

            // Verifica se o retorno é 200
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Done!");
            }
        }

        /// <summary>
        /// Localhost POST registo corrente ??
        /// url: https://localhost:44355/api/covidList/saveCurrent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void button1_Click(object sender, EventArgs e)
        {
            string url = "https://localhost:44355/api/covidList/saveCurrent";
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            //Definir tipo de resultado: JSON
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var stringContent = new StringContent("", Encoding.UTF8, "application/json");   //Header

            // Espera o resultado
            HttpResponseMessage response = await client.PostAsync(url, stringContent);  //Post

            string result = response.Content.ReadAsStringAsync().Result;

            // Verifica se o retorno é 200
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Done!");
            }
        }

        #endregion

        #endregion

        #region Cloud - Services

        /// <summary>
        /// Get all regists
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void button4_Click(object sender, EventArgs e)
        {
            string url = "https://localhost:44331/";
            string json = await ShowHistory(url);
            //DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(List<Indicador>),settings);
            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(List<Indicador>));
            var ms = new MemoryStream(Encoding.Unicode.GetBytes(json));
            List<Indicador> h = (List<Indicador>)jsonSerializer.ReadObject(ms);
            dataGridView1.DataSource = h;
        }

        #endregion

        #region Aux

        private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {

        }

        /// <summary>
        /// GET assincrono
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public async Task<string> MyGetAsync(string uri)
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(uri);

            //will throw an exception if not successful
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            return content;
        }

        /// <summary>
        /// GET all
        /// </summary>
        public async Task<string> ShowHistory(string url)
        {
            //h1 - WebClient
            //WebClient client = new WebClient();
            //string json = client.DownloadString(url);

            //h2 - HttpClient
            string json = await MyGetAsync(url);
            return json;

        }

        #endregion

    }

    #region Models

    [DataContractAttribute]
    public class AuthenticateResponse
    {
        [DataMemberAttribute]
        public string name { get; set; }
        [DataMemberAttribute]
        public string token { get; set; }


        public AuthenticateResponse(string user, string token)
        {
            name = user;
            this.token = token;
        }
    }

    // NewtonSoft:
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Indicador
    {
        public int recuperados { get; set; }
        public int mortes { get; set; }
        public int infetados { get; set; }

        public string date { get; set; }
    }

    public class History
    {
        public List<Indicador> indicadores { get; set; }
        //[JsonIgnore]
        public int TotIndicadores { get; set; }      //Property omitida na serialização
    }

    /// <summary>
    /// ???
    /// </summary>
    [DataContractAttribute]
    public class User
    {
        [DataMemberAttribute]
        public string name { get; set; }

        [DataMemberAttribute]
        public int id { get; set; }

        [DataMemberAttribute]
        public string role { get; set; }

        [DataMemberAttribute]
        public object token { get; set; }
    }

    #endregion
}
