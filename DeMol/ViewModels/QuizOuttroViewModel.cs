using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeMol.ViewModels
{
    public class QuizOuttroViewModel : Screen
    {
        private readonly ShellViewModel conductor;
        private readonly SimpleContainer container;

        public QuizOuttroViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;
        }

        private string naam;

        public string Naam
        {
            get { return naam; }
            set
            {
                if (Set(ref naam, value))
                {
                    NotifyOfPropertyChange(() => Message);
                }
            }
        }

        public string Message => $"{Naam}, je antwoorden zijn genoteerd.\n\nJe krijgt nu te zien of jij morgen de mol bent.";


        public void Next()
        {
            var dagIdMorgen = container.GetInstance<ShellViewModel>().Dag + 1;
            var dagenData = container.GetInstance<ShellViewModel>().DagenData;

            var morgen = dagenData.Dagen.Single(d => d.Id == dagIdMorgen);

            var jijBentDeMolViewModel = container.GetInstance<JijBentDeMolViewModel>();
            jijBentDeMolViewModel.Dag = new DagViewModel(morgen.Id, morgen.Naam);
            jijBentDeMolViewModel.IsMorgen = true;
            jijBentDeMolViewModel.Naam = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Naam.ToLower());

            jijBentDeMolViewModel.DoNext = (_) =>
            {
                var quizNaamViewModel = container.GetInstance<QuizNaamViewModel>();
                quizNaamViewModel.DoNext = (vm) =>
                {
                    var quizBenJijDeMolViewModel = container.GetInstance<QuizBenJijDeMolViewModel>();
                    quizBenJijDeMolViewModel.Naam = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(vm.Naam.ToLower());
                    conductor.ActivateItem(quizBenJijDeMolViewModel);
                };
                conductor.ActivateItem(quizNaamViewModel);
            };
            conductor.ActivateItem(jijBentDeMolViewModel);


        }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (e?.Key == Key.Enter)
            {
                Next();
            }
        }
    }
}
