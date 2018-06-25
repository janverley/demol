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
                var juist = 0;

                for (int i = 0; i < juisteAntwoorden.Count; i++)
                {
                    var a = speler.Antwoorden[i].Trim().ToLower();
                    var ja = juisteAntwoorden[i].Trim().ToLower();

                    if (a.SafeEqual(ja))
                    {
                        juist++;
                    }
                }

                scores.Add(new Score { Speler = speler.Naam, juisteAntwoorden = juist, tijd = speler.Tijd });
            }

            var dagwinnaar = scores.OrderByDescending(s => s.juisteAntwoorden).ThenBy(s => s.tijd).First();

            if ((adminData.Opdrachten.op1 == Status.OK || adminData.Opdrachten.op1 == Status.NA) // geslaagd of niet gespeeld
            &&
                (adminData.Opdrachten.op2 == Status.OK || adminData.Opdrachten.op2 == Status.NA) // geslaagd of niet gespeeld
                &&
                    (adminData.Opdrachten.op3 == Status.OK || adminData.Opdrachten.op3 == Status.NA)) // geslaagd of niet gespeeld
            {
                // dag winnaar

                Winnaar = dagwinnaar.Speler;


                Text = $"Alle opdrachten geslaaagd! {Environment.NewLine} De Dagwinnaar is {dagwinnaar.Speler}!  De mol was vandaag {deMol.Naam}.";
                Text += $"{Environment.NewLine}{dagwinnaar.Speler} had {dagwinnaar.juisteAntwoorden} juiste antwoorden.";

                var exaequos = scores.Where(s => !s.Equals(dagwinnaar) && s.juisteAntwoorden == dagwinnaar.juisteAntwoorden);
                if (exaequos.Any())
                {
                    Text += $"{Environment.NewLine}Ex-aequo: Er waren {exaequos.Count()} spelers met evenveel juiste antwoorden als de winnaar ({dagwinnaar.juisteAntwoorden}), maar die waren trager dan {dagwinnaar.Speler}: {string.Join(",", exaequos.Select(s => s.Speler))}";
                }

            }
            else
            {
                var result = "?";

                // mol geraden? 
                var raders = antwoorden.Spelers.Where(s => !s.IsDeMol).Where(s => s.DeMolIs.SafeEqual(deMol.Naam));
                if (raders.Count() >= container.GetInstance<ShellViewModel>().AantalSpelersDieDeMolMoetenGeradenHebben)
                {
                    // de mol is geraden door
                    Winnaar = "Niemand";
                    result = $"Geen winnaar!{Environment.NewLine}De mol was vandaag {deMol.Naam}, maar {raders.Count()} spelers hebben geraden wie de mol was: {string.Join(", ", raders.Select(s => s.Naam))}."
                        +
                        $"{Environment.NewLine}{dagwinnaar.Speler} had de meeste juiste antwoorden vandaag, of was het snelst.";
                }
                else
                {
                    Winnaar = deMol.Naam;
                    if (!raders.Any())
                    {
                        result = $"De Mol is winnaar!{Environment.NewLine}De mol was vandaag {deMol.Naam}, en niemand heeft geraden wie de mol was.";

                    }
                    else
                    {
                        result = $"De Mol is winnaar!{Environment.NewLine}De mol was vandaag {deMol.Naam}, en dat is enkel geraden door: {string.Join(", ", raders.Select(s => s.Naam))}.";
                    }
                }

                Text = $"{Environment.NewLine}{result}";
            }
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

        struct Score
        {
            public string Speler;
            public int juisteAntwoorden;
            public TimeSpan tijd;
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
