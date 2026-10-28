#pragma warning disable CS0618 // legacy wrapper under test
using System;
using System.Threading.Tasks;
using HotConfig.Business;
using HotConfig.Business.Base;
using HotConfig.Data.Dapper;
using HotConfig.Domain.Abstract.Base;
using HotConfig.Domain.Concrete.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HotConfig.Tests
{
    [TestClass]
    public class ConfigurationReaderTests
    {
        private IConfigurationReader ConfigurationReader { get; set; }
        private IConfigurationBusiness ConfigurationBusiness { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            try
            {
                var connectionStringConfigModel = new ConnectionStringConfigModel
                                                  {
                                                      MySqlConnectionString = "mysqlconnectionstring",
                                                      RedisConnectionString = "redisconnectionstring"
                                                  };
                this.ConfigurationReader = new ConfigurationReader("SERVICE-A", connectionStringConfigModel, 100);

                this.ConfigurationBusiness = new ConfigurationBusiness(new ConfigurationRepository(connectionStringConfigModel.MySqlConnectionString));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);

                throw;
            }
        }

        [TestMethod]
        public void GetValue_ServiceA_SiteName_String()
        {
            var siteName = this.ConfigurationReader.GetValue<string>("sitename");

            Assert.AreEqual("www.boyner.com.tr", siteName);
        }

        [TestMethod]
        public void GetValue_ServiceA_BasePrice_Double()
        {
            var basePrice = this.ConfigurationReader.GetValue<double>("baseprice");

            const double expected = 100.02;
            Assert.AreEqual(expected, basePrice);
        }

        [TestMethod]
        public void GetValue_ServiceA_MaxThreadCount_Int()
        {
            var maxThreadCount = this.ConfigurationReader.GetValue<int>("maxthreadcount");

            const int expected = 100;
            Assert.AreEqual(expected, maxThreadCount);
        }

        [TestMethod]
        public void GetValue_ServiceA_IsBasketEnabled_Boolean()
        {
            var isBasketEnabled = this.ConfigurationReader.GetValue<bool>("isbasketenabled");

            const bool expected = true;
            Assert.AreEqual(expected, isBasketEnabled);
        }

        [TestMethod]
        public void GetValue_ServiceA_MailEnabled_IsNotActive_ReturnNull()
        {
            var mailEnabled = this.ConfigurationReader.GetValue<int?>("mailenabled");

            int? expected = null;
            Assert.AreEqual(expected, mailEnabled);
        }

        [TestMethod]
        public async Task GetValue_ServiceA_MailEnabled_RefreshValue()
        {
            var mailEnabledConfiguration = await this.GetAsync("mailenabled");

            var mailEnabled = this.ConfigurationReader.GetValue<bool?>("mailenabled");
            Assert.AreEqual(mailEnabledConfiguration.IsActive ? true : (bool?) null, mailEnabled);

            mailEnabledConfiguration.IsActive = !mailEnabledConfiguration.IsActive;
            await this.UpdateAsync(mailEnabledConfiguration);
            mailEnabled = this.ConfigurationReader.GetValue<bool?>("mailenabled");
            Assert.AreEqual(mailEnabledConfiguration.IsActive ? true : (bool?) null, mailEnabled);
        }

        private async Task<ConfigurationModel> GetAsync(string key)
        {
            return await this.ConfigurationBusiness.GetAsync(key);
        }

        private async Task<ConfigurationModel> UpdateAsync(ConfigurationModel model)
        {
            await this.ConfigurationBusiness.UpdateAsync(model);

            return model;
        }

    }
}