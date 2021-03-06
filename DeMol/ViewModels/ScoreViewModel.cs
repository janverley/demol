using Caliburn.Micro;

namespace DeMol.ViewModels
{
    internal class ScoreViewModel : Screen
    {
        private string text;

        public string Text
        {
            get => text;
            set => Set(ref text, value);
        }

        public string Naam { get; set; }

        public void Next()
        {
            var q = Parent as QuizViewModel;
            q.StartQuiz(Naam);
        }
    }
}