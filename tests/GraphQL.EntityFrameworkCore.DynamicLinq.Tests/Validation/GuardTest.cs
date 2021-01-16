using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Xunit;
using GraphQL.EntityFrameworkCore.DynamicLinq.Validation;
namespace GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Validation
{
    public class GuardTest
    {
        [Fact]
        public void Guard_NotNullOrEmptyArgumentException()
        {
            Assert.Throws<ArgumentException>(() => TestNotNullOrEmpty(""));
        }
        [Fact]
        public void Guard_NotNullOrEmpty_ReturnsSameStringIfNotNull()
        {
            string source = "Test String";
            var test = TestNotNullOrEmpty(source);
            Assert.Equal(source, test);
        }

        private string TestNotNullOrEmpty(string testParameter)
        {
            return Guard.NotNullOrEmpty(testParameter, nameof(testParameter));
        }
        [Fact]
        public void Guard_ListNotNullOrEmptyArgumentException()
        {

            Assert.Throws<ArgumentException>(() => TestListNotNullOrEmpty(new List<string>()));
        }
        [Fact]
        public void Guard_NotNullOrEmpty_ReturnsSameListIfNotNullOrEmpty()
        {
            var source = new List<string>() { "Test" };
            var test = TestListNotNullOrEmpty(source);
            Assert.Equal(source, test);
        }
        private IList<T> TestListNotNullOrEmpty<T>(IList<T> testList)
        {
            return Guard.NotNullOrEmpty(testList, nameof(testList));
        }
    }
}
