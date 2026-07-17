using Microsoft.AspNetCore.Mvc;
using TaskOption;

namespace Library.Service.TaskServices
{
    public class AppTaskService : ControllerBase
    {
        public async Task<ActionResult<T>> SingleTask<T>(SingleTaskOptions<T> options)
        {
            try
            {
                T task = await options.Task();

                return Ok(task);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ProblemDetails
                    {
                        Title = options.ErrorMessage,
                        Detail = ex.Message,
                        Status = StatusCodes.Status500InternalServerError
                    }
                );
            }
        }

        public async Task<ActionResult<TResult>> MultiTask<T1, T2, TResult>(MultiTaskOptions<T1, T2, TResult> options)
        {
            try
            {
                Task<T1> task1 = options.Task1();
                Task<T2> task2 = options.Task2();

                await Task.WhenAll(task1, task2);

                return options.ResultFactory(
                    task1.Result,
                    task2.Result
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ProblemDetails
                    {
                        Title = options.ErrorMessage,
                        Detail = ex.Message,
                        Status = StatusCodes.Status500InternalServerError
                    }
                );
            }
        }

        public async Task<ActionResult<TResult>> TripleTask<T1, T2, T3, TResult>(TripleTaskOptions<T1, T2, T3, TResult> options)
        {
            try
            {
                Task<T1> task1 = options.Task1();
                Task<T2> task2 = options.Task2();
                Task<T3> task3 = options.Task3();

                await Task.WhenAll(task1, task2, task3);

                return options.ResultFactory(
                    task1.Result,
                    task2.Result,
                    task3.Result
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ProblemDetails
                    {
                        Title = options.ErrorMessage,
                        Detail = ex.Message,
                        Status = StatusCodes.Status500InternalServerError
                    }
                );
            }
        }

        public async Task<ActionResult> SqlFunc(SqlTaskOptions options)
        {
            try
            {
                await options.Sql();

                return Ok(new { message = options.SuccessMessage });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ProblemDetails
                    {
                        Title = options.ErrorMessage,
                        Detail = ex.Message,
                        Status = StatusCodes.Status500InternalServerError
                    }
                );
            }
        }
    }

}
