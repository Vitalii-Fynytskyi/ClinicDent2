namespace ClinicDent2.Interfaces
{
    public interface IBrowserTabControl
    {
        public void TabActivated();
        public void TabDeactivated();
        public void TabClosed();

        public void CommitChanges();
    }
}
