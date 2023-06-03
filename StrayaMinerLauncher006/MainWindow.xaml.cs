using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LibreHardwareMonitor.Hardware;
using System.Reflection.Metadata;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Xml.Linq;
using System.Transactions;
using System.Data.Common;


/*  ////////////////////////////////////////////////////////////////////////////////////////////////////////
*  GoTo: search for {-n-} where n is the number
*  1 :{-1-}
*  2 :{-2-}
*  3 :{-3-}
*  4 :{-4-}
*  5 :{-5-}
*  ///////////////////////////////////////////////////////////////////////////////////////////////////////*/

namespace StrayaMinerLauncher006
{
    public class Ticker
    {
        public string? At { get; set; }
        public double AvgPrice { get; set; }
        public double High { get; set; }
        public double Last { get; set; }
        public double Low { get; set; }
        public double Open { get; set; }
        public string? PriceChangePercent { get; set; }
        public double Volume { get; set; }
        public double Amount { get; set; }
    }

    public class Transaction
    {
        public string txid { get; set; }
        public decimal amount { get; set; }
        public string category { get; set; }
        public string confirmations { get; set; }
    }

    public partial class MainWindow : Window
    {
        public bool SettingsDirty = false;
        string pathToWallet = Properties.Settings.Default.PathToWallet;
        string pathToCLI = Properties.Settings.Default.PathToCLI;
        string pathToMiner = Properties.Settings.Default.PathToMiner;

        public MainWindow()
        {
            InitializeComponent();
            frameMain.NavigationService.Navigate(new Uri("Pages\\Miner.xaml", UriKind.Relative));
            
            frameSettings.Visibility = Visibility.Hidden;
        }

        public static string Deserialize_json(string json)
        {
            string? Result = JsonConvert.DeserializeObject<string>(json);
            return Result;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnCloseApplication_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnStrayaMiner_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (frameSettings.Visibility == Visibility.Hidden)
            {
                frameSettings.Visibility = Visibility.Visible;
                frameSettings.NavigationService.Navigate(new Uri("Pages\\Settings.xaml", UriKind.Relative));
            }
        }

        private void btnHelpInformation_Click(object sender, RoutedEventArgs e)
        {
            if (scrollViewInfoHelp.Visibility == Visibility.Hidden)
            {
                scrollViewInfoHelp.Visibility = Visibility.Visible;
                frameInfoHelp.NavigationService.Navigate(new Uri("Pages\\InformationHelp.xaml", UriKind.Relative));
            }else
            {
                scrollViewInfoHelp.Visibility = Visibility.Hidden;
                frameInfoHelp.NavigationService.Navigate(null);
            }
        }

        public void SaveAndCloseSettings()
        {
            Properties.Settings.Default.Save();
            frameSettings.Visibility = Visibility.Hidden;
            frameSettings.Content = null;
        }

        public void CloseSettings()
        {
            if (frameSettings.Visibility == Visibility.Visible && SettingsDirty == true)
            {
                MessageBoxResult result = MessageBox.Show("Do you want to save the settings?", "Wait", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        // User clicked Yes button
                        //Properties.Settings.Default.Save();
                        break;
                    case MessageBoxResult.No:
                        // User clicked No button
                        frameSettings.Content = null;
                        frameSettings.Visibility = Visibility.Hidden;
                        break;
                    case MessageBoxResult.Cancel:
                        // User clicked Cancel button, do nothing
                        // write a function to revert changes
                        break;
                }
            }
            else if (frameSettings.Visibility == Visibility.Visible && SettingsDirty == false)
            {
                frameSettings.Content = null;
                frameSettings.Visibility = Visibility.Hidden;
            }
            if (SettingsDirty == false)
            {
                frameSettings.Content = null;
                frameSettings.Visibility = Visibility.Hidden;
            }
        }
        /*  ////////////////////////////////////////////////////////////////////////////////////////////////////////
        *  {-1-n-} - Output to User
        *  ///////////////////////////////////////////////////////////////////////////////////////////////////////*/

      


