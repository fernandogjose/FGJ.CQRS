﻿using Fgj.Cqrs.Domain.Commands;
using Fgj.Cqrs.Domain.Interfaces.SqlServerRepositories;
using Fgj.Cqrs.Domain.Interfaces.Validations;
using Fgj.Cqrs.Domain.Share.Commands;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fgj.Cqrs.Domain.Handlers
{
    public class UserCreateCommandHandler : IRequestHandler<UserCreateCommand, ResponseCommand>
    {
        private readonly IUserSqlServerRepository _userSqlServerRepository;
        private readonly IUserValidation _userValidation;

        public UserCreateCommandHandler(IUserSqlServerRepository userSqlServerRepository, IUserValidation userValidation)
        {
            _userSqlServerRepository = userSqlServerRepository;
            _userValidation = userValidation;
        }

        public Task<ResponseCommand> Handle(UserCreateCommand request, CancellationToken cancellationToken)
        {
            // Validações de dados
            if (!request.IsValid())
            {
                return Task.FromResult(new ResponseCommand(false, request.Errors));
            }

            // Validações de persistencia
            Tuple<bool, string> validation = _userValidation.IsDuplicateName(request.Guid, request.Name);
            if (validation.Item1)
            {
                request.AddError(validation.Item2);
                return Task.FromResult(new ResponseCommand(false, request.Errors));
            }

            // Persistir
            int userId = _userSqlServerRepository.Create(request);

            // Response
            return Task.FromResult(new ResponseCommand(true, userId));
        }
    }
}