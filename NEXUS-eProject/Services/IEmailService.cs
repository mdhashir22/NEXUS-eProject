using NEXUS_eProject.Models;

namespace NEXUS_eProject.Services
{
    public interface IEmailService
    {
        // =====================================================
        // CONTACT FORM EMAIL
        // =====================================================

        Task SendContactEmailAsync(
            ContactFormViewModel model);
    }
}