namespace BasketAPI.Services
{
    public class IdentityService : IIdentityService
    {
        private IHttpContextAccessor _contextAccessor;

        public IdentityService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
        public string GetUserIdentity()
        {
            return _contextAccessor.HttpContext.User.FindFirst("sub").Value;
        }
    }
}
