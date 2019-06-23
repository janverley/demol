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
        private readonly bool meerdereOptiesMogelijk;
        private string text;
        private string antwoord;
        private bool showAntwoord;

        public QuizVraagViewModel(string text, IEnumerable<string> opties, bool meerdereOptiesMogelijk)
        {
            Text = text;
            this.meerdereOptiesMogelijk = meerdereOptiesMogelijk;

            if (meerdereOptiesMogelijk)
            {
                MeerdereOpties = new BindableCollection<MeerdereOptieViewModel>(opties.Select(s => new MeerdereOptieViewModel(s)));
                Opties = new BindableCollection<OptieViewModel>();
            }
            else
            {
                MeerdereOpties = new BindableCollection<MeerdereOptieViewModel>();
                Opties = new BindableCollection<OptieViewModel>(opties.Select(s => new OptieViewModel(s)));
            }
            ShowAntwoord = !opties.Any();
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
        public BindableCollection<MeerdereOptieViewModel> MeerdereOpties { get; set; }

        public string AntwoordToNote
        {
            get
            {
                if (meerdereOptiesMogelijk)
                {
                    return string.Join(",", MeerdereOpties.Where(o => o.IsSelected).Select(o => o.OptieText));
                }
                else
                {
                    if (Opties.Any())
                    {
                        return Opties.FirstOrDefault(o => o.IsSelected)?.OptieText ?? $"NIKS_{DateTime.UtcNow.Ticks}";
                    }
                    else
                    {
                        return Antwoord?? $"NIKS_{DateTime.UtcNow.Ticks}";
                    }
                }
            }
        }

    }
}
