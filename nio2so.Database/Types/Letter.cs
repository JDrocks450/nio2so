using nio2so.DataService.Common.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.DataService.Common.Types
{
    public record Letter(AvatarIDToken SenderID, AvatarIDToken ReceiverID, string SenderDisplayName, string Title, string Body, DateTime SentTime)
    {
    }
}
