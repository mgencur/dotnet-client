using Infinispan.HotRod.Config;
using NUnit.Framework;
using Infinispan.HotRod.TestSuites;

namespace Infinispan.HotRod.Tests
{
    class NearCacheFailoverTest
    {
        private IRemoteCache<string, string> cache1;
        private IRemoteCache<string, string> cache2;

        [TestFixtureSetUp]
        public void BeforeClass()
        {
            IMarshaller marshaller = new JBasicMarshaller();
            ConfigurationBuilder conf1 = new ConfigurationBuilder();
            conf1.Marshaller(marshaller)
                 .AddServer().Host("127.0.0.1").Port(11222)
                 .NearCache().Mode(NearCacheMode.INVALIDATED).MaxEntries(-1);
            RemoteCacheManager manager1 = new RemoteCacheManager(conf1.Build(), true);
            cache1 = manager1.GetCache<string, string>();

            ConfigurationBuilder conf2 = new ConfigurationBuilder();
            conf2.Marshaller(marshaller)
                 .AddServer().Host("127.0.0.1").Port(11322)
                 .NearCache().Mode(NearCacheMode.INVALIDATED).MaxEntries(-1);
            RemoteCacheManager remoteManager = new RemoteCacheManager(conf2.Build(), true);
            cache2 = remoteManager.GetCache<string, string>();
        }

        [Test]
        public void ClientsInvalidatedTest()
        {
            cache1.Clear();
            cache2.Clear();

            for (int i=0; i != 100; i++)
            {
                cache1.Put("k" + i, "v" + i);
                Assert.AreEqual("v" + i, cache1.Get("k" + i)); //fill near cache1
                Assert.AreEqual("v" + i, cache2.Get("k" + i)); //fill near cache2
            }
            System.Console.WriteLine();
            System.Console.WriteLine("client1 hits: " + cache1.Stats().GetIntStatistic("hits"));
            System.Console.WriteLine("client2 hits: " + cache2.Stats().GetIntStatistic("hits"));

            for (int i = 0; i != 100; i++)
            {
                Assert.AreEqual("v" + i, cache1.Get("k" + i));
                Assert.AreEqual("v" + i, cache2.Get("k" + i));
            }
            System.Console.WriteLine("client1 hits: " + cache1.Stats().GetIntStatistic("hits"));
            System.Console.WriteLine("client2 hits: " + cache2.Stats().GetIntStatistic("hits"));

            NearCacheFailoverTestSuite.server1.ShutDownHotrodServer();

            for (int i = 0; i != 100; i++)
            {
                Assert.AreEqual("v" + i, cache1.Get("k" + i)); //fill near cache1
                Assert.AreEqual("v" + i, cache2.Get("k" + i)); //fill near cache2
            }

            System.Console.WriteLine("client1 hits: " + cache1.Stats().GetIntStatistic("hits"));
            System.Console.WriteLine("client2 hits: " + cache2.Stats().GetIntStatistic("hits"));
        }
    }
}