using LibreHardwareMonitor.Hardware;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Path = System.IO.Path;

namespace StrayaMinerLauncher006.Pages
{
    /// <summary>
    /// Interaction logic for Miner.xaml
    /// </summary>
    public partial class Miner : Page
    {
        // reference to the main window so we can run the commands on this page
        MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
        string networkName = "";
        string TransactionFee = "";
        string NahYeahDunno = Properties.Settings.Default.PreferedTicker;

        public double currentImmatureBalance = 0;

        public int blockCountTotal = Properties.Settings.Default.UserBlocksMinedTotal;

        public int NumCpuCores = 0;
        public int NumCpuThreads = 0;

        public float? CPU_Package_Max_Temp = 0;

        public double? CPU_Max_Temp1 = 0;
        public double? CPU_Max_Temp2 = 0;
        public double? CPU_Max_Temp3 = 0;
        public double? CPU_Max_Temp4 = 0;

        public double? CPU_Max_Load_Total = 0;

        public double? CPU1_Max_Load1 = 0;
        public double? CPU1_Max_Load2 = 0;
        public double? CPU2_Max_Load1 = 0;
        public double? CPU2_Max_Load2 = 0;
        public double? CPU3_Max_Load1 = 0;
        public double? CPU3_Max_Load2 = 0;
        public double? CPU4_Max_Load1 = 0;
        public double? CPU4_Max_Load2 = 0;

        public DispatcherTimer UpdateTimer1 = new DispatcherTimer();
        public DispatcherTimer UpdateTimerPayout = new DispatcherTimer();

        public Miner()
        {
            InitializeComponent();

            // updateUItimer 1 
            UpdateTimer1.Interval = TimeSpan.FromSeconds(Properties.Settings.Default.DeviceMonitorTime);
            UpdateTimer1.Tick += UpdateTimer1_Tick;
            UpdateTimer1.Start();

            // updateTimerPayout
            UpdateTimerPayout.Interval = TimeSpan.FromSeconds(60);
            UpdateTimerPayout.Tick += UpdateTimerPayout_Tick;


            //RunCLIAsync("getmininginfo");
            // update the info at the top of the application
            var taskMiningInfo = Task.Run(() => OutputToUser_miningInfo());
            Task resultMining = taskMiningInfo;

            var taskPeerInfo = Task.Run(() => OutputToUser_PeerInfo());
            Task resultPeer = taskPeerInfo;

            var taskWalletInfo = Task.Run(() => OutputToUser_WalletInfo());
            Task resultWallet = taskWalletInfo;

            var taskDeviceInfo = Task.Run(() => OutputToUser_DeviceInfo(false));
            Task resultDevMon = taskDeviceInfo;

            //var taskListTX = Task.Run(() => OutputToUser_ListTransactions());
            //Task resultListTX = taskListTX;

            //string x = mainWindow.GetCoinData().Result.ToString();
            //rtb1.AppendText(x);
        }

        private void UpdateTimerPayout_Tick(object? sender, EventArgs e)
        {
            var taskWalletInfoPayout = Task.Run(() => OutputToUser_WalletInfo());
            Task resultWalletPayout = taskWalletInfoPayout;
        }

        private void UpdateTimer1_Tick(object? sender, EventArgs e)
        {
            var taskDeviceInfo = Task.Run(() => OutputToUser_DeviceInfo(false));
            Task resultDevMon = taskDeviceInfo;
        }

        private async Task<string> RunCLIAsync(string argument)
        {
            var getinfo = await mainWindow.RunCLI(argument);
            return getinfo;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            UpdateTimer1.Stop();
        }

        private string GetTimeIn12HourFormat()
        {
            // Get the current time
            DateTime dateTime = DateTime.Now;

            // Format the time in 12 hour format
            string timeIn12HourFormat = dateTime.ToString("hh:mm tt");

            // Return the time in 12 hour format
            return timeIn12HourFormat;
        }

        /*  ////////////////////////////////////////////////////////////////////////////////////////////////////////
        *  {-1-n-} - Output to User
        *  ///////////////////////////////////////////////////////////////////////////////////////////////////////*/

