using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Web;
using System.Web.Routing;
using System.Reflection;

namespace UrlsAndRoutes.Tests
{
    [TestClass]
    public class RouteTests
    {
        private HttpContextBase CreateHttpContext(string targetUrl = null, string httpMethod = "GET")
        {
            Mock<HttpRequestBase> mockRequest = new Mock<HttpRequestBase>();
            mockRequest.Setup(r => r.AppRelativeCurrentExecutionFilePath)
                .Returns(targetUrl);
            mockRequest.Setup(r => r.HttpMethod).Returns(httpMethod);

            Mock<HttpResponseBase> mockResponse = new Mock<HttpResponseBase>();
            mockResponse.Setup(r => r.ApplyAppPathModifier(It.IsAny<string>()))
                .Returns<string>(s => s);

            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>();
            mockContext.Setup(c => c.Request).Returns(mockRequest.Object);
            mockContext.Setup(c => c.Response).Returns(mockResponse.Object);

            return mockContext.Object;
        }

        private void TestRouteMatch(string url, string controller, string action,
            object routeProperties = null, string httpMethod = "GET")
        {
            RouteCollection routes = new RouteCollection();
            RouteConfig.RegisterRoutes(routes);

            RouteData result = routes.GetRouteData(CreateHttpContext(url, httpMethod));

            Assert.IsNotNull(result);
            Assert.IsTrue(TestIncomingRouteResult(result, controller, action, routeProperties));
        }

        private void TestRouteFail(string url)
        {
            RouteCollection collection = new RouteCollection();
            RouteConfig.RegisterRoutes(collection);

            RouteData result = collection.GetRouteData(CreateHttpContext(url));
            Assert.IsTrue(result == null || result.Route == null);
        }

        [TestMethod]
        public void TestIncomingRoutes()
        {
            TestRouteMatch("~/Admin/Index", "Admin", "Index");
            TestRouteMatch("~/A/B", "A", "B");

            TestRouteFail("~/A");
            TestRouteFail("~/A/B/C");
        }

        private bool TestIncomingRouteResult(RouteData routeResult, string controller,
            string action, object propertySet = null)
        {
            Func<object, object, bool> valCompare = (v1, v2) =>
              {
                  return StringComparer.InvariantCultureIgnoreCase.Compare(v1, v2) == 0;
              };

            bool result = valCompare(routeResult.Values["controller"], controller)
                && valCompare(routeResult.Values["action"], action);
            if (propertySet != null)
            {
                PropertyInfo[] propInfo = propertySet.GetType().GetProperties();
                foreach (PropertyInfo pi in propInfo)
                {
                    if (!(routeResult.Values.ContainsKey(pi.Name)
                        && valCompare(routeResult.Values[pi.Name], pi.GetValue(propertySet, null))))
                    {
                        result = false;
                        break;
                    }
                }
            }
            return result;
        }
    }
}
