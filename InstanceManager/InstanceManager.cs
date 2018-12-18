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
      static string pipeName = System.Reflection.Assembly.GetExecutingAssembly().GetName() + "-instance-lock";
      PipeStream pipeStream;
      public InstanceManager(string[] args)
      {
         try
         {
            StartServer();
         }
         catch (IOException)
         {
            pipeStream = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.WriteThrough);
            (pipeStream as NamedPipeClientStream).Connect();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(string[]));
            ser.WriteObject(pipeStream, args);
            pipeStream.WaitForPipeDrain();

            throw new Exception("Already running");
         }
      }

      ~InstanceManager()
      {
         pipeStream.Dispose();
      }

      private void StartServer()
      {
         // TODO Add security.
         //PipeSecurity ps = new PipeSecurity();
         pipeStream = new NamedPipeServerStream(pipeName, PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.None, 1024 * 128, 1024);
      }

      // TODO Add cancel token.
      public async void InstanceStartup(Action<string[]> onStartupNextInstance)
      {
         while (true)
         {
            NamedPipeServerStream server = pipeStream as NamedPipeServerStream;

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
