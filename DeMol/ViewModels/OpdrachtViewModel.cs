using Caliburn.Micro;
using DeMol.Model;

namespace DeMol.ViewModels
{
    public class OpdrachtViewModel : PropertyChangedBase
    {
        //private readonly int selectedDagId;
        private int effectiefVerdiend;
        private int maxTeVerdienen;

        private string naam;
        private bool vandaagGespeeld;
        private string error;

        public OpdrachtViewModel(OpdrachtData opdrachtData, bool vandaagGespeeld, int maxTeVerdienen, int effectiefVerdiend)
        {
            //this.selectedDagId = selectedDagId;
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
            set
            {
                Set(ref vandaagGespeeld, value);
                // if (vandaagGespeeld)
                // {
                //     OpdrachtData.GespeeldOpDag = selectedDagId;
                // }
                // else
                // {
                //     OpdrachtData.GespeeldOpDag = -1;
                // }
            }
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

        public int EffectiefVerdiend
        {
            get => effectiefVerdiend;
            set
            {
                if(Set(ref effectiefVerdiend, value))
                    Check();
            }
        }

        public string Id => OpdrachtData.Opdracht;
        public OpdrachtData OpdrachtData { get; }
    }
}