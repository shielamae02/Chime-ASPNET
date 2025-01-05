namespace Chime_ASPNET.Services.Email;

public static class EmailTemplate
{
    public static string GetEmailTemplate(string subject, string content)
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
                        font-family: Arial, sans-serif;
                        background-color: #f6f6f6;
                        margin: 0;
                        padding: 0;
                    }
                    .email-container {
                        max-width: 600px;
                        margin: 20px auto;
                        background: #ffffff;
                        border: 1px solid #ddd;
                        border-radius: 8px;
                        overflow: hidden;
                    }
                    .email-header {
                        background-color: #164863;
                        color: #ffffff;
                        padding: 20px;
                        text-align: center;
                    }
                    .email-header h1 {
                        margin: 0;
                        font-size: 24px;
                    }
                    .email-body {
                        padding: 20px;
                        color: #333333;
                        line-height: 1.6;
                        text-align: center;
                        align-items: center;
                    }
                    .email-body h2 {
                        font-size: 20px;
                        color: #164863;
                    }
                    .email-footer {
                        padding: 15px;
                        text-align: center;
                        font-size: 12px;
                        color: #666666;
                        border-top: 1px solid #ddd;
                    }
                    .btn {
                        display: inline-block;
                        padding: 10px 20px;
                        margin-top: 20px;
                        background-color: #DFF4F3;
                        color: #ffffff;
                        text-decoration: none;
                        border-radius: 5px;
                        text-decoration: none;
                    }
                    .btn:hover {
                        background-color: #DDF2FD;
                    }
                </style>
            </head>
            <body>
                <div class="email-container">
                    <div class="email-header">
                        <h1>Chime</h1>
                        <h2>{{subject}}</h2>
                    </div>
                    
                    <div class="email-body">
                        <div>{{content}}</div>
                    </div>
                    
                    <div class="email-footer">
                        <p>&copy; 2024 Chime. All rights reserved.</p>
                        <p>Please disregard this message if you did not request a password change.</p>
                    </div>
                </div>
            </body>
            </html>
        """
        .Replace("{{subject}}", subject)
        .Replace("{{content}}", content);
    }

}
