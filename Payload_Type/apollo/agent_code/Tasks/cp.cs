﻿#define COMMAND_NAME_UPPER

#if DEBUG
#define CP
#endif

#if CP

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApolloInterop.Classes;
using ApolloInterop.Interfaces;
using ApolloInterop.Structs.MythicStructs;
using System.Runtime.Serialization;
using ApolloInterop.Serializers;
using System.Threading;
using System.IO;

namespace Tasks
{
    public class cp : Tasking
    {
        [DataContract]
        internal struct CpParameters
        {
            [DataMember(Name = "source")]
            public string SourceFile;
            [DataMember(Name = "destination")]
            public string DestinationFile;
        }
        public cp(IAgent agent, Task task) : base(agent, task)
        {

        }

        public override void Kill()
        {
            _cancellationToken.Cancel();
        }

        public override System.Threading.Tasks.Task CreateTasking()
        {
            return new System.Threading.Tasks.Task(() =>
            {
                CpParameters parameters = _jsonSerializer.Deserialize<CpParameters>(_data.Parameters);
                TaskResponse resp;
                List<IMythicMessage> artifacts = new List<IMythicMessage>();
                try
                {
                    FileInfo source = new FileInfo(parameters.SourceFile);
                    artifacts.Add(Artifact.FileOpen(source.FullName));
                    File.Copy(parameters.SourceFile, parameters.DestinationFile);
                    FileInfo dest = new FileInfo(parameters.DestinationFile);
                    artifacts.Add(Artifact.FileWrite(dest.FullName, source.Length));
                    resp = CreateTaskResponse(
                        $"Copied {source.FullName} to {dest.FullName}",
                        true,
                        "completed",
                        artifacts.ToArray());
                } catch (Exception ex)
                {
                    resp = CreateTaskResponse($"Failed to copy file: {ex.Message}", true, "error", artifacts.ToArray());
                }
                _agent.GetTaskManager().AddTaskResponseToQueue(resp);
            }, _cancellationToken.Token);
        }
    }
}
#endif