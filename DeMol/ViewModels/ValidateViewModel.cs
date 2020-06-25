﻿using System.Linq;
using Caliburn.Micro;
using DeMol.Model;

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

        public bool CanShowResult => Checks.All(c => c.IsOk);

        protected override void OnActivate()
        {
            base.OnActivate();

             Checks.Add(new CheckViewModel($"Dag {container.GetInstance<ShellViewModel>().Dag} administratie saved:",
                Util.DataFileFoundAndValid<AdminData>(container.GetInstance<ShellViewModel>().Dag)));


            var admin = Util.GetAdminDataOfSelectedDag(container);

            foreach (var gespeeldeOpdrachtData in admin.OpdrachtenGespeeld)
            {

                var antwoordendata = Util.SafeReadJson<AntwoordenData>(gespeeldeOpdrachtData.OpdrachtId);

                Checks.Add(new CheckViewModel($"Aantal Antwoorden in opdracht {gespeeldeOpdrachtData.OpdrachtId}: {antwoordendata.Spelers.Count}",
                    antwoordendata.Spelers.Count == container.GetInstance<ShellViewModel>().AantalSpelers));

                Checks.Add(new CheckViewModel($"Aantal Mollen in opdracht {gespeeldeOpdrachtData.OpdrachtId}: {antwoordendata.Spelers.Count(s => s.IsDeMol)}",
                    antwoordendata.Spelers.Count(s => s.IsDeMol) == 1));
            }
        }

        public void InvalidateAnswers()
        {
            var x = container.GetInstance<InvalidateViewModel>();
            conductor.ActivateItem(x);
        }


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