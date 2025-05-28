using MailKit.Net.Smtp;
using MimeKit;



namespace HeThongBanHang.Services
{
    public class EmailService
    {
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Shop Quần Áo", "lehoanggia85@gmail.com")); // sửa lại
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 587, false);
                await client.AuthenticateAsync("lehoanggia85@gmail.com", "wssw wexw tqrb udth"); // dùng app password
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}
