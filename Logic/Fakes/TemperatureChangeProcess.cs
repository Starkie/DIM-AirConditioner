namespace Dim.AirConditioner.Logic.Fakes
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary> Represents the proccess a temperature change by an <see cref="IAirConditioner"/>. </summary>
    internal class TemperatureChangeProcess
    {
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly Task temperatureChangeTask;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TemperatureChangeProcess"/> class.
        /// </summary>
        /// <param name="temperatureChangeTask"> The task controlling a temperature change. </param>
        /// <param name="cancellationTokenSource">
        ///     The cancelation token source, to cancel the task.
        /// </param>
        public TemperatureChangeProcess(Task temperatureChangeTask, CancellationTokenSource cancellationTokenSource)
        {
            this.temperatureChangeTask = temperatureChangeTask;
            this.cancellationTokenSource = cancellationTokenSource;
        }

        /// <summary> Is the current task running. </summary>
        /// <returns> A boolean indicating whether the task is currently running or not. </returns>
        public bool IsRunning()
        {
            return !(this.temperatureChangeTask.IsCompleted
                || this.temperatureChangeTask.IsCanceled
                || this.temperatureChangeTask.IsFaulted);
        }

        /// <summary> Cancels the current temperature change process. </summary>
        /// <returns> A task that enables this method to be awaited. </returns>
        public async Task Cancel()
        {
            // If the task finished, do nothing.
            if (!this.IsRunning())
            {
                return;
            }

            this.cancellationTokenSource.Cancel();

            await this.temperatureChangeTask;
        }
    }
}