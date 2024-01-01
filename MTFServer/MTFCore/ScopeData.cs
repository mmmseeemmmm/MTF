using System;
using System.Collections.Generic;
using MTFClientServerCommon;

namespace MTFCore
{
    public class ScopeData
    {
        private Guid EventsDisabledBy;
        private Guid NotifyEventsDisabledBy;
        private Dictionary<Guid, object> lastActivityResult = new Dictionary<Guid, object>();
        private ScopeData parentData;
        private List<ScopeData> childs = new List<ScopeData>();
        public ScopeData()
        {
            ExecutingActivityPath = new List<Guid>();
            SequenceVariant = new SequenceVariant();
            RaiseEvents = true;
            RaiseNotifyEvents = true;
        }
        public bool RaiseEvents { get; private set; }
        public bool RaiseNotifyEvents { get; private set; }
        public List<Guid> ExecutingActivityPath { get; set; }
        public DeviceUnderTestInfo DeviceUnderTestInfo { get; set; }

        public string ExecutingActivityPahtAsString
        {
            get { return string.Join("->", ExecutingActivityPath); }
        }

        public ScopeData CreateChild()
        {
            var newScope = new ScopeData
            {
                ExecutingActivityPath = new List<Guid>(this.ExecutingActivityPath),
                RaiseEvents = this.RaiseEvents,
                RaiseNotifyEvents = this.RaiseNotifyEvents,
                DeviceUnderTestInfo =  this.DeviceUnderTestInfo,
                parentData = this,
                SequenceVariant = SequenceVariant.Clone() as SequenceVariant,
            };

            childs.Add(newScope);

            return newScope;
        }

        public void RemoveChild(ScopeData scope)
        {
            childs.Remove(scope);
        }

        public void DisableEvents(Guid id)
        {
            if (!RaiseEvents)
            {
                return;
            }

            RaiseEvents = false;
            EventsDisabledBy = id;
        }

        public void EnableEvents(Guid id)
        {
            if (id != EventsDisabledBy)
            {
                return;
            }

            RaiseEvents = true;
        }

        public void DisableNotifyEvents(Guid id)
        {
            if (!RaiseNotifyEvents)
            {
                return;
            }

            RaiseNotifyEvents = false;
            NotifyEventsDisabledBy = id;
        }

        public void EnableNotifyEvents(Guid id)
        {
            if (id != NotifyEventsDisabledBy)
            {
                return;
            }

            RaiseNotifyEvents = true;
        }

        public void SetActivityResult(Guid activityId, object result)
        {
            lastActivityResult[activityId] = result;
            if (parentData != null)
            {
                parentData.SetActivityResult(activityId, result);
            }
        }

        public void RemoveActivityResult(Guid activityId)
        {
            if (lastActivityResult.ContainsKey(activityId))
            {
                lastActivityResult.Remove(activityId);
            }
            if (parentData != null)
            {
                parentData.RemoveActivityResult(activityId);
            }
        }

        public object GetActivityResult(Guid activityId)
        {

            if (lastActivityResult.ContainsKey(activityId))
            {
                return lastActivityResult[activityId];
            }
            else if (parentData != null)
            {
                return parentData.GetActivityResult(activityId);
            }
            throw new Exception(string.Format("Activity result for activity id {0} not found, please check your sequence", activityId));
        }

        public bool ContainsActivityResult(Guid activityId)
        {
            var contains = lastActivityResult.ContainsKey(activityId);
            return (contains == false && parentData != null) ? parentData.ContainsActivityResult(activityId) : contains;
        }

        public SequenceVariant SequenceVariant { get; set; }
        public IEnumerable<string> SequenceVariantGroups => SequenceVariant?.Groups;

        public IEnumerable<string> SequenceVariantValue(string groupName) => SequenceVariant?.GetVariant(groupName);
    }
}
