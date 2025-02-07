﻿using Devon4Net.Infrastructure.Common.Extensions;
using Devon4Net.Infrastructure.Common.Helpers.Interfaces;
using Devon4Net.Infrastructure.Common;
using Devon4Net.Infrastructure.RabbitMQ.Commands;
using Devon4Net.Infrastructure.RabbitMQ.Common;
using Devon4Net.Infrastructure.RabbitMQ.Domain.Entities;
using Devon4Net.Infrastructure.RabbitMQ.Domain.ServiceInterfaces;
using LiteDB;

namespace Devon4Net.Infrastructure.RabbitMQ.Data.Service
{
    public class RabbitMqBackupLiteDbService : IRabbitMqBackupLiteDbService
    {
        private LiteDb.Repository.ILiteDbRepository<RabbitBackup> RabbitMqBackupLiteDbRepository { get; }
        private IJsonHelper JsonHelper { get; }

        public RabbitMqBackupLiteDbService(LiteDb.Repository.ILiteDbRepository<RabbitBackup> rabbitMqBackupLiteDbRepository, IJsonHelper jsonHelper)
        {
            RabbitMqBackupLiteDbRepository = rabbitMqBackupLiteDbRepository;
            JsonHelper = jsonHelper;
        }
        public BsonValue CreateMessageBackup(Command command, QueueActions action = QueueActions.Sent, bool increaseRetryCounter = false, string additionalData = null, string errorData = null)
        {
            try
            {
                if (command?.InternalMessageIdentifier == null || command.InternalMessageIdentifier.IsNullOrEmptyGuid())
                {
                    throw new ArgumentException("The provided command  and the command identifier cannot be null ");
                }

                var backUp = new RabbitBackup
                {
                    Id = Guid.NewGuid(),
                    InternalMessageIdentifier = command.InternalMessageIdentifier,
                    Retries = increaseRetryCounter ? 1 : 0,
                    AdditionalData = string.IsNullOrEmpty(additionalData) ? string.Empty : additionalData,
                    IsError = false,
                    MessageContent = GetSerializedContent(command),
                    MessageType = command.MessageType,
                    TimeStampUTC = command.Timestamp.ToUniversalTime(),
                    Action = action.ToString(),
                    Error = string.IsNullOrEmpty(errorData) ? string.Empty : errorData
                };

                return RabbitMqBackupLiteDbRepository.Create(backUp);
            }
            catch (Exception ex)
            {
                Devon4NetLogger.Error($"Error storing data with LiteDb: {ex.Message} {ex.InnerException}");
                Devon4NetLogger.Error(ex);
                throw;
            }
        }

        private string GetSerializedContent(Command command)
        {
            var typedCommand = Convert.ChangeType(command, command.GetType());
            var serializedContent = JsonHelper.Serialize(typedCommand, false);
            return serializedContent;
        }
    }
}
