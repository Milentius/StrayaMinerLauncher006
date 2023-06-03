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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace StrayaMinerLauncher006.Pages
{
    /// <summary>
    /// Interaction logic for InformationHelp.xaml
    /// </summary>
    public partial class InformationHelp : Page
    {
        public InformationHelp()
        {
            InitializeComponent();
            //string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "myfile.txt");
            //richTextBox1.LoadFile(path);
        }
    }
}
