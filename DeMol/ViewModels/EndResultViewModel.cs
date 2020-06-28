using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using Caliburn.Micro;
using DeMol.Model;
using DeMol.Properties;

namespace DeMol.ViewModels
{
    public class EndResultViewModel : Screen
    {
        private readonly Dictionary<string, int> aantalPasvragenPerNaam;
        private readonly Dictionary<string, int> aantalRadersPerOpdrachtId;
        private readonly Dictionary<string, AntwoordenData> alleAntwoordenPerOpdrachtId;
        private readonly Dictionary<string, string> alleJuisteAntwoorden;
        private readonly List<Scores> alleScores;
        private readonly ShellViewModel conductor;
        private readonly SimpleContainer container;
        private readonly FinaleAntwoordenData finaleantwoordenData;
        private readonly List<OpdrachtData> gespeeldeOpdrachten;
        private readonly int groepspot;
        private readonly Dictionary<string, string> molPerOpdrachtId;
        private readonly Dictionary<string, string> radersPerOpdrachtId;

        private readonly DispatcherTimer timer = new DispatcherTimer();
        private bool canAntwoorden;
        private bool canUitslag;
        private readonly int maxTeVerdienen;

        private string text;

        private string winnaar;

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
                Checks.Add(new CheckViewModel($"Administratie saved {dag.Naam} :",
                    Util.DataFileFoundAndValid<AdminData>(dag.Id)));


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
                        aantalPasvragenPerNaam[naam] += pasvragenVerdiend.PasVragenVerdiend;
                    }
                    else
                    {
                        aantalPasvragenPerNaam.Add(naam, pasvragenVerdiend.PasVragenVerdiend);
                    }
                }
            }

            gespeeldeOpdrachten = Util.AlleOpdrachtData().Where(od => gespeeldeOpdrachtenIds.Any(i => i == od.Opdracht))
                .ToList();

            alleAntwoordenPerOpdrachtId = new Dictionary<string, AntwoordenData>();
            alleJuisteAntwoorden = new Dictionary<string, string>();
            molPerOpdrachtId = new Dictionary<string, string>();
            radersPerOpdrachtId = new Dictionary<string, string>();
            aantalRadersPerOpdrachtId = new Dictionary<string, int>();

            groepspot = 0;
            maxTeVerdienen = 0;
            foreach (var opdrachtData in gespeeldeOpdrachten)
            {
                var antwoordengevonden = Util.DataFileFoundAndValid<AntwoordenData>(opdrachtData.Opdracht);

                Checks.Add(new CheckViewModel($"Antwoorden saved {Util.OpdrachtUiNaam(opdrachtData)} :",
                    antwoordengevonden));

                if (antwoordengevonden)
                {
                    var antwoorden = Util.SafeReadJson<AntwoordenData>(opdrachtData.Opdracht);
                    alleAntwoordenPerOpdrachtId.Add(opdrachtData.Opdracht, antwoorden);

                    Checks.Add(new CheckViewModel(
                        $"Opdracht {Util.OpdrachtUiNaam(opdrachtData)}: Aantal Antwoorden: {antwoorden.Spelers.Count}",
                        antwoorden.Spelers.Count == container.GetInstance<ShellViewModel>().AantalSpelers));
                    Checks.Add(new CheckViewModel(
                        $"Opdracht {Util.OpdrachtUiNaam(opdrachtData)}: Aantal Mollen: {antwoorden.Spelers.Count(s => s.IsDeMol)}",
                        antwoorden.Spelers.Count(s => s.IsDeMol) == 1));
                    Checks.Add(new CheckViewModel($"Dubbel geantwoord in opdracht {Util.OpdrachtUiNaam(opdrachtData)}:",
                        Util.CheckForDoubles(antwoorden.Spelers)));

                    groepspot += antwoorden.EffectiefVerdiend;
                    maxTeVerdienen += antwoorden.MaxTeVerdienen;

                    var molNaam = antwoorden.Spelers.First(s => s.IsDeMol)?.Naam ?? "?";

                    molPerOpdrachtId.Add(opdrachtData.Opdracht, molNaam);

                    foreach (var juistAntwoord in antwoorden.Spelers.First(s => s.IsDeMol).Antwoorden)
                    {
                        alleJuisteAntwoorden.Add(juistAntwoord.Key, juistAntwoord.Value);
                    }

                    var aantalRaders = antwoorden
                        .Spelers
                        .Where(s => !s.IsDeMol)
                        .Count(s => s.DeMolIs.SafeEqual(molNaam));
                    aantalRadersPerOpdrachtId.Add(opdrachtData.Opdracht, aantalRaders);

                    var radersLijst = string.Join(", ", antwoorden.Spelers
                        .Where(s => !s.IsDeMol)
                        .Where(s => s.DeMolIs.SafeEqual(molNaam)).Select(s => s.Naam));
                    radersPerOpdrachtId.Add(opdrachtData.Opdracht, radersLijst);
                }
            }

            finaleantwoordenData = Util.SafeReadJson<FinaleAntwoordenData>();

            var finaleDoorIedereenGespeeld = finaleantwoordenData.Spelers.Count ==
                                             container.GetInstance<ShellViewModel>().AantalSpelers;
            Checks.Add(new CheckViewModel($"Finale: Aantal Antwoorden: {finaleantwoordenData.Spelers.Count}",
                finaleDoorIedereenGespeeld));

            if (finaleDoorIedereenGespeeld)
            {
                foreach (var speler in container.GetInstance<ShellViewModel>().Spelerdata.Spelers)
                {
                    var scores = new Scores();
                    scores.Naam = speler.Naam;
                    scores.totaleTijd = TimeSpan.Zero;

                    var pasvragenVerdiend = aantalPasvragenPerNaam.Any(kvp => kvp.Key.SafeEqual(speler.Naam))
                        ? aantalPasvragenPerNaam.Single(kvp => kvp.Key.SafeEqual(speler.Naam)).Value
                        : 0;

                    scores.aantalPasVragenVerdiend = pasvragenVerdiend;

                    foreach (var opdrachtData in gespeeldeOpdrachten)
                    {
                        if (alleAntwoordenPerOpdrachtId.ContainsKey(opdrachtData.Opdracht))
                        {
                            var antwoorden = alleAntwoordenPerOpdrachtId[opdrachtData.Opdracht];

                            var a = antwoorden.Spelers.FirstOrDefault(s => s.Naam.SafeEqual(speler.Naam));

                            if (a != null)
                            {
                                scores.totaleTijd += a.Tijd;

                                if (a.IsDeMol)
                                {
                                    if (aantalRadersPerOpdrachtId[opdrachtData.Opdracht] <
                                        Settings.Default.AantalSpelersDieDeMolMoetenGeradenHebben)
                                    {
                                        scores.verdiendAlsMol +=
                                            antwoorden.MaxTeVerdienen - antwoorden.EffectiefVerdiend;
                                    }

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

                    var x = Math.Min(scores.aantalVragenJuistBeantwoord + scores.aantalPasVragenVerdiend,
                        scores.aantalVragenBeantwoord);
                    scores.percentage = x / scores.aantalVragenBeantwoord;
                    scores.finalePercentage =
                        scores.finaleAantalVragenJuistBeantwoord / scores.finaleAantalVragenBeantwoord;

                    scores.totaalPercentage = (scores.percentage + scores.finalePercentage) / 2;

                    alleScores.Add(scores);
                }


                Util.SafeFileWithBackup(new ScoresData
                {
                    MaxTeVerdienen = maxTeVerdienen,
                    GroepsPot = groepspot,
                    Scores = alleScores.OrderByDescending(s => s.totaalPercentage).ThenBy(s => s.totaleTijd).ToList()
                });
            }
        }


        public bool ShowChecks { get; private set; }

        public bool CanUitslag
        {
            get => canUitslag;
            set => Set(ref canUitslag, value);
        }

        public bool CanAntwoorden
        {
            get => canAntwoorden;
            set => Set(ref canAntwoorden, value);
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


            var scores = alleScores.OrderByDescending(s => s.totaalPercentage).ThenBy(s => s.totaleTijd).First();

            Winnaar = scores.Naam;

            var sb = new StringBuilder();

            sb.AppendLine($"- behaalde in totaal {scores.totaalPercentage:P},");
            sb.AppendLine(
                $"- beantwoorde in de week {scores.aantalVragenJuistBeantwoord}/{scores.aantalVragenBeantwoord} vragen juist");
            sb.AppendLine($"- won {scores.aantalPasVragenVerdiend} pasvragen");
            sb.AppendLine(
                $"- beantwoorde in de finale {scores.finaleAantalVragenJuistBeantwoord}/{scores.finaleAantalVragenBeantwoord} vragen juist");
            sb.AppendLine(
                $"- op een totale tijd van {scores.totaleTijd.Hours} uur, {scores.totaleTijd.Minutes} minuten en {scores.totaleTijd.Seconds} seconden");
            sb.AppendLine();
            sb.AppendLine(
                $"- was bij {scores.aantalKeerMolGeweest} opdrachten de mol en verdiende daarmee {scores.verdiendAlsMol.ToString("C0", CultureInfo.GetCultureInfo("nl-be"))}");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine($"De Groepspot bevat {groepspot.ToString("C0", CultureInfo.GetCultureInfo("nl-be"))}");

            sb.AppendLine();
            sb.AppendLine("Proficiat!");


            Text = sb.ToString();
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

            sb.AppendLine($"GroepsPot: {groepspot.ToString("C0", CultureInfo.GetCultureInfo("nl-be"))}");
            sb.AppendLine($"Max te verdienen: {maxTeVerdienen.ToString("C0", CultureInfo.GetCultureInfo("nl-be"))}");

            sb.AppendLine();
            sb.AppendLine("Mollen:");

            foreach (var opdrachtData in gespeeldeOpdrachten)
            {
                sb.AppendLine(
                    $"\tOpdracht: {Util.OpdrachtUiNaam(opdrachtData)}:");

                var molNaam = molPerOpdrachtId[opdrachtData.Opdracht];
                var raders = radersPerOpdrachtId[opdrachtData.Opdracht];
                var molIsNIETGeraden = aantalRadersPerOpdrachtId[opdrachtData.Opdracht] <
                                       Settings.Default.AantalSpelersDieDeMolMoetenGeradenHebben;
                var voordeMol = !molIsNIETGeraden
                    ? "Niks"
                    : (alleAntwoordenPerOpdrachtId[opdrachtData.Opdracht].MaxTeVerdienen -
                       alleAntwoordenPerOpdrachtId[opdrachtData.Opdracht].EffectiefVerdiend)
                    .ToString("C0", CultureInfo.GetCultureInfo("nl-be"));

                sb.AppendLine(
                    $"\t\tDe mol was {molNaam}, geraden door: {raders} => {(molIsNIETGeraden ? "Goed gedaan mol!" : "Meup!")}");
                sb.AppendLine(
                    $"\t\tDit levert de mol {molNaam} {voordeMol} op");
            }

            sb.AppendLine();
            sb.AppendLine("Punten:");

            foreach (var scores in alleScores.OrderByDescending(s => s.totaalPercentage).ThenBy(s => s.totaleTijd))
            {
                sb.AppendLine(scores.Naam);

                sb.AppendLine(
                    $"\tIn de week: ({scores.aantalVragenJuistBeantwoord} + {scores.aantalPasVragenVerdiend} pasvragen) / {scores.aantalVragenBeantwoord} -> {scores.percentage:P}");
                sb.AppendLine(
                    $"\tFinale: {scores.finaleAantalVragenJuistBeantwoord} / {scores.finaleAantalVragenBeantwoord} -> {scores.finalePercentage:P}");

                sb.AppendLine(
                    $"\tTotaal: ({scores.percentage:P} + {scores.finalePercentage:P})/2 = {scores.totaalPercentage:P}");
                sb.AppendLine(
                    $"\tTotale Tijd: {scores.totaleTijd:g}");
                sb.AppendLine(
                    $"\t{scores.aantalKeerMolGeweest} keer mol geweest");
                sb.AppendLine(
                    $"\tVerdiend als Mol: {scores.verdiendAlsMol.ToString("C0", CultureInfo.GetCultureInfo("nl-be"))}");
            }

            Text = sb.ToString();
        }

        public void Uitslag()
        {
            Text = "De einduitslag van De Mol...";

            timer.Start();
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            if (Checks.All(c => c.IsOk))
            {
                Text = "Alles Ok!";

                ShowChecks = false;
                CanUitslag = true;
                CanAntwoorden = true;
            }
            else
            {
                ShowChecks = true;
            }
        }
    }
}