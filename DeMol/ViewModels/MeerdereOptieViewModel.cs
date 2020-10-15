using Caliburn.Micro;

namespace DeMol.ViewModels
{
    public class MeerdereOptieViewModel : PropertyChangedBase
    {
        private bool isSelected;
        private string optieText;

        public MeerdereOptieViewModel(string optieText)
        {
            this.optieText = optieText;
        }

        public string OptieText
        {
            get => optieText;
            set => Set(ref optieText, value);
        }

        public bool IsSelected
        {
            get => isSelected;
            set => Set(ref isSelected, value);
        }
    }
}