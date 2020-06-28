using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using DeMol.Model;
using DeMol.Properties;

namespace DeMol.ViewModels
{
    public class FinaleVragenLijstViewModel : Conductor<object>.Collection.OneActive
    {
        private readonly ShellViewModel conductor;
        private readonly SimpleContainer container;

        private readonly List<OpdrachtData> gespeeldeOpdrachten;

        public FinaleVragenLijstViewModel(SimpleContainer container, ShellViewModel conductor)
        {
            this.container = container;
            this.conductor = conductor;

            var gespeeldeOpdrachtenIds = new List<string>();

            foreach (var dag in container.GetInstance<ShellViewModel>().DagenData.Dagen)
            {
                var adminadata = Util.SafeReadJson<AdminData>(dag.Id);

                foreach (var gespeeldeOpdrachtData in adminadata.OpdrachtenGespeeld)
                {
                    gespeeldeOpdrachtenIds.Add(gespeeldeOpdrachtData.OpdrachtId);
                }
            }

            gespeeldeOpdrachten = Util.AlleOpdrachtData().Where(od => gespeeldeOpdrachtenIds.Any(i => i == od.Opdracht))
                .ToList();
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
    }
}