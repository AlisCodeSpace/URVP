using System.Net;
using System.Net.Mail;
using RICHConnect.Backend.Application.Interfaces.Notifications;
using RICHConnect.Backend.Application.Interfaces.Settings;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Entities.Notifications;
using RICHConnect.Backend.Application.Validators.Configuration;

namespace RICHConnect.Backend.Application.Services.Notifications
{
    public class EmailService : IEmailService
    {
        private const string SmtpPasswordKey = "SMTP.Password";
        private const int MaxExceptionLength = 4000;

        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly AppDbContext _dbContext;
        private readonly ISettingsService _settingsService;
        private readonly SMTP _config;
        private readonly SMTP _configLocal;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly bool _isEmailConfigured;

        public EmailService(
            IConfiguration configuration,
            ILogger<EmailService> logger,
            AppDbContext dbContext,
            ISettingsService settingsService)
        {
            _configuration = configuration;
            _logger = logger;
            _dbContext = dbContext;
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

            // Load SMTP configurations
            _config = _configuration.GetSection("SMTP").Get<SMTP>()
                ?? throw new InvalidOperationException("SMTP configuration not found");
            _configLocal = _configuration.GetSection("SMTPLocal").Get<SMTP>()
                ?? throw new InvalidOperationException("SMTPLocal configuration not found");

            // Fallback (local) credentials: Username from config, password from DB (same as primary via GetPrimarySmtpOptionsAsync)

            _fromEmail = _config.From;
            _fromName = "RICHConnect";

            // Check if email is properly configured
            _isEmailConfigured = _config.Send || _configLocal.Send;

            if (!_isEmailConfigured)
            {
                _logger.LogWarning("Email service is not enabled. Both SMTP.Send and SMTPLocal.Send are set to false.");
            }
        }

        /// <summary>
        /// Resolves primary SMTP options.
        /// - Password comes from DB setting (SMTP.Password).
        /// - Username comes from appsettings (SMTPUsername) and falls back to the From address.
        /// - From email and display name come from appsettings (SMTP.From / default _fromName).
        /// </summary>
        private async Task<(string Username, string Password, string From, string FromName)> GetPrimarySmtpOptionsAsync(CancellationToken ct = default)
        {
            var password = await _settingsService.GetValueAsync(SmtpPasswordKey, ct)
                ?? string.Empty;

            var from = _config.From;
            var fromName = _fromName;

            var username = _configuration["SMTPUsername"]
                ?? from;

            return (username, password, from, fromName);
        }

        public async Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string body, string? actionUrl = null, string? actionText = null)
        {
            // Check if body is already HTML (contains HTML tags)
            var isHtml = body.Contains("<html", StringComparison.OrdinalIgnoreCase) || 
                         body.Contains("<div", StringComparison.OrdinalIgnoreCase) ||
                         body.Contains("<table", StringComparison.OrdinalIgnoreCase);
            
            // If it's plain text (notification message), wrap it in the unified template
            string emailBody;
            if (!isHtml)
            {
                emailBody = GetUnifiedEmailTemplate(subject, body, actionUrl, actionText);
            }
            else
            {
                // Already HTML (e.g., from contact form), use as-is
                emailBody = body;
            }
            
            return await SendEmail(toEmail, emailBody, subject, null, null);
        }

        public async Task<bool> SendEmailAsync(string toEmail, string toName, string templateType, Dictionary<string, string> templateData)
        {
            var template = GetEmailTemplate(templateType);
            if (template == null)
            {
                _logger.LogWarning("Email template not found: {TemplateType}", templateType);
                return false;
            }

            var subject = ReplaceTemplateVariables(template.Subject, templateData);
            var body = ReplaceTemplateVariables(template.Body, templateData);

            return await SendEmailAsync(toEmail, toName, subject, body);
        }

        public async Task<bool> SendBulkEmailAsync(List<(string email, string name)> recipients, string subject, string body)
        {
            if (!_isEmailConfigured)
            {
                _logger.LogWarning("Bulk email not sent - Email service not configured. Recipients: {Count}, Subject: {Subject}", 
                    recipients.Count, subject);
                return false;
            }

            if (recipients == null || recipients.Count == 0)
            {
                _logger.LogWarning("Bulk email not sent - no recipients provided");
                return false;
            }

            try
            {
                // Send emails individually using SMTP
                var tasks = recipients.Select(async recipient =>
                {
                    try
                    {
                        return await SendEmail(recipient.email, body, subject, null, null);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending bulk email to {Email}", recipient.email);
                        return false;
                    }
                });

                var results = await Task.WhenAll(tasks);
                var successCount = results.Count(r => r);

                _logger.LogInformation("Bulk email completed. Sent: {SuccessCount}/{TotalCount}", 
                    successCount, recipients.Count);
                
                return successCount == recipients.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending bulk email");
                return false;
            }
        }

