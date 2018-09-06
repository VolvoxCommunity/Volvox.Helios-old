using System;
using System.Threading.Tasks;

namespace Tests.Integration.TestAuth
{
    public class BasicAuthEvents
    {
        public Func<ValidatePrincipalContext, Task> OnValidatePrincipal { get; set; } = context => Task.CompletedTask;

        public virtual Task ValidatePrincipalAsync(ValidatePrincipalContext context) => this.OnValidatePrincipal(context);
    }
}