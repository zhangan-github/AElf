﻿using System;
using System.IO;
using AElf.Database;
using AElf.Database.Config;
using AElf.Kernel;
using AElf.Kernel.Modules.AutofacModule;
using AElf.Kernel.Node;
using Autofac;
using Autofac.Core;

namespace AElf.Benchmark
{
    public class Program
    {
        
        public static void Main()
        {
            Hash chainId = Hash.Generate();
            var builder = new ContainerBuilder();
            builder.RegisterModule(new MainModule());
            builder.RegisterModule(new MetadataModule(chainId));
            var dataConfig = new DatabaseConfig();
            dataConfig.Type = DatabaseType.Ssdb;
            dataConfig.Host = "192.168.197.28";
            dataConfig.Port = 8888;
            builder.RegisterModule(new DatabaseModule(new DatabaseConfig()));
            builder.RegisterType<Benchmarks>().WithParameter("chainId", chainId).WithParameter("maxTxNum", 2);
            var container = builder.Build();
            
            if (container == null)
            {
                Console.WriteLine("IoC setup failed");
                return;
            }

            if (!CheckDBConnect(container))
            {
                Console.WriteLine("Database connection failed");
                return;
            }
            
            using(var scope = container.BeginLifetimeScope())
            {
                var benchmarkTps = scope.Resolve<Benchmarks>();

                
                /*
                 var baseline = benchmarkTps.SingleGroupBenchmark(2000, 1).Result;
                Console.WriteLine("Base line");
                foreach (var kv in baseline)
                {
                    Console.WriteLine(kv.Key + ": " + kv.Value);
                }
                var baseline = benchmarkTps.SingleGroupBenchmark(3000, 1).Result;
                Console.WriteLine("Base line");
                foreach (var kv in baseline)
                {
                    Console.WriteLine(kv.Key + ": " + kv.Value);
                }
                Console.WriteLine("Base line");
                foreach (var kv in baseline)
                {
                    Console.WriteLine(kv.Key + ": " + kv.Value);
                }

                for (double i = 0; i < 1; i+= 0.2)
                {
                    var resDict = benchmarkTps.SingleGroupBenchmark(3000, 0).Result;

                    Console.WriteLine("--------------------\n" + "Tx count: " + 3000 + "| Conflict rate: " + i);
                    foreach (var kv in resDict)
                    {
                        Console.WriteLine(kv.Key + ": " + kv.Value);
                    }
                }
                */
                var multiGroupRes = benchmarkTps.MultipleGroupBenchmark(1, 1).Result;
            }
        }
        
        private static bool CheckDBConnect(IContainer container)
        {
            var db = container.Resolve<IKeyValueDatabase>();
            return db.IsConnected();
        }
    }
}