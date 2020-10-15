using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace DeMol.Views
{
    /// <summary>
    ///     Interaction logic for QuizView.xaml
    /// </summary>
    public partial class QuizNaamView : UserControl
    {
        public QuizNaamView()
        {
            InitializeComponent();
            Dispatcher.BeginInvoke(new Action(() => Keyboard.Focus(Naam)), DispatcherPriority.Loaded);
        }
    }
}