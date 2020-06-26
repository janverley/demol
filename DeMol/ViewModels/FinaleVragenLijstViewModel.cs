using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Caliburn.Micro;
using DeMol.Model;
using DeMol.Properties;

namespace DeMol.ViewModels
{
    public class FinaleVragenLijstViewModel : Conductor<object>.Collection.OneActive
    {
        private readonly SimpleContainer container;
        private readonly ShellViewModel conductor;

        private readonly List<OpdrachtData> gespeeldeOpdrachten;

        public FinaleVragenLijstViewModel(SimpleContainer container, ShellViewModel conductor)
        {
            this.container = container;
            this.conductor = conductor;

            // alle admindatas ophalen
            // alle gespeekde opdracthen in lijst samensteken

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
        
        public string Naam { get; set; }

        protected override void OnActivate()
        {
            Items.Clear();
            var opdrachtenCount = gespeeldeOpdrachten.Count();

            for (var i = 0; i < opdrachtenCount; i++)
            {
                var opdrachtData = gespeeldeOpdrachten[i];

                var antwoordendata = Util.SafeReadJson<AntwoordenData>(opdrachtData.Opdracht);

                var deMol = antwoordendata.Spelers.First(s => s.IsDeMol);
                if (Naam.SafeEqual(deMol.Naam))
                {
                    // niks over vragen
                    continue;
                }
                
                var wieisdemol = container.GetInstance<QuizWieIsDeMolViewModel>();
                wieisdemol.Naam = Naam;
                wieisdemol.OpdrachtData = opdrachtData;
                wieisdemol.DoNext = model =>
                {
                    var x =
                        Items.SkipWhile(item => !Equals(item, ActiveItem)).Skip(1)
                                .FirstOrDefault(y => y is FinaleQuizVragenViewModel) as
                            FinaleQuizVragenViewModel;

                    x.Message = $"Opdracht: {Util.OpdrachtUiNaam(opdrachtData)}";
                    
                    x.DeMolIs = model.DeMolIs;
                    ActivateItem(x);
                };

                Items.Add(wieisdemol);

                var vragen = container.GetInstance<FinaleQuizVragenViewModel>();

                var vragenCodes = VragenCodesFromGespeeldeOpdracht(opdrachtData);

                vragen.VragenCodes = vragenCodes;
                vragen.OpdrachtId = opdrachtData.Opdracht;
                vragen.Naam = Naam;
                vragen.DoNext = model2 =>
                {
                    var isLast = !Items.SkipWhile(item => !Equals(item, ActiveItem)).Skip(1).Any();

                    if (isLast)
                    {
                        var adminData = Util.SafeReadJson<AdminData>("finale");
                        adminData.HeeftQuizGespeeld.Add(new SpelerInfo {Naam = Naam});
                        Util.SafeAdminData(container, adminData);

                        var q = Parent as FinaleQuizViewModel;
                        q.StartSmoel();
                    }
                    else
                    {
                        ActivateItem(Items.SkipWhile(item => !Equals(item, ActiveItem)).Skip(1).FirstOrDefault());
                    }
                };
                Items.Add(vragen);
            }

            if (Items.Any())
            {
                ActivateItem(Items.First());
                
            }
            else
            {
                var x = container.GetInstance<MenuViewModel>();
                conductor.ActivateItem(x);
            }
            base.OnActivate();
        }

        // private void StartLoop(string naam, OpdrachtData gespeeldeOpdracht)
        // {
        //     var opdrachtId = gespeeldeOpdracht.Opdracht;
        //
        //     var op = opdrachtenData.FirstOrDefault(o => o.Opdracht.SafeEqual(opdrachtId));
        //
        //     var x2 = container.GetInstance<QuizBenJijDeMolViewModel>();
        //     x2.Naam = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(naam.ToLower());
        //     x2.OpdrachtData = op;
        //
        //     ActivateItem(x2);
        // }

        private static List<string> VragenCodesFromGespeeldeOpdracht(OpdrachtData gespeeldeOpdracht)
        {
            var vragenCodes = new List<string>();
            var opdrachtVragen = Util.SafeReadJson<OpdrachtData>(gespeeldeOpdracht.Opdracht);

            for (var i = vragenCodes.Count; i < Settings.Default.AantalVragenPerOdrachtFinale; i++)
            {
                var r = new Random().Next(opdrachtVragen.Vragen.Count);
                var x = Util.GetVraagAndCode(opdrachtVragen, r);
                
                if (!vragenCodes.Contains(x.Item1))
                {
                    vragenCodes.Add(x.Item1);
                }
                else
                {
                    i--;
                }
            }
            return vragenCodes;
        }

        // private void BeginVragenVoorMol(string naam,
        //                                 OpdrachtData gespeeldeOpdracht)
        // {
        //     var vragenCodes = VragenCodesFromGespeeldeOpdracht(gespeeldeOpdracht);
        //
        //     var x3 = container.GetInstance<QuizVragenViewModel>();
        //             
        //     x3.VragenCodes = vragenCodes;
        //     x3.OpdrachtId = gespeeldeOpdracht.Opdracht;
        //     x3.Naam = naam;
        //     x3.IsDeMol = true;
        //     x3.DeMolIs = naam;
        //             
        //     ActiveItem = x3;
        //     x3.Activate();
        // }
        //
        // private void BeginVragenVoorNietMol(object sender, DeactivationEventArgs e, string naam,
        //                                     OpdrachtData gespeeldeOpdracht)
        // {
        //     var vragenCodes = VragenCodesFromGespeeldeOpdracht(gespeeldeOpdracht);
        //
        //
        //     var ss = (QuizWieIsDeMolViewModel) sender;
        //     
        //
        //     var x3 = container.GetInstance<QuizVragenViewModel>();
        //             
        //     x3.VragenCodes = vragenCodes;
        //     x3.OpdrachtId = gespeeldeOpdracht.Opdracht;
        //     x3.Naam = naam;
        //     x3.IsDeMol = false;
        //     x3.DeMolIs = ss.Opties.Single(o => o.IsSelected).OptieText;
        //             
        //     ActivateItem(x3);
        //
        // }

        // public void OnQuizBenJijDeMolViewModelClose(string naam, OpdrachtData opdrachData, bool isDeMol)
        // {
        //     if (!isDeMol)
        //     {
        //         var x4 = container.GetInstance<QuizWieIsDeMolViewModel>();
        //         x4.OpdrachtData = opdrachData;
        //         x4.Naam = naam;
        //         ActivateItem(x4);
        //
        //         //x4.Deactivated += (sender, e) => BeginVragenVoorNietMol(sender, e, naam, opdrachData);
        //     }
        //         
        //     //BeginVragenVoorMol(naam, opdrachData);           
        // }
    }
}