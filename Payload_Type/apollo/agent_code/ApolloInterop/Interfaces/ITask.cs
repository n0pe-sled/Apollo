﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ApolloInterop.Interfaces
{
    public interface ITask
    {
        string ID();
        Task CreateTasking();
        void Kill();
    }
}
