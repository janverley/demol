using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using Caliburn.Micro;
using DeMol.Model;

namespace DeMol.ViewModels
{
    public class EndResultViewModel : Screen
    {
        private readonly ShellViewModel conductor;
        private readonly SimpleContainer container;

        private readonly DispatcherTimer timer = new DispatcherTimer();

        private string text;

        private string winnaar;
        private bool canUitslag;
        private List<OpdrachtData> gespeeldeOpdrachten;

        public EndResultViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;

            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(5);
            
            var gespeeldeOpdrachtenIds = new List<string>();

            foreach (var dag in container.GetInstance<ShellViewModel>().DagenData.Dagen)
            {
                var adminadata = Util.SafeReadJson<AdminData>(dag.Id);

                foreach (var gespeeldeOpdrachtData in adminadata.OpdrachtenGespeeld)
                {
                    gespeeldeOpdrachtenIds.Add(gespeeldeOpdrachtData.OpdrachtId);
                }
            }
            
             gespeeldeOpdrachten = Util.AlleOpdrachtData().Where(od => gespeeldeOpdrachtenIds.Any(i => i == od.Opdracht)).ToList();


        }

        public bool CanUitslag
        {
            get => canUitslag;
            set => Set(ref canUitslag, value);
        }

        public string Winnaar
        {
            get => winnaar;
            set => Set(ref winnaar, value);
        }

        public string Text
        {
            get => text;
            set => Set(ref text, value);
        }

        public BindableCollection<CheckViewModel> Checks { get; set; } = new BindableCollection<CheckViewModel>();

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();

            var scores = new List<Score>();

            foreach (var speler in container.GetInstance<ShellViewModel>().Spelerdata.Spelers)
            {
                scores.Add(new Score {Speler = speler.Naam, juisteAntwoorden = 0, tijd = TimeSpan.Zero});
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

            Text = "De EindWinnaar had de meeste vragen juist beantwoord in het finale spel.";

            var exaequos = scores.Where(s =>
                !s.Equals(eindWinnaar) && s.juisteAntwoorden == eindWinnaar.juisteAntwoorden);
            if (exaequos.Any())
            {
                Text +=
                    $"{Environment.NewLine}Ex-aequo: Er waren {exaequos.Count()} spelers met evenveel juiste antwoorden als de winnaar ({eindWinnaar.juisteAntwoorden}), maar die waren trager dan {eindWinnaar.Speler}: {string.Join(",", exaequos.Select(s => s.Speler))}";
            }

        var overzichtSB = new StringBuilder();


            overzichtSB.AppendLine("Spelers:");
            foreach (var score in scores)
            {
                overzichtSB.AppendLine($"{score.Speler}: {score.juisteAntwoorden} ({score.tijd})");
            }

            overzichtSB.AppendLine("Antwoorden:");
            foreach (var fv in finaleData.FinaleVragen)
            {
                overzichtSB.AppendLine(
                    $"{fv.Dag.Naam} {fv.Description}: {fv.Vraag.Text}({fv.VraagCode}): {fv.JuistAntwoord}");
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
            var sb = new StringBuilder();


            sb.AppendLine("Mollen:");

            foreach (var opdrachtData in gespeeldeOpdrachten)
            {
                    var antwoorden = Util.SafeReadJson<AntwoordenData>(opdrachtData.Opdracht);
                    var mol = antwoorden.Spelers.First(s => s.IsDeMol)?.Naam??"?";
                    var raders = antwoorden.Spelers
                        .Where(s => !s.IsDeMol)
                        .Where(s => s.DeMolIs.SafeEqual(mol)).Select(s => s.Naam);
                    sb.AppendLine(
                        $"Opdracht: {Util.OpdrachtUiNaam(opdrachtData)}: De mol was {mol}, geraden door: {string.Join(",", raders)}");
            }

            Text = sb.ToString();
        }

        public void Uitslag()
        {
            Text = "De einduitslag van De Mol...";

            timer.Start();

            
            // var x = container.GetInstance<DagResultaatViewModel>();
            //
            //
            // x.Text = overzichtSB.ToString();
            //
            //
            // conductor.ActivateItem(x);
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            CanUitslag = false;

            foreach (var dag in container.GetInstance<ShellViewModel>().DagenData.Dagen)
            {
                Checks.Add(new CheckViewModel($"Dag {dag.Id} administratie saved:",
                    Util.DataFileFoundAndValid<AdminData>(dag.Id)));
            }
            
            foreach (var opdrachtData in gespeeldeOpdrachten)
            {
                var antwoorden = Util.SafeReadJson<AntwoordenData>(opdrachtData.Opdracht);
                
                Checks.Add(new CheckViewModel($"Opdracht {Util.OpdrachtUiNaam(opdrachtData)}: Aantal Antwoorden: {antwoorden.Spelers.Count}",
                    antwoorden.Spelers.Count == container.GetInstance<ShellViewModel>().AantalSpelers));
                Checks.Add(new CheckViewModel($"Opdracht {Util.OpdrachtUiNaam(opdrachtData)}: Aantal Mollen: {antwoorden.Spelers.Count(s => s.IsDeMol)}",
                    antwoorden.Spelers.Count(s => s.IsDeMol) == 1));
            }

            var finaleantwoordenData = Util.SafeReadJson<FinaleAntwoordenData>();

            Checks.Add(new CheckViewModel($"Finale: Aantal Antwoorden: {finaleantwoordenData.Spelers.Count}",
                finaleantwoordenData.Spelers.Count == container.GetInstance<ShellViewModel>().AantalSpelers));



            if (!Checks.All(c => c.IsOk))
            {
                CanUitslag = true;
            }
        }
    }
}