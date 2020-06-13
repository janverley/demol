using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace DeMol.Views
{
    /// <summary>
    ///     Interaction logic for QuizVraagView.xaml
    /// </summary>
    public partial class QuizVraagView : UserControl
    {
        public QuizVraagView()
        {
            InitializeComponent();
            Dispatcher.BeginInvoke(new Action(() => Keyboard.Focus(Antwoord)), DispatcherPriority.Loaded);
        }
    }
}