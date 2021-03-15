using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Uppgift2.Core.Interfaces;
using Uppgift2.Core.Services;

namespace Uppgift2.Core
{
    public class RegisterServicesComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Register<IEmailService, EmailService>(Lifetime.Request);
        }
    }
}
