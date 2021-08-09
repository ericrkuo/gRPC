using Grpc.Core;
using gRPCCommon;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace gRPCServer.Services
{
    public class GrpcDemoService: gRPCDemo.gRPCDemoBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GrpcDemoService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<SampleReply> UnaryCall(SampleRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Received unary call from client with name {0}", request.Name);
            return Task.FromResult(new SampleReply
            {
                Message = "Hello " + request.Name
            });
        }
    }
}
