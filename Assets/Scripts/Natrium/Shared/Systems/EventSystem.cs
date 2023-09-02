using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEngine.Events;
using System;

namespace Natrium.Shared.Systems
{
    [Serializable] public class CustomUnityEvent : UnityEvent<Stream>
    {
    }

    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial class EventSystem : SystemBase
    {
        private static Dictionary<Events, CustomUnityEvent> _handlers;
        private static Queue<Tuple<Events, Stream>> _eventsQueue;

        private static bool _running;

        protected override void OnCreate()
        {
            base.OnCreate();

            _handlers = new Dictionary<Events, CustomUnityEvent>();
            foreach (var evnt in (Events[])Enum.GetValues(typeof(Events)))
                _handlers[evnt] = new CustomUnityEvent();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            _running = true;
            _eventsQueue = new Queue<Tuple<Events, Stream>>();
        }

        protected override void OnStopRunning()
        {
            _eventsQueue.Clear();
            //mEventsQueue = null;

            _running = false;

            base.OnStopRunning();
        }

        protected override void OnDestroy()
        {
            //foreach (var evnt in (Events[])Enum.GetValues(typeof(Events)))
            //    mHandlers[evnt] = null;

            _handlers.Clear();
            //mHandlers = null;
            
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            foreach (var e in _eventsQueue)
            {
                if (_handlers.ContainsKey(e.Item1))
                {
                    if (e.Item2 != null)
                        e.Item2.Position = 0;

                    Debug.Log($"'{World.Unmanaged.Name}' Dispatching Event: {e.Item1}");
                    _handlers[e.Item1]?.Invoke(e.Item2);
                }
                else
                    Debug.Log($"'{World.Unmanaged.Name}' There are no mHandlers for the Event: {e.Item1}");

                e.Item2?.Dispose();
            }

            _eventsQueue.Clear();
        }

        public static void DispatchEvent(Events evnt, Stream stream = null)
        {
            if (_running)
                _eventsQueue.Enqueue(new Tuple<Events, Stream>(evnt, stream));
        }

        public static bool Subscribe(Events evnt, UnityAction<Stream> ua)
        {
            if (_handlers.ContainsKey(evnt))
            {
                _handlers[evnt].AddListener(ua);
                Debug.Log($"{ua.Target} {ua.Method} Subscribed to {evnt}");
                return true;
            }

            Debug.LogWarning($"{ua.Target} {ua.Method} Tried to subscribe to {evnt}");
            return false;
        }

        public static bool UnSubscribe(Events evnt, UnityAction<Stream> ua)
        {
            if (_handlers.ContainsKey(evnt))
            {
                _handlers[evnt].RemoveListener(ua);
                Debug.Log($"{ua.Target} {ua.Method} UnSubscribed from {evnt}");
                return true;
            }

            Debug.LogWarning($"{ua.Target} {ua.Method} Tried to unsubscribe from {evnt}");
            return false;
        }
    }
}