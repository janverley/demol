using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using DeMol.Model;

namespace DeMol.ViewModels
{
    public class VragenLijstViewModel : Conductor<object>.Collection.OneActive
    {
        private readonly SimpleContainer container;

        private readonly List<OpdrachtData> gespeeldeOpdrachten;

        public VragenLijstViewModel(SimpleContainer container)
        {
            this.container = container;


            var adminData = Util.GetAdminDataOfSelectedDag(container);

            var opdrachtenData = Util.AlleOpdrachtData();

            gespeeldeOpdrachten = opdrachtenData
                .Where(od => adminData.OpdrachtenGespeeld
                    .Any(og => og.OpdrachtId == od.Opdracht))
                .ToList();
        }


        public bool CanPrevious => true;
        public bool CanNext => true;
        public string Naam { get; set; }

        protected override void OnActivate()
        {
            Items.Clear();
            var opdrachtenCount = gespeeldeOpdrachten.Count();

            for (var i = 0; i < opdrachtenCount; i++)
            {
                var opdrachtData = gespeeldeOpdrachten[i];

                var jijmol = container.GetInstance<QuizBenJijDeMolViewModel>();
                jijmol.Naam = Naam;
                jijmol.OpdrachtData = opdrachtData;
                jijmol.DoNext = model =>
                {
                    if (model.IsDeMol)
                    {
                        var x =
                            Items.SkipWhile(item => !Equals(item, ActiveItem)).Skip(1)
                                    .FirstOrDefault(y => y is QuizVragenViewModel) as
                                QuizVragenViewModel;
                        x.IsDeMol = true;
                        ActivateItem(x);
                    }
                    else
                    {
                        var x =
                            Items.SkipWhile(item => !Equals(item, ActiveItem)).Skip(1)
                                    .FirstOrDefault(y => y is QuizWieIsDeMolViewModel) as
                                QuizWieIsDeMolViewModel;
                        ActivateItem(x);
                    }
                };
                Items.Add(jijmol);

                var wieisdemol = container.GetInstance<QuizWieIsDeMolViewModel>();
                wieisdemol.Naam = Naam;
                wieisdemol.OpdrachtData = opdrachtData;
                wieisdemol.DoNext = model =>
                {
                    var x =
                        Items.SkipWhile(item => !Equals(item, ActiveItem)).Skip(1)
                                .FirstOrDefault(y => y is QuizVragenViewModel) as
                            QuizVragenViewModel;
                    x.IsDeMol = false;
                    x.DeMolIs = model.DeMolIs;
                    ActivateItem(x);
                };

                Items.Add(wieisdemol);

                var vragen = container.GetInstance<QuizVragenViewModel>();

                var vragenCodes = VragenCodesFromGespeeldeOpdracht(opdrachtData);

                vragen.VragenCodes = vragenCodes;
                vragen.OpdrachtId = opdrachtData.Opdracht;
                vragen.Naam = Naam;
                vragen.DoNext = model2 =>
                {
                    var isLast = !Items.SkipWhile(item => !Equals(item, ActiveItem)).Skip(1).Any();

                    if (isLast)
                    {
                        var adminData = Util.GetAdminDataOfSelectedDag(container);
                        adminData.HeeftQuizGespeeld.Add(new SpelerInfo {Naam = Naam});
                        Util.SafeAdminData(container, adminData);


                        var q = Parent as QuizViewModel;
                        q.StartSmoel();
                    }
                    else
                    {
                        ActivateItem(Items.SkipWhile(item => !Equals(item, ActiveItem)).Skip(1).FirstOrDefault());
                    }
                };
                Items.Add(vragen);
            }

            if (!Items.Any())
            {
                var boodschap = container.GetInstance<BoodschapViewModel>();
                boodschap.Text = "Er zijn geen opdrachten gespeeld vandaag.";
                
                Items.Add(boodschap);
            }
            
            ActivateItem(Items.First());
            base.OnActivate();
        }


        private static List<string> VragenCodesFromGespeeldeOpdracht(OpdrachtData gespeeldeOpdracht)
        {
            var vragenCodes = new List<string>();

            var opdrachtVragen = Util.SafeReadJson<OpdrachtData>(gespeeldeOpdracht.Opdracht);

            for (var i = 0; i < opdrachtVragen.Vragen.Count; i++)
            {
                var x = Util.GetVraagAndCode(opdrachtVragen, i);
                vragenCodes.Add(x.Item1);
            }

            // var extraVragen = Util.SafeReadJson<OpdrachtData>("x");
            // for (var i = vragenCodes.Count; i < Settings.Default.aantalVragenPerDag; i++)
            // {
            //     var r = new Random().Next(extraVragen.Vragen.Count);
            //     var x = Util.GetVraagAndCode(extraVragen, r);
            //
            //     if (!vragenCodes.Contains(x.Item1))
            //     {
            //         vragenCodes.Add(x.Item1);
            //     }
            //     else
            //     {
            //         i--;
            //     }
            // }

            return vragenCodes;
        }
    }
}