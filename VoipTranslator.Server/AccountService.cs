﻿using System;
using System.Linq;
using VoipTranslator.Protocol;
using VoipTranslator.Protocol.Commands;
using VoipTranslator.Server.Entities;
using VoipTranslator.Server.Interfaces;

namespace VoipTranslator.Server
{
    public class AccountService
    {
        private readonly CommandBuilder _commandBuilder;
        private readonly ConnectionsManager _connectionsManager;
        private readonly IUsersRepository _usersRepository;

        public AccountService(ConnectionsManager connectionsManager, 
            IUsersRepository usersRepository,
            CommandBuilder commandBuilder)
        {
            _commandBuilder = commandBuilder;
            _connectionsManager = connectionsManager;
            _usersRepository = usersRepository;
            _connectionsManager.CommandRecieved += _connectionsManager_OnCommandRecieved;
        }

        private void _connectionsManager_OnCommandRecieved(object sender, Interfaces.RemoteUserCommandEventArgs e)
        {
            switch (e.Command.Name)
            {
                case CommandName.Register:
                    HandleRegistration(e.RemoteUser, e.Command);
                    break;

                case CommandName.Authentication:
                    HandleAuthentication(e.RemoteUser, e.Command);
                    break;
            }
        }

        private async void HandleRegistration(RemoteUser remoteUser, Command command)
        {
            var request = _commandBuilder.GetUnderlyingObject<RegistrationRequest>(command);
            var result = new RegistrationResult();

            if (string.IsNullOrEmpty(request.Number) ||
                request.Number.Length < 4 ||
                request.Number.Trim(' ', '+').ToCharArray().Any(i => !char.IsDigit(i)))
            {
                result.Result = RegistrationResultType.NotRegistered;
            }
            else
            {
                remoteUser.User.Number = request.Number;
                remoteUser.User.UserId = Guid.NewGuid().ToString();
                _usersRepository.Add(remoteUser.User);
                result.Result = RegistrationResultType.Success;
                result.UserId = remoteUser.User.UserId;
            }
            _commandBuilder.ChangeUnderlyingObject(command, result);
            await remoteUser.Peer.SendCommand(command);
        }

        private async void HandleAuthentication(RemoteUser remoteUser, Command command)
        {
            var request = _commandBuilder.GetUnderlyingObject<AuthenticationRequest>(command);
            var result = new AuthenticationResult();

            var user = _usersRepository.GetById(request.UserId);
            if (user != null)
            {
                result.Result = AuthenticationResultType.Success;
                user.OsName = request.OsName;
                user.PushUri = request.PushUri;
                user.DeviceName = request.DeviceName;
            }
            else
            {
                result.Result = AuthenticationResultType.NotRegistered;
            }
            _commandBuilder.ChangeUnderlyingObject(command, result);
            await remoteUser.Peer.SendCommand(command);
        }
    }
}
