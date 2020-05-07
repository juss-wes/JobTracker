namespace JobTracker.Security
{
    public interface IPasswordHelper
    {
        string Hash(string password);
        (bool Verified, bool NeedsUpgrade) Check(string hash, string password);
    }
}