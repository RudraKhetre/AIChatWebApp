using System.Threading.Tasks;

namespace AIChatWebApp.Services
{
    public interface IGroqService
    {
        Task<string> AskAI(string prompt);
    }
}
