using Caliburn.Micro;

namespace DeMol.ViewModels
{
    public class PasVraagViewModel : PropertyChangedBase
    {
        private string naam;
        private int pasvragenVerdiend;

        public string Naam
        {
            get => naam;
            set => Set(ref naam, value);
        }

        public int PasVragenVerdiend
        {
            get => pasvragenVerdiend;
            set => Set(ref pasvragenVerdiend, value);
        }
    }
}