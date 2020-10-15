using System;
using System.Linq;
using Caliburn.Micro;
using DeMol.Model;

namespace DeMol.ViewModels
{
    public class QuizVraagViewModel : PropertyChangedBase
    {
        private readonly bool meerdereOptiesMogelijk;
        private string antwoord;
        private bool showAntwoord;
        private string text;

        public QuizVraagViewModel(Vraag vraag, string vraagID)
        {
            Text = $"{vraag.Text} ({vraagID})";
            meerdereOptiesMogelijk = vraag.MeerdereOptiesMogelijk;
            VraagID = vraagID;
            if (vraag.MeerdereOptiesMogelijk)
            {
                MeerdereOpties =
                    new BindableCollection<MeerdereOptieViewModel>(vraag.Opties.Select(s =>
                        new MeerdereOptieViewModel(s)));
                Opties = new BindableCollection<OptieViewModel>();
            }
            else
            {
                MeerdereOpties = new BindableCollection<MeerdereOptieViewModel>();
                Opties = new BindableCollection<OptieViewModel>(vraag.Opties.Select(s => new OptieViewModel(s)));
            }

            ShowAntwoord = !vraag.Opties.Any();
        }

        public string Text
        {
            get => text;
            set => Set(ref text, value);
        }

        public string VraagID { get; }

        public string Antwoord
        {
            get => antwoord;
            set => Set(ref antwoord, value);
        }

        public bool ShowAntwoord
        {
            get => showAntwoord;
            set => Set(ref showAntwoord, value);
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

                if (Opties.Any())
                {
                    return Opties.FirstOrDefault(o => o.IsSelected)?.OptieText ?? $"NIKS_{DateTime.UtcNow.Ticks}";
                }

                return Antwoord ?? $"NIKS_{DateTime.UtcNow.Ticks}";
            }
        }
    }
}