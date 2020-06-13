using Caliburn.Micro;

namespace DeMol.ViewModels
{
    public class OptieViewModel : PropertyChangedBase
    {
        private bool isSelected;
        private string optieText;

        public OptieViewModel(string optieText)
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