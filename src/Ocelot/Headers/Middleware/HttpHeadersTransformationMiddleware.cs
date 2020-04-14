namespace Ocelot.Headers.Middleware
{
    using Infrastructure.RequestData;
    using Microsoft.AspNetCore.Http;
    using Ocelot.Logging;
    using Ocelot.Middleware;
    using System.Threading.Tasks;

    public class HttpHeadersTransformationMiddleware : OcelotMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHttpContextRequestHeaderReplacer _preReplacer;
        private readonly IHttpResponseHeaderReplacer _postReplacer;
        private readonly IAddHeadersToResponse _addHeadersToResponse;
        private readonly IAddHeadersToRequest _addHeadersToRequest;

        public HttpHeadersTransformationMiddleware(RequestDelegate next,
            IOcelotLoggerFactory loggerFactory,
            IHttpContextRequestHeaderReplacer preReplacer,
            IHttpResponseHeaderReplacer postReplacer,
            IAddHeadersToResponse addHeadersToResponse,
            IAddHeadersToRequest addHeadersToRequest,
            IRequestScopedDataRepository repo
            )
                : base(loggerFactory.CreateLogger<HttpHeadersTransformationMiddleware>(), repo)
        {
            _addHeadersToResponse = addHeadersToResponse;
            _addHeadersToRequest = addHeadersToRequest;
            _next = next;
            _postReplacer = postReplacer;
            _preReplacer = preReplacer;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var preFAndRs = DownstreamContext.Data.DownstreamReRoute.UpstreamHeadersFindAndReplace;

            //todo - this should be on httprequestmessage not httpcontext?
            _preReplacer.Replace(httpContext, preFAndRs);

            _addHeadersToRequest.SetHeadersOnDownstreamRequest(DownstreamContext.Data.DownstreamReRoute.AddHeadersToUpstream, httpContext);

            await _next.Invoke(httpContext);

            // todo check errors is ok
            //todo put this check on the base class?
            if (Errors.Data.Count > 0)
            {
                return;
            }

            var postFAndRs = DownstreamContext.Data.DownstreamReRoute.DownstreamHeadersFindAndReplace;

            _postReplacer.Replace(DownstreamContext.Data, httpContext, postFAndRs);

            _addHeadersToResponse.Add(DownstreamContext.Data.DownstreamReRoute.AddHeadersToDownstream, DownstreamContext.Data.DownstreamResponse);
        }
    }
}
