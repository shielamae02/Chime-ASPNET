namespace Chime_ASPNET.Services.Email;

public static class EmailTemplate
{
    public static string ForgotPasswordTemplate(string subject, string content)
    {
        return """
            <!DOCTYPE html>
                <html lang="en">
                <head>
                    <meta charset="UTF-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1.0">
                    <title>{{subject}}</title>
                    <style>
                    body {
                        margin: 0;
                        padding: 20px;
                        font-family: Arial, sans-serif;
                        background-color: #F4F4F4;
                    }

                    .email-container {
                        background-color: #FFFFFF;
                        margin-top: 20px;
                        padding: 20px;
                        width: 100%;
                        border-radius: 8px;
                        box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
                    }

                    .email-header {
                        text-align: center;
                        padding: 20px;
                        display: flex;
                        flex-direction: column;
                        gap: 8px;
                    }

                    .email-header h1 {
                        color: #333333;
                        font-size: 24px;
                        margin: 0;
                    }

                    .email-header h2 {
                        color: #555555;
                        font-size: 18px;
                        margin: 0;
                    }

                    .email-body {
                        text-align: center;
                        padding: 20px;
                    }

                    .email-body p {
                        color: #555555;
                        font-size: 16px;
                    }

                    .email-body a {
                        background-color: #007BFF;
                        color: #FFFFFF;
                        text-decoration: none;
                        padding: 12px 20px;
                        border-radius: 5px;
                        font-size: 16px;
                    }
                    </style>
                </head>
                <body>
                    <div class="email-container">
                    <div class="email-header">
                        <h1>Chime</h1>
                        <h2>{{ subject }}</h2>
                    </div>
                    <div class="email-body">
                        <p>We received a request to reset your password. If you did not request a password reset, please ignore this email.</p>
                        <p>To reset your password, click the link below:</p>
                        <a href="{{content}}">Reset Password</a>
                    </div>
                    </div>
                </body>
                </html>
            """
            .Replace("{{subject}}", subject)
            .Replace("{{content}}", content);
    }
}
