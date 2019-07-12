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
    public class EndResultViewModel : Screen
    {
        private readonly ShellViewModel conductor;
        private readonly SimpleContainer container;

        private readonly DispatcherTimer timer = new DispatcherTimer();

        public EndResultViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;

            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(5);
        }

        private StringBuilder overzichtSB = new StringBuilder();

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();

            List<Score> scores = new List<Score>();

            foreach (var speler in container.GetInstance<ShellViewModel>().Spelerdata.Spelers)
            {
                scores.Add(new Score { Speler = speler.Naam, juisteAntwoorden = 0, tijd = TimeSpan.Zero });
            }

            var finaleData = Util.SafeReadJson<FinaleData>();
            var finaleAntwoorden = Util.SafeReadJson<AntwoordenData>("finale");

            foreach (var speler in finaleAntwoorden.Spelers)
            {
                var score = scores.Single(s => s.Speler.SafeEqual(speler.Naam));
                score.tijd = speler.Tijd;
                foreach (var finaleVraag in finaleData.FinaleVragen)
                {
                    if (speler.Antwoorden[finaleVraag.VraagCode].SafeEqual(finaleVraag.JuistAntwoord))
                    {
                        score.juisteAntwoorden++;
                    }
                }
            }
            var eindWinnaar = scores.OrderByDescending(s => s.juisteAntwoorden).ThenBy(s => s.tijd).First();

            Winnaar = eindWinnaar.Speler;

            Text = $"De EindWinnaar had de meeste vragen juist beantwoord in het finale spel.";

            var exaequos = scores.Where(s => !s.Equals(eindWinnaar) && s.juisteAntwoorden == eindWinnaar.juisteAntwoorden);
            if (exaequos.Any())
            {
                Text += $"{Environment.NewLine}Ex-aequo: Er waren {exaequos.Count()} spelers met evenveel juiste antwoorden als de winnaar ({eindWinnaar.juisteAntwoorden}), maar die waren trager dan {eindWinnaar.Speler}: {string.Join(",", exaequos.Select(s => s.Speler))}";
            }

            overzichtSB.AppendLine($"Spelers:");
            foreach (var score in scores)
            {
                overzichtSB.AppendLine($"{score.Speler}: {score.juisteAntwoorden} ({score.tijd})");
            }
            overzichtSB.AppendLine($"Antwoorden:");
            foreach (var fv in finaleData.FinaleVragen)
            {
                overzichtSB.AppendLine($"{fv.Dag.Naam} {fv.Description}: {fv.Vraag.Text}({fv.VraagCode}): {fv.JuistAntwoord}");
            }

        }

        protected override void OnDeactivate(bool close)
        {
            timer.Stop();
            base.OnDeactivate(close);
        }

        public void Menu()
        {
            var x = container.GetInstance<MenuViewModel>();
            conductor.ActivateItem(x);
        }
        public void Antwoorden()
        {
            var x = container.GetInstance<DagResultaatViewModel>();


            x.Text = overzichtSB.ToString();


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

            foreach (var dag in container.GetInstance<ShellViewModel>().DagenData.Dagen)
            {
                var antwoorden = Util.SafeReadJson<AntwoordenData>(dag.Id);

                Checks.Add(new CheckViewModel($"Dag {dag.Id} administratie saved:", Util.DataFileFoundAndValid<AdminData>(dag.Id)));
                Checks.Add(new CheckViewModel($"Aantal Antwoorden: {antwoorden.Spelers.Count}", antwoorden.Spelers.Count == container.GetInstance<ShellViewModel>().AantalSpelers));
                Checks.Add(new CheckViewModel($"Aantal Mollen: {antwoorden.Spelers.Count(s => s.IsDeMol)}", antwoorden.Spelers.Count(s => s.IsDeMol) == 1));
            }


            if (!Checks.All(c => c.IsOk))
            {
                ShowChecks = true;
                NotifyOfPropertyChange(() => ShowChecks);
            }
            else
            {

                Text = "De einduitslag van De Mol...";

                timer.Start();
            }
        }
        public BindableCollection<CheckViewModel> Checks { get; set; } = new BindableCollection<CheckViewModel>();
        public bool ShowChecks { get; private set; } = false;


    }
}
