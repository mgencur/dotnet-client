using System;
using NUnit.Framework;
using Infinispan.HotRod.Config;

namespace Infinispan.HotRod.Tests
{
    class SSLTest
    {
        [Test]
        public void SSLSuccessfullServerAndClientAuthTest()
        {
            ConfigurationBuilder conf = new ConfigurationBuilder();
            conf.AddServer().Host("127.0.0.1").Port(11222).ConnectionTimeout(90000).SocketTimeout(900);
            conf.Marshaller(new JBasicMarshaller());
            registerServerCAFile(conf, "infinispan-ca.pem");
            registerClientCertificateFile(conf, "client-ca.pem");

            RemoteCacheManager remoteManager = new RemoteCacheManager(conf.Build(), true);
            IRemoteCache<string, string> testCache = remoteManager.GetCache<string, string>();

            testCache.Clear();
            string k1 = "key13";
            string v1 = "boron";
            testCache.Put(k1, v1);
            Assert.AreEqual(v1, testCache.Get(k1));
        }

        [Test]
        [Ignore("HRCPP-284")]
        [ExpectedException(typeof(Infinispan.HotRod.Exceptions.TransportException), ExpectedMessage = "SSL_get_peer_certificate")]
        public void SSLClientExpectingDifferentServerIdentityTest()
        {
            ConfigurationBuilder conf = new ConfigurationBuilder();
            conf.AddServer().Host("127.0.0.1").Port(11222).ConnectionTimeout(90000).SocketTimeout(900);
            conf.Marshaller(new JBasicMarshaller());
            registerServerCAFile(conf, "malicious.pem");
            registerClientCertificateFile(conf, "client-ca.pem");

            RemoteCacheManager remoteManager = new RemoteCacheManager(conf.Build(), true);
            IRemoteCache<string, string> testCache = remoteManager.GetCache<string, string>();
            
            testCache.Clear();
            string k1 = "key13";
            string v1 = "boron";
            testCache.Put(k1, v1);
            Assert.Fail("Should not get here");
        }

        [Test]
        [Ignore("HRCPP-284")]
        [ExpectedException(typeof(Infinispan.HotRod.Exceptions.TransportException), ExpectedMessage = "SSL_CTX_load_verify_locations")]
        public void SSLClientNotUsingCertificatesTest()
        {
            ConfigurationBuilder conf = new ConfigurationBuilder();
            conf.AddServer().Host("127.0.0.1").Port(11222).ConnectionTimeout(90000).SocketTimeout(900);
            conf.Marshaller(new JBasicMarshaller());
            conf.Ssl().Enable();
            RemoteCacheManager remoteManager = new RemoteCacheManager(conf.Build(), true);
            IRemoteCache<string, string> testCache = remoteManager.GetCache<string, string>();

            testCache.Clear();
            string k1 = "key13";
            string v1 = "boron";
            testCache.Put(k1, v1);
            Assert.Fail("Should not get here");
        }

        [Test]
        [Ignore("HRCPP-284")]
        public void SSLMaliciousClientTest()
        {
            ConfigurationBuilder conf = new ConfigurationBuilder();
            conf.AddServer().Host("127.0.0.1").Port(11222).ConnectionTimeout(90000).SocketTimeout(900);
            conf.Marshaller(new JBasicMarshaller());
            registerServerCAFile(conf, "infinispan-ca.pem");
            registerClientCertificateFile(conf, "malicious.pem");

            RemoteCacheManager remoteManager = new RemoteCacheManager(conf.Build(), true);
            IRemoteCache<string, string> testCache = remoteManager.GetCache<string, string>();

            testCache.Clear();
            string k1 = "key13";
            string v1 = "boron";
            testCache.Put(k1, v1);
            Assert.AreEqual(v1, testCache.Get(k1));
        }

        void registerServerCAFile(ConfigurationBuilder conf, string filename = "")
        {
            SslConfigurationBuilder sslConfB = conf.Ssl();
            if (filename != "")
            {
                checkFileExists(filename);
                sslConfB.Enable().ServerCAFile(filename);
            }
        }

        void registerClientCertificateFile(ConfigurationBuilder conf, string filename = "")
        {
            SslConfigurationBuilder sslConfB = conf.Ssl();
            if (filename != "")
            {
                checkFileExists(filename);
                sslConfB.Enable().ClientCertificateFile(filename);
            }
        }

        void checkFileExists(string filename)
        {
            if (!System.IO.File.Exists(filename))
            {
                Console.WriteLine("File not found: " + filename);
                Environment.Exit(-1);
            }
        }
    }
}
