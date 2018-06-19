using Admin.Model;
using Caliburn.Micro;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.ViewModels
{
    public class QuizIntroViewModel : Screen
    {
        public QuizIntroViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        private string naam;
        private INavigationService navigationService;
        private int dag;

        public int Dag
        {
            get { return dag; }
            set
            {
                Set(ref dag, value);
            }
        }

        public string Naam
        {
            get { return naam; }
            set
            {
                if (Set(ref naam, value))
                {
                    NotifyOfPropertyChange(() => CanStart);
                }
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate();

        }

        public bool CanStart => !string.IsNullOrEmpty(Naam);
        public void Start()
        {
            navigationService.NavigateToViewModel<QuizVragenViewModel>(new Dictionary<string, object> { { "Dag", Dag }, { "Speler", Naam } });
        }
        public void Stop()
        {
            navigationService.NavigateToViewModel<MenuViewModel>();
        }
    }
}
