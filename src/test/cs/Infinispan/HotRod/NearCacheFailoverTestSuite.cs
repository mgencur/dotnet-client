using Infinispan.HotRod.Tests;
using Infinispan.HotRod.Tests.Util;
using NUnit.Framework;
using System.Collections;

namespace Infinispan.HotRod.TestSuites
{
    public class NearCacheFailoverTestSuite
    {
        internal static HotRodServer server1;
        internal static HotRodServer server2;

        [TestFixtureSetUp]
        public void BeforeSuite()
        {
            server1 = new HotRodServer("clustered.xml");
            server1.StartHotRodServer();
            server2 = new HotRodServer("clustered.xml", "-Djboss.socket.binding.port-offset=100", 11322);
            server2.StartHotRodServer();
        }

        [TestFixtureTearDown]
        public void AfterSuite()
        {
            if (server1.IsRunning(2000))
                server1.ShutDownHotrodServer();
            if (server2.IsRunning(2000))
                server2.ShutDownHotrodServer();
        }

        [Suite]
        public static IEnumerable Suite
        {
            get
            {
                var suite = new ArrayList();
                suite.Add(new NearCacheFailoverTest());
                return suite;
            }
        }
    }
}