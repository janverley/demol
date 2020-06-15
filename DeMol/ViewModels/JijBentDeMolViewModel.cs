// using System;
// using System.Windows.Threading;
// using Caliburn.Micro;
//
// namespace DeMol.ViewModels
// {
//     internal class JijBentDeMolViewModel : Screen
//     {
//         private readonly ShellViewModel conductor;
//         private readonly SimpleContainer container;
//         private readonly int timeoutMolAanduiden;
//         private readonly DispatcherTimer timer = new DispatcherTimer();
//         private string message;
//
//         private string naam;
//
//         public JijBentDeMolViewModel(ShellViewModel conductor, SimpleContainer container)
//         {
//             this.conductor = conductor;
//             this.container = container;
//
//             timeoutMolAanduiden = container.GetInstance<ShellViewModel>().TimeoutMolAanduiden;
//
//             timer.Tick += Timer_Tick;
//             timer.Interval = TimeSpan.FromSeconds(timeoutMolAanduiden);
//         }
//
//         public string Message
//         {
//             get => message;
//             set => Set(ref message, value);
//         }
//
//         public string Naam
//         {
//             get => naam;
//             set => Set(ref naam, value);
//         }
//
//         public DagViewModel Dag { get; internal set; }
//
//         public Action<JijBentDeMolViewModel> DoNext { get; set; }
//         public bool IsMorgen { get; internal set; }
//
//         private void Timer_Tick(object sender, EventArgs e)
//         {
//             timer.Stop();
//             Next();
//         }
//
//         protected override void OnActivate()
//         {
//             base.OnActivate();
//
//
//             var vandaagMorgen = IsMorgen ? "morgen" : "vandaag";
//
//             var ja =
//                 $"{Naam}, {vandaagMorgen} ben jij de mol.\n\nProbeer zo veel mogelijk opdrachten te laten mislukken! \nVeel succes!";
//             var nee =
//                 $"{Naam}, {vandaagMorgen} ben jij niet de mol.\n\nProbeer de mol te ontmaskeren en onthoud zo veel mogelijk van wat er gebeurd.\nVeel succes!";
//
//             var m = container.GetInstance<ShellViewModel>().IsDeMol(Dag.Id, Naam) ? ja : nee;
//
//             Message =
//                 $"Opgelet, deze boodschap verdwijnt na {timeoutMolAanduiden} seconden.\n\n{m}\n\n Roep de volgende speler.";
//
//             timer.Start();
//         }
//
//         protected override void OnDeactivate(bool close)
//         {
//             timer.Stop();
//             base.OnDeactivate(close);
//         }
//
//         public void Next()
//         {
//             DoNext(this);
//         }
//
//         public void Menu()
//         {
//             var x = container.GetInstance<MenuViewModel>();
//             conductor.ActivateItem(x);
//         }
//     }
// }