using Exico.Shopify.Web.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Test.Exico.Shopify.Web.Core
{
    [Collection("Sequential")]
    public class Test_Hash_helper
    {

        public Test_Hash_helper()
        {
            this.HashValue = "21232f297a57a5a743894a0e4a801fc3";
            this.HashString = "admin";
        }

        public string HashValue { get; }
        public string HashString { get; }

        [Fact]
        public void Test_MD5_Generation()
        {
            Assert.Equal(this.HashValue.ToLower(), HashHelper.CreateMD5(this.HashString).ToLower());
        }
    }
}
