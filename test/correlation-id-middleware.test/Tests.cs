using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Xunit;
using CorrelationId;
using Microsoft.AspNetCore.TestHost;

namespace CorrelationId.Tests
{
    public class Tests
    {
        [Fact]
        public async void CallMiddleware_WithValidOptions_ThrowsNoException()
        {
            //Arrange
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseCorrelationIdMiddleware(new CorrelationIdMiddlewareOptions());
                    app.UseMiddleware<FakeMiddleware>(TimeSpan.FromMilliseconds(5));
                });

            var server = new TestServer(builder);

            //Act 
            var expectedValue = "{BEBC13D6-7AD0-4CCF-9D86-9B3697A64EAB}";
            var requestMessage = new HttpRequestMessage(new HttpMethod("GET"), "/delay/");
            requestMessage.Headers.TryAddWithoutValidation(CorrelationIdMiddlewareOptions.DefaultHeader, expectedValue);
            var responseMessage = await server.CreateClient().SendAsync(requestMessage);

            //Assert
            Assert.Contains(expectedValue, responseMessage.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public async void CallMiddleware_WithWrongHeaderName_DefaultsToDefaultTraceIdentifier()
        {
            //Arrange
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseCorrelationIdMiddleware(new CorrelationIdMiddlewareOptions());
                    app.UseMiddleware<FakeMiddleware>(TimeSpan.FromMilliseconds(5));
                });

            var server = new TestServer(builder);

            //Act 
            var expectedValue = "{BEBC13D6-7AD0-4CCF-9D86-9B3697A64EAB}";
            var requestMessage = new HttpRequestMessage(new HttpMethod("GET"), "/delay/");
            requestMessage.Headers.TryAddWithoutValidation("X-Custom-Correlation-Id", expectedValue);
            var responseMessage = await server.CreateClient().SendAsync(requestMessage);

            //Assert
            Assert.DoesNotContain(expectedValue, responseMessage.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public async void CallMiddleware_WithWrongConfigurationHeaderName_DefaultsToDefaultTraceIdentifier()
        {
            //Arrange
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseCorrelationIdMiddleware(new CorrelationIdMiddlewareOptions() {Header = "SomeOtherHeader"});
                    app.UseMiddleware<FakeMiddleware>(TimeSpan.FromMilliseconds(5));
                });

            var server = new TestServer(builder);

            //Act 
            var expectedValue = "{BEBC13D6-7AD0-4CCF-9D86-9B3697A64EAB}";
            var requestMessage = new HttpRequestMessage(new HttpMethod("GET"), "/delay/");
            requestMessage.Headers.TryAddWithoutValidation(CorrelationIdMiddlewareOptions.DefaultHeader, expectedValue);
            var responseMessage = await server.CreateClient().SendAsync(requestMessage);

            //Assert
            Assert.DoesNotContain(expectedValue, responseMessage.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public async void CallMiddleware_WithNonDefaultHeaderName_UsesConfiguredHeader()
        {
            //Arrange
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseCorrelationIdMiddleware(new CorrelationIdMiddlewareOptions() { Header = "X-SomeOtherHeader" });
                    app.UseMiddleware<FakeMiddleware>(TimeSpan.FromMilliseconds(5));
                });

            var server = new TestServer(builder);

            //Act 
            var expectedValue = "{BEBC13D6-7AD0-4CCF-9D86-9B3697A64EAB}";
            var requestMessage = new HttpRequestMessage(new HttpMethod("GET"), "/delay/");
            requestMessage.Headers.TryAddWithoutValidation("X-SomeOtherHeader", expectedValue);
            var responseMessage = await server.CreateClient().SendAsync(requestMessage);

            //Assert
            Assert.Contains(expectedValue, responseMessage.Content.ReadAsStringAsync().Result);
        }

    }
}
