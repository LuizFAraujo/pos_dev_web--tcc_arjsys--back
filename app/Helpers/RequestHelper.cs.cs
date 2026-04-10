using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Http;

namespace Api_ArjSys_Tcc.Helpers;

/// <summary>
/// Métodos utilitários para análise de requests HTTP.
/// </summary>
public static class RequestHelper
{
    /// <summary>
    /// Verifica se o request veio da própria máquina onde o backend roda.
    /// Funciona com localhost, hostname, IP de rede, ou qualquer DNS que aponte pra cá.
    /// Lê X-Forwarded-For primeiro (Caddy reverse proxy), senão usa RemoteIpAddress.
    /// </summary>
    public static bool IsLocalRequest(HttpContext context)
    {
        // Caddy (reverse proxy) repassa o IP real no X-Forwarded-For
        var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();

        if (!string.IsNullOrEmpty(forwarded))
        {
            var ip = forwarded.Split(',')[0].Trim();

            if (IPAddress.TryParse(ip, out var parsedIp))
                return IsLocalIp(parsedIp);

            return false;
        }

        // Fallback: acesso direto sem proxy
        var remoteIp = context.Connection.RemoteIpAddress;
        if (remoteIp == null) return false;

        return IsLocalIp(remoteIp);
    }

    /// <summary>
    /// Verifica se o IP é da própria máquina (loopback ou qualquer interface de rede local).
    /// </summary>
    private static bool IsLocalIp(IPAddress ip)
    {
        // Loopback (127.0.0.1 / ::1)
        if (IPAddress.IsLoopback(ip))
            return true;

        // Compara com todos os IPs das interfaces de rede da máquina
        try
        {
            var localIps = Dns.GetHostAddresses(Dns.GetHostName());

            foreach (var localIp in localIps)
            {
                if (ip.Equals(localIp))
                    return true;
            }
        }
        catch { }

        return false;
    }
}