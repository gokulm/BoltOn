using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using BoltOn.Data.CosmosDb;

namespace BoltOn.Data.CosmosDb
{
    public class Repository<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        private readonly Container _taskContainer;

        public Repository(CosmosClient cosmosClient, string databaseName, string? containerName = null)
        {
            var taskContainerName = containerName ?? typeof(TEntity).Name.Pluralize();
            _taskContainer = cosmosClient.GetContainer(databaseName, taskContainerName);
        }

		public Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<TEntity>> AddAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAsync(object id, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task<TEntity> GetByIdAsync(object id, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes)
		{
			throw new NotImplementedException();
		}

		public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		// public async Task<IEnumerable<TEntity>> GetAllAsync(string userId)
		// {
		//     var query = _taskContainer.GetItemLinqQueryable<TEntity>()
		//         .Where(t => t.UserId == userId)
		//         .ToFeedIterator();

		//     var tasks = new List<TasksDocument>();
		//     while (query.HasMoreResults)
		//     {
		//         var response = await query.ReadNextAsync();
		//         tasks.AddRange(response);
		//     }

		//     return tasks;
		// }


		// public async Task<TasksDocument> GetTaskByIdAsync(string taskId, string userId)
		// {
		//        var query = _taskContainer.GetItemLinqQueryable<TasksDocument>()
		//     .Where(t => t.Id == taskId && t.UserId == userId)
		//     .Take(1)
		//     .ToQueryDefinition();

		//     var sqlQuery = query.QueryText; // Retrieve the SQL query

		//     var response = await _taskContainer.GetItemQueryIterator<TasksDocument>(query).ReadNextAsync();
		//     return response.FirstOrDefault();
		// }

		// public async Task<IEnumerable<TasksDocument>> GetAllTasksAsync(string userId)
		// {
		//     var query = _taskContainer.GetItemLinqQueryable<TasksDocument>()
		//         .Where(t => t.UserId == userId)
		//         .ToFeedIterator();

		//     var tasks = new List<TasksDocument>();
		//     while (query.HasMoreResults)
		//     {
		//         var response = await query.ReadNextAsync();
		//         tasks.AddRange(response);
		//     }

		//     return tasks;
		// }

		// public async Task<TasksDocument> CreateTaskAsync(TasksDocument task)
		// {
		//     var response = await _taskContainer.CreateItemAsync(task);
		//     return response.Resource;
		// }

		// public async Task<TasksDocument> UpdateTaskAsync(TasksDocument task)
		// {
		//     var response = await _taskContainer.ReplaceItemAsync(task, task.Id);
		//     return response.Resource;
		// }

		// public async Task DeleteTaskAsync(string taskId, string userId)
		// {
		//     await _taskContainer.DeleteItemAsync<TasksDocument>(taskId, new PartitionKey(userId));
		// }

		// public async Task<TasksDocument> UpdateSubtaskStatusAsync(string taskId, string subtaskId, string status)
		// {
		//     var query = _taskContainer.GetItemLinqQueryable<TasksDocument>()
		//         .Where(t => t.Id == taskId)
		//         .Take(1)
		//         .ToFeedIterator();

		//     var response = await query.ReadNextAsync();
		//     var task = response.FirstOrDefault();

		//     if (task == null)
		//     {
		//         return null;
		//     }

		//     var subtask = task.Subtasks.FirstOrDefault(s => s.Id == subtaskId);
		//     if (subtask == null)
		//     {
		//         return null;
		//     }

		//     subtask.Status = status;

		//     await _taskContainer.ReplaceItemAsync(task, task.Id);

		//     return task;
		// }

		// public async Task<TasksDocument> UpdateAttachmentsAsync(string taskId, List<Attachment> attachmentsToAdd, List<string> attachmentIdsToDelete)
		// {
		//     var query = _taskContainer.GetItemLinqQueryable<TasksDocument>()
		//         .Where(t => t.Id == taskId)
		//         .Take(1)
		//         .ToFeedIterator();

		//     var response = await query.ReadNextAsync();
		//     var task = response.FirstOrDefault();

		//     if (task == null)
		//     {
		//         return null;
		//     }

		//     // Add new attachments
		//     if (attachmentsToAdd != null)
		//     {
		//         task.Attachments.AddRange(attachmentsToAdd);
		//     }

		//     // Delete attachments
		//     if (attachmentIdsToDelete != null)
		//     {
		//         task.Attachments.RemoveAll(a => attachmentIdsToDelete.Contains(a.Id));
		//     }

		//     await _taskContainer.ReplaceItemAsync(task, task.Id);

		//     return task;
		// }

	}
}