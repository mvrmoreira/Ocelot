﻿namespace Ocelot.Security.IPSecurity
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.Configuration;
    using Ocelot.Middleware;
    using Ocelot.Responses;
    using System.Net;
    using System.Threading.Tasks;

    public class IPSecurityPolicy : ISecurityPolicy
    {
        public async Task<Response> Security(DownstreamContext downstreamContext, HttpContext httpContext)
        {
            IPAddress clientIp = httpContext.Connection.RemoteIpAddress;
            SecurityOptions securityOptions = downstreamContext.DownstreamReRoute.SecurityOptions;
            if (securityOptions == null)
            {
                return new OkResponse();
            }

            if (securityOptions.IPBlockedList != null)
            {
                if (securityOptions.IPBlockedList.Exists(f => f == clientIp.ToString()))
                {
                    var error = new UnauthenticatedError($" This request rejects access to {clientIp.ToString()} IP");
                    return new ErrorResponse(error);
                }
            }

            if (securityOptions.IPAllowedList != null && securityOptions.IPAllowedList.Count > 0)
            {
                if (!securityOptions.IPAllowedList.Exists(f => f == clientIp.ToString()))
                {
                    var error = new UnauthenticatedError($"{clientIp.ToString()} does not allow access, the request is invalid");
                    return new ErrorResponse(error);
                }
            }

            return await Task.FromResult(new OkResponse());
        }
    }
}
