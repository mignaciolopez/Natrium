using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEngine.Events;
using System;
using Unity.Collections;

namespace Natrium.Shared.Systems
{
    [Serializable] public class CustomUnityEvent : UnityEvent<Stream>
    {
    }

    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    //[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial class EventSystem : SystemBase
    {
        private static Dictionary<Events, CustomUnityEvent> _handlers;
        private static Queue<Tuple<Events, Stream>> _eventsQueue;
        private static FixedString128Bytes _worldName;

        protected override void OnCreate()
        {
            base.OnCreate();

            _handlers = new Dictionary<Events, CustomUnityEvent>();
            foreach (var evnt in (Events[])Enum.GetValues(typeof(Events)))
                _handlers[evnt] = new CustomUnityEvent();
            _worldName = World.Unmanaged.Name;
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            _eventsQueue = new Queue<Tuple<Events, Stream>>();
        }

        protected override void OnStopRunning()
        {
            _eventsQueue.Clear();
            //_eventsQueue = null;

            base.OnStopRunning();
        }

        protected override void OnDestroy()
        {
            //foreach (var evnt in (Events[])Enum.GetValues(typeof(Events)))
            //    _handlers[evnt] = null;

            _handlers.Clear();
            //_handlers = null;
            
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            foreach (var e in _eventsQueue)
                DispatchEvent(e.Item1, e.Item2);

            _eventsQueue.Clear();
        }

        public static void EnqueueEvent(Events evnt, Stream stream = null)
        {
            Debug.Log($"'{_worldName}' Enqueuing Event: {evnt}");
            _eventsQueue.Enqueue(new Tuple<Events, Stream>(evnt, stream));
        }
        
        public static void DispatchEvent(Events evnt, Stream stream = null)
        {
            if (_handlers.ContainsKey(evnt))
            {
                if (stream != null)
                    stream.Position = 0;

                Debug.Log($"'{_worldName}' Dispatching Event: {evnt}");
                _handlers[evnt]?.Invoke(stream);
            }
            else
                Debug.Log($"'{_worldName}' There are no mHandlers for the Event: {evnt}");

            stream?.Dispose();
        }

        public static bool Subscribe(Events evnt, UnityAction<Stream> ua)
        {
            if (_handlers.ContainsKey(evnt))
            {
                _handlers[evnt].AddListener(ua);
                Debug.Log($"'{_worldName}' {ua.Target} {ua.Method} Subscribed to {evnt}");
                return true;
            }

            Debug.LogWarning($"'{_worldName}' {ua.Target} {ua.Method} Tried to subscribe to {evnt}");
            return false;
        }

        public static bool UnSubscribe(Events evnt, UnityAction<Stream> ua)
        {
            if (_handlers.ContainsKey(evnt))
            {
                _handlers[evnt].RemoveListener(ua);
                Debug.Log($"'{_worldName}' {ua.Target} {ua.Method} UnSubscribed from {evnt}");
                return true;
            }

            Debug.LogWarning($"'{_worldName}' {ua.Target} {ua.Method} Tried to unsubscribe from {evnt}");
            return false;
        }
    }
}