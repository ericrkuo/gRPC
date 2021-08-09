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

        public override async Task ServerSideStream(SampleRequest request, IServerStreamWriter<SampleReply> responseStream, ServerCallContext context)
        {
            _logger.LogInformation("Received server streaming call from client with name {0}", request.Name);

            foreach (char c in request.Name.ToCharArray())
            {
                if (context.CancellationToken.IsCancellationRequested) 
                {
                    _logger.LogWarning("Received cancellation request");
                    return;
                };

                var reply = new SampleReply()
                {
                    Message = string.Format("Streaming letter by letter: {0}", c)
                };
                await responseStream.WriteAsync(reply);
                await Task.Delay(1000);
            }
        }

        public override async Task<SampleReply> ClientSideStream(IAsyncStreamReader<SampleRequest> requestStream, ServerCallContext context)
        {
            _logger.LogInformation("Received client streaming call from client");

            var result = string.Empty;

            await foreach (var request in requestStream.ReadAllAsync())
            {
                _logger.LogInformation("Client streaming request with name: {0}", request.Name);
                result += request.Name;
            }

            return new SampleReply { Message = result };
        }

        /// <summary>
        /// Reads client stream, cumuluates messages, and streams back to client
        /// </summary>
        public override async Task BidirectionalStream(IAsyncStreamReader<SampleRequest> requestStream, IServerStreamWriter<SampleReply> responseStream, ServerCallContext context)
        {
            var cumulativeResult = string.Empty;

            await foreach (var request in requestStream.ReadAllAsync())
            {
                _logger.LogInformation("Server received request with name: {0}", request.Name);
                
                cumulativeResult += string.Format("{0}, ", request.Name);
                await Task.Delay(1000);
                await responseStream.WriteAsync(new SampleReply { Message = cumulativeResult });
            }
        }
    }
}
