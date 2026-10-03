namespace NEXUS_eProject.Services
{
    public interface INexusAiService
    {
        Task<string> GetResponseAsync(string userMessage);
    }
}