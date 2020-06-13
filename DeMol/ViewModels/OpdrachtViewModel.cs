using Caliburn.Micro;

namespace DeMol.ViewModels
{
    public class OpdrachtViewModel : PropertyChangedBase
    {
        private string naam;
        private bool vandaagGespeeld;

        public string Naam
        {
            get => naam;
            set => Set(ref naam, value);
        }

        public bool VandaagGespeeld
        {
            get => vandaagGespeeld;
            set => Set(ref vandaagGespeeld, value);
        }

        public string Id { get; internal set; }
    }
}