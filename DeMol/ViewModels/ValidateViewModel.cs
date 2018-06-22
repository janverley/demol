using Caliburn.Micro;
using DeMol.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMol.ViewModels
{
    public class ValidateViewModel : Screen
    {
        private readonly ShellViewModel conductor;
        private readonly SimpleContainer container;

        public ValidateViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;
        }

        public BindableCollection<CheckViewModel> Checks { get; set; } = new BindableCollection<CheckViewModel>();

        public BindableCollection<string> Notas { get; set; } = new BindableCollection<string>();

        protected override void OnActivate()
        {
            base.OnActivate();

            var antwoorden = Util.SafeReadJson<AntwoordenData>($@".\Files\antwoorden.{container.GetInstance<MenuViewModel>().SelectedDag.Id}.json");

            Checks.Add(new CheckViewModel($"Aantal Antwoorden: {antwoorden.Spelers.Count}", antwoorden.Spelers.Count == container.GetInstance<MenuViewModel>().AantalSpelers));
            Checks.Add(new CheckViewModel($"Aantal Mollen: {antwoorden.Spelers.Count(s => s.IsDeMol)}", antwoorden.Spelers.Count(s => s.IsDeMol) == 1));

        }
        public void InvalidateAnswers()
        {
            var x = container.GetInstance<InvalidateViewModel>();
            conductor.ActivateItem(x);
        }

        public bool CanShowResult => Checks.All(c => c.IsOk);

        public void ShowResult()
        {
            var x = container.GetInstance<ResultViewModel>();
            conductor.ActivateItem(x);
        }

        public void Menu()
        {
            var x = container.GetInstance<MenuViewModel>();
            conductor.ActivateItem(x);
        }
    }
}
