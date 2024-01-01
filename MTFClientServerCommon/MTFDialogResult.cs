namespace MTFClientServerCommon
{
    public class MTFDialogResult
    {
        public MTFDialogResultEnum Result { get; set; }
        public string TextResult { get; set; }
    }

    public enum MTFDialogResultEnum
    {
        Ok,
        Cancel,
        Yes,
        No,
        TextResult
    }
}
