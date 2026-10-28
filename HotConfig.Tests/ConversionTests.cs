using System;
using HotConfig.Extensions;
using Xunit;

namespace HotConfig.Tests
{
    public class ConversionTests
    {
        [Fact]
        public void Converts_string() => Assert.Equal("abc", "abc".ConvertTo<string>());

        [Fact]
        public void Converts_int() => Assert.Equal(42, "42".ConvertTo<int>());

        [Theory]
        [InlineData("true", true)]
        [InlineData("False", false)]
        public void Converts_bool(string source, bool expected) => Assert.Equal(expected, source.ConvertTo<bool>());

        [Fact]
        public void Converts_double_with_invariant_culture() => Assert.Equal(1.5, "1.5".ConvertTo<double>());

        [Fact]
        public void Converts_nullable() => Assert.Equal(7, "7".ConvertTo<int?>());

        [Fact]
        public void Empty_or_null_gives_default()
        {
            Assert.Equal(0, "".ConvertTo<int>());
            Assert.Null(((string) null).ConvertTo<int?>());
            Assert.Equal(9, "".ConvertTo(9));
        }

        [Theory]
        [InlineData(typeof(int), "Int32")]
        [InlineData(typeof(int?), "Int32")]
        [InlineData(typeof(bool?), "Boolean")]
        [InlineData(typeof(string), "String")]
        [InlineData(typeof(double), "Double")]
        public void Underlying_type_name(Type type, string name) => Assert.Equal(name, type.GetUnderlyingType().Name);
    }
}
