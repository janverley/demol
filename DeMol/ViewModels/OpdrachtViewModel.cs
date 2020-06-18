using Caliburn.Micro;
using DeMol.Model;

namespace DeMol.ViewModels
{
    public class OpdrachtViewModel : PropertyChangedBase
    {
        private readonly int selectedDagId;
        private int effectiefVerdiend;
        private int maxTeVerdienen;

        private string naam;
        private bool vandaagGespeeld;

        public OpdrachtViewModel(OpdrachtData opdrachtData, int selectedDagId)
        {
            this.selectedDagId = selectedDagId;
            OpdrachtData = opdrachtData;
            Naam = Util.OpdrachtUINaam(opdrachtData);
            VandaagGespeeld = opdrachtData.GespeeldOpDag == selectedDagId;
            MaxTeVerdienen = opdrachtData.MaxTeVerdienenBedrag;
            EffectiefVerdiend = opdrachtData.VerdiendBedrag;
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
                if (vandaagGespeeld)
                {
                    OpdrachtData.GespeeldOpDag = selectedDagId;
                }
                else
                {
                    OpdrachtData.GespeeldOpDag = -1;
                }
            }
        }

        public int MaxTeVerdienen
        {
            get => maxTeVerdienen;
            set
            {
                if (Set(ref maxTeVerdienen, value))
                {
                    OpdrachtData.MaxTeVerdienenBedrag = value;
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
                    OpdrachtData.VerdiendBedrag = value;
                }
            }
        }

        public string Id => OpdrachtData.Opdracht;
        public OpdrachtData OpdrachtData { get; }
    }
}