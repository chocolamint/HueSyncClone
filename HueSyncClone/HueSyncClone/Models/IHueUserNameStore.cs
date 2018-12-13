namespace HueSyncClone.Models
{
    public interface IHueUserNameStore
    {
        string Load();
        void Save(string userName);
    }
}
