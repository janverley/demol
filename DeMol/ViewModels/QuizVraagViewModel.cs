using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeMol.ViewModels
{
    public class QuizVraagViewModel : PropertyChangedBase
    {
        private string text;
        private string antwoord;
        private bool showAntwoord;

        public QuizVraagViewModel(string text, IEnumerable<string> opties)
        {
            Text = text;
            Opties = new BindableCollection<OptieViewModel>(opties.Select(s => new OptieViewModel(s)));
            ShowAntwoord = !Opties.Any();
        }

        public string Text
        {
            get { return text; }
            set
            {
                Set(ref text, value);
            }
        }

        public string Antwoord
        {
            get { return antwoord; }
            set
            {
                Set(ref antwoord, value);
            }
        }
        public bool ShowAntwoord
        {
            get { return showAntwoord; }
            set
            {
                Set(ref showAntwoord, value);
            }
        }

        public BindableCollection<OptieViewModel> Opties { get; set; }

        public string AntwoordToNote
        {
            get
            {
                if (Opties.Any())
                {
                    return Opties.FirstOrDefault(o => o.IsSelected)?.OptieText??$"NIKS_{DateTime.UtcNow.Ticks}";
                }
                else
                {
                    return Antwoord;
                }
            }
        }

    }
}
