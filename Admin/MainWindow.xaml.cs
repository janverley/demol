using Admin.Model;
using MahApps.Metro.Controls;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace Admin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected override void OnInitialized(EventArgs e)
        {
            this.WindowState = WindowState.Maximized;
            base.OnInitialized(e);

            string contents = File.ReadAllText(@".\Files\admin.1.json");
            var data = JsonConvert.DeserializeObject<AdminData>(contents);
        }

    }
}