        /*  ////////////////////////////////////////////////////////////////////////////////////////////////////////
         *  {-2-n-} - Strayacoin Miner
         *  ///////////////////////////////////////////////////////////////////////////////////////////////////////*/

        private void function1()
        {

        }

        /*  ////////////////////////////////////////////////////////////////////////////////////////////////////////
         *  {-3-n-} - Device Monitor
         *  ///////////////////////////////////////////////////////////////////////////////////////////////////////*/

        public List<ISensor> zsens = new();
        // Function for getting CPU temperatures and loads for Miner Core #1
        public Tuple<float?, float?, float?, float?> GetDeviceInfo()
        {
            float? CpuTempMax1 = 0.0f;
            
            float? CpuTemp1 = 0.0f;
            
            float? CpuC1T1_Load1 = 0.0f;
            float? CpuC1T2_Load1 = 0.0f;
            
            Computer computer1 = new Computer();
            computer1.Open();
            computer1.IsCpuEnabled = true;

            foreach (IHardware hardwareItem in computer1.Hardware)
            {
                if (hardwareItem.HardwareType == HardwareType.Cpu)
                {
                    hardwareItem.Update();

                    foreach (ISensor sensor in hardwareItem.Sensors)
                    {
                        
                        if(sensor != null)
                        {
                            zsens.Add(sensor);
                        }
                    }
                }
            }
            computer1.Close();
            return Tuple.Create(CpuTemp1, CpuTempMax1, CpuC1T1_Load1, CpuC1T2_Load1);
        }



        /*  ////////////////////////////////////////////////////////////////////////////////////////////////////////
         *  {-4-n-} - Wallet , Coin , User Info
         *  ///////////////////////////////////////////////////////////////////////////////////////////////////////*/


        public async Task<string> RunCLI(string argument)
        {
            // check if the pathToCLI setting exists
            if (string.IsNullOrEmpty(Properties.Settings.Default.PathToCLI))
            {
                return "PathToCLI setting is missing or empty.";
            }

            // check if the executable file exists at the specified path
            if (!File.Exists(Properties.Settings.Default.PathToCLI))
            {
                return $"Cannot find StrayaCLI executable at {Properties.Settings.Default.PathToCLI}.";
            }

            if (Properties.Settings.Default.PathToCLI.ToString().StartsWith("* * *"))
            {
                return "Path To CLI setting Is Wrong.";
            }

            Process StrayaCLI = new Process();
            ProcessStartInfo StartInfo = new ProcessStartInfo();
            StrayaCLI.StartInfo = StartInfo;
            StartInfo.FileName = Properties.Settings.Default.PathToCLI;
            StartInfo.WorkingDirectory = @System.IO.Path.GetDirectoryName(Properties.Settings.Default.PathToCLI);
            StartInfo.Arguments = argument;
            StartInfo.CreateNoWindow = true;
            StartInfo.UseShellExecute = false;
            StartInfo.RedirectStandardError = true;
            StartInfo.RedirectStandardOutput = true;

            StrayaCLI.Start();

            StrayaCLI.WaitForExit();



            if (argument == "getwalletinfo")
            {
                // define a string and store the JSON string that is returned by the process after it finishes reading it
                string output = await StrayaCLI.StandardOutput.ReadToEndAsync();

                // Parse the JSON output to a JSON object that we can use to extract the key/value pairs
                JObject jsonObject = JObject.Parse(output);

                //return the output as its own string to satisfy the function and so we can re-use it if needed
                string output_getwalletinfo = output;
                return output_getwalletinfo;
            }
            else if (argument == "getblockchaininfo")
            {
                // define a string and store the JSON string that is returned by the StrayaCLIess after it finishes reading it
                string output = await StrayaCLI.StandardOutput.ReadToEndAsync();

                // Parse the JSON output to a JSON object that we can use to extra the key/value pairs
                JObject jsonObject = JObject.Parse(output);

                //return the output as its own string to satisfy the function and so we can re-use it if needed
                string output_getblockchaininfo = output;
                return output_getblockchaininfo;
            }
            else if (argument == "getmininginfo")
            {
                // define a string and store the JSON string that is returned by the StrayaCLIess after it finishes reading it
                string output = await StrayaCLI.StandardOutput.ReadToEndAsync();

                // Parse the JSON output to a JSON object that we can use to extra the key/value pairs
                JObject jsonObject = JObject.Parse(output);

                //return the output as its own string to satisfy the function and so we can re-use it if needed
                string output_getmininginfo = output;
                return output_getmininginfo;
            }
            else if (argument == "getpeerinfo")
            {
                // define a string and store the JSON string that is returned by the StrayaCLIess after it finishes reading it
                string output = await StrayaCLI.StandardOutput.ReadToEndAsync();

                JArray jsonArray = JArray.Parse(output);
                string output_peerinfo = output;
                return output_peerinfo;
            }
            else if (argument == "listtransactions \"*\" 1")
            {
                string output = await StrayaCLI.StandardOutput.ReadToEndAsync();

                
                string output_listtransactions = output;
                return output_listtransactions;
            }
                return "Done";
        }

