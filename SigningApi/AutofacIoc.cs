using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Autofac;
using Autofac.Integration.Mvc;
using Funq;
using ServiceStack.Configuration;

namespace Outercurve.SigningApi
{
    /// <summary>
    /// 
    /// </summary>
    /// <from>http://stackoverflow.com/questions/14835471/configuring-lifetime-scopes-in-autofac-when-used-as-servicestacks-ioc</from>
    public class AutofacIocAdapter : IContainerAdapter
    {
        private readonly IContainer _autofacRootContainer;
        private readonly Container _funqContainer;

        public AutofacIocAdapter(IContainer autofacRootContainer, Container funqContainer)
        {
            // Register a RequestLifetimeScopeProvider (from Autofac.Integration.Mvc) with Funq
            var lifetimeScopeProvider = new RequestLifetimeScopeProvider(autofacRootContainer);
            funqContainer.Register<ILifetimeScopeProvider>(x => lifetimeScopeProvider);
            // Store the autofac application (root) container, and the funq container for later use
            _autofacRootContainer = autofacRootContainer;
            _funqContainer = funqContainer;
        }

        public T Resolve<T>()
        {
            return ActiveScope.Resolve<T>();
        }

        public T TryResolve<T>()
        {
            T result;
            if (ActiveScope.TryResolve(out result))
            {
                return result;
            }
            return default(T);
        }

        private ILifetimeScope ActiveScope
        {
            get
            {
                // If there is an active HttpContext, retrieve the lifetime scope by resolving
                // the ILifetimeScopeProvider from Funq.  Otherwise, use the application (root) container.
                return HttpContext.Current == null
                    ? _autofacRootContainer
                    : _funqContainer.Resolve<ILifetimeScopeProvider>().GetLifetimeScope(x => new ContainerBuilder());
            }
        }
    }
}