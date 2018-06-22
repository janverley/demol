using Caliburn.Micro;
using DeMol.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
            NotifyOfPropertyChange(() => Text);
        }

        public void Menu()
        {
            var x = container.GetInstance<MenuViewModel>();
            conductor.ActivateItem(x);
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

            string contents = File.ReadAllText($@".\Files\admin.{container.GetInstance<MenuViewModel>().SelectedDag.Id}.json");
            var data = JsonConvert.DeserializeObject<AdminData>(contents);

            AntwoordenData antwoorden = new AntwoordenData { Dag = container.GetInstance<MenuViewModel>().SelectedDag.Id.ToString() };
            if (File.Exists($@".\Files\antwoorden.{container.GetInstance<MenuViewModel>().SelectedDag.Id}.json"))
            {
                string antwoordenJson = File.ReadAllText($@".\Files\antwoorden.{container.GetInstance<MenuViewModel>().SelectedDag.Id}.json");
                antwoorden = JsonConvert.DeserializeObject<AntwoordenData>(antwoordenJson);
            }

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

            if (data.Opdrachten.op1 != Status.NOK && 
                data.Opdrachten.op2 != Status.NOK && 
                data.Opdrachten.op3 != Status.NOK)
            {
                // dag winnaar

                var dagwinnaar = scores.OrderBy(s => s.juisteAntwoorden).ThenBy(s=>s.tijd).First();

                text = $"De Dagwinnaar is {dagwinnaar.Speler}! De mol was vandaag {deMol.Naam}.";

            }
            else
            {
                // mol


                // mol geraden? 
                var geradendoor = antwoorden.Spelers.Count(s => s.DeMolIs.SafeEqual(deMol.Naam));

                var raders = antwoorden.Spelers.Where(s => s.DeMolIs.SafeEqual(deMol.Naam)).Select(s => s.Naam);

                if (geradendoor >= container.GetInstance<MenuViewModel>().AantalSpelersDieDeMolMoetenGeradenHebben)
                {
                    // de mol is geraden door
                    text = $"Geen winnaar: de mol was vandaag {deMol.Naam}, maar {geradendoor} spelers hebben geraden wie de mol was: {string.Join(", ", raders)}.";
                }
                else
                {
                    if (geradendoor == 0)
                    {
                        text = $"De Mol is winnaar: de mol was vandaag {deMol.Naam}, en niemand heeft geraden wie de mol was.";

                    }
                    else
                    {
                        text = $"De Mol is winnaar: de mol was vandaag {deMol.Naam}, en maar dat is enkel geraden door: {string.Join(", ", raders)}.";
                    }
                }
            }



            timer.Start();
        }

    }
}
