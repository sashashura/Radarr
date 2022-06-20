using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NLog;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;
using Radarr.Http.Extensions;

namespace Radarr.Http.Authentication
{
    public interface IAuthenticationService
    {
        void LogUnauthorized(HttpRequest context);
        User Login(HttpRequest request, string username, string password);
        void Logout(HttpContext context);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private const string AnonymousUser = "Anonymous";
        private static readonly Logger _authLogger = LogManager.GetLogger("Auth");
        private readonly IUserService _userService;

        private static string API_KEY;
        private static AuthenticationType AUTH_METHOD;

        public AuthenticationService(IOptionsMonitor<ConfigFileOptions> configFileOptions, IUserService userService)
        {
            _userService = userService;
            API_KEY = configFileOptions.CurrentValue.ApiKey;
            AUTH_METHOD = configFileOptions.CurrentValue.AuthenticationMethod;
        }

        public User Login(HttpRequest request, string username, string password)
        {
            if (AUTH_METHOD == AuthenticationType.None)
            {
                return null;
            }

            var user = _userService.FindUser(username, password);

            if (user != null)
            {
                LogSuccess(request, username);

                return user;
            }

            LogFailure(request, username);

            return null;
        }

        public void Logout(HttpContext context)
        {
            if (AUTH_METHOD == AuthenticationType.None)
            {
                return;
            }

            if (context.User != null)
            {
                LogLogout(context.Request, context.User.Identity.Name);
            }
        }

        public void LogUnauthorized(HttpRequest context)
        {
            _authLogger.Info("Auth-Unauthorized ip {0} url '{1}'", context.GetRemoteIP(), context.Path);
        }

        private void LogInvalidated(HttpRequest context)
        {
            _authLogger.Info("Auth-Invalidated ip {0}", context.GetRemoteIP());
        }

        private void LogFailure(HttpRequest context, string username)
        {
            _authLogger.Warn("Auth-Failure ip {0} username '{1}'", context.GetRemoteIP(), username);
        }

        private void LogSuccess(HttpRequest context, string username)
        {
            _authLogger.Info("Auth-Success ip {0} username '{1}'", context.GetRemoteIP(), username);
        }

        private void LogLogout(HttpRequest context, string username)
        {
            _authLogger.Info("Auth-Logout ip {0} username '{1}'", context.GetRemoteIP(), username);
        }
    }
}
