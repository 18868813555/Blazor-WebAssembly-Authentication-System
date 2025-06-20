using System.Text.Json;
using System.Text;

namespace Client.Services;

public class JwtService
{
    public static string? GetEmailFromToken(string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
                return null;

            // JWT格式: header.payload.signature
            var parts = token.Split('.');
            if (parts.Length != 3)
                return null;

            // 解码payload部分
            var payload = parts[1];
            
            // 添加Base64填充
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var jsonBytes = Convert.FromBase64String(payload);
            var json = Encoding.UTF8.GetString(jsonBytes);
            
            using var document = JsonDocument.Parse(json);
            
            // 从sub claim中获取邮箱
            if (document.RootElement.TryGetProperty("sub", out var subElement))
            {
                return subElement.GetString();
            }
            
            return null;
        }
        catch
        {
            return null;
        }
    }

    public static string? GetUserIdFromToken(string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var parts = token.Split('.');
            if (parts.Length != 3)
                return null;

            var payload = parts[1];
            
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var jsonBytes = Convert.FromBase64String(payload);
            var json = Encoding.UTF8.GetString(jsonBytes);
            
            using var document = JsonDocument.Parse(json);
            
            // 从uid claim中获取用户ID
            if (document.RootElement.TryGetProperty("uid", out var uidElement))
            {
                return uidElement.GetString();
            }
            
            return null;
        }
        catch
        {
            return null;
        }
    }

    public static bool IsTokenExpired(string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
                return true;

            var parts = token.Split('.');
            if (parts.Length != 3)
                return true;

            var payload = parts[1];
            
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var jsonBytes = Convert.FromBase64String(payload);
            var json = Encoding.UTF8.GetString(jsonBytes);
            
            using var document = JsonDocument.Parse(json);
            
            // 检查exp claim
            if (document.RootElement.TryGetProperty("exp", out var expElement))
            {
                var exp = expElement.GetInt64();
                var expDateTime = DateTimeOffset.FromUnixTimeSeconds(exp);
                return expDateTime <= DateTimeOffset.UtcNow;
            }
            
            return true;
        }
        catch
        {
            return true;
        }
    }
} 