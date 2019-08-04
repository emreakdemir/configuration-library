using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using CodeSide.Domain.Concrete.Model;
using CodeSide.Redis.Abstract;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CodeSide.ConfigurationApi.ActionFilters
{
    public class OperationLogActionFilter : IActionFilter
    {
        private IRedisHashManager RedisManager { get; }

        public OperationLogActionFilter(IRedisHashManager redisManager)
        {
            this.RedisManager = redisManager;
        }

        private static IPHostEntry IpHostInfo => Dns.GetHostEntry(Dns.GetHostName());

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;

            var ipAddress = IpHostInfo.AddressList.Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                                .Select(address => address.ToString())
                                .FirstOrDefault();

            var operationLogModel = new OperationLogModel
                                    {
                                        IpAddress = ipAddress,
                                        RequestedController = (string)context.RouteData.Values["controller"],
                                        RequestedAction =(string)context.RouteData.Values["action"],
                                        RequestMethod = request.Method,
                                        RequestPath = request.Path.ToString(),
                                        OperationDate = DateTime.Now
                                    };

            this.RedisManager.Set(new CacheModel
                                  {
                                      Model = operationLogModel,
                                      Key = operationLogModel.CacheKey,
                                      Renewable = false
                                  });
        }

        public void OnActionExecuted(ActionExecutedContext context)
        { }
    }
}