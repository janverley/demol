using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Caliburn.Micro;
using DeMol.Model;
using DeMol.Properties;

namespace DeMol.ViewModels
{
    public class EndResultViewModel : Screen
    {
        private readonly Dictionary<string, int> aantalPasvragenPerNaam;
        private readonly Dictionary<string, int> aantalMolJuistGeradenPerNaam;
        private readonly Dictionary<string, int> aantalMolletjeJuistGeradenPerNaam;
        private readonly Dictionary<string, int> aantalRadersPerOpdrachtId;
        private readonly Dictionary<string, AntwoordenData> alleAntwoordenPerOpdrachtId;
        private readonly Dictionary<string, string> alleJuisteAntwoorden;
        private readonly List<Scores> alleScores;
        private readonly ShellViewModel conductor;
        private readonly SimpleContainer container;
        //private readonly FinaleAntwoordenData finaleantwoordenData;
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
            aantalMolJuistGeradenPerNaam = new Dictionary<string, int>();
            aantalMolletjeJuistGeradenPerNaam = new Dictionary<string, int>();

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

                    var aantalmoljuist =
                        (pasvragenVerdiend.Dag1MolOk ? 1 : 0)
                        + (pasvragenVerdiend.Dag2MolOk ? 1 : 0)
                        + (pasvragenVerdiend.Dag3MolOk ? 1 : 0)
                        + (pasvragenVerdiend.Dag4MolOk ? 1 : 0)
                        + (pasvragenVerdiend.Dag5MolOk ? 1 : 0);

                    var aantalmolletjejuist =
                        (pasvragenVerdiend.Dag1MolletjeOk ? 1 : 0)
                        + (pasvragenVerdiend.Dag2MolletjeOk ? 1 : 0)
                        + (pasvragenVerdiend.Dag3MolletjeOk ? 1 : 0)
                        + (pasvragenVerdiend.Dag4MolletjeOk ? 1 : 0)
                        + (pasvragenVerdiend.Dag5MolletjeOk ? 1 : 0);

                    if (aantalMolJuistGeradenPerNaam.ContainsKey(naam))
                    {
                        aantalMolJuistGeradenPerNaam[naam] += aantalmoljuist;
                    }
                    else
                    {
                        aantalMolJuistGeradenPerNaam.Add(naam, aantalmoljuist);
                    }

