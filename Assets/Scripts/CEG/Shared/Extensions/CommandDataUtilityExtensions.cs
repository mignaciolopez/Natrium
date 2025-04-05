using Unity.Entities;
using Unity.NetCode;

namespace CEG.Shared.Extensions
{
    public static class CommandDataUtilityExtensions
    {
        public static bool GetLastTick<T>(this DynamicBuffer<T> commandArray, out T commandData)
            where T : unmanaged, ICommandData
        {
            commandData = default;
            var maxTick = commandArray.Length > 0 ? commandArray[0].Tick : NetworkTick.Invalid;

            for (int i = 0; i < commandArray.Length; ++i)
            {
                commandData = commandArray[i];
                if (commandData.Tick.IsValid && !commandData.Tick.IsNewerThan(maxTick))
                {
                    maxTick = commandData.Tick;
                    commandData = commandArray[i];
                }
            }

            return maxTick.IsValid;
        }

        public static bool GetLastCommand<T>(this DynamicBuffer<T> commandArray, out T commandData)
            where T : unmanaged, ICommandData
        {
            commandData = default;
            
            if (commandArray.Length > 0)
                commandData = commandArray[^1];
            
            return commandArray.Length > 0;
        }

        /// <summary>
        /// Get latest command data for given target tick.
        /// For example, if command buffer contains ticks 3,4,5,6 and targetTick is 5
        /// it will return tick 5 (latest without going over). If the command buffer is
        /// 1,2,3 and targetTick is 5 it will return nothing and false.
        /// </summary>
        /// <param name="commandArray">Command input buffer.</param>
        /// <param name="targetTick">Target tick to fetch from.</param>
        /// <param name="commandData">The last-received input.</param>
        /// <param name="useNetCodeAPI">If true, use the original GetDataAtTick function provided by the NetCode team.</param>
        /// <typeparam name="T">Command input buffer type.</typeparam>
        /// <returns>Returns true if targetTick was found, false when targetTick is not found in the buffer.</returns>
        public static bool GetDataAtTick<T>(this DynamicBuffer<T> commandArray, NetworkTick targetTick, out T commandData, bool useNetCodeAPI)
            where T : unmanaged, ICommandData
        {
            if (useNetCodeAPI)
                return commandArray.GetDataAtTick(targetTick, out commandData);
            
            var found = false;
            commandData = default;
            
            if (!targetTick.IsValid)
                return found;

            for (var i = 0; i < commandArray.Length; ++i)
            {
                var tick = commandArray[i].Tick;
                if (tick != targetTick)
                    continue;
                
                found = true;
                commandData = commandArray[i];
                break;
            }

            return found;
        }
    }
}