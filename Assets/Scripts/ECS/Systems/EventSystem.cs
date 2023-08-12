using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEngine.Events;
using System;

namespace Natrium
{

    [System.Serializable]
    public class CustomUnityEvent : UnityEvent<CustomStream>
    {
    }

    public partial class EventSystem : SystemBase
    {
        private static Dictionary<Events, CustomUnityEvent> mHandlers;
        private static Queue<Tuple<Events, CustomStream>> mEventsQueue;

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

            mEventsQueue = new Queue<Tuple<Events, CustomStream>>();
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();

            mEventsQueue.Clear();
            mEventsQueue = null;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var evnt in (Events[])Enum.GetValues(typeof(Events)))
                mHandlers[evnt] = null;

            mHandlers.Clear();
            mHandlers = null;
        }

        protected override void OnUpdate()
        {
            foreach (var e in mEventsQueue)
            {
                if (mHandlers.ContainsKey(e.Item1))
                {
                    if (e.Item2 != null)
                        e.Item2.Position = 0;

                    Debug.Log("Dispatching Event: " + e.Item1.ToString());
                    mHandlers[e.Item1]?.Invoke(e.Item2);
                }
                else
                    Debug.LogWarning("There are no mHandlers for the Event: " + e.Item1.ToString());

                e.Item2.Dispose();
            }

            mEventsQueue.Clear();
        }

        public static void DispatchEvent(Events evnt, CustomStream stream = null)
        {
            if (mEventsQueue != null)
                mEventsQueue.Enqueue(new Tuple<Events, CustomStream>(evnt, stream));
        }

        public static bool Subscribe(Events evnt, UnityAction<CustomStream> ua)
        {
            if (mHandlers.ContainsKey(evnt))
            {
                mHandlers[evnt].AddListener(ua);
                Debug.Log(ua.Target.ToString() + " " + ua.Method.ToString() + " Subscribed to " + evnt.ToString());
                return true;
            }

            Debug.LogWarning(ua.Target.ToString() + " " + ua.Method.ToString() + " Tried to subscribe to " + evnt.ToString());
            return false;
        }

        public static bool UnSubscribe(Events evnt, UnityAction<CustomStream> ua)
        {
            if (mHandlers.ContainsKey(evnt))
            {
                mHandlers[evnt].RemoveListener(ua);
                Debug.Log(ua.Target.ToString() + " " + ua.Method.ToString() + " Unsubscribed from " + evnt.ToString());
                return true;
            }

            Debug.LogWarning(ua.Target.ToString() + " " + ua.Method.ToString() + " Tried to Unsubscribe from " + evnt.ToString());
            return false;
        }
    }
}