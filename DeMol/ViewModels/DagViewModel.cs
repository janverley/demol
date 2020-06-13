namespace DeMol.ViewModels
{
    public class DagViewModel
    {
        public DagViewModel(int id, string text)
        {
            Id = id;
            Text = text;
        }

        public int Id { get; }
        public string Text { get; }
    }
}