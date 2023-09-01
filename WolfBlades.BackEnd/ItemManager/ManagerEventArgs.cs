namespace WolfBlades.BackEnd.ItemManager;

public class ManagerEventArgs : EventArgs
{
    public IItem? Item { get; set; }
}