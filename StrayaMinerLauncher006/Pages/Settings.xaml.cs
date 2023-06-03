using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PathIO = System.IO.Path;

namespace StrayaMinerLauncher006.Pages
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Page
    {
        public bool PathIsDirty = false;
        public bool MarketIsDirty = false;
        public bool ApiKeyIsDirty = false;
        public bool TickerIsDirty = false;
        public bool DeviceMonIsDirty = false;
        public bool AffinintyIsDirty = false;
        public bool PriorityIsDirty = false;

        MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
        public Settings()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            LoadPath();
            LoadMarket();
            LoadTicker();
            LoadApiKey();
            LoadDeviceMonitor();
            LoadAffinitySettings();
            LoadPrioritySettings();
        }

        private void btnCloseSettings_Click(object sender, RoutedEventArgs e)
        {
            if(mainWindow.SettingsDirty == false)
            {
                
            }
            else if(mainWindow.SettingsDirty == true)
            {
                mainWindow.SaveAndCloseSettings();
            }
        }

        private void btnSaveCloseSettings_Click(object sender, RoutedEventArgs e)
        {
            SavePath();
            SaveMarket();
            SaveTicker();
            SaveDeviceMonitor();
            SaveApi();
            SaveAffinity();
            SavePriority();

            mainWindow.SaveAndCloseSettings();
        }

        private void btnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            SavePath();
            SaveMarket();
            SaveTicker();
            SaveDeviceMonitor();
            SaveApi();
            SaveAffinity();
            SavePriority();

        }

        private void btnDefaultSettings_Click(object sender, RoutedEventArgs e)
        {
            // reset all settings to their default values
            tbPathToWallet.Text = "REQUIRED - Set The Path To The 'strayacoin-qt.exe'; File";
            rbMarketNone.IsChecked = true;
            rbDevMonTime5Sec.IsChecked = true;
            
            if(tbExchangeApiKey.Text.Length > 0)
            {
                MessageBoxResult mBoxResult = MessageBox.Show("Do you want to reset your API Key and Secret Key?", "Warning", MessageBoxButton.YesNo,MessageBoxImage.Question);
                if (mBoxResult == MessageBoxResult.Yes) { tbExchangeApiKey.Text = "OPTIONAL - Enter your API Key here for trading"; Properties.Settings.Default.ApiKey = ""; pbExchangeApiSecret.Clear(); Properties.Settings.Default.ApiSecretKey = ""; }
            }
        }

        private void btnShowSecret_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void tbPathToWallet_TextChanged(object sender, TextChangedEventArgs e)
        {
            PathIsDirty = true;
        }

        private void tbExchangeApiKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbExchangeApiKey.Text))
            {
                lblApiKeyPlaceholderText.Visibility = Visibility.Hidden;
            }
            else
            {
                lblApiKeyPlaceholderText.Visibility = Visibility.Visible;
            }
        }

        private void pbExchangeApiSecret_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnBrowseToWallet_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                // set the textbox to the path including file
                tbPathToWallet.Text = openFileDialog.FileName;
            }
        }

        private void btnClearApiKey_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult mBoxResult = MessageBox.Show("Do you want to clear your API Key?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (mBoxResult == MessageBoxResult.Yes) { lblApiKeyPlaceholderText.Content = "OPTIONAL - Enter your API Key here for trading"; Properties.Settings.Default.ApiKey = ""; tbExchangeApiKey.Clear(); tbExchangeApiKey.Focus(); }
            
        }

        private void btnClearApiSecretKey_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult mBoxResult = MessageBox.Show("Do you want to clear your API Secret Key?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (mBoxResult == MessageBoxResult.Yes) { pbExchangeApiSecret.Clear(); pbExchangeApiSecret.Focus(); }
        }

        private void SavePath()
        {
            if (tbPathToWallet.Text.Length > 0 && tbPathToWallet.Text.EndsWith(@"\strayacoin-qt.exe"))
            {
                Properties.Settings.Default.PathToWallet = @tbPathToWallet.Text;
                Properties.Settings.Default.PathToCLI = @PathIO.GetDirectoryName(Properties.Settings.Default.PathToWallet) + @"\strayacoin-cli.exe";
                Properties.Settings.Default.PathToMiner = @PathIO.GetDirectoryName(Properties.Settings.Default.PathToWallet) + @"\mine.bat";
                
            }
            else
            {
                tbPathToWallet.Text = $"Path Incorrect - Please browse to the 'strayacoin-qt.exe' file";
                Properties.Settings.Default.PathToWallet = tbPathToWallet.Text;
                
            }
        }

        private void SaveMarket()
        {
            if (rbMarketNone.IsChecked == true) { Properties.Settings.Default.PreferedMarket = "HODL"; }
            else if (rbMarketTradeOgre.IsChecked == true) { Properties.Settings.Default.PreferedMarket = "TradeOgre"; }
            else if (rbMarketAltMarkets.IsChecked == true) { Properties.Settings.Default.PreferedMarket = "AltMarket"; }
            else if (rbMarketUnnamed.IsChecked == true) { Properties.Settings.Default.PreferedMarket = "Unnamed"; }
            else { rbMarketNone.IsChecked = true; Properties.Settings.Default.PreferedMarket = "HODL"; }

        }

        private void SaveApi()
        {

            if (tbExchangeApiKey.Text.Length > 0) { Properties.Settings.Default.ApiKey = tbExchangeApiKey.Text; }
            if (pbExchangeApiSecret.Password.Length > 0) { Properties.Settings.Default.ApiSecretKey = pbExchangeApiSecret.Password; }
        }

        private void SaveDeviceMonitor()
        {

            if (rbDevMonTime3Sec.IsChecked == true) { Properties.Settings.Default.DeviceMonitorTime = 3; }
            else if (rbDevMonTime5Sec.IsChecked == true) { Properties.Settings.Default.DeviceMonitorTime = 5; }
            else if (rbDevMonTime10Sec.IsChecked == true) { Properties.Settings.Default.DeviceMonitorTime = 10; }
            else if (rbDevMonTime20Sec.IsChecked == true) { Properties.Settings.Default.DeviceMonitorTime = 20; }
            else if (rbDevMonTime30Sec.IsChecked == true) { Properties.Settings.Default.DeviceMonitorTime = 30; }
            else if (rbDevMonTime1Min.IsChecked == true) { Properties.Settings.Default.DeviceMonitorTime = 60; }

        }


        private void SaveTicker()
        {

            if (rbPrefTickNah.IsChecked == true) { Properties.Settings.Default.PreferedTicker = "NAH"; }
            if (rbPrefTickYeah.IsChecked == true) { Properties.Settings.Default.PreferedTicker = "Yeah"; }
            if (rbPrefTickDunno.IsChecked == true) { Properties.Settings.Default.PreferedTicker = "Dunno"; }
        }

        private void SaveAffinity()
        {
            if (cbAffinityOn.IsChecked == true)
            {
                Properties.Settings.Default.MinerCore1 = cBoxAffinityCore1.SelectedIndex + 1;
                Properties.Settings.Default.MinerCore2 = cBoxAffinityCore2.SelectedIndex + 1;
                Properties.Settings.Default.MinerCore3 = cBoxAffinityCore3.SelectedIndex + 1;
                Properties.Settings.Default.MinerCore4 = cBoxAffinityCore4.SelectedIndex + 1;
            }
        }

        private void SavePriority()
        {

        }

        private void LoadPath()
        {
            if (Properties.Settings.Default.PathToWallet.Length > 0) { tbPathToWallet.Text = Properties.Settings.Default.PathToWallet; } else { tbPathToWallet.Text = "REQUIRED - Set The Path To The 'strayacoin-qt.exe' File"; }

        }

        private void LoadMarket()
        {
            if (Properties.Settings.Default.PreferedMarket == "HODL") { rbMarketNone.IsChecked = true; }
            else if (Properties.Settings.Default.PreferedMarket == "TradeOgre") { rbMarketTradeOgre.IsChecked = true; }
            else if (Properties.Settings.Default.PreferedMarket == "AltMarkets") { rbMarketAltMarkets.IsChecked = true; }
            else if (Properties.Settings.Default.PreferedMarket == "Unnamed") { rbMarketUnnamed.IsChecked = true; }
            else { rbMarketNone.IsChecked = true; }


        }

        private void LoadTicker()
        {
            if (Properties.Settings.Default.PreferedTicker == "NAH") { rbPrefTickNah.IsChecked = true; }
            else if (Properties.Settings.Default.PreferedTicker == "Yeah") { rbPrefTickYeah.IsChecked = true; }
            else if (Properties.Settings.Default.PreferedTicker == "Dunno") { rbPrefTickDunno.IsChecked = true; }
        }

        private void LoadDeviceMonitor()
        {
            if (Properties.Settings.Default.DeviceMonitorTime == 3) { rbDevMonTime3Sec.IsChecked = true; }
            else if (Properties.Settings.Default.DeviceMonitorTime == 5) { rbDevMonTime5Sec.IsChecked = true; }
            else if (Properties.Settings.Default.DeviceMonitorTime == 10) { rbDevMonTime10Sec.IsChecked = true; }
            else if (Properties.Settings.Default.DeviceMonitorTime == 20) { rbDevMonTime20Sec.IsChecked = true; }
            else if (Properties.Settings.Default.DeviceMonitorTime == 30) { rbDevMonTime30Sec.IsChecked = true; }
            else if (Properties.Settings.Default.DeviceMonitorTime == 60) { rbDevMonTime1Min.IsChecked = true; }
        }

        private void LoadApiKey()
        {
            if (Properties.Settings.Default.ApiKey.Length > 0) { tbExchangeApiKey.Text = Properties.Settings.Default.ApiKey; }
            if (Properties.Settings.Default.ApiSecretKey.Length > 0) { pbExchangeApiSecret.Password = Properties.Settings.Default.ApiSecretKey; }
        }

        private void LoadAffinitySettings()
        {
            if (Properties.Settings.Default.CustomAffinity == true)
            {
                cbAffinityOn.IsChecked = true;
                cBoxAffinityCore1.SelectedIndex = Properties.Settings.Default.MinerCore1 - 1;
                cBoxAffinityCore2.SelectedIndex = Properties.Settings.Default.MinerCore2 - 1;
                cBoxAffinityCore3.SelectedIndex = Properties.Settings.Default.MinerCore3 - 1;
                cBoxAffinityCore4.SelectedIndex = Properties.Settings.Default.MinerCore4 - 1;
                cBoxAffinityCore1.IsEnabled = true;
                cBoxAffinityCore2.IsEnabled = true;
                cBoxAffinityCore3.IsEnabled = true;
                cBoxAffinityCore4.IsEnabled = true;
            }
            else if (Properties.Settings.Default.CustomAffinity == false)
            {
                cbPriorityOn.IsChecked = false;
                cBoxAffinityCore1.SelectedIndex = Properties.Settings.Default.MinerCore1 - 1;
                cBoxAffinityCore2.SelectedIndex = Properties.Settings.Default.MinerCore2 - 1;
                cBoxAffinityCore3.SelectedIndex = Properties.Settings.Default.MinerCore3 - 1;
                cBoxAffinityCore4.SelectedIndex = Properties.Settings.Default.MinerCore4 - 1;
                cBoxAffinityCore1.IsEnabled = false;
                cBoxAffinityCore2.IsEnabled = false;
                cBoxAffinityCore3.IsEnabled = false;
                cBoxAffinityCore4.IsEnabled = false;
            }
        }

        private void LoadPrioritySettings()
        {
            if (Properties.Settings.Default.CustomPriority == true)
            {
                cbPriorityOn.IsChecked = true;
                cBoxPriority1.SelectedIndex = Properties.Settings.Default.PriorityIndex1;
                cBoxPriority2.SelectedIndex = Properties.Settings.Default.PriorityIndex2;
                cBoxPriority3.SelectedIndex = Properties.Settings.Default.PriorityIndex3;
                cBoxPriority4.SelectedIndex = Properties.Settings.Default.PriorityIndex4;
                cBoxPriority1.IsEnabled = true;
                cBoxPriority2.IsEnabled = true;
                cBoxPriority3.IsEnabled = true;
                cBoxPriority4.IsEnabled = true;
            }
            else if (Properties.Settings.Default.CustomPriority == false)
            {
                cbPriorityOn.IsChecked = false;
                cBoxPriority1.SelectedIndex = Properties.Settings.Default.PriorityIndex1;
                cBoxPriority2.SelectedIndex = Properties.Settings.Default.PriorityIndex2;
                cBoxPriority3.SelectedIndex = Properties.Settings.Default.PriorityIndex3;
                cBoxPriority4.SelectedIndex = Properties.Settings.Default.PriorityIndex4;
                cBoxPriority1.IsEnabled = false;
                cBoxPriority2.IsEnabled = false;
                cBoxPriority3.IsEnabled = false;
                cBoxPriority4.IsEnabled = false;
            }
        }


    }
}