        public async Task<bool> SendEmailFromUserAsync(string fromEmail, string fromName, string toEmail, string toName, string subject, string body)
        {
            // SMTP: Use the configured sender email as the actual sender
            // Set the "Reply-To" header to the user's email
            if (!_isEmailConfigured)
            {
                _logger.LogWarning("User email not sent - Email service not configured. From: {FromEmail}, To: {ToEmail}", 
                    fromEmail, toEmail);
                return false;
            }

            if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(toEmail))
            {
                _logger.LogWarning("User email not sent - from or to email is null or empty");
                return false;
            }

            var (smtpUsername, smtpPassword, fromAddress, fromDisplayName) = await GetPrimarySmtpOptionsAsync();

            // Create a custom mail message with Reply-To header
            using var client = new SmtpClient(_config.Server, _config.Port);
            client.UseDefaultCredentials = false;
            // Allow explicit SSL config while preserving the existing port-based default.
            client.EnableSsl = _config.EnableSsl ?? _config.Port != 25;
            if (!string.IsNullOrWhiteSpace(smtpUsername) && !string.IsNullOrWhiteSpace(smtpPassword))
            {
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
            }

            using var mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(fromAddress, fromName ?? fromDisplayName);
            mailMessage.ReplyToList.Add(new MailAddress(fromEmail, fromName));
            mailMessage.Subject = subject;
            mailMessage.IsBodyHtml = true;
            mailMessage.To.Add(new MailAddress(toEmail, toName));
            mailMessage.Body = body;

            var log = new EmailLog()
            {
                Body = string.Empty,
                Bcc = string.Empty,
                Cc = string.Empty,
                From = fromAddress,
                To = toEmail
            };

            try
            {
                bool primarySucceeded = false;
                Exception? primaryException = null;
                
                // Attempt primary SMTP
                try
                {
                    if (_config.Send)
                    {
                        await client.SendMailAsync(mailMessage);
                        primarySucceeded = true;
                        log.Success = true;
                        _logger.LogInformation("User email sent successfully via primary SMTP. From: {FromEmail}, To: {ToEmail}, Subject: {Subject}", 
                            fromEmail, toEmail, subject);
                    }
                    else
                    {
                        _logger.LogWarning("Primary SMTP sending is disabled (SMTP.Send=false). Attempting fallback. From: {FromEmail}, To: {ToEmail}",
                            fromEmail, toEmail);
                    }
                }
                catch (Exception ex)
                {
                    primaryException = ex;
                    _logger.LogWarning(ex, "Primary SMTP failed. Attempting fallback. From: {FromEmail}, To: {ToEmail}", fromEmail, toEmail);
                }

                // Try fallback if primary didn't succeed
                if (!primarySucceeded)
                {
                    using (var localClient = new SmtpClient(_configLocal.Server, _configLocal.Port))
                    {
                        localClient.UseDefaultCredentials = false;
                        // Allow explicit SSL config while preserving the existing port-based default.
                        localClient.EnableSsl = _configLocal.EnableSsl ?? _configLocal.Port != 25;
                        
                        // Use DB password (same as primary), but allow different username for fallback server
                        var localUsername = _configuration["SMTPLocalUsername"] ?? _configLocal.From;
                        if (!string.IsNullOrWhiteSpace(localUsername) && !string.IsNullOrWhiteSpace(smtpPassword))
                        {
                            localClient.Credentials = new NetworkCredential(localUsername, smtpPassword);
                        }

                        try
                        {
                            if (_configLocal.Send)
                            {
                                await localClient.SendMailAsync(mailMessage);
                                log.Success = true;
                                
                                // Build exception message that includes primary failure reason
                                var primaryReason = primaryException?.ToString() ?? "Primary SMTP sending is disabled (SMTP.Send=false).";
                                log.Exception = TruncateException(primaryReason);
                                
                                _logger.LogInformation("Fallback SMTP succeeded after primary failure. From: {FromEmail}, To: {ToEmail}, Subject: {Subject}", 
                                    fromEmail, toEmail, subject);
                            }
                            else
                            {
                                log.Success = false;
                                var primaryReason = primaryException?.ToString() ?? "Primary SMTP sending is disabled (SMTP.Send=false).";
                                log.Exception = TruncateException($"Local SMTP sending is disabled (SMTPLocal.Send=false).{Environment.NewLine}{primaryReason}");
                                _logger.LogError("Both primary and fallback SMTP are disabled. From: {FromEmail}, To: {ToEmail}", fromEmail, toEmail);
                            }
                        }
                        catch (Exception ex2)
                        {
                            log.Success = false;
                            var primaryReason = primaryException?.ToString() ?? "Primary SMTP sending is disabled (SMTP.Send=false).";
                            log.Exception = TruncateException($"{ex2}{Environment.NewLine}{primaryReason}");
                            _logger.LogError(ex2, "Both SMTP servers failed. Primary: {PrimaryReason}, Local: {LocalEx}", 
                                primaryException?.Message ?? "disabled", ex2.Message);
                        }
                    }
                }
            }
            finally
            {
                log.Id = Guid.NewGuid();
                log.CreatedOn = DateTime.UtcNow;
                log.ModifiedOn = DateTime.UtcNow;
                _dbContext.EmailLogs.Add(log);
                await _dbContext.SaveChangesAsync();
            }