                    if (aantalMolletjeJuistGeradenPerNaam.ContainsKey(naam))
                    {
                        aantalMolletjeJuistGeradenPerNaam[naam] += aantalmolletjejuist;
                    }
                    else
                    {
                        aantalMolletjeJuistGeradenPerNaam.Add(naam, aantalmolletjejuist);
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

                    if (Checks.Any(c => !c.IsOk))
                    {
                        continue;
                    }
                    
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

            if (Checks.Any(c => !c.IsOk))
            {
                return;
            }
            
            {
                foreach (var speler in container.GetInstance<ShellViewModel>().Spelerdata.Spelers)
                {
                    var scores = new Scores();
                    scores.Naam = speler.Naam;
                    scores.totaleTijd = TimeSpan.Zero;

                    //scores.aantalVragenBeantwoord = 10;
                    
                    var pasvragenVerdiend = aantalPasvragenPerNaam.Any(kvp => kvp.Key.SafeEqual(speler.Naam))
                        ? aantalPasvragenPerNaam.Single(kvp => kvp.Key.SafeEqual(speler.Naam)).Value
                        : 0;

                    scores.aantalPasVragenVerdiend = pasvragenVerdiend;

                    
                    var aantalMoljuist = aantalMolJuistGeradenPerNaam.Any(kvp => kvp.Key.SafeEqual(speler.Naam))
                        ? aantalMolJuistGeradenPerNaam.Single(kvp => kvp.Key.SafeEqual(speler.Naam)).Value
                        : 0;

                    scores.aantalKeerMolJuistGeraden = aantalMoljuist; 
                    
                    var aantalMolletjeJuist = aantalMolletjeJuistGeradenPerNaam.Any(kvp => kvp.Key.SafeEqual(speler.Naam))
                    ? aantalMolletjeJuistGeradenPerNaam.Single(kvp => kvp.Key.SafeEqual(speler.Naam)).Value
                    : 0;

                    scores.aantalKeerMolletjeJuistGeraden = aantalMolletjeJuist;
                        
                    foreach (var opdrachtData in gespeeldeOpdrachten)
                    {
                        if (alleAntwoordenPerOpdrachtId.ContainsKey(opdrachtData.Opdracht))
                        {
                            var antwoorden = alleAntwoordenPerOpdrachtId[opdrachtData.Opdracht];

                            var spelerDieAntwoord = antwoorden.Spelers.FirstOrDefault(s => s.Naam.SafeEqual(speler.Naam));

                            var molNaam = antwoorden.Spelers.First(s => s.IsDeMol)?.Naam ?? "?";
                            
                            //if (spelerDieAntwoord != null)
                            {
                                scores.totaleTijd += spelerDieAntwoord.Tijd;

                                if (spelerDieAntwoord.IsDeMol)
                                {
                                    continue;
                                }

                                //scores.aantalVragenBeantwoord+=2;
                                if (spelerDieAntwoord.DeMolIs.SafeEqual(molNaam))
                                {
                                    scores.WistWieDeMolWas(opdrachtData.Opdracht);
                                    //scores.aantalVragenJuistBeantwoord+=2;
                                }

                                foreach (var antwoord in spelerDieAntwoord.Antwoorden)
                                {
                                    scores.aantalVragenBeantwoord++;
                                    var juistAntwoord = 
                                        alleJuisteAntwoorden.ContainsKey(antwoord.Key) ? alleJuisteAntwoorden[antwoord.Key] : "?";

                                    if (antwoord.Value.SafeEqual(juistAntwoord))
                                    {
                                        scores.aantalVragenJuistBeantwoord++;
                                    }
                                }
                            }
                        }
                    }


                    var x = Math.Min(
                        scores.aantalVragenJuistBeantwoord + 
                        scores.aantalPasVragenVerdiend +
                        scores.aantalKeerMolJuistGeraden + 
                        scores.aantalKeerMolletjeJuistGeraden, 
                        scores.aantalVragenBeantwoord);
                    scores.percentage = (scores.aantalVragenBeantwoord > 0) ? x / scores.aantalVragenBeantwoord : 0m;
                    scores.totaalPercentage = scores.percentage;

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
                $"- beantwoorde {scores.aantalVragenJuistBeantwoord}/{scores.aantalVragenBeantwoord} vragen juist");

            if (scores.MolGeraden)
            {
                sb.AppendLine($"- Wist wie De Mol was");
            }
            if (scores.MolletjeGeraden)
            {
                sb.AppendLine($"- Wist wie Het Molletje was");
            }
            
            sb.AppendLine($"- won {scores.aantalPasVragenVerdiend} pasvragen");
            sb.AppendLine(
                $"- raadde {scores.aantalKeerMolJuistGeraden} keer De Mol juist");
            sb.AppendLine(
                $"- raadde {scores.aantalKeerMolletjeJuistGeraden} keer Het Molletje juist");
            sb.AppendLine(
                $"- op een totale tijd van {scores.totaleTijd.Hours} uur, {scores.totaleTijd.Minutes} minuten en {scores.totaleTijd.Seconds} seconden");

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
                    $"\t\tDe Mol was {molNaam}, geraden door: {raders} => {(molIsNIETGeraden ? "Goed gedaan Mol!" : $":( Geraden door meer dan {Settings.Default.AantalSpelersDieDeMolMoetenGeradenHebben} spelers")}");
            }

            sb.AppendLine();
            sb.AppendLine("Punten:");

            foreach (var scores in alleScores.OrderByDescending(s => s.totaalPercentage).ThenBy(s => s.totaleTijd))
            {
                sb.AppendLine(scores.Naam);
                if (scores.MolGeraden)
                {
                    sb.AppendLine($"\tWist wie De Mol was");
                }
                if (scores.MolletjeGeraden)
                {
                    sb.AppendLine($"\tWist wie Het Molletje was");
                }

                sb.AppendLine(
                    $"\t( Dagen De Mol juist geraden: {scores.aantalKeerMolJuistGeraden} +");
                sb.AppendLine(
                    $"\tDagen Het Molletje juist geraden: {scores.aantalKeerMolletjeJuistGeraden} +");
                sb.AppendLine(
                    $"\tJuiste antwoorden: {scores.aantalVragenJuistBeantwoord} + ");
                sb.AppendLine(
                    $"\tPasvragen: {scores.aantalPasVragenVerdiend} ) / {scores.aantalVragenBeantwoord} -> {scores.percentage:P}");
                sb.AppendLine(
                    $"\tTotale Tijd: {scores.totaleTijd:g}");
            }

            Text = sb.ToString();
        }

        public void Uitslag()
        {
            Winnaar = "";
            Text = "De einduitslag van De Mol...";
            
            timer.Start();
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            if (Checks.All(c => c.IsOk))
            {
                Text = "";

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