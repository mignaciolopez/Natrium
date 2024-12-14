using System.Collections.Generic;
using Unity.Entities;
using UnityEngine.Events;
using System;

namespace Natrium.Shared.Systems
{
    [Serializable] public class CustomUnityEvent : UnityEvent<Stream>
    {
    }
    
    public partial class EventSystem : SystemBase
    {
        private static Dictionary<Events, CustomUnityEvent> _handlers;
        private static Queue<Tuple<Events, Stream>> _eventsQueue;

        protected override void OnCreate()
        {
            base.OnCreate();
            Log.Verbose($"[{World.Name}] OnCreate");

            _handlers = new Dictionary<Events, CustomUnityEvent>();

            foreach (var evnt in (Events[])Enum.GetValues(typeof(Events)))
            {
                _handlers.Add(evnt, new CustomUnityEvent());
                Log.Verbose($"[{World.Name}] Handler created for {evnt}");
            }
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            Log.Verbose($"[{World.Name}] OnStartRunning");
            _eventsQueue = new Queue<Tuple<Events, Stream>>();
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
            Log.Verbose($"[{World.Name}] OnStopRunning");
            _eventsQueue.Clear();
            //_eventsQueue = null;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Log.Verbose($"[{World.Name}] OnDestroy");
            //foreach (var evnt in (Events[])Enum.GetValues(typeof(Events)))
            //    _handlers[evnt] = null;

            //_handlers.Clear();
            //_handlers = null;
        }

        protected override void OnUpdate()
        {
            foreach (var e in _eventsQueue)
                DispatchEvent(e.Item1, e.Item2);

            _eventsQueue.Clear();
        }

        public static void EnqueueEvent(Events evnt, Stream stream = null)
        {
            Log.Debug($"Enqueuing Event: {evnt}");
            _eventsQueue.Enqueue(new Tuple<Events, Stream>(evnt, stream));
        }
        
        public static void DispatchEvent(Events evnt, Stream stream = null)
        {
            if (_handlers.ContainsKey(evnt))
            {
                if (stream != null)
                    stream.Position = 0;

                Log.Verbose($"Dispatching Event: {evnt}");
                _handlers[evnt]?.Invoke(stream);
            }
            else
                Log.Error($"There are no mHandlers for the Event: {evnt}");

            stream?.Dispose();
        }

        public static bool Subscribe(Events evnt, UnityAction<Stream> ua)
        {
            if (_handlers.ContainsKey(evnt))
            {
                _handlers[evnt].AddListener(ua);
                Log.Verbose($"{ua.Target} {ua.Method} Subscribed to {evnt}");
                return true;
            }

            Log.Warning($"{ua.Target} {ua.Method} Tried to subscribe to {evnt}");
            return false;
        }

        public static bool UnSubscribe(Events evnt, UnityAction<Stream> ua)
        {
            if (_handlers.ContainsKey(evnt))
            {
                _handlers[evnt].RemoveListener(ua);
                Log.Verbose($"{ua.Target} {ua.Method} UnSubscribed from {evnt}");
                return true;
            }

            Log.Warning($"{ua.Target} {ua.Method} Tried to unsubscribe from {evnt}");
            return false;
        }
    }
}