        private async void OutputToUser_miningInfo()
        {

            // assign a variable to store the result of the CLI call
            string miningInfo = RunCLIAsync("getmininginfo").Result;
            // deserialize the output from RunStrayacoinCli
            Tuple<int?, double?, double?, string> deserializedMiningInfo = mainWindow.Deserialize_Output_MiningInfo(miningInfo);

            // use a dispatcher to update the UI on a UI thread for performance
            Dispatcher.Invoke(new Action(() =>
            {
                // if blocks is not null and greater thn 0 set label green
                if (deserializedMiningInfo.Item1 != null && deserializedMiningInfo.Item1 > 0)
                {
                    int networkblocks = deserializedMiningInfo.Item1.Value; // change deserializedMiningInfo.Item1 to an int variable
                    string formattednetworkblocks = networkblocks.ToString("N0"); // format the int with commas
                    lblNetworkBlocksMined.Content = formattednetworkblocks; // set the label content to the formatted string
                    lblNetworkBlocksMined.Foreground = Brushes.Green;

                    int userblocks = Properties.Settings.Default.UserBlocksMinedTotal;
                    string formatteduserblocks = userblocks.ToString("N0");
                    lblUserBlocksMinedTotal.Content = Properties.Settings.Default.UserBlocksMinedTotal;
                    lblUserBlocksMinedTotal.Foreground = Brushes.Green;
                }
                else
                {
                    lblNetworkBlocksMined.Content = 0;
                    lblNetworkBlocksMined.Foreground = Brushes.Red;
                    lblUserBlocksMinedTotal.Content = 0;
                    lblUserBlocksMinedTotal.Foreground = Brushes.Red;
                }
                // if difficulty is not null and greater thn 0 set label green
                if (deserializedMiningInfo.Item2 != null && deserializedMiningInfo.Item2 > 0.000000)
                {
                    //lblNetworkDifficultyResult.Content = deserializedMiningInfo.Item2;
                    lblNetworkDifficulty.Content = string.Format("{0:F6}", deserializedMiningInfo.Item2);
                    lblNetworkDifficulty.Foreground = Brushes.Green;
                }
                else
                {
                    lblNetworkDifficulty.Content = 0;
                    lblNetworkDifficulty.Foreground = Brushes.Red;
                }

                if (deserializedMiningInfo.Item3 != null && deserializedMiningInfo.Item3 > 0.000000)
                {
                    // divide the hashrate so its readable by the user ()
                    var shortHashrate = deserializedMiningInfo.Item3 / 1000;

                    //assign the data do the labels and set the color
                    lblNetworkHashrate.Content = string.Format("{0:F2}", shortHashrate) + " KH/s";
                    lblNetworkHashrate.Foreground = Brushes.Green;
                }
                else
                {
                    lblNetworkHashrate.Content = 0;
                    lblNetworkHashrate.Foreground = Brushes.Red;
                }

                // set the name and online status of the network at once since for the network to return a name it must be online
                if (deserializedMiningInfo.Item4 != null)
                {
                    if (deserializedMiningInfo.Item4 == "main")
                    {
                        networkName = "Main";

                        lblNetworkStatus.Content = "Online";
                        lblNetworkStatus.Foreground = Brushes.Green;
                    }
                    else if (deserializedMiningInfo.Item4 == "test")
                    {
                        networkName = "Test";

                        lblNetworkStatus.Content = "Online";
                        lblNetworkStatus.Foreground = Brushes.Green;
                    }
                    else if (deserializedMiningInfo.Item4 == "regtest")
                    {
                        networkName = "RegTest";

                        lblNetworkStatus.Content = "Online";
                        lblNetworkStatus.Foreground = Brushes.Green;
                    }

                }
                else
                {
                    networkName = "Unknown";

                    lblNetworkStatus.Content = "Offline";
                    lblNetworkStatus.Foreground = Brushes.Red;
                }
            }));
        }

        private void OutputToUser_PeerInfo()
        {

            // assign a variable to store the result of the CLI call
            string peerinfo = RunCLIAsync("getpeerinfo").Result;
            // deserialize the output from RunStrayacoinCli
            Tuple<int, int> deserializedPeerInfo = mainWindow.Deserialize_Output_PeerInfo(peerinfo);

            int? InboundConnections = deserializedPeerInfo.Item1;
            int? OutboundConnections = deserializedPeerInfo.Item2;


            Dispatcher.Invoke(new Action(() =>
            {
                if (InboundConnections != null)
                {
                    lblConnectedClientsIn.Foreground = Brushes.Green;
                    lblConnectedClientsIn.Content = InboundConnections;
                }
                else
                {
                    lblConnectedClientsIn.Foreground = Brushes.Red;
                }

                if (OutboundConnections != null)
                {
                    lblConnectedClientsOut.Foreground = Brushes.Green;
                    lblConnectedClientsOut.Content = OutboundConnections;
                }
                else
                {
                    lblConnectedClientsOut.Foreground = Brushes.Red;
                }
            }));
        }

        private void OutputToUser_WalletInfo()
        {

            // assign a variable to store the result of the CLI call
            string WalletInfo = RunCLIAsync("getwalletinfo").Result;
            // deserialize the output from RunStrayacoinCli
            Tuple<double?, double?, double?, double?, int?> deserializedWalletInfo = mainWindow.Deserialize_Output_WalletInfo(WalletInfo);
            //TxCount = deserializedWalletInfo.Item5;
            Dispatcher.Invoke(new Action(() =>
            {
                // wallet balance
                if (deserializedWalletInfo.Item1 != null && deserializedWalletInfo.Item1.HasValue)
                {
                    if (deserializedWalletInfo.Item1 > 1000)
                    {
                        double number = deserializedWalletInfo.Item1.Value; // get the value of deserializedWalletInfo.Item1
                        string formatted = number.ToString($"N0" + $" {NahYeahDunno}"); // format the double with commas
                        lblWalletBalance.Content = formatted; // set the label content to the formatted string
                        lblWalletBalance.Foreground = Brushes.Green;
                    }
                    else
                    {
                        lblWalletBalance.Content = deserializedWalletInfo.Item1 + $" {NahYeahDunno}";
                        lblWalletBalance.Foreground = Brushes.Green;
                    }
                }
                else
                {
                    lblWalletBalance.Content = $"0 {NahYeahDunno}";
                    lblWalletBalance.Foreground = Brushes.Red;
                }

                // unconfirmed balance
                if (deserializedWalletInfo.Item2 != null)
                {
                    lblWalletUnconfirmed.Content = deserializedWalletInfo.Item2 + $" {NahYeahDunno}";
                    lblWalletUnconfirmed.Foreground = Brushes.Green;

                }
                else
                {
                    lblWalletUnconfirmed.Content = $"0 {NahYeahDunno}";
                    lblWalletUnconfirmed.Foreground = Brushes.Red;
                }

                // immature balance
                if (deserializedWalletInfo.Item3 != null)
                {
                    lblWalletImmature.Content = deserializedWalletInfo.Item3 + $" {NahYeahDunno}";
                    lblWalletImmature.Foreground = Brushes.Green;

                }
                else
                {
                    lblWalletImmature.Content = $"0 {NahYeahDunno}";
                    lblWalletImmature.Foreground = Brushes.Red;
                }

                // transaction fee
                if (deserializedWalletInfo.Item4 != null)
                {
                    // this is turned off at the moment
                    TransactionFee = deserializedWalletInfo.Item5 + $" {NahYeahDunno}";
                    TransactionFee = $"0 {NahYeahDunno}";
                    lblTransactionCount.Foreground = Brushes.Red;

                }
                else
                {
                    TransactionFee = $"0 {NahYeahDunno}";
                    lblTransactionCount.Foreground = Brushes.Red;
                }

                // transaction count
                if (deserializedWalletInfo.Item5 != null)
                {
                    lblTransactionCount.Content = deserializedWalletInfo.Item5 + " Transactions";
                    lblTransactionCount.Foreground = Brushes.Green;

                }
                else
                {
                    lblTransactionCount.Content = "0 Transactions";
                    lblTransactionCount.Foreground = Brushes.Red;
                }

                // if blocks is not null and greater thn 0 set label green
                //tBlockWalletInfoOutput.Text =
                //  " _________________________________________________\n"
                //+ "| Total Wallet Balance: " + deserializedWalletInfo.Item1 + " {NahYeahDunno}" + "\n"
                //+ " _________________________________________________\n"
                //+ "| Unconfirmed Balance: " + deserializedWalletInfo.Item2 + " {NahYeahDunno}" + "\n"
                //+ " _________________________________________________\n"
                //+ "| Immature Balance: " + deserializedWalletInfo.Item3 + " {NahYeahDunno}" + "\n"
                //+ " _________________________________________________\n"
                //+ "| Transaction Fee: " + deserializedWalletInfo.Item4 + " {NahYeahDunno} per KB" + "\n"
                //+ " _________________________________________________\n"
                //+ "| Number of Transactions: " + deserializedWalletInfo.Item5;
            }));
        }

