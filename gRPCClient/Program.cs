using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using gRPCCommon;

namespace GrpcGreeterClient
{
    class Program
    {
        private static CancellationTokenSource TokenSource { get; set; }

        static async Task Main(string[] args)
        {
            Console.Write("Select type of call: Unary[0], Server[1], Client[2], Bidirectional[3]: ");
            String option = Console.ReadLine();

            var defaultMethodConfig = new MethodConfig
            {
                Names = { MethodName.Default },
                RetryPolicy = new RetryPolicy
                {
                    MaxAttempts = 5,
                    InitialBackoff = TimeSpan.FromSeconds(1),
                    MaxBackoff = TimeSpan.FromSeconds(5),
                    BackoffMultiplier = 1.5,
                    RetryableStatusCodes = { StatusCode.Unavailable }
                }
            };

            // The port number(5001) must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions
            {
                ServiceConfig = new ServiceConfig { MethodConfigs = { defaultMethodConfig } }
            });

            var client = new gRPCDemo.gRPCDemoClient(channel);

            TokenSource = new CancellationTokenSource();

            Console.CancelKeyPress += delegate
            {
                Console.WriteLine("Cancelling token...");
                TokenSource.Cancel();
                Console.WriteLine("Token cancelled");
            };

            switch (option)
            {
                case "0":
                    await UnaryCallDemo(client);
                    break;
                case "1":
                    await ServerStreamDemo(client);
                    break;
                case "2":
                    await ClientStreamDemo(client);
                    break;
                case "3":
                    await BidirectionalStreamDemo(client);
                    break;
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static async Task BidirectionalStreamDemo(gRPCDemo.gRPCDemoClient client)
        {
            using var call = client.BidirectionalStream(cancellationToken: TokenSource.Token);

            var responseReaderTask = Task.Run(async () =>
            {
                await foreach (var reply in call.ResponseStream.ReadAllAsync(cancellationToken: TokenSource.Token))
                {
                    Console.WriteLine("Server is streaming message to client: {0}", reply.Message);
                }
            });

            string[] messages = { "hi", "my", "name", "is", "GreeterClient" };

            foreach (var message in messages)
            {
                await call.RequestStream.WriteAsync(new SampleRequest { Name = message });
                await Task.Delay(2000);
            }

            await Task.WhenAll(call.RequestStream.CompleteAsync(), responseReaderTask);
        }

        private static async Task ClientStreamDemo(gRPCDemo.gRPCDemoClient client)
        {
            using var call = client.ClientSideStream(cancellationToken: TokenSource.Token);

            string[] messages = { "hi", "my", "name", "is", "GreeterClient" };

            foreach (var message in messages)
            {
                await call.RequestStream.WriteAsync(new SampleRequest { Name = message });
                await Task.Delay(1000);
            }

            await call.RequestStream.CompleteAsync();
            var reply = await call.ResponseAsync;

            Console.WriteLine("Client received reply with message: {0}", reply.Message);
        }

        private static async Task ServerStreamDemo(gRPCDemo.gRPCDemoClient client)
        {
            using var call = client.ServerSideStream(new SampleRequest { Name = "GreeterClient" }, cancellationToken: TokenSource.Token);

            await foreach (var reply in call.ResponseStream.ReadAllAsync(cancellationToken: TokenSource.Token))
            {
                Console.WriteLine("Server is streaming message to client: {0}", reply.Message);
            }
        }

        private static async Task UnaryCallDemo(gRPCDemo.gRPCDemoClient client)
        {
            var reply = await client.UnaryCallAsync(new SampleRequest { Name = "SampleName" });
            Console.WriteLine("Client received reply with message: {0}", reply.Message);
        }

        private static async Task GreeterClient(GrpcChannel channel)
        {
            var client = new Greeter.GreeterClient(channel);
            var reply = await client.SayHelloAsync(
                              new HelloRequest { Name = "GreeterClient" },
                              cancellationToken: TokenSource.Token);
            Console.WriteLine("Greeting: " + reply.Message);
        }
    }
}