        public Tuple<string?, string?, decimal?, string?> Deserialize_Output_ListTransactions(string input_JSON)
        {
            string? txcategory = "";
            string? txid = "";
            decimal? txamount = 0;
            string? txconfirmations = "";
            // Read the output and deserialize it into a list of transactions
            Transaction[] transactions = JsonConvert.DeserializeObject<Transaction[]>(input_JSON);

            // Process the transactions
            foreach (Transaction transaction in transactions)
            {
                txid = transaction.txid;
                txamount = transaction.amount;
                txcategory = transaction.category;

                // Process the transaction data as per your requirements
                // You can filter, count, or perform any other operations here
                // based on the specific properties of the transactions
                //Console.WriteLine($"Transaction ID: {txid}");
                //Console.WriteLine($"Amount: {txamount}");
                //Console.WriteLine($"Category: {txcategory}");
                //Console.WriteLine("-----------------------");
            }

            return Tuple.Create(txcategory, txid, txamount, txconfirmations);
        }

        public Tuple<int?, double?, double?, string> Deserialize_Output_MiningInfo(string input_JSON)
        {
            // Deserialize the Input_JSON string and extract the Key/Value pairs we want to work with
            JObject Output_JSON = JObject.Parse(input_JSON);

            // get blocks and return it as a nullable int called blocks
            int? blocks = (int?)Output_JSON["blocks"] as int?;

            // get difficulty and return it as a nullable double called difficulty
            double? difficulty = (double?)Output_JSON["difficulty"] as double?;

            // get networkhashps and return it as a nullable double called networkhashps
            double? networkhashps = (double?)Output_JSON["networkhashps"] as double?;

            // get chain and return it as a nullable string called chain
            string? chain = (string?)Output_JSON["chain"] as string;

            // as a safety net we will use the null-coalescing operator (??) to provide a default value to the above variables if the result is null
            // blocks default
            blocks ??= 0;

            // difficulty default
            difficulty ??= 0.0;

            // networkhashps default
            networkhashps ??= 0.0;

            // chain default
            chain ??= "Unknown";

            // create and return a Tuple object to return multiple data-types
            return Tuple.Create(blocks, difficulty, networkhashps, chain);
        }