        private void OutputToUser_ListTransactions()
        {

            // assign a variable to store the result of the CLI call
            string txList = RunCLIAsync("listtransations \"*\" 1").Result;
            // deserialize the output from RunStrayacoinCli
            Tuple<string?, string?, decimal?, string?> deserializedtxList = mainWindow.Deserialize_Output_ListTransactions(txList);
            // tuple = category, txid, ,payment amount, confirmations 
            string? txtype = deserializedtxList.Item1;
            

            Dispatcher.Invoke(new Action(() =>
            {
                if (deserializedtxList.Item1 != null)
                {
                    if (txtype == "generate")
                    {
                        // set the payment type to mining payout if the category is generate with green text
                        lblLastPaymentType.Content = "Mining Payout";
                        lblLastPaymentType.Foreground = Brushes.Green;
                    }
                    else if (txtype == "receive")
                    {
                        // set the payment type to Received Payment if the category is receive with green text
                        lblLastPaymentType.Content = "Received Payment";
                        lblLastPaymentType.Foreground = Brushes.Green;
                    }
                    else 
                    {
                        //otherwise set the payment type to unknown for a red text
                        lblLastPaymentType.Content = "Unknown";
                        lblLastPaymentType.Foreground = Brushes.Red;
                    }
                }

                if(deserializedtxList.Item3 != null)
                {
                    // set the payment amount to the amount received and set the text to green
                    lblLastPaymentAmount.Content = deserializedtxList.Item3;
                    lblLastPaymentAmount.Foreground = Brushes.Green;
                }
                else
                {
                    // set the payment amount to 0 and set the text to red
                    lblLastPaymentAmount.Content = "0";
                    lblLastPaymentAmount.Foreground = Brushes.Red;
                }
                    //string  deserializedtxList.

            }));
        }

        private void OutputToUser_DeviceInfo(bool coresandthreads)
        {
            Tuple<float?, float?, float?, float?> DeviceInfo = mainWindow.GetDeviceInfo();
            if (CPU_Max_Temp1 < DeviceInfo.Item1)
            {
                CPU_Max_Temp1 = DeviceInfo.Item1;
            }
            Dispatcher.Invoke(new Action(() =>
            {
                int threads = Environment.ProcessorCount;
                int cores = threads / 2;
                lblCpuCoresNum.Content = cores.ToString();
                lblCpuThreadsNum.Content = threads.ToString();
                foreach (ISensor sensor in mainWindow.zsens)
                {
                    // get the package temp to display
                    if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("Package")) { if (sensor.Value < 80) { lblTempPackage.Foreground = Brushes.Green; } else if (sensor.Value >= 81 && sensor.Value < 90) { lblTempPackage.Foreground = Brushes.Yellow; }; if (CPU_Package_Max_Temp < sensor.Value) { CPU_Package_Max_Temp = sensor.Value; } lblTempPackage.Content = sensor.Value + "  Current  |  Max  " + CPU_Package_Max_Temp; }
                    // get the core temperatures - will be updated to use the cores selected for mining
                    //if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("CPU Core #1") && !sensor.Name.Contains("Thread") && !sensor.Name.Contains("TjMax")) { if (sensor.Value < 70) { lblTempCore1.Foreground = Brushes.Green; } else if (sensor.Value > 70 && sensor.Value < 90) { lblTempCore1.Foreground = Brushes.Yellow; }; lblTempCore1.Content = sensor.Value; if (CPU_Max_Temp1 < sensor.Value) { CPU_Max_Temp1 = sensor.Value; } NumCpuCores++; }
                    //if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("CPU Core #2") && !sensor.Name.Contains("Thread") && !sensor.Name.Contains("TjMax")) { if (sensor.Value < 70) { lblTempCore2.Foreground = Brushes.Green; } else if (sensor.Value > 70 && sensor.Value < 90) { lblTempCore2.Foreground = Brushes.Yellow; }; lblTempCore2.Content = sensor.Value; if (CPU_Max_Temp2 < sensor.Value) { CPU_Max_Temp2 = sensor.Value; } NumCpuCores++; }
                    //if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("CPU Core #3") && !sensor.Name.Contains("Thread") && !sensor.Name.Contains("TjMax")) { if (sensor.Value < 70) { lblTempCore3.Foreground = Brushes.Green; } else if (sensor.Value > 70 && sensor.Value < 90) { lblTempCore3.Foreground = Brushes.Yellow; }; lblTempCore3.Content = sensor.Value; if (CPU_Max_Temp3 < sensor.Value) { CPU_Max_Temp3 = sensor.Value; } NumCpuCores++; }
                    //if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("CPU Core #4") && !sensor.Name.Contains("Thread") && !sensor.Name.Contains("TjMax")) { if (sensor.Value < 70) { lblTempCore4.Foreground = Brushes.Green; } else if (sensor.Value > 70 && sensor.Value < 90) { lblTempCore4.Foreground = Brushes.Yellow; }; lblTempCore4.Content = sensor.Value; if (CPU_Max_Temp4 < sensor.Value) { CPU_Max_Temp4 = sensor.Value; } NumCpuCores++; }
                    if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("CPU Core #1") && !sensor.Name.Contains("Thread") && !sensor.Name.Contains("TjMax")) { if (sensor.Value < 80) { lblTempCore1.Foreground = Brushes.Green; } else if (sensor.Value >= 81 && sensor.Value < 90) { lblTempCore1.Foreground = Brushes.Yellow; }; if (CPU_Max_Temp1 < sensor.Value) { CPU_Max_Temp1 = sensor.Value; } lblTempCore1.Content = sensor.Value + "  Current  |  Max  " + CPU_Max_Temp1; NumCpuCores++; }
                    if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("CPU Core #2") && !sensor.Name.Contains("Thread") && !sensor.Name.Contains("TjMax")) { if (sensor.Value < 80) { lblTempCore2.Foreground = Brushes.Green; } else if (sensor.Value >= 81 && sensor.Value < 90) { lblTempCore2.Foreground = Brushes.Yellow; }; if (CPU_Max_Temp2 < sensor.Value) { CPU_Max_Temp2 = sensor.Value; } lblTempCore2.Content = sensor.Value + "  Current  |  Max  " + CPU_Max_Temp1; NumCpuCores++; }
                    if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("CPU Core #3") && !sensor.Name.Contains("Thread") && !sensor.Name.Contains("TjMax")) { if (sensor.Value < 80) { lblTempCore3.Foreground = Brushes.Green; } else if (sensor.Value >= 81 && sensor.Value < 90) { lblTempCore3.Foreground = Brushes.Yellow; }; if (CPU_Max_Temp3 < sensor.Value) { CPU_Max_Temp3 = sensor.Value; } lblTempCore3.Content = sensor.Value + "  Current  |  Max  " + CPU_Max_Temp1; NumCpuCores++; }
                    if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("CPU Core #4") && !sensor.Name.Contains("Thread") && !sensor.Name.Contains("TjMax")) { if (sensor.Value < 80) { lblTempCore4.Foreground = Brushes.Green; } else if (sensor.Value >= 81 && sensor.Value < 90) { lblTempCore4.Foreground = Brushes.Yellow; }; if (CPU_Max_Temp4 < sensor.Value) { CPU_Max_Temp4 = sensor.Value; } lblTempCore4.Content = sensor.Value + "  Current  |  Max  " + CPU_Max_Temp1; NumCpuCores++; }


