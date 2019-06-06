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

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();

            List<Score> scores = new List<Score>();

            foreach (var speler in container.GetInstance<ShellViewModel>().Spelerdata.Spelers)
            {

                scores.Add(new Score { Speler = speler.Naam, juisteAntwoorden = 0, tijd = TimeSpan.Zero}); 
            }

            foreach (var dag in container.GetInstance<ShellViewModel>().DagenData.Dagen)
            {
                var adminData = Util.SafeReadJson<AdminData>(dag.Id);
                var antwoorden = Util.SafeReadJson<AntwoordenData>(dag.Id);

                var deMol = antwoorden.Spelers.Single(s => s.IsDeMol);
                var juisteAntwoorden = deMol.Antwoorden;

                var totaalJuisteAntwoordenVandaag = 0;
                var totaalTijdVandaag = TimeSpan.Zero;

                foreach (var speler in antwoorden.Spelers.Where(s => !s.IsDeMol))
                {
                    var juist = adminData.Pasvragen.Single(pv => pv.Naam.SafeEqual(speler.Naam)).PasVragenVerdiend;
                    
                    for (int i = 0; i < juisteAntwoorden.Count; i++)
                    {
                        var a = speler.Antwoorden[i].Trim().ToLower();
                        var ja = juisteAntwoorden[i].Trim().ToLower();

                        if (a.SafeEqual(ja))
                        {
                            juist++;
                            totaalJuisteAntwoordenVandaag++;
                        }
                    }

                    var score = scores.Single(s => s.Speler.SafeEqual(speler.Naam));

                    score.juisteAntwoorden += juist;
                    score.tijd += speler.Tijd;
                    totaalTijdVandaag+=speler.Tijd;
                }

                // geef de mol het gemiddelde
                var molscore = scores.Single(s => s.Speler.SafeEqual(antwoorden.Spelers.Single(ss => ss.IsDeMol).Naam));
                var aantalNietMollen = antwoorden.Spelers.Count - 1;
                molscore.juisteAntwoorden += totaalJuisteAntwoordenVandaag / aantalNietMollen;
                molscore.tijd += TimeSpan.FromTicks(totaalTijdVandaag.Ticks / aantalNietMollen);
            }

            var eindWinnaar = scores.OrderByDescending(s => s.juisteAntwoorden).ThenBy(s => s.tijd).First();

            Winnaar = eindWinnaar.Speler;

            Text = $"De EindWinnaar had de meeste vragen juist beantwoord in heel het spel.";

            var exaequos = scores.Where(s => !s.Equals(eindWinnaar) && s.juisteAntwoorden == eindWinnaar.juisteAntwoorden);
            if (exaequos.Any())
            {
                Text += $"{Environment.NewLine}Ex-aequo: Er waren {exaequos.Count()} spelers met evenveel juiste antwoorden als de winnaar ({eindWinnaar.juisteAntwoorden}), maar die waren trager dan {eindWinnaar.Speler}: {string.Join(",", exaequos.Select(s => s.Speler))}";
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
