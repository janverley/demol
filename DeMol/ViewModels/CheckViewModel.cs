using Caliburn.Micro;

namespace DeMol.ViewModels
{
    public class CheckViewModel : PropertyChangedBase
    {
        public CheckViewModel(string text, bool isOk)
        {
            Text = text;
            IsOk = isOk;
        }

        public string Text { get; }
        public bool IsOk { get; }
    }
}