                    //if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Core #1 Thread #1")) { if (CPU1_Max_Load1 < sensor.Value) { CPU1_Max_Load1 = sensor.Value; } lblLoadC1T1.Content = sensor.Value + "  Current  |  Max  " + CPU1_Max_Load1; }
                    //if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Core #1 Thread #2")) { if (CPU1_Max_Load2 < sensor.Value) { CPU1_Max_Load2 = sensor.Value; } lblLoadC1T2.Content = sensor.Value + "  Current  |  Max  " + CPU1_Max_Load2; }
                    //if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Core #2 Thread #1")) { if (CPU2_Max_Load1 < sensor.Value) { CPU2_Max_Load1 = sensor.Value; } lblLoadC1T1.Content = sensor.Value + "  Current  |  Max  " + CPU2_Max_Load1; }
                    //if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Core #2 Thread #2")) { if (CPU2_Max_Load2 < sensor.Value) { CPU2_Max_Load2 = sensor.Value; } lblLoadC1T2.Content = sensor.Value + "  Current  |  Max  " + CPU2_Max_Load2; }
                    //if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Core #3 Thread #1")) { if (CPU3_Max_Load1 < sensor.Value) { CPU3_Max_Load1 = sensor.Value; } lblLoadC1T1.Content = sensor.Value + "  Current  |  Max  " + CPU3_Max_Load1; }
                    //if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Core #3 Thread #2")) { if (CPU3_Max_Load2 < sensor.Value) { CPU3_Max_Load2 = sensor.Value; } lblLoadC1T2.Content = sensor.Value + "  Current  |  Max  " + CPU3_Max_Load2; }
                    //if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Core #4 Thread #1")) { if (CPU4_Max_Load1 < sensor.Value) { CPU4_Max_Load1 = sensor.Value; } lblLoadC1T1.Content = sensor.Value + "  Current  |  Max  " + CPU4_Max_Load1; }
                    //if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Core #4 Thread #2")) { if (CPU4_Max_Load2 < sensor.Value) { CPU4_Max_Load2 = sensor.Value; } lblLoadC1T2.Content = sensor.Value + "  Current  |  Max  " + CPU4_Max_Load2; }

                    if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("CPU Total")) { if (CPU_Max_Load_Total < sensor.Value) { CPU_Max_Load_Total = sensor.Value; } lblLoadTotal.Content = sensor.Value; }

