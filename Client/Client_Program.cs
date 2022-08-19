using System;
using System.Security;
using Billing;
using Grpc.Net.Client;
using GrpcServer;

namespace Client
{
   
    internal class Client_Program
    {

        static async Task Main(string[] args)
        {
            
            List<float> m_list = new List<float>(100);
            float rates = 0;
            string GRPCSERVER = "https://localhost:7183";
            var channel = GrpcChannel.ForAddress(GRPCSERVER);
            var billingClient = new Billing.Billing.BillingClient(channel);
            List<Billing.UserProfile> users = new List<Billing.UserProfile>();
            List<Billing.Coin> coins = new List<Billing.Coin>();
            

            using (var call = billingClient.ListUsers(new Billing.None()))
            {
                while (await call.ResponseStream.MoveNext(default))
                {
                    var current = call.ResponseStream.Current;                   
                    Console.WriteLine(current);
                }
            }

    
            var call2 = billingClient.CoinsEmission(new EmissionAmount { Amount = 10 });
            var call3 = billingClient.MoveCoins(new MoveCoinsTransaction { Amount = 2, DstUser = "maria", SrcUser = "boris" });
            var call4 = billingClient.MoveCoins(new MoveCoinsTransaction { Amount = 4, DstUser = "oleg", SrcUser = "maria" });

            var call5 = billingClient.LongestHistoryCoin(new Billing.None());





            Console.ReadLine();
        }
    }
}