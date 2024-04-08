namespace ClinicDent2.Interfaces
{
    public interface IBrowserTabControl
    {
        public void TabActivated();
        public bool TabDeactivated();
        public void TabClosed();
        public void Notify(int notificationCode, object param);
    }
}