                    if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Core #1 Thread #1")) { if (CPU1_Max_Load1 < sensor.Value) { CPU1_Max_Load1 = sensor.Value; } lblLoadC1T1.Content = sensor.Value; }
                    if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Core #1 Thread #2")) { if (CPU1_Max_Load2 < sensor.Value) { CPU1_Max_Load2 = sensor.Value; } lblLoadC1T2.Content = sensor.Value; }
                    if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Core #2 Thread #1")) { if (CPU2_Max_Load1 < sensor.Value) { CPU2_Max_Load1 = sensor.Value; } lblLoadC2T1.Content = sensor.Value; }
                    if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Core #2 Thread #2")) { if (CPU2_Max_Load2 < sensor.Value) { CPU2_Max_Load2 = sensor.Value; } lblLoadC2T2.Content = sensor.Value; }
                    if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Core #3 Thread #1")) { if (CPU3_Max_Load1 < sensor.Value) { CPU3_Max_Load1 = sensor.Value; } lblLoadC3T1.Content = sensor.Value; }
                    if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Core #3 Thread #2")) { if (CPU3_Max_Load2 < sensor.Value) { CPU3_Max_Load2 = sensor.Value; } lblLoadC3T2.Content = sensor.Value; }
                    if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Core #4 Thread #1")) { if (CPU4_Max_Load1 < sensor.Value) { CPU4_Max_Load1 = sensor.Value; } lblLoadC4T1.Content = sensor.Value; }
                    if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Core #4 Thread #2")) { if (CPU4_Max_Load2 < sensor.Value) { CPU4_Max_Load2 = sensor.Value; } lblLoadC4T2.Content = sensor.Value; }


                    if (sensor.SensorType == SensorType.Clock && sensor.Name.Contains("CPU Core #1")) { lblClockCore1.Content = sensor.Value; }
                    if (sensor.SensorType == SensorType.Clock && sensor.Name.Contains("CPU Core #2")) { lblClockCore2.Content = sensor.Value; }
                    if (sensor.SensorType == SensorType.Clock && sensor.Name.Contains("CPU Core #3")) { lblClockCore3.Content = sensor.Value; }
                    if (sensor.SensorType == SensorType.Clock && sensor.Name.Contains("CPU Core #4")) { lblClockCore4.Content = sensor.Value; }

                    if (sensor.SensorType == SensorType.Power && sensor.Name.Contains("Package")) { lblPowerCpuPackage.Content = sensor.Value; }
                    if (sensor.SensorType == SensorType.Power && sensor.Name.Contains("Cores")) { lblPowerCpuCores.Content = sensor.Value; }
                    if (sensor.SensorType == SensorType.Power && sensor.Name.Contains("Memory")) { lblPowerMemory.Content = sensor.Value; }

