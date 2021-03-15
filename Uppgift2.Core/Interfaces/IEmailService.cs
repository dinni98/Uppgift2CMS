using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uppgift2.Core.ViewModel;

namespace Uppgift2.Core.Interfaces
{
    public interface IEmailService
    {
        void SendContactNotificationToAdmin(ContactFormViewModel vm);
    }
}
