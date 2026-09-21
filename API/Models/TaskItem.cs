using API.Enums;
using Newtonsoft.Json;

namespace API.Models
{
    /// <summary>
    /// Represents a model for a task.
    /// </summary>
    public class TaskItem
    {
        /// <summary>
        /// The unique ID for the record set
        /// </summary>
        [JsonProperty("id")]
        public Guid Id              { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The name of the task
        /// </summary>
        public string TaskName      { get; set; }

        /// <summary>
        /// The task assignee
        /// </summary>
        public string? Assignee     { get; set; }

        /// <summary>
        /// The deadline for the task
        /// </summary>
        public DateTime? Deadline   { get; set; }

        /// <summary>
        /// The partition key for Cosmos DB
        /// </summary>
        public string PartitionKey => nameof(Keys.TaskPartitionKey);
    }
}