                    if (sensor.SensorType == SensorType.Voltage && sensor.Name.Contains("CPU Core") && !sensor.Name.Contains("#")) { lblVoltageCpuPackage.Content = sensor.Value; }
                    if (sensor.SensorType == SensorType.Voltage && sensor.Name.Contains("CPU Core #1")) { lblVoltageCore1.Content = sensor.Value; }
                    if (sensor.SensorType == SensorType.Voltage && sensor.Name.Contains("CPU Core #2")) { lblVoltageCore2.Content = sensor.Value; }
                    if (sensor.SensorType == SensorType.Voltage && sensor.Name.Contains("CPU Core #3")) { lblVoltageCore3.Content = sensor.Value; }
                    if (sensor.SensorType == SensorType.Voltage && sensor.Name.Contains("CPU Core #4")) { lblVoltageCore4.Content = sensor.Value; }
                }
            }));
        }
        /*  ////////////////////////////////////////////////////////////////////////////////////////////////////////
        *  {-1-n-} - Miner 1
        *  ///////////////////////////////////////////////////////////////////////////////////////////////////////*/

        private int _miner1ProcessId = -1;
        //public bool hasTicked = false;
        public DispatcherTimer timeroutput1 = new DispatcherTimer();// Create a new instance of the DispatcherTimer class
        private async Task StartMiner1Async()
        {
            int blockCount = 1; // Initialize the block count to 1

            //create the process startinfo andset it up
            ProcessStartInfo Miner1StartInfo = new ProcessStartInfo();
            if(Properties.Settings.Default.PathToWallet.Length <= 0){MessageBox.Show("Path to Wallet has not been set, please go to settings.", "Path Error",MessageBoxButton.OK,MessageBoxImage.Error);return;}else
            {Miner1StartInfo.FileName = Properties.Settings.Default.PathToMiner;}
            Miner1StartInfo.WorkingDirectory = Path.GetDirectoryName(Properties.Settings.Default.PathToMiner);
            Miner1StartInfo.CreateNoWindow = true;
            Miner1StartInfo.UseShellExecute = false;
            Miner1StartInfo.RedirectStandardError = true;
            Miner1StartInfo.RedirectStandardOutput = true;


            using (Process Miner1 = new Process())
            {
                Miner1.StartInfo = Miner1StartInfo;

                // Start the process
                Miner1.Start();
                UpdateTimerPayout.Start();
                _miner1ProcessId = Miner1.Id;

                if (Properties.Settings.Default.CustomAffinity == true)
                {
                    int coreIndex = Properties.Settings.Default.MinerCore1;
                    if (coreIndex >= Environment.ProcessorCount)
                    {
                        throw new ArgumentException($"CPU core {coreIndex} does not exist.");
                    }
                    IntPtr coreMask = (IntPtr)(1 << coreIndex);
                    Miner1.ProcessorAffinity = coreMask;
                }


                // Read the output while the process is still running
                while (!Miner1.HasExited)
                {
                    string? outputLine = await Miner1.StandardOutput.ReadLineAsync();


                    // Check if the output contains the word generate we send the custom output
                    if (outputLine.Contains("generate") && !outputLine.Contains("%x"))
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            rtbMinerOutput1Custom.AppendText($"{GetTimeIn12HourFormat()} Miner 1: Generating Block {blockCount}, Please Be Patient. \n");   
                        }));
                    }

                    // if the output does not end with "]" we send all normal outputs
                    if (!outputLine.Contains("]"))
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            rtbMinerOutput1Normal.AppendText(outputLine + "\n");
                            rtbMinerOutput1Normal.ScrollToEnd();
                        }));
                    } // if the output does contain "]" we send a normal output and a custom output
                    else if (outputLine.Contains("]"))
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            blockCount++;
                            Miner1Add1Block();
                            rtbMinerOutput1Normal.AppendText(outputLine + "\n");
                            rtbMinerOutput1Normal.ScrollToEnd();

                            rtbMinerOutput1Custom.AppendText($"{GetTimeIn12HourFormat()}: Block {blockCount} Generation Complete\nPayout - n\nTransactions - n\n");
                            lblUserBlocksMinedSession.Content = blockCount.ToString();
                            rtbMinerOutput1Custom.ScrollToEnd();
                        }));
                    }
                }
                await Miner1.WaitForExitAsync();
                if (_miner1ProcessId == -1)
                {
                    timeroutput1.Stop();
                }
            }
        }

        private void Miner1Add1Block()
        {
            blockCountTotal++;
            Properties.Settings.Default.UserBlocksMinedTotal++;
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
            lblUserBlocksMinedTotal.Content = blockCountTotal.ToString();
        }

        private void StopMiner1()
        {
            if (_miner1ProcessId != -1)
            {
                Process Miner1Process = Process.GetProcessById(_miner1ProcessId);


                Miner1Process.Kill(); // Kill all instances of the Miner1 process with the specified PID
                Miner1Process.WaitForExit(); // Wait for the process to exit before continuing
                timeroutput1.Stop();

                _miner1ProcessId = -1; // Reset the stored PID to -1 since the process has been stopped
            }
        }

        private async void btnStartStopMiner1_Click(object sender, RoutedEventArgs e)
        {
            if (btnStartStopMiner1Normal.Content.ToString() == "Start Miner")
            {

                if (_miner1ProcessId == -1)
                {
                    btnStartStopMiner1Normal.Content = "Stop Miner";
                    btnStartStopMiner1Custom.Content = "Stop Miner";
                    await StartMiner1Async();
                }
                else
                {
                    rtbMinerOutput1Normal.AppendText("Miner Already Running");
                }

            }
            else if (btnStartStopMiner1Normal.Content.ToString() == "Stop Miner")
            {
                if (_miner1ProcessId > -1)
                {
                    btnStartStopMiner1Normal.Content = "Start Miner";
                    btnStartStopMiner1Custom.Content = "Start Miner";
                    StopMiner1();
                    UpdateTimerPayout.Stop();
                }
                else
                {
                    rtbMinerOutput1Normal.AppendText("Miner Not Running");
                }
            }
        }

        /*  ////////////////////////////////////////////////////////////////////////////////////////////////////////
        *  {-1-n-} - Miner 2
        *  ///////////////////////////////////////////////////////////////////////////////////////////////////////*/

        private int _miner2ProcessId = -1;
        //public bool hasTicked = false;
        public DispatcherTimer timeroutput2 = new DispatcherTimer();// Create a new instance of the DispatcherTimer class
        private async Task StartMiner2Async()
        {
            int blockCount = 1; // Initialize the block count to 2

            //create the process startinfo andset it up
            ProcessStartInfo Miner2StartInfo = new ProcessStartInfo();
            if (Properties.Settings.Default.PathToWallet.Length <= 0) { MessageBox.Show("Path to Wallet has not been set, please go to settings.", "Path Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            else
            { 
                Miner2StartInfo.FileName = Properties.Settings.Default.PathToMiner; }
                Miner2StartInfo.WorkingDirectory = Path.GetDirectoryName(Properties.Settings.Default.PathToMiner);
                Miner2StartInfo.CreateNoWindow = true;
                Miner2StartInfo.UseShellExecute = false;
                Miner2StartInfo.RedirectStandardError = true;
                Miner2StartInfo.RedirectStandardOutput = true;


            using (Process Miner2 = new Process())
            {
                Miner2.StartInfo = Miner2StartInfo;

                // Start the process
                Miner2.Start();
                //UpdateTimerPayout.Start();
                _miner2ProcessId = Miner2.Id;

                if (Properties.Settings.Default.CustomAffinity == true)
                {
                    int coreIndex = Properties.Settings.Default.MinerCore2;
                    if (coreIndex >= Environment.ProcessorCount)
                    {
                        throw new ArgumentException($"CPU core {coreIndex} does not exist.");
                    }
                    IntPtr coreMask = (IntPtr)(1 << coreIndex);
                    Miner2.ProcessorAffinity = coreMask;
                }


                // Read the output while the process is still running
                while (!Miner2.HasExited)
                {
                    string? outputLine = await Miner2.StandardOutput.ReadLineAsync();


                    // Check if the output contains the word generate we send the custom output
                    if (outputLine.Contains("generate") && !outputLine.Contains("%x"))
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            rtbMinerOutput2Custom.AppendText($"{GetTimeIn12HourFormat()}: Miner 2: Generating Block {blockCount}, Please Be Patient. \n");
                            rtbMinerOutput2Normal.ScrollToEnd();
                        }));
                    }

                    // if the output does not end with "]" we send all normal outputs
                    if (!outputLine.Contains("]"))
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            rtbMinerOutput2Normal.AppendText(outputLine + "\n");
                            rtbMinerOutput2Normal.ScrollToEnd();
                        }));
                    } // if the output does contain "]" we send a normal output and a custom output
                    else if (outputLine.Contains("]"))
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            blockCount++;
                            Miner2Add1Block();
                            rtbMinerOutput2Normal.AppendText(outputLine + "\n");
                            rtbMinerOutput2Normal.ScrollToEnd();

                            rtbMinerOutput2Custom.AppendText($"{GetTimeIn12HourFormat()}: Block {blockCount} Generation Complete\nPayout - n\nTransactions - n\n");
                            rtbMinerOutput2Normal.ScrollToEnd();
                            lblUserBlocksMinedSession.Content = blockCount.ToString();
                        }));
                    }
                }
                await Miner2.WaitForExitAsync();
                if (_miner2ProcessId == -1)
                {
                    //timeroutput2.Stop();
                }
            }
        }

        private void Miner2Add1Block()
        {
            blockCountTotal++;
            Properties.Settings.Default.UserBlocksMinedTotal++;
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
            lblUserBlocksMinedTotal.Content = blockCountTotal.ToString();
        }

        private void StopMiner2()
        {
            if (_miner2ProcessId != -1)
            {
                Process Miner2Process = Process.GetProcessById(_miner2ProcessId);


                Miner2Process.Kill(); // Kill all instances of the Miner2 process with the specified PID
                Miner2Process.WaitForExit(); // Wait for the process to exit before continuing
                //timeroutput2.Stop();

                _miner2ProcessId = -1; // Reset the stored PID to -1 since the process has been stopped
            }
        }

        private async void btnStartStopMiner2Normal_Click(object sender, RoutedEventArgs e)
        {
            if (btnStartStopMiner2Normal.Content.ToString() == "Start Miner")
            {

                if (_miner2ProcessId == -1)
                {
                    btnStartStopMiner2Normal.Content = "Stop Miner";
                    btnStartStopMiner2Custom.Content = "Stop Miner";
                    await StartMiner2Async();
                }
                else
                {
                    rtbMinerOutput2Normal.AppendText("Miner Already Running");
                }

            }
            else if (btnStartStopMiner2Normal.Content.ToString() == "Stop Miner")
            {
                if (_miner2ProcessId > -1)
                {
                    btnStartStopMiner2Normal.Content = "Start Miner";
                    btnStartStopMiner2Custom.Content = "Start Miner";
                    StopMiner2();
                    //UpdateTimerPayout.Stop();
                }
                else
                {
                    rtbMinerOutput2Normal.AppendText("Miner Not Running");
                }
            }
        }


        /*  ////////////////////////////////////////////////////////////////////////////////////////////////////////
       *  {-1-n-} - Miner 3
       *  ///////////////////////////////////////////////////////////////////////////////////////////////////////*/

        private int _miner3ProcessId = -1;
        //public bool hasTicked = false;
        public DispatcherTimer timeroutput3 = new DispatcherTimer();// Create a new instance of the DispatcherTimer class
        private async Task StartMiner3Async()
        {
            int blockCount = 1; // Initialize the block count to 2

            //create the process startinfo andset it up
            ProcessStartInfo Miner3StartInfo = new ProcessStartInfo();
            if (Properties.Settings.Default.PathToWallet.Length <= 0) { MessageBox.Show("Path to Wallet has not been set, please go to settings.", "Path Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            else
            {
                Miner3StartInfo.FileName = Properties.Settings.Default.PathToMiner;
            }
            Miner3StartInfo.WorkingDirectory = Path.GetDirectoryName(Properties.Settings.Default.PathToMiner);
            Miner3StartInfo.CreateNoWindow = true;
            Miner3StartInfo.UseShellExecute = false;
            Miner3StartInfo.RedirectStandardError = true;
            Miner3StartInfo.RedirectStandardOutput = true;


            using (Process Miner3 = new Process())
            {
                Miner3.StartInfo = Miner3StartInfo;

                // Start the process
                Miner3.Start();
                //UpdateTimerPayout.Start();
                _miner3ProcessId = Miner3.Id;

                if (Properties.Settings.Default.CustomAffinity == true)
                {
                    int coreIndex = Properties.Settings.Default.MinerCore2;
                    if (coreIndex >= Environment.ProcessorCount)
                    {
                        throw new ArgumentException($"CPU core {coreIndex} does not exist.");
                    }
                    IntPtr coreMask = (IntPtr)(1 << coreIndex);
                    Miner3.ProcessorAffinity = coreMask;
                }


                // Read the output while the process is still running
                while (!Miner3.HasExited)
                {
                    string? outputLine = await Miner3.StandardOutput.ReadLineAsync();


                    // Check if the output contains the word generate we send the custom output
                    if (outputLine.Contains("generate") && !outputLine.Contains("%x"))
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            rtbMinerOutput3Custom.AppendText($"{GetTimeIn12HourFormat()}: Miner 3: Generating Block {blockCount}, Please Be Patient. \n");
                            rtbMinerOutput3Normal.ScrollToEnd();
                        }));
                    }

                    // if the output does not end with "]" we send all normal outputs
                    if (!outputLine.Contains("]"))
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            rtbMinerOutput3Normal.AppendText(outputLine + "\n");
                            rtbMinerOutput3Normal.ScrollToEnd();
                        }));
                    } // if the output does contain "]" we send a normal output and a custom output
                    else if (outputLine.Contains("]"))
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            blockCount++;
                            Miner3Add1Block();
                            rtbMinerOutput3Normal.AppendText(outputLine + "\n");
                            rtbMinerOutput3Normal.ScrollToEnd();

                            rtbMinerOutput3Custom.AppendText($"{GetTimeIn12HourFormat()}: Block {blockCount} Generation Complete\nPayout - n\nTransactions - n\n");
                            rtbMinerOutput3Normal.ScrollToEnd();
                            lblUserBlocksMinedSession.Content = blockCount.ToString();
                        }));
                    }
                }
                await Miner3.WaitForExitAsync();
                if (_miner3ProcessId == -1)
                {
                    //timeroutput2.Stop();
                }
            }
        }

        private void Miner3Add1Block()
        {
            blockCountTotal++;
            Properties.Settings.Default.UserBlocksMinedTotal++;
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
            lblUserBlocksMinedTotal.Content = blockCountTotal.ToString();
        }

        private void StopMiner3()
        {
            if (_miner3ProcessId != -1)
            {
                Process Miner3Process = Process.GetProcessById(_miner3ProcessId);


                Miner3Process.Kill(); // Kill all instances of the Miner3 process with the specified PID
                Miner3Process.WaitForExit(); // Wait for the process to exit before continuing
                //timeroutput2.Stop();

                _miner3ProcessId = -1; // Reset the stored PID to -1 since the process has been stopped
            }
        }


        private async void btnStartStopMiner3Normal_Click(object sender, RoutedEventArgs e)
        {
            if (btnStartStopMiner3Normal.Content.ToString() == "Start Miner")
            {

                if (_miner3ProcessId == -1)
                {
                    btnStartStopMiner3Normal.Content = "Stop Miner";
                    btnStartStopMiner3Custom.Content = "Stop Miner";
                    await StartMiner3Async();
                }
                else
                {
                    rtbMinerOutput3Normal.AppendText("Miner Already Running");
                }

            }
            else if (btnStartStopMiner3Normal.Content.ToString() == "Stop Miner")
            {
                if (_miner3ProcessId > -1)
                {
                    btnStartStopMiner3Normal.Content = "Start Miner";
                    btnStartStopMiner3Custom.Content = "Start Miner";
                    StopMiner3();
                    //UpdateTimerPayout.Stop();
                }
                else
                {
                    rtbMinerOutput3Normal.AppendText("Miner Not Running");
                }
            }
        }


        /*  ////////////////////////////////////////////////////////////////////////////////////////////////////////
*  {-1-n-} - Miner 4
*  ///////////////////////////////////////////////////////////////////////////////////////////////////////*/

        private int _miner4ProcessId = -1;
        //public bool hasTicked = false;
        public DispatcherTimer timeroutput4 = new DispatcherTimer();// Create a new instance of the DispatcherTimer class
        private async Task StartMiner4Async()
        {
            int blockCount = 1; // Initialize the block count to 2

            //create the process startinfo andset it up
            ProcessStartInfo Miner4StartInfo = new ProcessStartInfo();
            if (Properties.Settings.Default.PathToWallet.Length <= 0) { MessageBox.Show("Path to Wallet has not been set, please go to settings.", "Path Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            else
            {
                Miner4StartInfo.FileName = Properties.Settings.Default.PathToMiner;
            }
            Miner4StartInfo.WorkingDirectory = Path.GetDirectoryName(Properties.Settings.Default.PathToMiner);
            Miner4StartInfo.CreateNoWindow = true;
            Miner4StartInfo.UseShellExecute = false;
            Miner4StartInfo.RedirectStandardError = true;
            Miner4StartInfo.RedirectStandardOutput = true;


            using (Process Miner4 = new Process())
            {
                Miner4.StartInfo = Miner4StartInfo;

                // Start the process
                Miner4.Start();
                //UpdateTimerPayout.Start();
                _miner3ProcessId = Miner4.Id;

                if (Properties.Settings.Default.CustomAffinity == true)
                {
                    int coreIndex = Properties.Settings.Default.MinerCore2;
                    if (coreIndex >= Environment.ProcessorCount)
                    {
                        throw new ArgumentException($"CPU core {coreIndex} does not exist.");
                    }
                    IntPtr coreMask = (IntPtr)(1 << coreIndex);
                    Miner4.ProcessorAffinity = coreMask;
                }


                // Read the output while the process is still running
                while (!Miner4.HasExited)
                {
                    string? outputLine = await Miner4.StandardOutput.ReadLineAsync();


                    // Check if the output contains the word generate we send the custom output
                    if (outputLine.Contains("generate") && !outputLine.Contains("%x"))
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            rtbMinerOutput4Custom.AppendText($"{GetTimeIn12HourFormat()}: Miner 4: Generating Block {blockCount}, Please Be Patient. \n");
                            rtbMinerOutput4Normal.ScrollToEnd();
                        }));
                    }

                    // if the output does not end with "]" we send all normal outputs
                    if (!outputLine.Contains("]"))
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            rtbMinerOutput4Normal.AppendText(outputLine + "\n");
                            rtbMinerOutput4Normal.ScrollToEnd();
                        }));
                    } // if the output does contain "]" we send a normal output and a custom output
                    else if (outputLine.Contains("]"))
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            blockCount++;
                            Miner4Add1Block();
                            rtbMinerOutput4Normal.AppendText(outputLine + "\n");
                            rtbMinerOutput4Normal.ScrollToEnd();

                            rtbMinerOutput4Custom.AppendText($"{GetTimeIn12HourFormat()}: Block {blockCount} Generation Complete\nPayout - n\nTransactions - n\n");
                            rtbMinerOutput4Normal.ScrollToEnd();
                            lblUserBlocksMinedSession.Content = blockCount.ToString();
                        }));
                    }
                }
                await Miner4.WaitForExitAsync();
                if (_miner4ProcessId == -1)
                {
                    //timeroutput2.Stop();
                }
            }
        }

        private void Miner4Add1Block()
        {
            blockCountTotal++;
            Properties.Settings.Default.UserBlocksMinedTotal++;
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
            lblUserBlocksMinedTotal.Content = blockCountTotal.ToString();
        }

        private void StopMiner4()
        {
            if (_miner4ProcessId != -1)
            {
                Process Miner4Process = Process.GetProcessById(_miner4ProcessId);


                Miner4Process.Kill(); // Kill all instances of the Miner4 process with the specified PID
                Miner4Process.WaitForExit(); // Wait for the process to exit before continuing
                //timeroutput2.Stop();

                _miner4ProcessId = -1; // Reset the stored PID to -1 since the process has been stopped
            }
        }

        private async void btnStartStopMiner4Normal_Click(object sender, RoutedEventArgs e)
        {
            if (btnStartStopMiner4Normal.Content.ToString() == "Start Miner")
            {

                if (_miner4ProcessId == -1)
                {
                    btnStartStopMiner4Normal.Content = "Stop Miner";
                    btnStartStopMiner4Custom.Content = "Stop Miner";
                    await StartMiner4Async();
                }
                else
                {
                    rtbMinerOutput4Normal.AppendText("Miner Already Running");
                }

            }
            else if (btnStartStopMiner4Normal.Content.ToString() == "Stop Miner")
            {
                if (_miner4ProcessId > -1)
                {
                    btnStartStopMiner4Normal.Content = "Start Miner";
                    btnStartStopMiner4Custom.Content = "Start Miner";
                    StopMiner4();
                    //UpdateTimerPayout.Stop();
                }
                else
                {
                    rtbMinerOutput4Normal.AppendText("Miner Not Running");
                }
            }
        }


    }
}
