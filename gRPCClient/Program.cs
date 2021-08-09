using System;
using System.Threading.Tasks;
using Grpc.Net.Client;
using gRPCCommon;

namespace GrpcGreeterClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Press enter to start");
            Console.ReadLine();

            // The port number(5001) must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");

            var client = new gRPCDemo.gRPCDemoClient(channel);
            var reply = await client.UnaryCallAsync(new SampleRequest { Name = "SampleName" });
            Console.WriteLine("Client received reply with message: {0}", reply.Message);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
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