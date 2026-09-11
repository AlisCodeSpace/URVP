IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731085159_InitialCreate'
)
BEGIN
    CREATE TABLE [DataProtectionKeys] (
        [Id] int NOT NULL IDENTITY,
        [FriendlyName] nvarchar(max) NULL,
        [Xml] nvarchar(max) NULL,
        CONSTRAINT [PK_DataProtectionKeys] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (ClientConnectionId:a5727afa-5550-4b57-a46f-b173ad3a2fd5
Error Number:1105,State:2,Class:17
   --- End of inner exception stack trace ---
   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.SqlServer.Update.Internal.SqlServerModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Storage.RelationalDatabase.SaveChangesAsync(IList`1 entries, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(IList`1 entriesToSave, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(StateManager stateManager, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal.SqlServerExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
   at FEA.URVP.Application.Services.Notifications.NotificationOutboxService.HandleRetryAsync(NotificationOutbox item, Exception exception, CancellationToken cancellationToken) in /src/FEA.URVP.Backend/FEA.URVP.Application/Services/Notifications/NotificationOutboxService.cs:line 222
[14:25:49 ERR] Failed executing DbCommand (86ms) [Parameters=[@p0='?' (DbType = Guid), @p1='?' (Size = 1000), @p2='?' (Size = 4000), @p3='?' (Size = 1000), @p4='?' (DbType = DateTime2), @p5='?' (Size = 4000), @p6='?' (Size = 256), @p7='?' (DbType = DateTime2), @p8='?' (DbType = Boolean), @p9='?' (Size = 256), @p13='?' (DbType = Guid), @p10='?' (DbType = DateTime2), @p11='?' (DbType = Int32), @p12='?' (Size = 32), @p15='?' (DbType = Guid), @p14='?' (Size = 32), @p20='?' (DbType = Guid), @p16='?' (Size = 4000), @p17='?' (DbType = DateTime2), @p18='?' (DbType = Int32), @p19='?' (Size = 32)], CommandType='Text', CommandTimeout='30']
SET NOCOUNT ON;
INSERT INTO [EmailLogs] ([Id], [Bcc], [Body], [Cc], [CreatedOn], [Exception], [From], [ModifiedOn], [Success], [To])
VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9);
UPDATE [NotificationOutbox] SET [NextRetryAt] = @p10, [RetryCount] = @p11, [Status] = @p12
OUTPUT 1
WHERE [Id] = @p13;
UPDATE [NotificationOutbox] SET [Status] = @p14
OUTPUT 1
WHERE [Id] = @p15;
UPDATE [NotificationOutbox] SET [ErrorMessage] = @p16, [NextRetryAt] = @p17, [RetryCount] = @p18, [Status] = @p19
OUTPUT 1
WHERE [Id] = @p20;
[14:25:49 ERR] An exception occurred in the database while saving changes for context type 'FEA.URVP.Infrastructure.Data.Context.AppDbContext'.
Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while saving the entity changes. See the inner exception for details.
 ---> Microsoft.Data.SqlClient.SqlException (0x80131904): Could not allocate space for object 'dbo.EmailLogs'.'PK_EmailLogs' in database 'provost-urvp' because the 'PRIMARY' filegroup is full due to lack of storage space or database files reaching the maximum allowed size. Note that UNLIMITED files are still limited to 16TB. Create the necessary space by dropping objects in the filegroup, adding additional files to the filegroup, or setting autogrowth on for existing files in the filegroup.
   at System.Threading.Tasks.ContinuationResultTaskFromResultTask`2.InnerInvoke()
   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
--- End of stack trace from previous location ---
   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
   at System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task& currentTaskSlot, Thread threadPoolThread)
--- End of stack trace from previous location ---
   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
ClientConnectionId:a5727afa-5550-4b57-a46f-b173ad3a2fd5
Error Number:1105,State:2,Class:17
   --- End of inner exception stack trace ---
   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.SqlServer.Update.Internal.SqlServerModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Storage.RelationalDatabase.SaveChangesAsync(IList`1 entries, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(IList`1 entriesToSave, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(StateManager stateManager, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal.SqlServerExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while saving the entity changes. See the inner exception for details.
 ---> Microsoft.Data.SqlClient.SqlException (0x80131904): Could not allocate space for object 'dbo.EmailLogs'.'PK_EmailLogs' in database 'provost-urvp' because the 'PRIMARY' filegroup is full due to lack of storage space or database files reaching the maximum allowed size. Note that UNLIMITED files are still limited to 16TB. Create the necessary space by dropping objects in the filegroup, adding additional files to the filegroup, or setting autogrowth on for existing files in the filegroup.
   at System.Threading.Tasks.ContinuationResultTaskFromResultTask`2.InnerInvoke()
   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
--- End of stack trace from previous location ---
   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
   at System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task& currentTaskSlot, Thread threadPoolThread)
--- End of stack trace from previous location ---
   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
ClientConnectionId:a5727afa-5550-4b57-a46f-b173ad3a2fd5
Error Number:1105,State:2,Class:17
   --- End of inner exception stack trace ---
   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.SqlServer.Update.Internal.SqlServerModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Storage.RelationalDatabase.SaveChangesAsync(IList`1 entries, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(IList`1 entriesToSave, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(StateManager stateManager, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal.SqlServerExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
[14:25:49 ERR] Outbox item e6de32d3-62ef-4594-ae36-ea5ec69f80cc failed; scheduling retry
Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while saving the entity changes. See the inner exception for details.
 ---> Microsoft.Data.SqlClient.SqlException (0x80131904): Could not allocate space for object 'dbo.EmailLogs'.'PK_EmailLogs' in database 'provost-urvp' because the 'PRIMARY' filegroup is full due to lack of storage space or database files reaching the maximum allowed size. Note that UNLIMITED files are still limited to 16TB. Create the necessary space by dropping objects in the filegroup, adding additional files to the filegroup, or setting autogrowth on for existing files in the filegroup.
   at System.Threading.Tasks.ContinuationResultTaskFromResultTask`2.InnerInvoke()
   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
--- End of stack trace from previous location ---
   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
   at System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task& currentTaskSlot, Thread threadPoolThread)
--- End of stack trace from previous location ---
   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
ClientConnectionId:a5727afa-5550-4b57-a46f-b173ad3a2fd5
Error Number:1105,State:2,Class:17
   --- End of inner exception stack trace ---
   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.SqlServer.Update.Internal.SqlServerModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Storage.RelationalDatabase.SaveChangesAsync(IList`1 entries, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(IList`1 entriesToSave, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(StateManager stateManager, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal.SqlServerExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
   at FEA.URVP.Application.Services.Notifications.NotificationOutboxService.ProcessOutboxAsync(CancellationToken cancellationToken) in /src/FEA.URVP.Backend/FEA.URVP.Application/Services/Notifications/NotificationOutboxService.cs:line 88
[14:25:49 INF] Analytics event email_send_failed NotificationId=03ee5d3b-afa3-4695-b73a-90293b986bf0 Reason=An error occurred while saving the entity changes. See the inner exception for details.
[14:25:49 ERR] Failed executing DbCommand (86ms) [Parameters=[@p0='?' (DbType = Guid), @p1='?' (Size = 1000), @p2='?' (Size = 4000), @p3='?' (Size = 1000), @p4='?' (DbType = DateTime2), @p5='?' (Size = 4000), @p6='?' (Size = 256), @p7='?' (DbType = DateTime2), @p8='?' (DbType = Boolean), @p9='?' (Size = 256), @p13='?' (DbType = Guid), @p10='?' (DbType = DateTime2), @p11='?' (DbType = Int32), @p12='?' (Size = 32), @p18='?' (DbType = Guid), @p14='?' (Size = 4000), @p15='?' (DbType = DateTime2), @p16='?' (DbType = Int32), @p17='?' (Size = 32), @p23='?' (DbType = Guid), @p19='?' (Size = 4000), @p20='?' (DbType = DateTime2), @p21='?' (DbType = Int32), @p22='?' (Size = 32)], CommandType='Text', CommandTimeout='30']
SET NOCOUNT ON;
INSERT INTO [EmailLogs] ([Id], [Bcc], [Body], [Cc], [CreatedOn], [Exception], [From], [ModifiedOn], [Success], [To])
VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9);
UPDATE [NotificationOutbox] SET [NextRetryAt] = @p10, [RetryCount] = @p11, [Status] = @p12
OUTPUT 1
WHERE [Id] = @p13;
UPDATE [NotificationOutbox] SET [ErrorMessage] = @p14, [NextRetryAt] = @p15, [RetryCount] = @p16, [Status] = @p17
OUTPUT 1
WHERE [Id] = @p18;
UPDATE [NotificationOutbox] SET [ErrorMessage] = @p19, [NextRetryAt] = @p20, [RetryCount] = @p21, [Status] = @p22
OUTPUT 1
WHERE [Id] = @p23;
[14:25:49 ERR] An exception occurred in the database while saving changes for context type 'FEA.URVP.Infrastructure.Data.Context.AppDbContext'.
Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while saving the entity changes. See the inner exception for details.
 ---> Microsoft.Data.SqlClient.SqlException (0x80131904): Could not allocate space for object 'dbo.EmailLogs'.'PK_EmailLogs' in database 'provost-urvp' because the 'PRIMARY' filegroup is full due to lack of storage space or database files reaching the maximum allowed size. Note that UNLIMITED files are still limited to 16TB. Create the necessary space by dropping objects in the filegroup, adding additional files to the filegroup, or setting autogrowth on for existing files in the filegroup.
   at System.Threading.Tasks.ContinuationResultTaskFromResultTask`2.InnerInvoke()
   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
--- End of stack trace from previous location ---
   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
   at System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task& currentTaskSlot, Thread threadPoolThread)
--- End of stack trace from previous location ---
   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
ClientConnectionId:a5727afa-5550-4b57-a46f-b173ad3a2fd5
Error Number:1105,State:2,Class:17
   --- End of inner exception stack trace ---
   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.SqlServer.Update.Internal.SqlServerModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Storage.RelationalDatabase.SaveChangesAsync(IList`1 entries, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(IList`1 entriesToSave, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(StateManager stateManager, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal.SqlServerExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while saving the entity changes. See the inner exception for details.
 ---> Microsoft.Data.SqlClient.SqlException (0x80131904): Could not allocate space for object 'dbo.EmailLogs'.'PK_EmailLogs' in database 'provost-urvp' because the 'PRIMARY' filegroup is full due to lack of storage space or database files reaching the maximum allowed size. Note that UNLIMITED files are still limited to 16TB. Create the necessary space by dropping objects in the filegroup, adding additional files to the filegroup, or setting autogrowth on for existing files in the filegroup.
   at System.Threading.Tasks.ContinuationResultTaskFromResultTask`2.InnerInvoke()
   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
--- End of stack trace from previous location ---
   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
   at System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task& currentTaskSlot, Thread threadPoolThread)
--- End of stack trace from previous location ---
   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
ClientConnectionId:a5727afa-5550-4b57-a46f-b173ad3a2fd5
Error Number:1105,State:2,Class:17
   --- End of inner exception stack trace ---
   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.SqlServer.Update.Internal.SqlServerModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Storage.RelationalDatabase.SaveChangesAsync(IList`1 entries, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(IList`1 entriesToSave, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(StateManager stateManager, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal.SqlServerExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
[14:25:49 ERR] Failed to persist retry state for outbox item e6de32d3-62ef-4594-ae36-ea5ec69f80cc
Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while saving the entity changes. See the inner exception for details.
 ---> Microsoft.Data.SqlClient.SqlException (0x80131904): Could not allocate space for object 'dbo.EmailLogs'.'PK_EmailLogs' in database 'provost-urvp' because the 'PRIMARY' filegroup is full due to lack of storage space or database files reaching the maximum allowed size. Note that UNLIMITED files are still limited to 16TB. Create the necessary space by dropping objects in the filegroup, adding additional files to the filegroup, or setting autogrowth on for existing files in the filegroup.
   at System.Threading.Tasks.ContinuationResultTaskFromResultTask`2.InnerInvoke()
   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
--- End of stack trace from previous location ---
   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
   at System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task& currentTaskSlot, Thread threadPoolThread)
--- End of stack trace from previous location ---
   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
ClientConnectionId:a5727afa-5550-4b57-a46f-b173ad3a2fd5
Error Number:1105,State:2,Class:17
   --- End of inner exception stack trace ---
   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.SqlServer.Update.Internal.SqlServerModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Storage.RelationalDatabase.SaveChangesAsync(IList`1 entries, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(IList`1 entriesToSave, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(StateManager stateManager, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal.SqlServerExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
   at FEA.URVP.Application.Services.Notifications.NotificationOutboxService.HandleRetryAsync(NotificationOutbox item, Exception exception, CancellationToken cancellationToken) in /src/FEA.URVP.Backend/FEA.URVP.Application/Services/Notifications/NotificationOutboxService.cs:line 222
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731085159_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] uniqueidentifier NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [Name] nvarchar(128) NOT NULL,
        [Role] tinyint NOT NULL,
        [ProfileImageUrl] nvarchar(512) NULL,
        [RegisteredAt] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731085159_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731085159_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260731085159_InitialCreate', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731093348_AddProjects'
)
BEGIN
    CREATE TABLE [Projects] (
        [Id] uniqueidentifier NOT NULL,
        [CreatedByUserId] uniqueidentifier NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [ResearchArea] tinyint NOT NULL,
        [IrbStage] tinyint NOT NULL,
        [BriefDescription] nvarchar(4000) NOT NULL,
        [ActivityType] tinyint NOT NULL,
        [VolunteersRequired] int NOT NULL,
        [VolunteersFilled] int NOT NULL DEFAULT 0,
        [MinQualifications] nvarchar(2000) NULL,
        [AdditionalComments] nvarchar(2000) NULL,
        [Status] tinyint NOT NULL,
        [FacultyNameSnapshot] nvarchar(128) NOT NULL,
        [AffiliationSnapshot] nvarchar(256) NOT NULL,
        [EmailSnapshot] nvarchar(256) NOT NULL,
        [UserNameSnapshot] nvarchar(64) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Projects] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Projects_VolunteersFilled] CHECK ([VolunteersFilled] >= 0),
        CONSTRAINT [CK_Projects_VolunteersRequired] CHECK ([VolunteersRequired] >= 1),
        CONSTRAINT [FK_Projects_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731093348_AddProjects'
)
BEGIN
    CREATE INDEX [IX_Projects_CreatedByUserId] ON [Projects] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731093348_AddProjects'
)
BEGIN
    CREATE INDEX [IX_Projects_Status_CreatedAt] ON [Projects] ([Status], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731093348_AddProjects'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260731093348_AddProjects', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731101216_UpdateProjectResearchAreasAndIrb'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Projects]') AND [c].[name] = N'ResearchArea');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Projects] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [Projects] DROP COLUMN [ResearchArea];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731101216_UpdateProjectResearchAreasAndIrb'
)
BEGIN
    ALTER TABLE [Projects] ADD [ResearchAreas] nvarchar(2000) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731101216_UpdateProjectResearchAreasAndIrb'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260731101216_UpdateProjectResearchAreasAndIrb', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731101526_UpdateProjectActivityTypes'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Projects]') AND [c].[name] = N'ActivityType');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Projects] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [Projects] DROP COLUMN [ActivityType];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731101526_UpdateProjectActivityTypes'
)
BEGIN
    ALTER TABLE [Projects] ADD [ActivityTypes] nvarchar(2000) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731101526_UpdateProjectActivityTypes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260731101526_UpdateProjectActivityTypes', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731102813_AddUserNameAndAffiliation'
)
BEGIN
    ALTER TABLE [Users] ADD [Affiliation] nvarchar(256) NOT NULL DEFAULT N'American University of Beirut';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731102813_AddUserNameAndAffiliation'
)
BEGIN
    ALTER TABLE [Users] ADD [UserName] nvarchar(64) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731102813_AddUserNameAndAffiliation'
)
BEGIN
    -- EXEC defers compile until after ALTER TABLE ADD [UserName] in this batch.
    EXEC(N'
        UPDATE [Users]
        SET [UserName] = CASE
            WHEN CHARINDEX(''@'', [Email]) > 1 THEN LEFT([Email], CHARINDEX(''@'', [Email]) - 1)
            ELSE [Email]
        END
        WHERE [UserName] = '''' OR [UserName] IS NULL;
    ');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731102813_AddUserNameAndAffiliation'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260731102813_AddUserNameAndAffiliation', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806100345_AddStudentProfiles'
)
BEGIN
    CREATE TABLE [StudentProfiles] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Gender] nvarchar(16) NOT NULL,
        [MobileNumber] nvarchar(32) NOT NULL,
        [Degree] nvarchar(32) NOT NULL,
        [ExpectedGraduationYear] int NOT NULL,
        [Languages] nvarchar(2000) NOT NULL,
        [OtherLanguages] nvarchar(256) NULL,
        [CompletedCredits] bit NOT NULL,
        [CumulativeAverage] decimal(5,2) NOT NULL,
        [ResearchTopics] nvarchar(2000) NOT NULL,
        [Publications] nvarchar(4000) NULL,
        [TranscriptFileName] nvarchar(256) NULL,
        [CitiFileName] nvarchar(256) NULL,
        [Availability] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_StudentProfiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StudentProfiles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806100345_AddStudentProfiles'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StudentProfiles_UserId] ON [StudentProfiles] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806100345_AddStudentProfiles'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260806100345_AddStudentProfiles', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806101152_AddFileStorageAndProfileFileIds'
)
BEGIN
    DECLARE @var2 nvarchar(max);
    SELECT @var2 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StudentProfiles]') AND [c].[name] = N'CitiFileName');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [StudentProfiles] DROP CONSTRAINT ' + @var2 + ';');
    ALTER TABLE [StudentProfiles] DROP COLUMN [CitiFileName];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806101152_AddFileStorageAndProfileFileIds'
)
BEGIN
    DECLARE @var3 nvarchar(max);
    SELECT @var3 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StudentProfiles]') AND [c].[name] = N'TranscriptFileName');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [StudentProfiles] DROP CONSTRAINT ' + @var3 + ';');
    ALTER TABLE [StudentProfiles] DROP COLUMN [TranscriptFileName];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806101152_AddFileStorageAndProfileFileIds'
)
BEGIN
    ALTER TABLE [StudentProfiles] ADD [CitiFileId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806101152_AddFileStorageAndProfileFileIds'
)
BEGIN
    ALTER TABLE [StudentProfiles] ADD [TranscriptFileId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806101152_AddFileStorageAndProfileFileIds'
)
BEGIN
    CREATE TABLE [FileStorage] (
        [Id] uniqueidentifier NOT NULL,
        [EntityType] nvarchar(50) NOT NULL,
        [EntityId] uniqueidentifier NOT NULL,
        [FileCategory] nvarchar(50) NOT NULL,
        [FileName] nvarchar(260) NOT NULL,
        [MimeType] nvarchar(100) NOT NULL,
        [FileSize] bigint NOT NULL,
        [ContentHash] varbinary(32) NOT NULL,
        [Content] varbinary(max) NOT NULL,
        [UploadedBy] uniqueidentifier NULL,
        [UploadedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_FileStorage] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_FileStorage_EntityType] CHECK ([EntityType] IN ('StudentProfile')),
        CONSTRAINT [CK_FileStorage_FileSize] CHECK (([FileCategory] IN ('Transcript', 'CitiCertification') AND [FileSize] <= 10485760))
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806101152_AddFileStorageAndProfileFileIds'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_FileStorage_ContentHash] ON [FileStorage] ([ContentHash]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806101152_AddFileStorageAndProfileFileIds'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_FileStorage_Entity] ON [FileStorage] ([EntityType], [EntityId], [FileCategory], [IsDeleted]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806101152_AddFileStorageAndProfileFileIds'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260806101152_AddFileStorageAndProfileFileIds', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806110218_AddProjectRankings'
)
BEGIN
    CREATE TABLE [ProjectRankings] (
        [Id] uniqueidentifier NOT NULL,
        [StudentUserId] uniqueidentifier NOT NULL,
        [ProjectId] uniqueidentifier NOT NULL,
        [Rank] tinyint NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ProjectRankings] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ProjectRankings_Rank] CHECK ([Rank] >= 1 AND [Rank] <= 3),
        CONSTRAINT [FK_ProjectRankings_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Projects] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ProjectRankings_Users_StudentUserId] FOREIGN KEY ([StudentUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806110218_AddProjectRankings'
)
BEGIN
    CREATE INDEX [IX_ProjectRankings_ProjectId] ON [ProjectRankings] ([ProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806110218_AddProjectRankings'
)
BEGIN
    CREATE INDEX [IX_ProjectRankings_ProjectId_Rank] ON [ProjectRankings] ([ProjectId], [Rank]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806110218_AddProjectRankings'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProjectRankings_StudentUserId_ProjectId] ON [ProjectRankings] ([StudentUserId], [ProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806110218_AddProjectRankings'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProjectRankings_StudentUserId_Rank] ON [ProjectRankings] ([StudentUserId], [Rank]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806110218_AddProjectRankings'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260806110218_AddProjectRankings', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811104337_AddValueListItems'
)
BEGIN
    CREATE TABLE [ValueListItems] (
        [Id] uniqueidentifier NOT NULL,
        [Kind] tinyint NOT NULL,
        [Name] nvarchar(256) NOT NULL,
        [SortOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ValueListItems] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811104337_AddValueListItems'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ValueListItems_Kind_Name] ON [ValueListItems] ([Kind], [Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811104337_AddValueListItems'
)
BEGIN
    CREATE INDEX [IX_ValueListItems_Kind_SortOrder] ON [ValueListItems] ([Kind], [SortOrder]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811104337_AddValueListItems'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260811104337_AddValueListItems', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811111636_AddDivisions'
)
BEGIN
    CREATE TABLE [Divisions] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(256) NOT NULL,
        [Description] nvarchar(1000) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Divisions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811111636_AddDivisions'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Divisions_Name] ON [Divisions] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811111636_AddDivisions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260811111636_AddDivisions', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901102542_AddNewsAndWorkshops'
)
BEGIN
    ALTER TABLE [FileStorage] DROP CONSTRAINT [CK_FileStorage_EntityType];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901102542_AddNewsAndWorkshops'
)
BEGIN
    ALTER TABLE [FileStorage] DROP CONSTRAINT [CK_FileStorage_FileSize];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901102542_AddNewsAndWorkshops'
)
BEGIN
    CREATE TABLE [NewsArticles] (
        [Id] uniqueidentifier NOT NULL,
        [Slug] nvarchar(160) NOT NULL,
        [Title] nvarchar(256) NOT NULL,
        [Excerpt] nvarchar(1000) NOT NULL,
        [Category] nvarchar(64) NOT NULL,
        [Author] nvarchar(128) NOT NULL,
        [Ticker] nvarchar(256) NOT NULL,
        [Body] nvarchar(max) NOT NULL,
        [PublishedAt] datetime2 NOT NULL,
        [Featured] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_NewsArticles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901102542_AddNewsAndWorkshops'
)
BEGIN
    CREATE TABLE [Workshops] (
        [Id] uniqueidentifier NOT NULL,
        [Title] nvarchar(256) NOT NULL,
        [Date] nvarchar(64) NOT NULL,
        [Time] nvarchar(64) NULL,
        [Location] nvarchar(256) NULL,
        [Description] nvarchar(2000) NOT NULL,
        [RegistrationUrl] nvarchar(500) NOT NULL,
        [PosterFileId] uniqueidentifier NULL,
        [PosterAlt] nvarchar(256) NULL,
        [SortOrder] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Workshops] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901102542_AddNewsAndWorkshops'
)
BEGIN
    EXEC(N'ALTER TABLE [FileStorage] ADD CONSTRAINT [CK_FileStorage_EntityType] CHECK ([EntityType] IN (''StudentProfile'', ''Workshop''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901102542_AddNewsAndWorkshops'
)
BEGIN
    EXEC(N'ALTER TABLE [FileStorage] ADD CONSTRAINT [CK_FileStorage_FileSize] CHECK ((([FileCategory] IN (''Transcript'', ''CitiCertification'') AND [FileSize] <= 10485760) OR ([FileCategory] = ''Poster'' AND [FileSize] <= 5242880)))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901102542_AddNewsAndWorkshops'
)
BEGIN
    CREATE INDEX [IX_NewsArticles_PublishedAt] ON [NewsArticles] ([PublishedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901102542_AddNewsAndWorkshops'
)
BEGIN
    CREATE UNIQUE INDEX [IX_NewsArticles_Slug] ON [NewsArticles] ([Slug]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901102542_AddNewsAndWorkshops'
)
BEGIN
    CREATE INDEX [IX_Workshops_SortOrder] ON [Workshops] ([SortOrder]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901102542_AddNewsAndWorkshops'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260901102542_AddNewsAndWorkshops', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901105014_AddSemesters'
)
BEGIN
    CREATE TABLE [Semesters] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(256) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [IsActive] bit NOT NULL,
        [ApplicationWindowStart] datetime2 NULL,
        [ApplicationWindowEnd] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Semesters] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901105014_AddSemesters'
)
BEGIN
    CREATE INDEX [IX_Semesters_IsActive] ON [Semesters] ([IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901105014_AddSemesters'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260901105014_AddSemesters', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902110528_AddFacultyCandidateRankings'
)
BEGIN
    CREATE TABLE [FacultyCandidateRankings] (
        [Id] uniqueidentifier NOT NULL,
        [ProjectId] uniqueidentifier NOT NULL,
        [StudentUserId] uniqueidentifier NOT NULL,
        [Rank] tinyint NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_FacultyCandidateRankings] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_FacultyCandidateRankings_Rank] CHECK ([Rank] >= 1),
        CONSTRAINT [FK_FacultyCandidateRankings_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Projects] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_FacultyCandidateRankings_Users_StudentUserId] FOREIGN KEY ([StudentUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902110528_AddFacultyCandidateRankings'
)
BEGIN
    CREATE INDEX [IX_FacultyCandidateRankings_ProjectId_Rank] ON [FacultyCandidateRankings] ([ProjectId], [Rank]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902110528_AddFacultyCandidateRankings'
)
BEGIN
    CREATE UNIQUE INDEX [IX_FacultyCandidateRankings_ProjectId_StudentUserId] ON [FacultyCandidateRankings] ([ProjectId], [StudentUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902110528_AddFacultyCandidateRankings'
)
BEGIN
    CREATE INDEX [IX_FacultyCandidateRankings_StudentUserId] ON [FacultyCandidateRankings] ([StudentUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902110528_AddFacultyCandidateRankings'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260902110528_AddFacultyCandidateRankings', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902113851_AddSemesterCycleDates'
)
BEGIN
    ALTER TABLE [Semesters] ADD [CycleEnd] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902113851_AddSemesterCycleDates'
)
BEGIN
    ALTER TABLE [Semesters] ADD [CycleStart] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902113851_AddSemesterCycleDates'
)
BEGIN
    EXEC(N'
        UPDATE [Semesters]
        SET [CycleStart] = COALESCE([ApplicationWindowStart], [CreatedAt])
        WHERE [IsActive] = 1 AND [CycleStart] IS NULL;
    ');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902113851_AddSemesterCycleDates'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260902113851_AddSemesterCycleDates', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902115234_AddMatchingRunsAndPlacements'
)
BEGIN
    CREATE TABLE [MatchingRuns] (
        [Id] uniqueidentifier NOT NULL,
        [SemesterId] uniqueidentifier NOT NULL,
        [Status] tinyint NOT NULL,
        [AlgorithmVersion] nvarchar(64) NOT NULL,
        [Seed] int NOT NULL,
        [StudentsConsidered] int NOT NULL,
        [ProjectsConsidered] int NOT NULL,
        [SeatsAvailable] int NOT NULL,
        [StudentsMatched] int NOT NULL,
        [TieBreaksUsed] int NOT NULL,
        [Warnings] nvarchar(max) NOT NULL,
        [CreatedByUserId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ConfirmedByUserId] uniqueidentifier NULL,
        [ConfirmedAt] datetime2 NULL,
        CONSTRAINT [PK_MatchingRuns] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MatchingRuns_Semesters_SemesterId] FOREIGN KEY ([SemesterId]) REFERENCES [Semesters] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902115234_AddMatchingRunsAndPlacements'
)
BEGIN
    CREATE TABLE [Placements] (
        [Id] uniqueidentifier NOT NULL,
        [MatchingRunId] uniqueidentifier NOT NULL,
        [ProjectId] uniqueidentifier NOT NULL,
        [StudentUserId] uniqueidentifier NOT NULL,
        [StudentRank] tinyint NOT NULL,
        [FacultyRank] tinyint NOT NULL,
        [ResolvedByTieBreak] bit NOT NULL,
        [Status] tinyint NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Placements] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Placements_MatchingRuns_MatchingRunId] FOREIGN KEY ([MatchingRunId]) REFERENCES [MatchingRuns] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Placements_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Projects] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Placements_Users_StudentUserId] FOREIGN KEY ([StudentUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902115234_AddMatchingRunsAndPlacements'
)
BEGIN
    CREATE INDEX [IX_MatchingRuns_SemesterId_Status] ON [MatchingRuns] ([SemesterId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902115234_AddMatchingRunsAndPlacements'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Placements_MatchingRunId_StudentUserId] ON [Placements] ([MatchingRunId], [StudentUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902115234_AddMatchingRunsAndPlacements'
)
BEGIN
    CREATE INDEX [IX_Placements_ProjectId_Status] ON [Placements] ([ProjectId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902115234_AddMatchingRunsAndPlacements'
)
BEGIN
    CREATE INDEX [IX_Placements_StudentUserId_Status] ON [Placements] ([StudentUserId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902115234_AddMatchingRunsAndPlacements'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260902115234_AddMatchingRunsAndPlacements', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906170051_AddNotifications'
)
BEGIN
    CREATE TABLE [EmailLogs] (
        [Id] uniqueidentifier NOT NULL,
        [From] nvarchar(256) NOT NULL,
        [To] nvarchar(256) NOT NULL,
        [Cc] nvarchar(1000) NULL,
        [Bcc] nvarchar(1000) NULL,
        [Body] nvarchar(max) NOT NULL,
        [Exception] nvarchar(4000) NULL,
        [Success] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NOT NULL,
        CONSTRAINT [PK_EmailLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906170051_AddNotifications'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Type] nvarchar(50) NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Message] nvarchar(1000) NOT NULL,
        [Data] nvarchar(4000) NULL,
        [ReferenceId] uniqueidentifier NULL,
        [ReferenceType] nvarchar(50) NULL,
        [IsRead] bit NOT NULL DEFAULT CAST(0 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [ReadAt] datetime2 NULL,
        [Priority] nvarchar(20) NOT NULL DEFAULT N'low',
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notifications_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906170051_AddNotifications'
)
BEGIN
    CREATE TABLE [UserNotificationSettings] (
        [UserId] uniqueidentifier NOT NULL,
        [EmailNotifications] bit NOT NULL DEFAULT CAST(1 AS bit),
        [InAppNotifications] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_UserNotificationSettings] PRIMARY KEY ([UserId]),
        CONSTRAINT [FK_UserNotificationSettings_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906170051_AddNotifications'
)
BEGIN
    CREATE TABLE [NotificationOutbox] (
        [Id] uniqueidentifier NOT NULL,
        [NotificationId] uniqueidentifier NOT NULL,
        [EventType] nvarchar(100) NOT NULL,
        [Status] nvarchar(32) NOT NULL DEFAULT N'Pending',
        [RetryCount] int NOT NULL DEFAULT 0,
        [NextRetryAt] datetime2 NULL,
        [ErrorMessage] nvarchar(4000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ProcessedAt] datetime2 NULL,
        CONSTRAINT [PK_NotificationOutbox] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_NotificationOutbox_Notifications_NotificationId] FOREIGN KEY ([NotificationId]) REFERENCES [Notifications] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906170051_AddNotifications'
)
BEGIN
    CREATE INDEX [IX_EmailLogs_CreatedOn] ON [EmailLogs] ([CreatedOn]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906170051_AddNotifications'
)
BEGIN
    CREATE INDEX [IX_NotificationOutbox_NotificationId] ON [NotificationOutbox] ([NotificationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906170051_AddNotifications'
)
BEGIN
    CREATE INDEX [IX_NotificationOutbox_Pending] ON [NotificationOutbox] ([Status], [NextRetryAt], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906170051_AddNotifications'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId_CreatedAt] ON [Notifications] ([UserId], [CreatedAt] DESC);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906170051_AddNotifications'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId_IsRead] ON [Notifications] ([UserId], [IsRead]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906170051_AddNotifications'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Notifications_UserId_Type_ReferenceId] ON [Notifications] ([UserId], [Type], [ReferenceId]) WHERE [ReferenceId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906170051_AddNotifications'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260906170051_AddNotifications', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907101302_MakeIrbOptionalAndReplaceCitiWithCv'
)
BEGIN
    ALTER TABLE [FileStorage] DROP CONSTRAINT [CK_FileStorage_FileSize];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907101302_MakeIrbOptionalAndReplaceCitiWithCv'
)
BEGIN
    EXEC sp_rename N'[StudentProfiles].[CitiFileId]', N'CvFileId', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907101302_MakeIrbOptionalAndReplaceCitiWithCv'
)
BEGIN
    DECLARE @var4 nvarchar(max);
    SELECT @var4 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Projects]') AND [c].[name] = N'IrbStage');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Projects] DROP CONSTRAINT ' + @var4 + ';');
    ALTER TABLE [Projects] ALTER COLUMN [IrbStage] tinyint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907101302_MakeIrbOptionalAndReplaceCitiWithCv'
)
BEGIN
    EXEC(N'UPDATE [FileStorage] SET [FileCategory] = ''Cv'' WHERE [FileCategory] = ''CitiCertification''');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907101302_MakeIrbOptionalAndReplaceCitiWithCv'
)
BEGIN
    EXEC(N'ALTER TABLE [FileStorage] ADD CONSTRAINT [CK_FileStorage_FileSize] CHECK ((([FileCategory] IN (''Transcript'', ''Cv'') AND [FileSize] <= 10485760) OR ([FileCategory] = ''Poster'' AND [FileSize] <= 5242880)))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907101302_MakeIrbOptionalAndReplaceCitiWithCv'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260907101302_MakeIrbOptionalAndReplaceCitiWithCv', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907114719_AddManualPlacementAssignments'
)
BEGIN
    ALTER TABLE [Placements] ADD [Source] tinyint NOT NULL DEFAULT CAST(0 AS tinyint);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907114719_AddManualPlacementAssignments'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_MatchingRuns_ManualPerSemester] ON [MatchingRuns] ([SemesterId]) WHERE [AlgorithmVersion] = N''manual/v1''');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907114719_AddManualPlacementAssignments'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260907114719_AddManualPlacementAssignments', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908070456_AddEmailSettings'
)
BEGIN
    CREATE TABLE [EmailSettings] (
        [Id] uniqueidentifier NOT NULL,
        [UserName] nvarchar(256) NULL,
        [ProtectedPassword] nvarchar(4000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        CONSTRAINT [PK_EmailSettings] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908070456_AddEmailSettings'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260908070456_AddEmailSettings', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910104347_AddHomeIntro'
)
BEGIN
    CREATE TABLE [HomeIntro] (
        [Id] uniqueidentifier NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [KeyPoints] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        CONSTRAINT [PK_HomeIntro] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910104347_AddHomeIntro'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260910104347_AddHomeIntro', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910104437_AddNewsArticleImages'
)
BEGIN
    ALTER TABLE [NewsArticles] ADD [ImageFileIds] nvarchar(max) NOT NULL DEFAULT N'[]';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910104437_AddNewsArticleImages'
)
BEGIN
    ALTER TABLE [FileStorage] DROP CONSTRAINT [CK_FileStorage_EntityType];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910104437_AddNewsArticleImages'
)
BEGIN
    ALTER TABLE [FileStorage] DROP CONSTRAINT [CK_FileStorage_FileSize];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910104437_AddNewsArticleImages'
)
BEGIN
    EXEC(N'ALTER TABLE [FileStorage] ADD CONSTRAINT [CK_FileStorage_EntityType] CHECK ([EntityType] IN (''StudentProfile'', ''Workshop'', ''NewsArticle''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910104437_AddNewsArticleImages'
)
BEGIN
    EXEC(N'ALTER TABLE [FileStorage] ADD CONSTRAINT [CK_FileStorage_FileSize] CHECK ((([FileCategory] IN (''Transcript'', ''Cv'') AND [FileSize] <= 10485760) OR ([FileCategory] IN (''Poster'', ''NewsImage'') AND [FileSize] <= 5242880)))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910104437_AddNewsArticleImages'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260910104437_AddNewsArticleImages', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910110033_AddHomeIntroHeadline'
)
BEGIN
    ALTER TABLE [HomeIntro] ADD [Headline] nvarchar(200) NOT NULL DEFAULT N'Research starts earlier than you think.';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910110033_AddHomeIntroHeadline'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260910110033_AddHomeIntroHeadline', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910111800_AddPublishedToNewsAndWorkshops'
)
BEGIN
    ALTER TABLE [NewsArticles] ADD [Published] bit NOT NULL DEFAULT CAST(1 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910111800_AddPublishedToNewsAndWorkshops'
)
BEGIN
    ALTER TABLE [Workshops] ADD [Published] bit NOT NULL DEFAULT CAST(1 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910111800_AddPublishedToNewsAndWorkshops'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260910111800_AddPublishedToNewsAndWorkshops', N'10.0.10');
END;

COMMIT;
GO

