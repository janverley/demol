using Caliburn.Micro;
using DeMol.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMol.ViewModels
{
    public class ValidateViewModel : Screen
    {
        private readonly ShellViewModel conductor;
        private readonly SimpleContainer container;

        public ValidateViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            AntwoordenData antwoorden = new AntwoordenData { Dag = container.GetInstance<MenuViewModel>().SelectedDag.Id.ToString() };
            if (File.Exists($@".\Files\antwoorden.{container.GetInstance<MenuViewModel>().SelectedDag.Id}.json"))
            {
                string antwoordenJson = File.ReadAllText($@".\Files\antwoorden.{container.GetInstance<MenuViewModel>().SelectedDag.Id}.json");
                antwoorden = JsonConvert.DeserializeObject<AntwoordenData>(antwoordenJson);
            }



        }
    }
}
