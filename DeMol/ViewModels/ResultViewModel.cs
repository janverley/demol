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

        public int Dag { get; set; }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var adminData = Util.SafeReadJson<AdminData>($@".\Files\admin.{Dag}.json");

            var antwoorden = Util.SafeReadJson<AntwoordenData>($@".\Files\antwoorden.{Dag}.json");

            var deMol = antwoorden.Spelers.Single(s => s.IsDeMol);
            var juisteAntwoorden = deMol.Antwoorden;

            List<Score> scores = new List<Score>();
            foreach (var speler in antwoorden.Spelers)
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

            if (adminData.Opdrachten.op1 == Status.OK || adminData.Opdrachten.op1 == Status.NA) // geslaagd of niet gespeeld
            {
                if (adminData.Opdrachten.op2 == Status.OK || adminData.Opdrachten.op2 == Status.NA) // geslaagd of niet gespeeld
                {
                    if (adminData.Opdrachten.op3 == Status.OK || adminData.Opdrachten.op3 == Status.NA) // geslaagd of niet gespeeld
                    {
                        // dag winnaar

                        var dagwinnaar = scores.OrderBy(s => s.juisteAntwoorden).ThenBy(s => s.tijd).First();

                        Winnaar = dagwinnaar.Speler;
                        

                        Text = $"Alle opdrachten geslaaagd! {Environment.NewLine} De Dagwinnaar is {dagwinnaar.Speler}!  De mol was vandaag {deMol.Naam}.";
                        Text += $"{Environment.NewLine}{dagwinnaar.Speler} had {dagwinnaar.juisteAntwoorden} juiste antwoorden.";

                        if (scores.Count(s => s.juisteAntwoorden == dagwinnaar.juisteAntwoorden) > 1)
                        {
                            Text += $"{Environment.NewLine}Ex-aequo: Er waren {scores.Count(s => s.juisteAntwoorden == dagwinnaar.juisteAntwoorden)} spelers met {dagwinnaar.juisteAntwoorden} juiste antwoorden, maar die waren trager dan {dagwinnaar.Speler}: {string.Join(",", scores.Where(s => ! s.Equals(dagwinnaar)).Select(s => s.Speler))}";
                        }
                            
                    }
                    else // op1 wel en op2 wel maar op 3 niet geslaagd
                    {
                        Text = $"Opdracht 3 was mislukt! {MolText(antwoorden)}";
                    }
                }
                else // op1 wel maar op2 niet geslaagd
                {
                    Text = $"Opdracht 2 was mislukt! {MolText(antwoorden)}";
                }
            }
            else // op1 niet geslaagd
            {
                // mol
                Text = $"Opdracht 1 was mislukt! {MolText(antwoorden)}";
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

            Text = "De uitslag van vandaag...";

            timer.Start();
        }

        private string MolText(AntwoordenData antwoorden)
        {
            var result = "?";

            // mol geraden? 
            var deMol = antwoorden.Spelers.Single(s => s.IsDeMol);
            var raders = antwoorden.Spelers.Where(s => !s.IsDeMol).Where(s => s.DeMolIs.SafeEqual(deMol.Naam));
            if (raders.Count() >= container.GetInstance<MenuViewModel>().AantalSpelersDieDeMolMoetenGeradenHebben)
            {
                // de mol is geraden door
                Winnaar = "Niemand";
                result = $"Geen winnaar!{Environment.NewLine}De mol was vandaag {deMol.Naam}, maar {raders.Count()} spelers hebben geraden wie de mol was: {string.Join(", ", raders.Select(s => s.Naam))}.";
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

            return $"{Environment.NewLine}{result}";
        }
    }
}
