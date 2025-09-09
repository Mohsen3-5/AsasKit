using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asas.Messaging.Domain;
/// <summary>In-process application event, used to decouple modules within the same process.</summary>
public interface IAppEvent : IDomainEvent
{
}

