// using System.Globalization;
// using System.Linq;
// using System.Windows.Input;
// using Caliburn.Micro;
// using DeMol.Model;
//
// namespace DeMol.ViewModels
// {
//     public class QuizOuttroViewModel : Screen
//     {
//         private readonly ShellViewModel conductor;
//         private readonly SimpleContainer container;
//
//         private string naam;
//
//         public QuizOuttroViewModel(ShellViewModel conductor, SimpleContainer container)
//         {
//             this.conductor = conductor;
//             this.container = container;
//         }
//
//         public string Naam
//         {
//             get => naam;
//             set
//             {
//                 if (Set(ref naam, value))
//                 {
//                     NotifyOfPropertyChange(() => Message);
//                 }
//             }
//         }
//
//         public string Message => GetErIsEenMorgen()
//             ? $"{Naam}, je antwoorden zijn genoteerd.\n\nJe krijgt nu te zien of jij morgen de mol bent."
//             : $"{Naam}, je antwoorden zijn genoteerd.";
//
//         private bool GetErIsEenMorgen()
//         {
//             var dagIdMorgen = container.GetInstance<ShellViewModel>().Dag + 1;
//             var dagenData = container.GetInstance<ShellViewModel>().DagenData;
//
//             var erIsEenMorgen = dagenData.Dagen.Any(d => d.Id == dagIdMorgen);
//
//             return erIsEenMorgen;
//         }
//
//         public void Next()
//         {
//             var smoelenViewModel = container.GetInstance<SmoelenViewModel>();
//
//             smoelenViewModel.CanSelectUserDelegate = name =>
//             {
//                 var antwoorden = Util.SafeReadJson<AntwoordenData>(container.GetInstance<ShellViewModel>().Dag);
//                 var result = !antwoorden.Spelers.Any(s => s.Naam.SafeEqual(name));
//                 return result;
//             };
//
//             smoelenViewModel.DoNext = vm =>
//             {
//                 var quizBenJijDeMolViewModel = container.GetInstance<QuizIntroViewModel>();
//                 quizBenJijDeMolViewModel.Naam = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(vm.Naam.ToLower());
//                 conductor.ActivateItem(quizBenJijDeMolViewModel);
//             };
//
//             if (GetErIsEenMorgen())
//             {
//                 var dagIdMorgen = container.GetInstance<ShellViewModel>().Dag + 1;
//                 var dagenData = container.GetInstance<ShellViewModel>().DagenData;
//
//                 var morgen = dagenData.Dagen.Single(d => d.Id == dagIdMorgen);
//
//                 var jijBentDeMolViewModel = container.GetInstance<JijBentDeMolViewModel>();
//                 jijBentDeMolViewModel.Dag = new DagViewModel(morgen.Id, morgen.Naam);
//                 jijBentDeMolViewModel.IsMorgen = true;
//                 jijBentDeMolViewModel.Naam = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Naam.ToLower());
//
//                 jijBentDeMolViewModel.DoNext = _ => { conductor.ActivateItem(smoelenViewModel); };
//                 conductor.ActivateItem(jijBentDeMolViewModel);
//             }
//             else
//             {
//                 conductor.ActivateItem(smoelenViewModel);
//             }
//         }
//
//         public void OnKeyDown(KeyEventArgs e)
//         {
//             if (e?.Key == Key.Enter)
//             {
//                 Next();
//             }
//         }
//     }
// }