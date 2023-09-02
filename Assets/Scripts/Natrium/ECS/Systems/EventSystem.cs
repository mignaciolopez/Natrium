using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEngine.Events;
using System;

namespace Natrium.Ecs.Systems
{
    [Serializable]
    public class CustomUnityEvent : UnityEvent<Stream>
    {
    }

    public partial class EventSystem : SystemBase
    {
        private static Dictionary<Events, CustomUnityEvent> _handlers;
        private static Queue<Tuple<Events, Stream>> _eventsQueue;

        protected override void OnCreate()
        {
            base.OnCreate();

            _handlers = new Dictionary<Events, CustomUnityEvent>();
            foreach (var nEvent in (Events[])Enum.GetValues(typeof(Events)))
                _handlers[nEvent] = new CustomUnityEvent();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();

            _eventsQueue = new Queue<Tuple<Events, Stream>>();
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();

            _eventsQueue.Clear();
            _eventsQueue = null;
        }

        protected override void OnDestroy()
        {
            foreach (var nEvent in (Events[])Enum.GetValues(typeof(Events)))
                _handlers[nEvent] = null;

            _handlers.Clear();
            _handlers = null;
         
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            foreach (var e in _eventsQueue)
            {
                DispatchEvent(e.Item1, e.Item2);
            }

            _eventsQueue.Clear();
        }

        public static void DispatchEvent(Events nEvent, Stream stream = null)
        {
            if (_handlers.ContainsKey(nEvent))
            {
                if (stream != null)
                    stream.Position = 0;

                Debug.Log($"Dispatching Event: {nEvent}");
                _handlers[nEvent].Invoke(stream);
            }
            else
                Debug.LogWarning($"There are no mHandlers for the Event: {nEvent}");
            
            stream?.Dispose();
        }
        
        public static void EnqueueEvent(Events nEvent, Stream stream = null)
        {
            _eventsQueue?.Enqueue(new Tuple<Events, Stream>(nEvent, stream));
        }

        public static bool Subscribe(Events nEvent, UnityAction<Stream> ua)
        {
            if (_handlers.ContainsKey(nEvent))
            {
                _handlers[nEvent].AddListener(ua);
                Debug.Log($"{ua.Target} {ua.Method} Subscribed to {nEvent}");
                return true;
            }

            Debug.LogWarning($"{ua.Target} {ua.Method} Tried to subscribe to {nEvent}");
            return false;
        }

        public static bool UnSubscribe(Events nEvent, UnityAction<Stream> ua)
        {
            if (_handlers.ContainsKey(nEvent))
            {
                _handlers[nEvent].RemoveListener(ua);
                Debug.Log($"{ua.Target} {ua.Method} Unsubscribed from {nEvent}");
                return true;
            }
            
            Debug.LogWarning($"{ua.Target} {ua.Method} Tried to Unsubscribe from {nEvent}");
            return false;
        }
    }
}