        public Tuple<double?, double?, double?, double?, int?> Deserialize_Output_WalletInfo(string input_JSON)
        {
            // Deserialize the Input_JSON string and extract the Key/Value pairs we want to work with
            JObject Output_JSON = JObject.Parse(input_JSON);


            // get wallet balance and return it as a nullable double called WalletBalance
            double? ConfirmedWalletBalance = (double?)Output_JSON["balance"] as double?;

            // get wallet unconfirmed balance and return it as a nullable double called UnconfirmedWalletBalance
            double? UnconfirmedWalletBalance = (double?)Output_JSON["unconfirmed_balance"] as double?;

            // get wallet immature balance and return it as a nullable double called ImmatureWalletBalance
            double? ImmatureWalletBalance = (double?)Output_JSON["immature_balance"] as double?;

            // get transaftion fee and return it as a nullable double called TransaftionFee
            double? TransactionFee = (double?)Output_JSON["immature_balance"] as double?;

            // get transaftion Count and return it as a nullable int called TxCount
            int? TxCount = (int?)Output_JSON["txcount"] as int?;

            // as a safety net we will use the null-coalescing operator (??) to provide a default value to the above variables if the result is null
            // Confirmed Balance default
            ConfirmedWalletBalance ??= 0.0;

            // Uncofirmed Balance default
            UnconfirmedWalletBalance ??= 0.0;

            // Immature Balance default
            ImmatureWalletBalance ??= 0.0;

            // Transaction Fee default
            TransactionFee ??= 0.0;

            // Transaction Count default
            TransactionFee ??= 0.0;

            // create and return a Tuple object to return multiple data-types
            return Tuple.Create(ConfirmedWalletBalance, UnconfirmedWalletBalance, ImmatureWalletBalance, TransactionFee, TxCount);
        }

        public Tuple<int, int> Deserialize_Output_PeerInfo(string input_JSON)
        {
            // if the peer returns true it is an inbound connection - "inbound": true|false,     (boolean) Inbound (true) or Outbound (false)
            // if the peer returns false it is an outbound connection

            // extract the array of peers
            JArray peers = JArray.Parse(input_JSON);

            // define variables to return number of connections
            int ActiveConnectionsIn = 0;
            int ActiveConnectionsOut = 0;

            // run a for loop on the peers array to check for tru or false return value
            foreach (JObject peer in peers)
            {
                // check each peer in the peers array to get the "inbound" value of each
                bool? isInbound = peer.GetValue("inbound").Value<bool?>();
                if ((bool)isInbound)
                {
                    // add one to ActiveConnectionsIn the current peer returns true
                    ActiveConnectionsIn++;
                }
                else if ((bool)!isInbound)
                {
                    // add one to ActiveConnectionsOut the current peer returns false
                    ActiveConnectionsOut++;
                }
            }
            // return the in and out connection via the variables we created
            return Tuple.Create(ActiveConnectionsIn, ActiveConnectionsOut);
        }
        /*  ////////////////////////////////////////////////////////////////////////////////////////////////////////
        *  {-5-n-} - Markets and Exchanges
        *  ///////////////////////////////////////////////////////////////////////////////////////////////////////*/
        string Result_tickers = "";
        public Ticker ticka = new();
        public async Task<string> GetCoinData()
        {
            var client = new HttpClient();
            var response = await client.GetAsync("https://v2.altmarkets.io/api/v2/peatio/public/markets/tickers");
            var json = await response.Content.ReadAsStringAsync();
            string result = Deserialize_json(json);
            return result;
        }

        public async Task<Ticker> OLD_GetCoinData()
        {
            // Create a HttpClient object.
            HttpClient client = new HttpClient();

            // Set the Content-Type header to application/json.
            client.DefaultRequestHeaders.Add("Content-Type", "application/json");

            // Send a GET request to the API endpoint.
            HttpResponseMessage response = await client.GetAsync("https://v2.altmarkets.io/api/v2/peatio/public/markets/tickers");

            // Check the response status code.
            if (response.StatusCode == HttpStatusCode.OK)
            {
                // The request was successful.
                string jsonString = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON string into a C# object.
                Ticker ticker = JsonConvert.DeserializeObject<Ticker>(jsonString);

                // Return the C# object.
                return ticker;
            }
            else
            {
                // The request failed.
                // Handle the error here.
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
            }
        }


    }
}
