using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus
{
    public class SubscriptionInfo
    {
        public Type HandlerType { get; }

        private SubscriptionInfo(Type handlerType)
        {
            HandlerType = handlerType;
        }

        public static SubscriptionInfo Create(Type handlerType) => new SubscriptionInfo(handlerType);
    }
}