            return log.Success;
        }

        public bool IsEmailConfigured()
        {
            return _isEmailConfigured;
        }

        public string GetEmailConfigurationStatus()
        {
            if (_isEmailConfigured)
            {
                return $"Email service configured for SMTP (Primary: {_config.Server}:{_config.Port}, Fallback: {_configLocal.Server}:{_configLocal.Port}, From: {_fromEmail})";
            }
            else
            {
                return "Email service not configured. Please enable SMTP.Send or SMTPLocal.Send in configuration";
            }
        }

        /// <summary>
        /// Core method to send email via SMTP with fallback support and logging
        /// </summary>
        private async Task<bool> SendEmail(
            string mailTo, 
            string mailBody, 
            string mailSubject, 
            byte[]? attachment = null, 
            string? attachmentName = null)
        {
            if (!_isEmailConfigured)
            {
                _logger.LogWarning("Email not sent - Email service not configured. To: {ToEmail}, Subject: {Subject}", mailTo, mailSubject);
                return false;
            }

            if (string.IsNullOrEmpty(mailTo))
            {
                _logger.LogWarning("Email not sent - recipient email is null or empty");
                return false;
            }

            var (smtpUsername, smtpPassword, fromAddress, fromDisplayName) = await GetPrimarySmtpOptionsAsync();

            using var client = new SmtpClient(_config.Server, _config.Port);
            client.UseDefaultCredentials = false;
            // Allow explicit SSL config while preserving the existing port-based default.
            client.EnableSsl = _config.EnableSsl ?? _config.Port != 25;
            if (!string.IsNullOrWhiteSpace(smtpUsername) && !string.IsNullOrWhiteSpace(smtpPassword))
            {
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
            }

            using var mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(fromAddress, fromDisplayName);
            mailMessage.Subject = mailSubject;
            mailMessage.IsBodyHtml = true;
            mailMessage.To.Add(mailTo);
            mailMessage.Body = mailBody;

            if (attachment != null && !string.IsNullOrEmpty(attachmentName))
            {
                var attachFile = new Attachment(new MemoryStream(attachment), attachmentName);
                mailMessage.Attachments.Add(attachFile);
            }

            var log = new EmailLog()
            {
                Body = string.Empty, // Don't store full body for privacy
                Bcc = string.Empty,
                Cc = string.Empty,
                From = fromAddress,
                To = mailTo
            };

            try
            {
                bool primarySucceeded = false;
                Exception? primaryException = null;
                
                // Attempt primary SMTP
                try
                {
                    _logger.LogInformation("Attempting to send email to {ToEmail} with subject: {Subject}", mailTo, mailSubject);
                    
                    if (_config.Send)
                    {
                        await client.SendMailAsync(mailMessage);
                        primarySucceeded = true;
                        log.Success = true;
                        _logger.LogInformation("Email sent successfully via primary SMTP. To: {ToEmail}, Subject: {Subject}", mailTo, mailSubject);
                    }
                    else
                    {
                        _logger.LogWarning("Primary SMTP sending is disabled (SMTP.Send=false). Attempting fallback. To: {ToEmail}, Subject: {Subject}",
                            mailTo, mailSubject);
                    }
                }
                catch (Exception ex)
                {
                    primaryException = ex;
                    _logger.LogWarning(ex, "Primary SMTP failed. Attempting fallback. To: {ToEmail}", mailTo);
                }

                // Try fallback if primary didn't succeed
                if (!primarySucceeded)
                {
                    using (var localClient = new SmtpClient(_configLocal.Server, _configLocal.Port))
                    {
                        localClient.UseDefaultCredentials = false;
                        // Allow explicit SSL config while preserving the existing port-based default.
                        localClient.EnableSsl = _configLocal.EnableSsl ?? _configLocal.Port != 25;
                        
                        // Use DB password (same as primary), but allow different username for fallback server
                        var localUsername = _configuration["SMTPLocalUsername"] ?? _configLocal.From;
                        if (!string.IsNullOrWhiteSpace(localUsername) && !string.IsNullOrWhiteSpace(smtpPassword))
                        {
                            localClient.Credentials = new NetworkCredential(localUsername, smtpPassword);
                        }

                        try
                        {
                            if (_configLocal.Send)
                            {
                                await localClient.SendMailAsync(mailMessage);
                                log.Success = true;
                                
                                // Build exception message that includes primary failure reason
                                var primaryReason = primaryException?.ToString() ?? "Primary SMTP sending is disabled (SMTP.Send=false).";
                                log.Exception = TruncateException(primaryReason);
                                
                                _logger.LogInformation("Fallback SMTP succeeded after primary failure. To: {ToEmail}, Subject: {Subject}", 
                                    mailTo, mailSubject);
                            }
                            else
                            {
                                log.Success = false;
                                var primaryReason = primaryException?.ToString() ?? "Primary SMTP sending is disabled (SMTP.Send=false).";
                                log.Exception = TruncateException($"Local SMTP sending is disabled (SMTPLocal.Send=false).{Environment.NewLine}{primaryReason}");
                                _logger.LogError("Both primary and fallback SMTP are disabled. To: {ToEmail}", mailTo);
                            }
                        }
                        catch (Exception ex2)
                        {
                            log.Success = false;
                            var primaryReason = primaryException?.ToString() ?? "Primary SMTP sending is disabled (SMTP.Send=false).";
                            log.Exception = TruncateException($"{ex2}{Environment.NewLine}{primaryReason}");
                            _logger.LogError(ex2, "Both SMTP servers failed. Primary: {PrimaryReason}, Local: {LocalEx}", 
                                primaryException?.Message ?? "disabled", ex2.Message);
                        }
                    }
                }
            }
            finally
            {
                // Save log to database
                log.Id = Guid.NewGuid();
                log.CreatedOn = DateTime.UtcNow;
                log.ModifiedOn = DateTime.UtcNow;
                _dbContext.EmailLogs.Add(log);
                await _dbContext.SaveChangesAsync();
            }

            return log.Success;
        }

        private static string? TruncateException(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return value.Length <= MaxExceptionLength
                ? value
                : value.Substring(0, MaxExceptionLength);
        }

        private EmailTemplate? GetEmailTemplate(string templateType)
        {
            // Simple template system - in production, load from database or file
            return templateType switch
            {
                "ChallengeSubmitted" => new EmailTemplate
                {
                    Subject = "New Challenge Submitted - RICHConnect",
                    Body = GetChallengeSubmittedTemplate()
                },
                "ChallengeApproved" => new EmailTemplate
                {
                    Subject = "Challenge Approved - RICHConnect",
                    Body = GetChallengeApprovedTemplate()
                },
                "ChallengeRejected" => new EmailTemplate
                {
                    Subject = "Challenge Update - RICHConnect",
                    Body = GetChallengeRejectedTemplate()
                },
                "FacultySpecialistInvited" => new EmailTemplate
                {
                    Subject = "Challenge Invitation - RICHConnect",
                    Body = GetFacultySpecialistInvitedTemplate()
                },
                "FacultySpecialistResponded" => new EmailTemplate
                {
                    Subject = "facultySpecialist Response - RICHConnect",
                    Body = GetFacultySpecialistRespondedTemplate()
                },
                "ChallengeMatched" => new EmailTemplate
                {
                    Subject = "Challenge Matched Successfully - RICHConnect",
                    Body = GetChallengeMatchedTemplate()
                },
                "PartnerRegistered" => new EmailTemplate
                {
                    Subject = "New Partner Registration - RICHConnect",
                    Body = GetPartnerRegisteredTemplate()
                },
                "PartnerApproved" => new EmailTemplate
                {
                    Subject = "Registration Approved - RICHConnect",
                    Body = GetPartnerApprovedTemplate()
                },
                "PartnerRejected" => new EmailTemplate
                {
                    Subject = "Registration Update - RICHConnect",
                    Body = GetPartnerRejectedTemplate()
                },
                "ThemeSubmitted" => new EmailTemplate
                {
                    Subject = "New Theme Submitted - RICHConnect",
                    Body = GetThemeSubmittedTemplate()
                },
                "ThemeApproved" => new EmailTemplate
                {
                    Subject = "Theme Approved - RICHConnect",
                    Body = GetThemeApprovedTemplate()
                },
                "ThemeRejected" => new EmailTemplate
                {
                    Subject = "Theme Update - RICHConnect",
                    Body = GetThemeRejectedTemplate()
                },
                _ => null
            };
        }

        private string ReplaceTemplateVariables(string template, Dictionary<string, string> data)
        {
            var result = template;
            foreach (var kvp in data)
            {
                result = result.Replace($"{{{kvp.Key}}}", kvp.Value);
            }
            return result;
        }

        private string GetChallengeSubmittedTemplate()
        {
            return @"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #2c3e50;'>New Challenge Submitted</h2>
                    <p>Hello {AdminName},</p>
                    <p>A new challenge has been submitted and requires your review:</p>
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p><strong>Challenge Title:</strong> {ChallengeTitle}</p>
                        <p><strong>Submitted By:</strong> {SubmittedBy}</p>
                        <p><strong>Theme:</strong> {ThemeName}</p>
                        <p><strong>Description:</strong></p>
                        <p style='margin-top: 6px;'>{Description}</p>
                    </div>
                    <p>Please review and take appropriate action.</p>
                    <p>Best regards,<br>RICHConnect Team</p>
                </div>";
        }

        private string GetChallengeApprovedTemplate()
        {
            return @"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #27ae60;'>Challenge Approved</h2>
                    <p>Hello {PartnerName},</p>
                    <p>Great news! Your challenge has been approved:</p>
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p><strong>Challenge Title:</strong> {ChallengeTitle}</p>
                        <p><strong>Theme:</strong> {ThemeName}</p>
                        <p><strong>Approved By:</strong> {ApprovedBy}</p>
                    </div>
                    <p>Your challenge is now available for facultySpecialist matching.</p>
                    <p>Best regards,<br>RICHConnect Team</p>
                </div>";
        }

        private string GetChallengeRejectedTemplate()
        {
            return @"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #e74c3c;'>Challenge Update</h2>
                    <p>Hello {PartnerName},</p>
                    <p>Your challenge requires some modifications:</p>
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p><strong>Challenge Title:</strong> {ChallengeTitle}</p>
                        <p><strong>Reason:</strong> {RejectionReason}</p>
                    </div>
                    <p>Please review the feedback and resubmit your challenge.</p>
                    <p>Best regards,<br>RICHConnect Team</p>
                </div>";
        }

        private string GetFacultySpecialistInvitedTemplate()
        {
            return @"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #3498db;'>Challenge Invitation</h2>
                    <p>Hello {FacultySpecialistName},</p>
                    <p>You have been invited to participate in a challenge:</p>
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p><strong>Challenge Title:</strong> {ChallengeTitle}</p>
                        <p><strong>Theme:</strong> {ThemeName}</p>
                        <p><strong>Partner:</strong> {PartnerName}</p>
                        <p><strong>Description:</strong></p>
                        <p style='margin-top: 6px;'>{Description}</p>
                    </div>
                    <p>Please click the button below to sign in to your account and view the challenge details.</p>
                    <div style='text-align: center; margin: 24px 0;'>
                        <a href='https://richconnect.aub.edu.lb/sign-in/' style='background-color: #3498db; color: #ffffff; padding: 12px 28px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: 600;'>Open Portal</a>
                    </div>
                    <p>If the button does not work, copy and paste this link into your browser:<br><a href='https://richconnect.aub.edu.lb/sign-in/'>https://richconnect.aub.edu.lb/sign-in/</a></p>
                    <p>Best regards,<br>RICHConnect Team</p>
                </div>";
        }

        private string GetPartnerRegisteredTemplate()
        {
            return @"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #2c3e50;'>New Partner Registration</h2>
                    <p>Hello {AdminName},</p>
                    <p>A new community partner has registered and requires approval:</p>
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p><strong>Company Name:</strong> {CompanyName}</p>
                        <p><strong>Contact Person:</strong> {ContactName}</p>
                        <p><strong>Email:</strong> {Email}</p>
                        <p><strong>Sector:</strong> {Sector}</p>
                    </div>
                    <p>Please review and take appropriate action.</p>
                    <p>Best regards,<br>RICHConnect Team</p>
                </div>";
        }

        private string GetPartnerApprovedTemplate()
        {
            return @"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #27ae60;'>Registration Approved</h2>
                    <p>Hello {ContactName},</p>
                    <p>Congratulations! Your company registration has been approved:</p>
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p><strong>Company Name:</strong> {CompanyName}</p>
                        <p><strong>Approved By:</strong> {ApprovedBy}</p>
                    </div>
                    <p>You can now submit challenges and collaborate with professors.</p>
                    <p>Best regards,<br>RICHConnect Team</p>
                </div>";
        }

        private string GetPartnerRejectedTemplate()
        {
            return @"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #e74c3c;'>Registration Update</h2>
                    <p>Hello {ContactName},</p>
                    <p>Your company registration requires some modifications:</p>
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p><strong>Company Name:</strong> {CompanyName}</p>
                        <p><strong>Reason:</strong> {RejectionReason}</p>
                    </div>
                    <p>Please review the feedback and resubmit your registration.</p>
                    <p>Best regards,<br>RICHConnect Team</p>
                </div>";
        }

        private string GetThemeSubmittedTemplate()
        {
            return @"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #2c3e50;'>New Theme Submitted</h2>
                    <p>Hello {AdminName},</p>
                    <p>A new theme has been submitted and requires review:</p>
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p><strong>Theme Title:</strong> {ThemeTitle}</p>
                        <p><strong>Submitted By:</strong> {SubmittedBy}</p>
                        <p><strong>Description:</strong> {Description}</p>
                        <p><strong>Expected Outcomes:</strong> {ExpectedOutcomes}</p>
                    </div>
                    <p>Please review and take appropriate action.</p>
                    <p>Best regards,<br>RICHConnect Team</p>
                </div>";
        }

        private string GetThemeApprovedTemplate()
        {
            return @"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #27ae60;'>Theme Approved</h2>
                    <p>Hello {FacultySpecialistName},</p>
                    <p>Congratulations! Your theme has been approved:</p>
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p><strong>Theme Title:</strong> {ThemeTitle}</p>
                        <p><strong>Approved By:</strong> {ApprovedBy}</p>
                    </div>
                    <p>Your theme is now available for challenge creation.</p>
                    <p>Best regards,<br>RICHConnect Team</p>
                </div>";
        }

        private string GetThemeRejectedTemplate()
        {
            return @"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #e74c3c;'>Theme Update</h2>
                    <p>Hello {FacultySpecialistName},</p>
                    <p>Your theme requires some modifications:</p>
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p><strong>Theme Title:</strong> {ThemeTitle}</p>
                        <p><strong>Reason:</strong> {RejectionReason}</p>
                    </div>
                    <p>Please review the feedback and resubmit your theme.</p>
                    <p>Best regards,<br>RICHConnect Team</p>
                </div>";
        }

        private string GetFacultySpecialistRespondedTemplate()
        {
            return @"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #3498db;'>facultySpecialist Response</h2>
                    <p>Hello {AdminName},</p>
                    <p>A facultySpecialist has responded to a challenge invitation:</p>
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p><strong>facultySpecialist Name:</strong> {FacultySpecialistName}</p>
                        <p><strong>Challenge Title:</strong> {ChallengeTitle}</p>
                        <p><strong>Theme:</strong> {ThemeName}</p>
                        <p><strong>Partner:</strong> {PartnerName}</p>
                        <p><strong>Decision:</strong> {Decision}</p>
                    </div>
                    <p>Please review the response and take appropriate action.</p>
                    <p>Best regards,<br>RICHConnect Team</p>
                </div>";
        }

        private string GetChallengeMatchedTemplate()
        {
            return @"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #27ae60;'>Challenge Matched Successfully</h2>
                    <p>Hello {AdminName},</p>
                    <p>A challenge has been successfully matched with professors:</p>
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p><strong>Challenge Title:</strong> {ChallengeTitle}</p>
                        <p><strong>Theme:</strong> {ThemeName}</p>
                        <p><strong>Partner:</strong> {PartnerName}</p>
                        <p><strong>facultySpecialist Count:</strong> {ProfessorCount}</p>
                        <p><strong>facultySpecialist Names:</strong> {FacultySpecialistNames}</p>
                    </div>
                    <p>The challenge is now ready for collaboration.</p>
                    <p>Best regards,<br>RICHConnect Team</p>
                </div>";
        }

        private string GetChallengeMatchedPartnerTemplate()
        {
            return @"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #27ae60;'>Challenge Matched with Professors</h2>
                    <p>Hello {PartnerName},</p>
                    <p>Great news! Your challenge has been successfully matched with professors:</p>
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p><strong>Challenge Title:</strong> {ChallengeTitle}</p>
                        <p><strong>Theme:</strong> {ThemeName}</p>
                        <p><strong>facultySpecialist Count:</strong> {ProfessorCount}</p>
                        <p><strong>facultySpecialist Names:</strong> {FacultySpecialistNames}</p>
                    </div>
                    <p>You can now collaborate with the assigned professors on your challenge.</p>
                    <p>Best regards,<br>RICHConnect Team</p>
                </div>";
        }

        /// <summary>
        /// Creates a unified HTML email template for all notifications
        /// </summary>
        private string GetUnifiedEmailTemplate(string title, string message, string? actionUrl = null, string? actionText = null)
        {
            var actionButton = string.Empty;
            var actionInstruction = string.Empty;
            if (!string.IsNullOrEmpty(actionUrl) && !string.IsNullOrEmpty(actionText))
            {
                actionInstruction = "<p style='margin: 20px 0 10px 0; color: #555; font-size: 15px;'>Please sign in to your account using the button below to view the challenge details.</p>";
                actionButton = $@"
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{actionUrl}' style='background-color: #3498db; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: 600;'>{actionText}</a>
                    </div>";
            }

            return $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>{WebUtility.HtmlEncode(title)}</title>
                </head>
                <body style='margin: 0; padding: 0; font-family: Arial, Helvetica, sans-serif; background-color: #f4f4f4;'>
                    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 20px 0;'>
                        <tr>
                            <td align='center'>
                                <table width='600' cellpadding='0' cellspacing='0' style='background-color: white; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
                                    <!-- Header -->
                                    <tr>
                                        <td style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 8px 8px 0 0;'>
                                            <h1 style='color: white; margin: 0; font-size: 24px; font-weight: 600;'>RICHConnect</h1>
                                        </td>
                                    </tr>
                                    
                                    <!-- Content -->
                                    <tr>
                                        <td style='padding: 40px 30px;'>
                                            <h2 style='color: #2c3e50; margin: 0 0 20px 0; font-size: 20px; font-weight: 600;'>{WebUtility.HtmlEncode(title)}</h2>
                                            <div style='color: #555; font-size: 15px; line-height: 1.6; white-space: pre-wrap;'>{WebUtility.HtmlEncode(message)}</div>
                                            {actionInstruction}
                                            {actionButton}
                                        </td>
                                    </tr>
                                    
                                    <!-- Footer -->
                                    <tr>
                                        <td style='background-color: #f8f9fa; padding: 20px 30px; text-align: center; border-radius: 0 0 8px 8px; border-top: 1px solid #e9ecef;'>
                                            <p style='color: #6c757d; margin: 0; font-size: 13px; line-height: 1.5;'>
                                                This is an automated notification from RICHConnect.<br>
                                                Please do not reply to this email.
                                            </p>
                                            <p style='color: #6c757d; margin: 10px 0 0 0; font-size: 12px;'>
                                                © {DateTime.UtcNow.Year} RICHConnect - American University of Beirut
                                            </p>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";
        }

        private class EmailTemplate
        {
            public string Subject { get; set; } = "";
            public string Body { get; set; } = "";
        }
    }
}
