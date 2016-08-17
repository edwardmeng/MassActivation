namespace MassActivation.Services
{
    public interface ICacheService
    {
        object Get(string key);

        void Set(string key, object value);
    }
}
