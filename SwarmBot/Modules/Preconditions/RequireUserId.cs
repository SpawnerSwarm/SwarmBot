using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace SwarmBot.Modules
{
    class RequireUserIdAttribute : PreconditionAttribute
    {
        ulong id;

        public RequireUserIdAttribute(ulong id)
        {
            this.id = id;
        }

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            if(context.User.Id == id) { return Task.FromResult(PreconditionResult.FromSuccess()); }
            else { return Task.FromResult(PreconditionResult.FromError($"Command requires user <@{id}>")); }
        }
    }
}
