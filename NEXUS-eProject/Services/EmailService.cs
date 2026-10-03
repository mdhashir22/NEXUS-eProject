using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using NEXUS_eProject.Models;

namespace NEXUS_eProject.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IConfiguration configuration,
            ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }


        // =====================================================
        // SEND CONTACT FORM EMAIL
        // =====================================================

        public async Task SendContactEmailAsync(
            ContactFormViewModel model)
        {
            // -------------------------------------------------
            // LOAD EMAIL SETTINGS
            // -------------------------------------------------

            var host =
                _configuration["EmailSettings:SmtpHost"];

            var portText =
                _configuration["EmailSettings:SmtpPort"];

            var username =
                _configuration["EmailSettings:Username"];

            var password =
                _configuration["EmailSettings:Password"];

            var receiverEmail =
                _configuration["EmailSettings:ReceiverEmail"];

            var senderName =
                _configuration["EmailSettings:SenderName"]
                ?? "NEXUS ISP";


            // -------------------------------------------------
            // VALIDATE CONFIGURATION
            // -------------------------------------------------

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(portText) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(receiverEmail))
            {
                throw new InvalidOperationException(
                    "Email settings are not configured correctly.");
            }


            if (!int.TryParse(portText, out var port))
            {
                throw new InvalidOperationException(
                    "SMTP port is invalid.");
            }


            // -------------------------------------------------
            // SERVICE / SUBJECT
            // -------------------------------------------------

            var selectedService =
                string.IsNullOrWhiteSpace(model.Service)
                    ? "General Inquiry"
                    : model.Service.Trim();


            var subject =
                $"NEXUS Contact Form - {selectedService}";


            // -------------------------------------------------
            // CREATE EMAIL
            // -------------------------------------------------

            var message = new MimeMessage();


            // FROM
            message.From.Add(
                new MailboxAddress(
                    senderName,
                    username));


            // TO
            message.To.Add(
                MailboxAddress.Parse(
                    receiverEmail));


            // REPLY TO CUSTOMER
            message.ReplyTo.Add(
                new MailboxAddress(
                    model.FullName,
                    model.Email));


            // SUBJECT
            message.Subject = subject;


            // -------------------------------------------------
            // EMAIL BODY
            // -------------------------------------------------

            var bodyBuilder =
                new BodyBuilder
                {
                    TextBody = $"""
                        A new message has been received from the NEXUS website.

                        --------------------------------------------------
                        CUSTOMER INFORMATION
                        --------------------------------------------------

                        Name:
                        {model.FullName}

                        Email:
                        {model.Email}

                        Phone:
                        {model.Phone}

                        Service:
                        {selectedService}

                        --------------------------------------------------
                        MESSAGE
                        --------------------------------------------------

                        {model.Message}

                        --------------------------------------------------

                        This message was submitted through the
                        NEXUS ISP Management System contact form.
                        """
                };


            message.Body =
                bodyBuilder.ToMessageBody();


            // -------------------------------------------------
            // SEND USING GMAIL SMTP
            // -------------------------------------------------

            using var smtp =
                new SmtpClient();


            try
            {
                // Port 587 + STARTTLS
                await smtp.ConnectAsync(
                    host,
                    port,
                    SecureSocketOptions.StartTls);


                // Gmail address + Google App Password
                await smtp.AuthenticateAsync(
                    username,
                    password);


                await smtp.SendAsync(
                    message);


                await smtp.DisconnectAsync(
                    true);


                _logger.LogInformation(
                    "NEXUS contact email sent successfully for {Email}.",
                    model.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send NEXUS contact email for {Email}.",
                    model.Email);


                if (smtp.IsConnected)
                {
                    await smtp.DisconnectAsync(
                        true);
                }


                throw;
            }
        }
    }
}