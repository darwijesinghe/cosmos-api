using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CosmosController : ControllerBase
    {
        // Services
        private readonly CosmosService        _cosmosService;
        private readonly IReadOnlySet<string> _allowedContainers;

        public CosmosController(CosmosService cosmosService, IReadOnlySet<string> allowedContainers)
        {
            _cosmosService     = cosmosService;
            _allowedContainers = allowedContainers;
        }

        /// <summary>
        /// Creates a new item to the db.
        /// </summary>
        /// <param name="containerName">The name of the container (Table).</param>
        /// <param name="data">The data to be added to the database.</param>
        /// <returns>
        /// A <see cref="OkResult"/> if success; otherwise error message.
        /// </returns>
        [HttpPost("create-record")]
        public async Task<IActionResult> AddTaskAsync(string containerName, [FromBody] TaskItem data)
        {
            try
            {
                // checks the container name
                if (!_allowedContainers.Contains(containerName))
                    return BadRequest("Invalid container name.");

                // adds data to the db
                await _cosmosService.AddTaskAsync(data, containerName, data.PartitionKey);
            }
            catch(CosmosException ex)
            {
                // extract the response to get the error message if needed
                return Problem(ex.Message);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }

            // returns the result
            return Ok();
        }

        /// <summary>
        /// Retrieves the data by given ID.
        /// </summary>
        /// <param name="id">The unique key of the record set.</param>
        /// <param name="containerName">The name of the container (Table).</param>
        /// <returns>
        /// A <see cref="OkObjectResult"/> if success; otherwise error message.
        /// </returns>
        [HttpGet("get-task")]
        public async Task<IActionResult> GetTaskAsync(string id, string containerName)
        {
            try
            {
                // checks the container name
                if (!_allowedContainers.Contains(containerName))
                    return BadRequest("Invalid container name.");

                // retrieves the data
                var task = await _cosmosService.GetTaskAsync<TaskItem>(id, containerName);
                if (task is null)
                    return Problem("Required data not found.");

                // returns the result
                return Ok(task);
            }
            catch (CosmosException ex)
            {
                return Problem(ex.Message);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves the all the data.
        /// </summary>
        /// <param name="containerName">The name of the container (Table).</param>
        /// <returns>
        /// A <see cref="OkObjectResult"/> if success; otherwise error message.
        /// </returns>
        [HttpGet("get-tasks")]
        public async Task<IActionResult> GetTasksAsync(string containerName)
        {
            try
            {
                // checks the container name
                if (!_allowedContainers.Contains(containerName))
                    return BadRequest("Invalid container name.");

                // retrieves the data
                var tasks = await _cosmosService.GetTasksAsync<TaskItem>("select * from c where is_defined(c.TaskName)", containerName);
                if (tasks is null || !tasks.Any())
                    return Problem("Required data not found.");

                // returns the result
                return Ok(tasks);
            }
            catch (CosmosException ex)
            {
                return Problem(ex.Message);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves all the tasks along with Assignee and days that left for the task.
        /// </summary>
        /// <param name="containerName">The name of the container (Table).</param>
        /// <returns>
        /// A <see cref="OkObjectResult"/> if success; otherwise error message.
        /// </returns>
        [HttpGet("get-remaining-days")]
        public async Task<IActionResult> GetRemainingDaysAsync(string containerName)
        {
            try
            {
                // checks the container name
                if (!_allowedContainers.Contains(containerName))
                    return BadRequest("Invalid container name.");

                // retrieves the data
                string query = "select c.Assignee, c.TaskName, udf.getDaysLeft(c.Deadline) as DayLeft from c where is_defined(c.TaskName)";
                var result   = await _cosmosService.GetTasksAsync(query, containerName);
                if (result is null || !result.Any())
                    return Problem("Required data not found.");

                // anonymous data result
                var data = result.Select(item => new
                {
                    TaskName = item.TaskName?.ToString(),
                    Assignee = item.Assignee?.ToString(),
                    DayLeft  = (int?)item.DayLeft ?? 0
                });

                // returns the result
                return Ok(data);
            }
            catch (CosmosException ex)
            {
                return Problem(ex.Message);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing record in the database.
        /// </summary>
        /// <param name="id">The unique key of the record set.</param>
        /// <param name="containerName">The name of the container (Table).</param>
        /// <param name="data">The updated data object.</param>
        /// <returns>
        /// A <see cref="OkResult"/> if success; otherwise error message.
        /// </returns>
        [HttpPut("update-task")]
        public async Task<IActionResult> UpdateItemAsync(string id, string containerName, [FromBody] TaskItem data)
        {
            try
            {
                // checks the container name
                if (!_allowedContainers.Contains(containerName))
                    return BadRequest("Invalid container name.");

                // checks the record existence
                var found = await _cosmosService.GetTaskAsync<TaskItem>(id, containerName);
                if(found is null)
                    return BadRequest("No data found to update.");

                // updates the record set
                await _cosmosService.UpdateItemAsync<TaskItem>(id, containerName, data);

                // returns the result
                return Ok();
            }
            catch (CosmosException ex)
            {
                return Problem(ex.Message);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        /// <summary>
        /// Deletes an existing record in the database.
        /// </summary>
        /// <param name="id">The unique key of the record set.</param>
        /// <param name="containerName">The name of the container (Table).</param>
        /// <returns>
        /// A <see cref="OkResult"/> if success; otherwise error message.
        /// </returns>
        [HttpDelete("delete-task")]
        public async Task<IActionResult> DeleteItemAsync(string id, string containerName)
        {
            try
            {
                // checks the container name
                if (!_allowedContainers.Contains(containerName))
                    return BadRequest("Invalid container name.");

                // checks the record existence
                var found = await _cosmosService.GetTaskAsync<TaskItem>(id, containerName);
                if(found is null)
                    return BadRequest("No data found to delete.");

                // updates the record set
                await _cosmosService.DeleteItemAsync(id, containerName);

                // returns the result
                return Ok();
            }
            catch (CosmosException ex)
            {
                return Problem(ex.Message);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        /// <summary>
        /// Handles batch data insertion.
        /// </summary>
        /// <param name="containerName">The name of the container (Table).</param>
        /// <param name="data">The batch data which is to be inserted.</param>
        /// <returns>
        /// A <see cref="OkResult"/> if success; otherwise error message.
        /// </returns>
        [HttpPost("bulk-insert")]
        public async Task<IActionResult> BulkInsert(string containerName, [FromBody] List<TaskItem> data)
        {
            try
            {
                // checks the container name
                if (!_allowedContainers.Contains(containerName))
                    return BadRequest("Invalid container name.");

                if(data is null || !data.Any())
                    return BadRequest("No data found to insert.");

                // gets the execution result
                int insertedCount = await _cosmosService.ExecuteBulkInsertAsync(containerName, data);
                if (insertedCount <= 0)
                    return Problem("Batch data insertion failed. Please contact the admin.");
                return Ok(new { InsertedCount = insertedCount });
            }
            catch (CosmosException ex)
            {
                return Problem(ex.Message);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
    }
}
