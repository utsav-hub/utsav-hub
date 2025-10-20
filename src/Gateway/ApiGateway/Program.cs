using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromMemory(
        new[]
        {
            new Yarp.ReverseProxy.Configuration.RouteConfig()
            {
                RouteId = "scheduling",
                ClusterId = "scheduling-cluster",
                Match = new Yarp.ReverseProxy.Configuration.RouteMatch { Path = "/scheduling/{**catch-all}" },
                Transforms = new[] { new Dictionary<string, string> { { "PathRemovePrefix", "/scheduling" } } }
            },
            new Yarp.ReverseProxy.Configuration.RouteConfig()
            {
                RouteId = "booking",
                ClusterId = "booking-cluster",
                Match = new Yarp.ReverseProxy.Configuration.RouteMatch { Path = "/booking/{**catch-all}" },
                Transforms = new[] { new Dictionary<string, string> { { "PathRemovePrefix", "/booking" } } }
            },
            new Yarp.ReverseProxy.Configuration.RouteConfig()
            {
                RouteId = "payment",
                ClusterId = "payment-cluster",
                Match = new Yarp.ReverseProxy.Configuration.RouteMatch { Path = "/payment/{**catch-all}" },
                Transforms = new[] { new Dictionary<string, string> { { "PathRemovePrefix", "/payment" } } }
            },
            new Yarp.ReverseProxy.Configuration.RouteConfig()
            {
                RouteId = "invoice",
                ClusterId = "invoice-cluster",
                Match = new Yarp.ReverseProxy.Configuration.RouteMatch { Path = "/invoice/{**catch-all}" },
                Transforms = new[] { new Dictionary<string, string> { { "PathRemovePrefix", "/invoice" } } }
            }
        },
        new[]
        {
            new Yarp.ReverseProxy.Configuration.ClusterConfig()
            {
                ClusterId = "scheduling-cluster",
                Destinations = new Dictionary<string, Yarp.ReverseProxy.Configuration.DestinationConfig>
                {
                    { "d1", new Yarp.ReverseProxy.Configuration.DestinationConfig { Address = "http://scheduling-api:8080" } }
                }
            },
            new Yarp.ReverseProxy.Configuration.ClusterConfig()
            {
                ClusterId = "booking-cluster",
                Destinations = new Dictionary<string, Yarp.ReverseProxy.Configuration.DestinationConfig>
                {
                    { "d1", new Yarp.ReverseProxy.Configuration.DestinationConfig { Address = "http://booking-api:8080" } }
                }
            },
            new Yarp.ReverseProxy.Configuration.ClusterConfig()
            {
                ClusterId = "payment-cluster",
                Destinations = new Dictionary<string, Yarp.ReverseProxy.Configuration.DestinationConfig>
                {
                    { "d1", new Yarp.ReverseProxy.Configuration.DestinationConfig { Address = "http://payment-api:8080" } }
                }
            },
            new Yarp.ReverseProxy.Configuration.ClusterConfig()
            {
                ClusterId = "invoice-cluster",
                Destinations = new Dictionary<string, Yarp.ReverseProxy.Configuration.DestinationConfig>
                {
                    { "d1", new Yarp.ReverseProxy.Configuration.DestinationConfig { Address = "http://invoice-api:8080" } }
                }
            }
        }
    );

var app = builder.Build();

app.MapReverseProxy();

app.Run();
