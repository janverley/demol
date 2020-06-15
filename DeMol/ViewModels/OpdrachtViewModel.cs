using Caliburn.Micro;

namespace DeMol.ViewModels
{
    public class OpdrachtViewModel : PropertyChangedBase
    {
        private string naam;
        private bool vandaagGespeeld;
        private int effectiefVerdiend;
        private int maxTeVerdienen;

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

        public int MaxTeVerdienen
        {
            get => maxTeVerdienen;
            set => Set(ref maxTeVerdienen, value);
        }

        public int EffectiefVerdiend
        {
            get => effectiefVerdiend;
            set => Set(ref effectiefVerdiend, value);
        }

        public string Id { get; internal set; }
    }
}