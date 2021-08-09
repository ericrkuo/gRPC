using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using gRPCCommon;

namespace GrpcGreeterClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Write("Select type of call: Unary[0], Server[1]: ");
            String option = Console.ReadLine();

            // The port number(5001) must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new gRPCDemo.gRPCDemoClient(channel);

            switch (option)
            {
                case "0":
                    await UnaryCallDemo(client);
                    break;
                case "1":
                    await ServerStreamDemo(client);
                    break;
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static async Task ServerStreamDemo(gRPCDemo.gRPCDemoClient client)
        {
            using var call = client.ServerSideStream(new SampleRequest { Name = "GreeterClient" });

            await foreach (var reply in call.ResponseStream.ReadAllAsync())
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
                              new HelloRequest { Name = "GreeterClient" });
            Console.WriteLine("Greeting: " + reply.Message);
        }
    }
}