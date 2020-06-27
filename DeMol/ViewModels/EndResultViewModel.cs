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
        private bool canUitslag;
        private readonly List<OpdrachtData> gespeeldeOpdrachten;

        private string text;

        private string winnaar;
        private Dictionary<string, int> aantalPasvragenPerNaam;
        private Dictionary<string, string> alleJuisteAntwoorden;
        private Dictionary<string, string> molPerOpdrachtId;
        private Dictionary<string, string> radersPerOpdrachtId;
        private Dictionary<string, int> aantalRadersPerOpdrachtId;
        private List<Scores> alleScores;
        private FinaleAntwoordenData finaleantwoordenData;

        public EndResultViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;

            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(5);

            alleScores = new List<Scores>();

            var gespeeldeOpdrachtenIds = new List<string>();

            aantalPasvragenPerNaam = new Dictionary<string, int>();
            
            foreach (var dag in container.GetInstance<ShellViewModel>().DagenData.Dagen)
            {
                var adminadata = Util.SafeReadJson<AdminData>(dag.Id);

                foreach (var gespeeldeOpdrachtData in adminadata.OpdrachtenGespeeld)
                {
                    gespeeldeOpdrachtenIds.Add(gespeeldeOpdrachtData.OpdrachtId);
                }

                foreach (var pasvragenVerdiend in adminadata.Pasvragen)
                {
                    var naam = pasvragenVerdiend.Naam;
                    if (aantalPasvragenPerNaam.ContainsKey(naam))
                    {
                        aantalPasvragenPerNaam[naam]+=pasvragenVerdiend.PasVragenVerdiend;
                    }
                    else
                    {
                        aantalPasvragenPerNaam.Add(naam, pasvragenVerdiend.PasVragenVerdiend);
                    }
                }
            }

            gespeeldeOpdrachten = Util.AlleOpdrachtData().Where(od => gespeeldeOpdrachtenIds.Any(i => i == od.Opdracht))
                .ToList();
            
            // alle juiste antwoorden
            alleJuisteAntwoorden = new Dictionary<string, string>();

            molPerOpdrachtId = new Dictionary<string, string>();
            radersPerOpdrachtId = new Dictionary<string, string>();
            aantalRadersPerOpdrachtId = new Dictionary<string, int>();
            
            
            foreach (var opdrachtData in gespeeldeOpdrachten)
            {
                var antwoorden = Util.SafeReadJson<AntwoordenData>(opdrachtData.Opdracht);
                var molNaam = antwoorden.Spelers.First(s => s.IsDeMol)?.Naam ?? "?";

                molPerOpdrachtId.Add(opdrachtData.Opdracht, molNaam);
                
                foreach (var juistAntwoord in antwoorden.Spelers.First(s => s.IsDeMol).Antwoorden)
                {
                    alleJuisteAntwoorden.Add(juistAntwoord.Key, juistAntwoord.Value);
                }
                
                aantalRadersPerOpdrachtId.Add(opdrachtData.Opdracht, antwoorden
                    .Spelers
                    .Where(s => !s.IsDeMol)
                    .Count(s => s.DeMolIs.SafeEqual(molNaam)));
                
                radersPerOpdrachtId.Add(opdrachtData.Opdracht, string.Join(", ", antwoorden.Spelers
                    .Where(s => !s.IsDeMol)
                    .Where(s => s.DeMolIs.SafeEqual(molNaam)).Select(s => s.Naam)));
            }

            finaleantwoordenData = Util.SafeReadJson<FinaleAntwoordenData>();
            
            foreach (var speler in container.GetInstance<ShellViewModel>().Spelerdata.Spelers)
            {
                var scores = new Scores();
                scores.Naam = speler.Naam;
                scores.totaleTijd = TimeSpan.Zero;
                
                // pasvragen tellen
                var pasvragenVerdiend = aantalPasvragenPerNaam.Any(kvp => kvp.Key.SafeEqual(speler.Naam))
                    ? aantalPasvragenPerNaam.Single(kvp => kvp.Key.SafeEqual(speler.Naam)).Value
                    : 0;

                scores.aantalPasVragenVerdiend = pasvragenVerdiend;
                
                foreach (var opdrachtData in gespeeldeOpdrachten)
                {
                    var antwoorden = Util.SafeReadJson<AntwoordenData>(opdrachtData.Opdracht);
                    var a = antwoorden.Spelers.First(s => s.Naam.SafeEqual(speler.Naam));

                    scores.totaleTijd += a.Tijd;
                    
                    if (a.IsDeMol)
                    {
                        scores.aantalKeerMolGeweest++;
                        continue;
                    }

                    foreach (var antwoord in a.Antwoorden)
                    {
                        scores.aantalVragenBeantwoord++;
                        var juistAntwoord = alleJuisteAntwoorden[antwoord.Key];

                        if (antwoord.Value.SafeEqual(juistAntwoord))
                        {
                            scores.aantalVragenJuistBeantwoord++;
                        }
                    }
                }


                var finaleAntwoorden = finaleantwoordenData.Spelers.Single(s => s.Naam.SafeEqual(speler.Naam));

                scores.totaleTijd += finaleAntwoorden.Tijd;
                
                foreach (var demolIsPerOpdrachtId in finaleAntwoorden.DeMolIsPerOpdrachtId)
                {
                    scores.finaleAantalVragenBeantwoord++;
                    
                    if (molPerOpdrachtId[demolIsPerOpdrachtId.Key].SafeEqual(demolIsPerOpdrachtId.Value))
                    {
                        scores.finaleAantalVragenJuistBeantwoord++;
                    }
                }

                foreach (var antwoordenPerVraagId in finaleAntwoorden.AntwoordenPerVraagId)
                {
                    scores.finaleAantalVragenBeantwoord++;

                    if (alleJuisteAntwoorden[antwoordenPerVraagId.Key].SafeEqual(antwoordenPerVraagId.Value))
                    {
                        scores.finaleAantalVragenJuistBeantwoord++;
                    }
                }
                
                
                // percetnage berekene

                var x = Math.Min(scores.aantalVragenJuistBeantwoord + scores.aantalPasVragenVerdiend, scores.aantalVragenBeantwoord);
                scores.percentage = x / scores.aantalVragenBeantwoord;
                scores.finalePercentage = scores.finaleAantalVragenJuistBeantwoord / scores.finaleAantalVragenBeantwoord;

                scores.totaalPercentage = (scores.percentage + scores.finalePercentage) / 2;
                
                alleScores.Add(scores);
            }

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
                var molNaam = molPerOpdrachtId[opdrachtData.Opdracht];
                var raders = radersPerOpdrachtId[opdrachtData.Opdracht];
                sb.AppendLine(
                    $"Opdracht: {Util.OpdrachtUiNaam(opdrachtData)}: De mol was {molNaam}, geraden door: {raders}");
            }

            sb.AppendLine("Punten:");

            foreach (var scores in alleScores. OrderByDescending(s => s.totaalPercentage).ThenBy(s => s.totaleTijd))
            {
                sb.AppendLine(
                    $"{scores.Naam}: ( {scores.aantalVragenJuistBeantwoord} + {scores.aantalPasVragenVerdiend} pasvragen) / {scores.aantalVragenBeantwoord} - {scores.aantalKeerMolGeweest} mol geweest");
                sb.AppendLine(
                    $"{scores.Naam}: Finale: {scores.finaleAantalVragenJuistBeantwoord} / {scores.finaleAantalVragenBeantwoord}");

                sb.AppendLine(
                    $"{scores.Naam}: Totaal: {scores.percentage:P} + {scores.finalePercentage:P} = {scores.totaalPercentage:P}");
                sb.AppendLine(
                    $"{scores.Naam}: Totaal: {scores.totaleTijd:g}");
                
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

                Checks.Add(new CheckViewModel(
                    $"Opdracht {Util.OpdrachtUiNaam(opdrachtData)}: Aantal Antwoorden: {antwoorden.Spelers.Count}",
                    antwoorden.Spelers.Count == container.GetInstance<ShellViewModel>().AantalSpelers));
                Checks.Add(new CheckViewModel(
                    $"Opdracht {Util.OpdrachtUiNaam(opdrachtData)}: Aantal Mollen: {antwoorden.Spelers.Count(s => s.IsDeMol)}",
                    antwoorden.Spelers.Count(s => s.IsDeMol) == 1));
            }

            Checks.Add(new CheckViewModel($"Finale: Aantal Antwoorden: {finaleantwoordenData.Spelers.Count}",
                finaleantwoordenData.Spelers.Count == container.GetInstance<ShellViewModel>().AantalSpelers));


            if (!Checks.All(c => c.IsOk))
            {
                CanUitslag = true;
            }
        }


        private struct Scores
        {
            public string Naam;
            public decimal aantalVragenBeantwoord;
            public int aantalVragenJuistBeantwoord;
            public int aantalPasVragenVerdiend;

            public decimal percentage;

            
            public decimal finaleAantalVragenBeantwoord;
            public int finaleAantalVragenJuistBeantwoord;

            public decimal finalePercentage;
            public decimal totaalPercentage;

            public int aantalKeerMolGeweest;
            
            public TimeSpan totaleTijd;

        }
    }
}