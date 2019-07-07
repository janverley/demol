using Caliburn.Micro;
using DeMol.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DeMol.ViewModels
{
    public class ResultViewModel : Screen
    {
        private readonly ShellViewModel conductor;
        private readonly SimpleContainer container;

        private readonly DispatcherTimer timer = new DispatcherTimer();

        public ResultViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;

            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(5);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var adminData = Util.SafeReadJson<AdminData>(container.GetInstance<ShellViewModel>().Dag);
            var antwoorden = Util.SafeReadJson<AntwoordenData>(container.GetInstance<ShellViewModel>().Dag);

            var deMol = antwoorden.Spelers.Single(s => s.IsDeMol);
            var juisteAntwoorden = deMol.Antwoorden;

            List<Score> scores = new List<Score>();
            foreach (var speler in antwoorden.Spelers.Where(s => !s.IsDeMol))
            {
                var juist = adminData.Pasvragen.Single(pv => pv.Naam.SafeEqual(speler.Naam)).PasVragenVerdiend;

                foreach (var juistAntwoord in juisteAntwoorden)
                {
                    var antwoordSpeler = speler.Antwoorden[juistAntwoord.Key];
                    if (antwoordSpeler.SafeEqual(juistAntwoord.Value))
                    {
                        juist++;
                    }
                }

                scores.Add(new Score { Speler = speler.Naam, juisteAntwoorden = juist, tijd = speler.Tijd });
            }

            var dagwinnaar = scores.OrderByDescending(s => s.juisteAntwoorden).ThenBy(s => s.tijd).First();

            var resultSB = new StringBuilder();

            resultSB.AppendLine($"- De Mol was vandaag: {deMol.Naam}!");

            resultSB.AppendLine($"- {dagwinnaar.Speler} had de meest vragen juist ({dagwinnaar.juisteAntwoorden}).");
            
            // mol geraden? 
            var raders = antwoorden.Spelers
                .Where(s => !s.IsDeMol)
                .Where(s => s.DeMolIs.SafeEqual(deMol.Naam));

            switch (raders.Count())
            {
               case 0:
                    resultSB.AppendLine($"- Niemand heeft geraden wie de mol was.");
                    break;
                case 1:
                    resultSB.AppendLine($"- Alleen {raders.First().Naam} heeft geraden wie de mol was.");
                    break;
                default:
                    resultSB.AppendLine($"- Deze {raders.Count()} spelers hebben geraden wie de mol was: {string.Join(",",raders.Select(r => r.Naam))}");
                    break;
            }

            Text = resultSB.ToString();

            timer.Stop();
        }

        public void Menu()
        {
            var x = container.GetInstance<MenuViewModel>();
            conductor.ActivateItem(x);
        }

        public void Antwoorden()
        {
            var x = container.GetInstance<DagResultaatViewModel>();
            conductor.ActivateItem(x);
        }

        private string winnaar;

        public string Winnaar
        {
            get { return winnaar; }
            set { Set(ref winnaar, value); }
        }

        private string text;

        public string Text
        {
            get { return text; }
            set { Set(ref text, value); }
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            var antwoorden = Util.SafeReadJson<AntwoordenData>(container.GetInstance<ShellViewModel>().Dag);

            Checks.Add(new CheckViewModel($"Dag {container.GetInstance<ShellViewModel>().Dag} administratie saved:", Util.DataFileFoundAndValid<AdminData>(container.GetInstance<ShellViewModel>().Dag)));
            Checks.Add(new CheckViewModel($"Aantal Antwoorden: {antwoorden.Spelers.Count}", antwoorden.Spelers.Count == container.GetInstance<ShellViewModel>().AantalSpelers));
            Checks.Add(new CheckViewModel($"Aantal Mollen: {antwoorden.Spelers.Count(s => s.IsDeMol)}", antwoorden.Spelers.Count(s => s.IsDeMol) == 1));

            if (!Checks.All(c => c.IsOk))
            {
                ShowChecks = true;
                NotifyOfPropertyChange(() => ShowChecks);
            }
            else
            {

                Text = "De uitslag van vandaag...";

                timer.Start();
            }
        }
        public BindableCollection<CheckViewModel> Checks { get; set; } = new BindableCollection<CheckViewModel>();
        public bool ShowChecks { get; private set; } = false;

    }
}
