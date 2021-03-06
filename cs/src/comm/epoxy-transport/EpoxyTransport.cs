﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Bond.Comm.Epoxy
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Bond.Comm.Service;

    public class EpoxyTransportBuilder : TransportBuilder<EpoxyTransport>
    {
        public override EpoxyTransport Construct()
        {
            return new EpoxyTransport(LayerStack);
        }
    }

    public class EpoxyTransport : Transport
    {
        public const int DefaultPort = 25188;

        readonly ILayerStack layerStack;
        public EpoxyTransport(ILayerStack layerStack)
        {
            // Layer stack may be null
            this.layerStack = layerStack;
        }

        public override ILayerStack LayerStack
        {
            get
            {
                return layerStack;
            }
        }

        public override Task<Connection> ConnectToAsync(string address, CancellationToken ct)
        {
            return ConnectToAsync(ParseStringAddress(address), ct).Upcast<EpoxyConnection, Connection>();
        }

        public Task<EpoxyConnection> ConnectToAsync(IPEndPoint endpoint)
        {
            return ConnectToAsync(endpoint, CancellationToken.None);
        }

        public async Task<EpoxyConnection> ConnectToAsync(IPEndPoint endpoint, CancellationToken ct)
        {
            Log.Information("{0}.{1}: Connecting to {2}.", nameof(EpoxyTransport), nameof(ConnectToAsync), endpoint);

            Socket socket = MakeClientSocket();
            await Task.Factory.FromAsync(socket.BeginConnect, socket.EndConnect, endpoint, state: null);

            // TODO: keep these in some master collection for shutdown
            var connection = EpoxyConnection.MakeClientConnection(this, socket);
            await connection.StartAsync();
            return connection;
        }

        public override Listener MakeListener(string address)
        {
            return MakeListener(ParseStringAddress(address));
        }

        public EpoxyListener MakeListener(IPEndPoint address)
        {
            return new EpoxyListener(this, address);
        }

        public override Task StopAsync()
        {
            return TaskExt.CompletedTask;
        }

        public static IPEndPoint ParseStringAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                throw new ArgumentException("Address cannot be null or empty", nameof(address));
            }

            int portStartIndex = address.IndexOf(':');

            string ipAddressPart;

            if (portStartIndex == -1)
            {
                ipAddressPart = address;
            }
            else
            {
                ipAddressPart = address.Substring(0, portStartIndex);
            }

            IPAddress ipAddr;
            if (!IPAddress.TryParse(ipAddressPart, out ipAddr))
            {
                throw new ArgumentException("Couldn't parse IP address from \"" + address + "\"", nameof(address));
            }

            int port;
            if (portStartIndex == -1)
            {
                port = DefaultPort;
            }
            else
            {
                string portPart = address.Substring(portStartIndex + 1);
                if (!int.TryParse(portPart, out port))
                {
                    throw new ArgumentException("Couldn't parse port from \"" + address + "\"", nameof(address));
                }
            }

            return new IPEndPoint(ipAddr, port);
        }

        private Socket MakeClientSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
    }
}
