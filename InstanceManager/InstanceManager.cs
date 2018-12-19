//  InstanceManager - DotNet Instance Manager library.
//  Copyright(C) 2018  Stephen Wheeler - 8bitbear.com
//  
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//  
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program. If not, see<https://www.gnu.org/licenses/>.

using System;
using System.IO.Pipes;
using System.IO;
using System.Runtime.Serialization.Json;

namespace InstanceManager
{
   public class InstanceManager
   {
      private static string _pipeName = System.Reflection.Assembly.GetExecutingAssembly().GetName() + "-instance-lock";
      private static string[] _args;
      PipeStream _pipeStream;

      public InstanceManager(string[] args)
      {
         _args = args;
         StartServer();
      }

      public InstanceManager(string[] args, string pipeName)
      {
         _pipeName = pipeName;
         _args = args;
         StartServer();
      }

      ~InstanceManager()
      {
         _pipeStream.Dispose();
      }

      private void StartServer()
      {
         try
         {
             // TODO Add security.
             //PipeSecurity ps = new PipeSecurity();
             _pipeStream = new NamedPipeServerStream(_pipeName, PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.None, 1024 * 128, 1024);
         }
         catch (IOException)
         {
            _pipeStream = new NamedPipeClientStream(".", _pipeName, PipeDirection.Out, PipeOptions.WriteThrough);
            (_pipeStream as NamedPipeClientStream).Connect();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(string[]));
            ser.WriteObject(_pipeStream, _args);
            _pipeStream.WaitForPipeDrain();

            throw new Exception("Already running");
         }
      }

      // TODO Add cancel token.
      public async void InstanceStartup(Action<string[]> onStartupNextInstance)
      {
         while (true)
         {
            NamedPipeServerStream server = _pipeStream as NamedPipeServerStream;

            await server.WaitForConnectionAsync();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(string[]));
            string[] cmdLine = ser.ReadObject(server) as string[];
            onStartupNextInstance(cmdLine);
            server.Disconnect();
            server.Dispose();
            StartServer();
         }
      }
   }
}
