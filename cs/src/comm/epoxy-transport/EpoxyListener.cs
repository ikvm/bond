// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Bond.Comm.Epoxy
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Bond.Comm.Service;

    public class EpoxyListener : Listener
    {
        private EpoxyTransport parentTransport;
        private System.Net.Sockets.TcpListener listener;
        private ServiceHost serviceHost;

        private object connectionsLock = new object();
        private HashSet<EpoxyConnection> connections;

        private Task acceptTask;

        private CancellationTokenSource shutdownTokenSource;

        public IPEndPoint ListenEndpoint { get; private set; }

        public EpoxyListener(EpoxyTransport parentTransport, IPEndPoint listenEndpoint)
        {
            this.parentTransport = parentTransport;
            listener = new TcpListener(listenEndpoint);
            ListenEndpoint = null;
            serviceHost = new ServiceHost(parentTransport);
            connections = new HashSet<EpoxyConnection>();
            shutdownTokenSource = new CancellationTokenSource();
        }

        public override string ToString()
        {
            return $"EpoxyListener({ListenEndpoint})";
        }

        public override bool IsRegistered(string serviceMethodName)
        {
            return serviceHost.IsRegistered(serviceMethodName);
        }

        public override void AddService<T>(T service)
        {
            Log.Information("{0}.{1}: Adding {2}.", this, nameof(AddService), typeof(T).Name);
            serviceHost.Register(service);
        }

        public override void RemoveService<T>(T service)
        {
            throw new NotImplementedException();
        }

        public override Task StartAsync()
        {
            listener.Start();
            ListenEndpoint = (IPEndPoint) listener.LocalEndpoint;
            acceptTask = Task.Run(() => AcceptAsync(shutdownTokenSource.Token), shutdownTokenSource.Token);
            return TaskExt.CompletedTask;
        }

        public override Task StopAsync()
        {
            shutdownTokenSource.Cancel();
            listener.Stop();

            return acceptTask;
        }

        internal Error InvokeOnConnected(ConnectedEventArgs args)
        {
            return OnConnected(args);
        }

        internal void InvokeOnDisconnected(DisconnectedEventArgs args)
        {
            OnDisconnected(args);
        }

        private async Task AcceptAsync(CancellationToken t)
        {
            Log.Information("{0}.{1}: Accepting connections...", this, nameof(AcceptAsync));
            while (!t.IsCancellationRequested)
            {
                Socket socket = null;

                try
                {
                    socket = await listener.AcceptSocketAsync();
                    var connection = EpoxyConnection.MakeServerConnection(
                        parentTransport,
                        this,
                        serviceHost,
                        socket);
                    socket = null; // connection now owns the socket and will close it

                    lock (connectionsLock)
                    {
                        connections.Add(connection);
                    }

                    await connection.StartAsync();
                    Log.Debug("{0}.{1}: Accepted connection from {2}.", 
                        this, nameof(AcceptAsync), connection.RemoteEndPoint);
                }
                catch (SocketException ex)
                {
                    Log.Fatal(ex, "{0}.{1}: Accept failed with error {2}.",
                        this, nameof(AcceptAsync), ex.SocketErrorCode);

                    ShutdownSocketSafe(socket);
                }
                catch (ObjectDisposedException)
                {
                    ShutdownSocketSafe(socket);

                    // TODO: ignoring this exception is needed during shutdown,
                    //       but there should be a cleaner way. We should
                    //       switch to having a proper life-cycle for a
                    //       connection.
                }
            }
            Log.Information("{0}.{1}: Shutting down.", this, nameof(AcceptAsync));
        }

        private static void ShutdownSocketSafe(Socket socket)
        {
            try
            {
                socket?.Shutdown(SocketShutdown.Both);
                socket?.Close();
            }
            catch (SocketException ex)
            {
                // We tried to cleanly shutdown the socket, oh well.
                Log.Debug(ex, "Exception encountered when shutting down a socket.");
            }
        }
    }
}
