using Microsoft.AspNetCore.Authentication;

namespace Tests.Integration.TestAuth
{
    public class BasicAuthOptions : AuthenticationSchemeOptions
    {
        public string Realm { get; set; }

        public new BasicAuthEvents Events
        {
            get
            {
                return (BasicAuthEvents)base.Events;
            }

            set
            {
                base.Events = value;
            }
        }

        public BasicAuthOptions()
        {
            Realm = BasicAuthenticationDefaults.Realm;
            this.Events = new BasicAuthEvents();
        }

    }
}