// using System.Windows.Input;
// using Caliburn.Micro;
//
// namespace DeMol.ViewModels
// {
//     internal class QuizIntroViewModel : Screen
//     {
//         private readonly ShellViewModel conductor;
//         private readonly SimpleContainer container;
//         private string naam;
//
//         public QuizIntroViewModel(ShellViewModel conductor, SimpleContainer container)
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
//                     var dag = container.GetInstance<ShellViewModel>().Dag;
//                     NotifyOfPropertyChange(nameof(Text));
//                 }
//             }
//         }
//
//         public string Text => "begin quiz";
//       
//         public void OnKeyDown(KeyEventArgs e)
//         {
//             if (e?.Key == Key.Enter)
//             {
//                 Start();
//             }
//
//             if (e?.Key == Key.Escape)
//             {
//                 Menu();
//             }
//         }
//
//         public void Start()
//         {
//          
//             {
//                 var x = container.GetInstance<QuizWieIsDeMolViewModel>();
//                 x.Naam = Naam;
//                 conductor.ActivateItem(x);
//             }
//         }
//
//         public void Menu()
//         {
//             var x = container.GetInstance<MenuViewModel>();
//             conductor.ActivateItem(x);
//         }
//     }
// }

