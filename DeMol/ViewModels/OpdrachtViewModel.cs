using Caliburn.Micro;
using DeMol.Model;

namespace DeMol.ViewModels
{
    public class OpdrachtViewModel : PropertyChangedBase
    {
        private int effectiefVerdiend;
        private string error;
        private int maxTeVerdienen;

        private string naam;
        private bool vandaagGespeeld;

        public OpdrachtViewModel(OpdrachtData opdrachtData, bool vandaagGespeeld, int maxTeVerdienen,
                                 int effectiefVerdiend)
        {
            OpdrachtData = opdrachtData;
            Naam = Util.OpdrachtUiNaam(opdrachtData);
            VandaagGespeeld = vandaagGespeeld;
            MaxTeVerdienen = maxTeVerdienen;
            EffectiefVerdiend = effectiefVerdiend;
        }

        public string Error
        {
            get => error;
            set => Set(ref error, value);
        }

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
            set
            {
                if (Set(ref maxTeVerdienen, value))
                {
                    Check();
                }
            }
        }

        public int EffectiefVerdiend
        {
            get => effectiefVerdiend;
            set
            {
                if (Set(ref effectiefVerdiend, value))
                {
                    Check();
                }
            }
        }

        public string Id => OpdrachtData.Opdracht;
        public OpdrachtData OpdrachtData { get; }

        private void Check()
        {
            if (EffectiefVerdiend > MaxTeVerdienen)
            {
                Error = "Da kan ni";
            }
            else
            {
                Error = "";
            }
        }
    }
}