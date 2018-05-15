﻿using MLAPI.NetworkingManagerComponents.Core;
using UnityEngine;

namespace MLAPI.Data.NetworkProfiler
{
    public static class NetworkProfiler
    {
        public static FixedQueue<ProfilerTick> Ticks = null;
        private static int tickHistory = 1024;
        private static int EventIdCounter = 0;
        private static bool isRunning = false;
        public static bool IsRunning
        {
            get
            {
                return isRunning;
            }
        }
        private static ProfilerTick CurrentTick;

        public static void Start(int historyLength)
        {
            if (isRunning)
                return;
            EventIdCounter = 0;
            Ticks = new FixedQueue<ProfilerTick>(historyLength);
            tickHistory = historyLength;
            CurrentTick = null;
            isRunning = true;
        }

        public static ProfilerTick[] Stop()
        {
            if (!isRunning)
                return new ProfilerTick[0];
            ProfilerTick[] ticks = new ProfilerTick[Ticks.Count];
            for (int i = 0; i < Ticks.Count; i++)
                ticks[i] = Ticks.ElementAt(i);
            
            Ticks = null; //leave to GC
            CurrentTick = null; //leave to GC
            isRunning = false;
            return ticks;
        }

        internal static void StartTick(TickType type)
        {
            if (!isRunning)
                return;
            if (Ticks.Count == tickHistory)
                Ticks.Dequeue();

            ProfilerTick tick = new ProfilerTick()
            {
                Type = type,
                Frame = Time.frameCount,
                EventId = EventIdCounter
            };
            EventIdCounter++;
            Ticks.Enqueue(tick);
            CurrentTick = tick;
        }

        internal static void EndTick()
        {
            if (!isRunning)
                return;
            if (CurrentTick == null)
                return;
            CurrentTick = null;
        }
        
        internal static void StartEvent(TickType eventType, uint bytes, int channelId, ushort messageId)
        {
            if (!isRunning)
                return;
            if (CurrentTick == null)
                return;
            string channelName = MessageManager.reverseChannels.ContainsKey(channelId) ? MessageManager.reverseChannels[channelId] : "INVALID_CHANNEL";
            string messageName = MessageManager.reverseMessageTypes.ContainsKey(messageId) ? MessageManager.reverseMessageTypes[messageId] : "INVALID_MESSAGE_TYPE";

            CurrentTick.StartEvent(eventType, bytes, channelName, messageName);
        }

        internal static void StartEvent(TickType eventType, uint bytes, string channelName, string messageName)
        {
            if (!isRunning)
                return;
            if (CurrentTick == null)
                return;
            
            CurrentTick.StartEvent(eventType, bytes, channelName, messageName);
        }

        internal static void EndEvent()
        {
            if (!isRunning)
                return;
            if (CurrentTick == null)
                return;
            CurrentTick.EndEvent();
        }
    }
}
