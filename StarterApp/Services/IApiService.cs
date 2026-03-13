namespace StarterApp.Services;

public interface IApiService
{
    Task<T?> GetAsync<T>(string endpoint);
    Task<T?> PostAsync<T>(string endpoint, object payload);
    Task<T?> PutAsync<T>(string endpoint, object payload);
    Task DeleteAsync(string endpoint);
    void SetAuthToken(string token);
}
