using Caliburn.Micro;
using DeMol.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMol.ViewModels
{
    class DagResultaatViewModel : Screen
    {
        private readonly ShellViewModel conductor;
        private readonly SimpleContainer container;
        private string text;

        public DagResultaatViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;
        }

        public string Text
        {
            get { return text; }
            set { Set(ref text, value); }
        }
        public void Menu()
        {
            var x = container.GetInstance<MenuViewModel>();
            conductor.ActivateItem(x);
        }

        protected override void OnActivate()
        {
            var resultSB = new StringBuilder();

            var adminData = Util.SafeReadJson<AdminData>(container.GetInstance<ShellViewModel>().Dag);
            var antwoorden = Util.SafeReadJson<AntwoordenData>(container.GetInstance<ShellViewModel>().Dag);

            var deMol = antwoorden.Spelers.Single(s => s.IsDeMol);
            var juisteAntwoorden = deMol.Antwoorden;

            foreach (var juistAntwoord in juisteAntwoorden)
            {
                var code = juistAntwoord.Key;
                var vraagText = Util.GetVraagFromCode(code).Text;

                var antwoord = juistAntwoord.Value;

                resultSB.AppendLine($"- {code} - {vraagText} - {antwoord}");
            }

            Text = resultSB.ToString();

            base.OnActivate();
        }
    }
}
