namespace MTFApp.UIHelpers.DragAndDrop
{
    public interface ITouchHelper
    {
        object SourceElement { get; set; }
        void Select();
        void Unselect();
    }
}
