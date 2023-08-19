using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEngine.Events;
using System;
using Unity.VisualScripting;
using Unity.Transforms;

namespace Natrium
{

    [System.Serializable]
    public class CustomUnityEvent : UnityEvent<CustomStream>
    {
    }

    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial class EventSystem : SystemBase
    {
        private static Dictionary<Events, CustomUnityEvent> mHandlers;
        private static Queue<Tuple<Events, CustomStream>> mEventsQueue;

        private static bool mRunning;

        private static string sWorldName;

        protected override void OnCreate()
        {
            base.OnCreate();

            mHandlers = new Dictionary<Events, CustomUnityEvent>();
            foreach (var evnt in (Events[])Enum.GetValues(typeof(Events)))
                mHandlers[evnt] = new CustomUnityEvent();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            sWorldName = World.Name;
            mRunning = true;
            mEventsQueue = new Queue<Tuple<Events, CustomStream>>();
        }

        protected override void OnStopRunning()
        {
            mEventsQueue.Clear();
            //mEventsQueue = null;

            mRunning = false;

            base.OnStopRunning();
        }

        protected override void OnDestroy()
        {
            //foreach (var evnt in (Events[])Enum.GetValues(typeof(Events)))
            //    mHandlers[evnt] = null;

            mHandlers.Clear();
            //mHandlers = null;
            
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            foreach (var e in mEventsQueue)
            {
                if (mHandlers.ContainsKey(e.Item1))
                {
                    if (e.Item2 != null)
                        e.Item2.Position = 0;

                    Debug.Log($"'{World.Unmanaged.Name}' Dispatching Event: {e.Item1}");
                    mHandlers[e.Item1]?.Invoke(e.Item2);
                }
                else
                    Debug.Log($"'{World.Unmanaged.Name}' There are no mHandlers for the Event: {e.Item1}");

                e.Item2?.Dispose();
            }

            mEventsQueue.Clear();
        }

        public static void DispatchEvent(Events evnt, CustomStream stream = null)
        {
            if (mRunning)
                mEventsQueue.Enqueue(new Tuple<Events, CustomStream>(evnt, stream));
        }

        public static bool Subscribe(Events evnt, UnityAction<CustomStream> ua)
        {
            if (mHandlers.ContainsKey(evnt))
            {
                mHandlers[evnt].AddListener(ua);
                Debug.Log($"'{sWorldName}' {ua.Target} {ua.Method} Subscribed to {evnt}");
                return true;
            }

            Debug.LogWarning($"'{sWorldName}' {ua.Target} {ua.Method} Tried to subscribe to {evnt}");
            return false;
        }

        public static bool UnSubscribe(Events evnt, UnityAction<CustomStream> ua)
        {
            if (mHandlers.ContainsKey(evnt))
            {
                mHandlers[evnt].RemoveListener(ua);
                Debug.Log($"'{sWorldName}' {ua.Target} {ua.Method} UnSubscribed from {evnt}");
                return true;
            }

            Debug.LogWarning($"'{sWorldName}' {ua.Target} {ua.Method} Tried to unsubscribe from {evnt}");
            return false;
        }
    }
}