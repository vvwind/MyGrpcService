using System;
using Billing;
using Grpc.Core;
using Microsoft.Extensions.Logging;
namespace GrpcServer.Services
{

    public class BillingService : Billing.Billing.BillingBase
    {
        float rates = 0;
        static List<Coin> coins = new List<Coin>();
        static List<UserProfile> users = new List<UserProfile>()
            {
                new UserProfile
                {
                    Name = "boris",
                    Rating = 5000,
                    Amount = 0,
                    Info = "",

                },

                 new UserProfile
                {
                    Name = "maria",
                    Rating = 1000,
                    Amount = 0,
                    Info = "",
                },

                  new UserProfile
                {
                    Name = "oleg",
                    Rating = 800,
                    Amount = 0,
                    Info = "",
                },

            };

      
        
        private readonly ILogger<BillingService> _logger;

        public BillingService(ILogger<BillingService> logger)
        {
            _logger = logger;

        }

        


        public override async Task ListUsers(None request, IServerStreamWriter<UserProfile> responseStream, ServerCallContext context)
        {

            foreach (var user in users)
            {
                await responseStream.WriteAsync(user);
            }
            
        }


        public override Task<Response> CoinsEmission(EmissionAmount request, ServerCallContext context)
        {
            Response output = new Response();
  
            output.Status =  Response.Types.Status.Ok;
          
            foreach(var user in users)
            {
                rates += user.Rating;
            }
            foreach (var user in users)
            {
                var my_amount = (request.Amount * user.Rating) / rates;
                my_amount = (float)(Math.Round(my_amount, 1));
                if (my_amount < 0.5)
                {
                    user.Amount += 1;
                    request.Amount -= 1;
                    
                }

            }
            foreach (var user in users)
            {
                var my_amount = (request.Amount * user.Rating) / rates;
                my_amount = (float)(Math.Round(my_amount, 1));
                
                if ((my_amount - (float)(Math.Truncate(my_amount))) >= 0.5)
                {
                    my_amount += 1;
                }
                int int_amount = (int)my_amount;
                if (user.Amount == 0)
                    user.Amount = int_amount;
               
                user.Info += int_amount.ToString();
                for (int i = 0; i < user.Amount; i++)
                {
                    Random rnd = new Random();
                    int numb = rnd.Next(1, 999999);
                    coins.Add(new Billing.Coin
                    {
                        Id = numb,

                        History = "1",
                    });
                    user.Info += $"{numb.ToString()}id";
                }

                

            }
            
            output.Comment = $"Emission {request.Amount} coins";
            
            return Task.FromResult(output);

        }


        public override Task<Response> MoveCoins(MoveCoinsTransaction request, ServerCallContext context)
        {
            Response output = new Response();
          
            foreach (var user in users)
            {

                if (user.Name == request.SrcUser)
                {
                    if (user.Amount >= request.Amount)
                    {
                       
                        long counter = request.Amount;
                        output.Status = Response.Types.Status.Ok;
                        output.Comment = $"moved {request.Amount} coins from {request.SrcUser} to {request.DstUser}";
                        user.Amount -= request.Amount;
                        string temp = user.Amount.ToString();
                        temp += user.Info.Substring(1);
                        user.Info = temp;
                        temp = "";
                        string temp2 = "";
                        string temp_name = request.DstUser;

                        for (int i = 1; i < user.Info.Length; i++)
                        {

                            if (counter == 0)
                            {
                                foreach (var user1 in users)
                                {

                                    if (user1.Name == temp_name)
                                    {
                                        user1.Amount += request.Amount;
                                        string tempa = user1.Amount.ToString();
                                        user1.Info += $"{user.Info.Substring(1, i)}";
                                        tempa += user1.Info.Substring(1);
                                        user1.Info = tempa;
                                        user.Info = user.Info.Remove(1, i);
                                        Console.WriteLine($"Amount {request.Amount}");
                                        Console.WriteLine("TO");
                                        Console.WriteLine($"{user1.Name} + {user1.Amount} + {user1.Info}");
                                        Console.WriteLine("FROM");
                                        Console.WriteLine($"{user.Name} + {user.Amount} + {user.Info}");

                                    }
                                }
                                break;
                            }
                            temp += user.Info[i];
                            temp2 += user.Info[i];
                            if (user.Info[i] == 'i')
                            {
                                counter -= 1;
                                temp2 = "";
                            }
                            else if (user.Info[i + 1] == 'i')
                            {
                                if (counter > 0)
                                {
                                    foreach (var coin in coins)
                                    {

                                        if (coin.Id.ToString() == temp2)
                                        {
                                            int temp_history = Int32.Parse(coin.History);
                                            temp_history += 1;
                                            coin.History = temp_history.ToString();

                                        }
                                    }
                                }
                            }
                            else if (user.Info[i] == 'd')
                            {
                                temp2 = "";
                            }
                        }
                    }
                    else 
                    {
                        output.Status = Response.Types.Status.Failed;
                        output.Comment = $"Coins movement failed";
                    }
                    break;
                }
                else
                {
                    output.Status = Response.Types.Status.Unspecified;
                    output.Comment = $"Coins movement failed";
                }
            }

           
            return Task.FromResult(output);
     
        }

        public override Task<Coin> LongestHistoryCoin(None request, ServerCallContext context)
        {
            int longest_history = 0;
            long longest_id = 0;
            foreach (var coin in coins)
            {
                if(Int32.Parse(coin.History) > longest_history)
                {
                    longest_history = Int32.Parse(coin.History);
                    longest_id = coin.Id;
                }
            }

            var new_coin = new Billing.Coin { History = longest_history.ToString(), Id = longest_id };
            Console.WriteLine($"Longest Coin {new_coin}");
            return Task.FromResult(new_coin);
        }



    } 
}
