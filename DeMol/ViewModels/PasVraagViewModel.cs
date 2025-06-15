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

        public bool Dag1MolOk { get; set; }
        public bool Dag1MolletjeOk { get; set; }
        public bool Dag2MolOk { get; set; }
        public bool Dag2MolletjeOk { get; set; }
        public bool Dag3MolOk { get; set; }
        public bool Dag3MolletjeOk { get; set; }
        public bool Dag4MolOk { get; set; }
        public bool Dag4MolletjeOk { get; set; }
        public bool Dag5MolOk { get; set; }
        public bool Dag5MolletjeOk { get; set; }
    }
}